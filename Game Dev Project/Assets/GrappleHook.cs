using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrappleHook : MonoBehaviour {

    // PUBLIC VARIABLES
    public Transform player;
    public Transform anchor;
    public Transform hook;
    public LineRenderer ropeRenderer;
    public DistanceJoint2D distanceJoint;
    public HookThrow hookThrow;


    // Defines which layers the rope can collide with
    public LayerMask ropeLayerMask;

    public float maxLength;

    // PRIVATE VARIABLES
    private Vector2 playerPos;

    public List<Vector2> ropePositions = new List<Vector2>();

    /* Stores int 1 or -1 for each rope vertex
     * depending on whether the rope was wrapped
     * clockwise or counter-clockwise */
    private List<int> ropeWraps = new List<int>();

    [SerializeField]
    private bool fire, reset;

    private double angle1, angle2, lastAngle1, lastAngle2;
    private double[][] angles;

    private void Start() {

        ResetRope();
    }



    private void Update() {

        playerPos = player.position;

        if (hookThrow.thrown) {

            hookThrow.thrown = false;
            ropePositions.Add(hook.position);
            ropePositions.Add(player.position);
            //distanceJoint.enabled = true;
        }
        else if (fire) {

            RaycastHit2D hit = Physics2D.Raycast(playerPos, new Vector2(0, 1), maxLength, ropeLayerMask);
            if (hit.collider != null) {

                ropePositions.Add(hit.point);
                distanceJoint.enabled = true;
            }
        }
        else if (reset) {

            reset = false;
            ResetRope();
        }

        // If the rope is currently active
        if (ropePositions.Count > 0) {

            HandleRopePositions();
            UpdateRopeRenderer();
        }

    }



    /* This method checks if 
     * another rope point should be created (wrap)
     * and if a rope point can be removed (unwrap) */
    private void HandleRopePositions() {

        ropePositions.Insert(0, hook.position);
        ropePositions.RemoveAt(1);

        ropePositions.Add(player.position);
        ropePositions.RemoveAt(ropePositions.Count() - 2);

        //GetAngles();

        //CheckUnwrap();

        CheckWrap();

        // Set the anchor position
        anchor.position = ropePositions.ElementAt(ropePositions.Count() - 1);



        /* Adjust the length of the rope
         * to match the max distance */
        distanceJoint.distance = (maxLength - GetDistance());

        // Hold the values for next update
        lastAngle1 = angle1;
        lastAngle2 = angle2;
    }



    /* Get the distance between each segment of the rope */
    private float GetDistance() {

        float distance = 0;
        for (int i = 0; i < ropePositions.Count - 1; i++)
            distance += Vector2.Distance(ropePositions.ElementAt(i), ropePositions.ElementAt(i + 1));

        return distance;
    }



    private void GetAngles() {

        if (ropePositions.Count > 1) {

            // Save the current angles
            angle1 = Vector2.SignedAngle(ropePositions.ElementAt(ropePositions.Count - 2), ropePositions.Last() - ropePositions.ElementAt(ropePositions.Count - 2));
            angle2 = Vector2.SignedAngle(ropePositions.ElementAt(ropePositions.Count - 2), playerPos - ropePositions.ElementAt(ropePositions.Count - 2));

            // Correct Euler angle overwrapping (prevent 180 looping to -180)
            while (angle1 < lastAngle1 - 180f) angle1 += 360f;
            while (angle1 > lastAngle1 + 180f) angle1 -= 360f;

            while (angle2 < lastAngle2 - 180f) angle2 += 360f;
            while (angle2 > lastAngle2 + 180f) angle2 -= 360f;
        }
    }



    /* This method uses angles to check
     * whether the rope can be unwrapped */
    private void CheckUnwrap() {

        if (ropePositions.Count > 1) {

            /* If the path to the second last point
             * is no longer obstructed, unwrap the rope */
            if (angle2 < angle1 && ropeWraps.Last() == 1) {

                ropeWraps.RemoveAt(ropeWraps.Count - 1);
                ropePositions.RemoveAt(ropePositions.Count - 1);
            }
            else if (angle2 > angle1 && ropeWraps.Last() == -1) {

                ropeWraps.RemoveAt(ropeWraps.Count - 1);
                ropePositions.RemoveAt(ropePositions.Count - 1);
            }
        }
    }



    /* This method checks if the rope has been
     * obstructed and can wrap around an object */
    private void CheckWrap() {

        RaycastHit2D ropeRaycast;

        int i = 0;

        do {

            // Define last rope point and RayCast to next
            ropeRaycast = Physics2D.Raycast(ropePositions.ElementAt(i),
                          (ropePositions.ElementAt(i) - ropePositions.ElementAt(i + 1)).normalized,
                          Vector2.Distance(ropePositions.ElementAt(i), ropePositions.ElementAt(i + 1)) - 0.1f,
                          ropeLayerMask);

            // If rope raycast is interrupted, add a vertex to the line
            if (ropeRaycast) {

                // Get collider and find its closest vertex to point of collision
                PolygonCollider2D colliderWithVertices = ropeRaycast.collider as PolygonCollider2D;

                if (colliderWithVertices) {

                    Vector2 closestPointToHit = GetClosestColliderPoint(ropeRaycast.point, colliderWithVertices);

                    // Add the new rope position
                    ropePositions.Insert(i+1,closestPointToHit);

                    // Save the new angle data immediately
                    angles[i][0] = Vector2.SignedAngle(ropePositions.ElementAt(i+2), ropePositions.ElementAt(i+1) - ropePositions.ElementAt(i+1));
                    angles[i][1] = Vector2.SignedAngle(ropePositions.ElementAt(i+2), ropePositions.ElementAt(i) - ropePositions.ElementAt(i+1));

                    //lastAngle1 = angle1 = Vector2.SignedAngle(ropePositions.ElementAt(ropePositions.Count - 2), ropePositions.Last() - ropePositions.ElementAt(ropePositions.Count - 2));
                    //lastAngle2 = angle2 = Vector2.SignedAngle(ropePositions.ElementAt(ropePositions.Count - 2), playerPos - ropePositions.ElementAt(ropePositions.Count - 2));

                    //Debug.Log("PLAYERANGLE = " + playerAngle + "\tHINGEANGLE = " + hingeAngle);

                    if (angle2 < angle1)
                        ropeWraps.Add(-1);
                    else if (angle2 > angle1)
                        ropeWraps.Add(1);
                }
            }

            i++;
        }
        while (i < ropePositions.Count() - 2);

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



    public void ResetRope()
    {
        ropePositions = new List<Vector2>();
        ropeWraps = new List<int>();
        distanceJoint.enabled = false;
        UpdateRopeRenderer();
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