using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using UnityEditor;

public class GrappleHook : MonoBehaviour {

    // PUBLIC VARIABLES
    public Transform player;
    public Transform anchor;
    public LineRenderer ropeRenderer;
    public DistanceJoint2D distanceJoint;

    public LayerMask ropeLayerMask;

    public float maxLength;

    // PRIVATE VARIABLES
    private Vector2 playerPos;

    private List<Vector2> ropePositions = new List<Vector2>();

    /* Stores int 1 or -1 for each rope vertex
     * depending on whether the rope was wrapped
     * clockwise or counter-clockwise */
    private List<int> ropeWraps = new List<int>();

    [SerializeField]
    private bool fire, reset;

    private double hingeAngle, playerAngle, lastHingeAngle, lastPlayerAngle;

    private void Start() {

        ResetRope();
    }


    private void Update() {

        playerPos = player.position;

        if (fire)
        {

            fire = false;
            RaycastHit2D hit = Physics2D.Raycast(playerPos, new Vector2(0, 1), maxLength, ropeLayerMask);
            if (hit.collider != null)
                ropePositions.Add(hit.point);
        }
        else if (reset)
            ResetRope();

        // If the rope is currently active
        if (ropePositions.Count > 0)
            HandleRopePositions();

        UpdateRopeRenderer();
    }



    /* This method checks if 
     * another rope point should be created (wrap)
     * and if a rope point can be removed (unwrap) */
    private void HandleRopePositions() {

        GetAngles();

        CheckUnwrap();
          
        CheckWrap();

        // Set the anchor position
        anchor.position = ropePositions.ElementAt(ropePositions.Count() - 1);

        /* Adjust the length of the rope
         * to match the max distance */
        distanceJoint.distance = (maxLength - GetDistance());

        // Hold the values for next update
        lastHingeAngle = hingeAngle;
        lastPlayerAngle = playerAngle;
    }



    /* Get the distance between each segment of the rope */
    private float GetDistance() {

        float distance = 0;
        for (int i = 0; i < ropePositions.Count - 1; i++)
            distance += Vector2.Distance(ropePositions.ElementAt(i), ropePositions.ElementAt(i + 1));

        return distance;
    }



    private void GetAngles() {

        if(ropePositions.Count > 1) {

            // Save the current angles
            hingeAngle = Vector2.SignedAngle(ropePositions.ElementAt(ropePositions.Count - 2), ropePositions.Last() - ropePositions.ElementAt(ropePositions.Count - 2));
            playerAngle = Vector2.SignedAngle(ropePositions.ElementAt(ropePositions.Count - 2), playerPos - ropePositions.ElementAt(ropePositions.Count - 2));

            // Correct Euler angle overwrapping (prevent 180 looping to -180)
            while (hingeAngle < lastHingeAngle - 180f) hingeAngle += 360f;
            while (hingeAngle > lastHingeAngle + 180f) hingeAngle -= 360f;

            while (playerAngle < lastPlayerAngle - 180f) playerAngle += 360f;
            while (playerAngle > lastPlayerAngle + 180f) playerAngle -= 360f;
        }
    }



    /* This method uses angles to check
     * whether the rope can be unwrapped */
    private void CheckUnwrap() {

        if(ropePositions.Count > 1) {

            /* If the path to the second last point
             * is no longer obstructed, unwrap the rope */
            if (playerAngle < hingeAngle && ropeWraps.Last() == 1) {

                ropeWraps.RemoveAt(ropeWraps.Count - 1);
                ropePositions.RemoveAt(ropePositions.Count - 1);
            }
            else if (playerAngle > hingeAngle && ropeWraps.Last() == -1) {

                ropeWraps.RemoveAt(ropeWraps.Count - 1);
                ropePositions.RemoveAt(ropePositions.Count - 1);
            }
        }
    }



    private void CheckWrap() {

        // Define last rope point and RayCast to next
        Vector2 lastRopePoint = ropePositions.Last();
        RaycastHit2D playerToLastRopePoint = Physics2D.Raycast(playerPos,
                                                (lastRopePoint - playerPos).normalized,
                                                Vector2.Distance(playerPos,
                                                lastRopePoint) - 0.1f,
                                                ropeLayerMask);

        // If rope raycast is interrupted, add a vertex to the line
        if (playerToLastRopePoint) {

            // Get collider and find its closest vertex to point of collision
            PolygonCollider2D colliderWithVertices = playerToLastRopePoint.collider as PolygonCollider2D;

            if (colliderWithVertices) {

                Vector2 closestPointToHit = GetClosestColliderPoint(playerToLastRopePoint.point, colliderWithVertices);

                // Add the new rope position
                ropePositions.Add(closestPointToHit);

                // Save the new angle data immediately
                lastHingeAngle = hingeAngle = Vector2.SignedAngle(ropePositions.ElementAt(ropePositions.Count - 2), ropePositions.Last() - ropePositions.ElementAt(ropePositions.Count - 2));
                lastPlayerAngle = playerAngle = Vector2.SignedAngle(ropePositions.ElementAt(ropePositions.Count - 2), playerPos - ropePositions.ElementAt(ropePositions.Count - 2));

                //Debug.Log("PLAYERANGLE = " + playerAngle + "\tHINGEANGLE = " + hingeAngle);

                if (playerAngle < hingeAngle)
                    ropeWraps.Add(-1);
                else if (playerAngle > hingeAngle)
                    ropeWraps.Add(1);
            }
        }

    }



    /* This method tells the rope's line renderer
     * the position of each vertex.
     * This is purely cosmetic. */
    private void UpdateRopeRenderer() {

        // Set position count of the line renderer
        ropeRenderer.positionCount = ropePositions.Count + 1;

        // Start at the top of the list and go through each rope position
        for (int i = ropeRenderer.positionCount - 1; i >= 0; i--) {

            // if not the last point of rope renderer
            if (i != ropeRenderer.positionCount - 1)
                // Set the position of the corresponding vertex i in the rope renderer
                ropeRenderer.SetPosition(i, ropePositions[i]);
            
            // If last, connect rope to player
            else
                ropeRenderer.SetPosition(i, playerPos);
        }
    }



    private void ResetRope()
    {
        ropePositions = new List<Vector2>();
        ropeWraps = new List<int>();


    }



    /* This code is wizardry!! Beware!!
     * Given a position, it will find the
     * nearest vertex of a polygon collider */
    private Vector2 GetClosestColliderPoint(Vector2 hit, PolygonCollider2D polyCollider)
    {
        var distanceDictionary = polyCollider.points.ToDictionary<Vector2, float, Vector2>(
            position => Vector2.Distance(hit, polyCollider.transform.TransformPoint(position)),
            position => polyCollider.transform.TransformPoint(position));

        var orderedDictionary = distanceDictionary.OrderBy(e => e.Key);
        return orderedDictionary.Any() ? orderedDictionary.First().Value : Vector2.zero;
    }
}