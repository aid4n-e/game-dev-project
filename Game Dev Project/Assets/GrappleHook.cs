using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using UnityEditor;

public class GrappleHook : MonoBehaviour {

    public Transform player;
    public LineRenderer ropeRenderer;

    public Vector2 playerPos;
    public LayerMask ropeLayerMask;

    private List<Vector2> ropePositions = new List<Vector2>();

    public List<int> angleWrap = new List<int>();
    public bool fire;

    private double hingeAngle, playerAngle, lastHingeAngle, lastPlayerAngle;

    void Update() {

        playerPos = player.position;

        if(fire) {

            fire = false;
            RaycastHit2D hit = Physics2D.Raycast(playerPos, new Vector2(0, 1), 20, ropeLayerMask);
            if (hit.collider != null)
                ropePositions.Add(hit.point);
        }

        // If the rope is currently active
        if (ropePositions.Count > 0)
            GetRopePositions();

        // Print rope angles
        if(ropePositions.Count > 1)
        {
            Debug.Log("A > L = " + hingeAngle);
            Debug.Log("L > P = " + playerAngle);
        }

        UpdateRopeRenderer();
    }



    // This method checks if the
    private void GetRopePositions() {

        // Define last rope point and RayCast to next
        Vector2 lastRopePoint = ropePositions.Last();
        RaycastHit2D playerToLastRopePoint = Physics2D.Raycast(playerPos,
                                                (lastRopePoint - playerPos).normalized,
                                                Vector2.Distance(playerPos,
                                                lastRopePoint) - 0.1f,
                                                ropeLayerMask);

        /* If there are more than two vertices,
         * this section checks if the rope can be unwrapped */
        if (ropePositions.Count > 1) {

            hingeAngle = Vector2.SignedAngle(ropePositions.ElementAt(ropePositions.Count - 2), ropePositions.Last() - ropePositions.ElementAt(ropePositions.Count - 2));
            playerAngle = Vector2.SignedAngle(ropePositions.ElementAt(ropePositions.Count - 2), playerPos - ropePositions.ElementAt(ropePositions.Count - 2));

            // Correct Euler angle overwrapping
            while (hingeAngle < lastHingeAngle - 180f) hingeAngle += 360f;
            while (hingeAngle > lastHingeAngle + 180f) hingeAngle -= 360f;

            while (playerAngle < lastPlayerAngle - 180f) playerAngle += 360f;
            while (playerAngle > lastPlayerAngle + 180f) playerAngle -= 360f;

            RaycastHit2D playerToSecondLastHit = Physics2D.Raycast(playerPos,
                                                    (ropePositions.ElementAt(ropePositions.Count - 2) - playerPos).normalized,
                                                    Vector2.Distance(playerPos,
                                                    ropePositions.ElementAt(ropePositions.Count - 2)) - 0.1f,
                                                    ropeLayerMask);

            /* If the path to the second last point
             * is no longer obstructed, unwrap the rope */
            CheckUnwrap();

            lastHingeAngle = hingeAngle;
            lastPlayerAngle = playerAngle;
            
        }

        // If rope raycast is interrupted, add a vertex to the line
        if (playerToLastRopePoint) {

            // Get collider and find its closest vertex to point of collision
            PolygonCollider2D colliderWithVertices = playerToLastRopePoint.collider as PolygonCollider2D;

            if (colliderWithVertices != null) {

                Vector2 closestPointToHit = GetClosestColliderPoint(playerToLastRopePoint.point, colliderWithVertices);

                // Add the new rope position
                ropePositions.Add(closestPointToHit);

                // Save the angle data for reference when unwrapping
                hingeAngle = Vector2.SignedAngle(ropePositions.ElementAt(ropePositions.Count - 2), ropePositions.Last() - ropePositions.ElementAt(ropePositions.Count - 2));
                playerAngle = Vector2.SignedAngle(ropePositions.ElementAt(ropePositions.Count - 2), playerPos - ropePositions.ElementAt(ropePositions.Count - 2));

                lastHingeAngle = hingeAngle;
                lastPlayerAngle = playerAngle;


                Debug.Log("PLAYERANGLE = " + playerAngle + "\tHINGEANGLE = " + hingeAngle);

                if (playerAngle < hingeAngle)
                    angleWrap.Add(-1);
                else if (playerAngle > hingeAngle)
                    angleWrap.Add(1);

            }
        }
    }


    private void CheckUnwrap()
    {

        if (playerAngle < hingeAngle && angleWrap.Last() == 1)
        {

            angleWrap.RemoveAt(angleWrap.Count - 1);
            ropePositions.RemoveAt(ropePositions.Count - 1);
        }
        else if (playerAngle > hingeAngle && angleWrap.Last() == -1)
        {

            angleWrap.RemoveAt(angleWrap.Count - 1);
            ropePositions.RemoveAt(ropePositions.Count - 1);
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
            if (i != ropeRenderer.positionCount - 1) {

                // Set the position of the corresponding vertex i in the rope renderer
                ropeRenderer.SetPosition(i, ropePositions[i]);
            }
            
            // If last, connect rope to player
            else {

                ropeRenderer.SetPosition(i, playerPos);
            }
        }
    }


    /* This code is wizardry!! Beware!!
     * Given a position, it will find the nearest vertex
     * of a polygon collider for you */
    private Vector2 GetClosestColliderPoint(Vector2 hit, PolygonCollider2D polyCollider)
    {
        var distanceDictionary = polyCollider.points.ToDictionary<Vector2, float, Vector2>(
            position => Vector2.Distance(hit, polyCollider.transform.TransformPoint(position)),
            position => polyCollider.transform.TransformPoint(position));

        var orderedDictionary = distanceDictionary.OrderBy(e => e.Key);
        return orderedDictionary.Any() ? orderedDictionary.First().Value : Vector2.zero;
    }
}