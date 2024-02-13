using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

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
    private Vector2 oldPlayerPos;

    public List<Vector2> ropePositions = new List<Vector2>();

    /* Stores int 1 or -1 for each rope vertex
     * depending on whether the rope was wrapped
     * clockwise or counter-clockwise */
    private List<int> angleWraps = new List<int>();

    [SerializeField]
    private bool fire, reset;

    private double angle1, angle2, lastAngle1, lastAngle2;
    public List<double[]> angles = new List<double[]>();
    public List<double[]> oldAngles = new List<double[]>();
    public List<Transform> holdPoints = new List<Transform>();

    private GameObject scalePoint, referencePoint;

    private void Start() {
        scalePoint = new GameObject("scalePoint");
        referencePoint = new GameObject("referencePoint");
        
        ResetRope();
        oldPlayerPos = player.position;
    }



    private void Update() {

        
        if (hookThrow.thrown) {

            hookThrow.thrown = false;
            ropePositions.Add(hook.position);
            ropePositions.Add(player.position);
            //distanceJoint.enabled = true;
        }
        else if (fire) {

            RaycastHit2D hit = Physics2D.Raycast(player.position, new Vector2(0, 1), maxLength, ropeLayerMask);
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

        oldPlayerPos = player.position;
    }



    /* This method checks if 
     * another rope point should be created (wrap)
     * and if a rope point can be removed (unwrap) */
    private void HandleRopePositions() {

        ropePositions.Insert(0, hook.position);
        ropePositions.RemoveAt(1);

        ropePositions.Add(player.position);
        ropePositions.RemoveAt(ropePositions.Count() - 2);

        MatchHoldPoints();

        float travelDistance = Vector2.Distance((Vector2)player.position, oldPlayerPos);
        int iterations = (int)Mathf.Floor(travelDistance / 0.2f);

        Vector2 oldRopePosition = ropePositions.Last();

        for (int i = 1; i <= iterations; i++) {

            ropePositions.RemoveAt(ropePositions.Count - 1);
            Vector2 interposition = oldPlayerPos + (i * 0.2f) * ((Vector2)player.position - oldPlayerPos).normalized;
            ropePositions.Add(interposition);

            CheckWrap();
        }

        ropePositions.RemoveAt(ropePositions.Count - 1);
        ropePositions.Add((Vector2)player.position);
        
        CheckWrap();
        GetAngles();
        CheckUnwrap();

        //foreach (double[] a in angles) Debug.Log(a[2]);

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

        if (ropePositions.Count > 2) {

            int i = 0;

            angles = new List<double[]>();

            do {

                double[] newAngle = new double[3];
                newAngle[0] = Vector2.SignedAngle(ropePositions.ElementAt(i + 1), ropePositions.ElementAt(i + 2) - ropePositions.ElementAt(i + 1));
                newAngle[1] = Vector2.SignedAngle(ropePositions.ElementAt(i + 1), ropePositions.ElementAt(i+1) - ropePositions.ElementAt(i)); 

                if(oldAngles.Count() > i) {

                    // Correct Euler angle overwrapping (prevent 180 looping to -180)
                    while (newAngle[0] < oldAngles.ElementAt(i)[0] - 180f) newAngle[0] += 360f;
                    while (newAngle[0] > oldAngles.ElementAt(i)[0] + 180f) newAngle[0] -= 360f;

                    while (newAngle[1] < oldAngles.ElementAt(i)[1] - 180f) newAngle[1] += 360f;
                    while (newAngle[1] > oldAngles.ElementAt(i)[1] + 180f) newAngle[1] -= 360f;
                }

                newAngle[2] = angleWraps.ElementAt(i);
                Debug.Log(i + " = " + newAngle[0] + ", " + newAngle[1] + ", " + newAngle[2]);

                // Save the new angle data immediately
                angles.Add(newAngle);

                if (i > 10) {
                    Debug.Log("NIGHTMARE2"); Debug.Break(); break;
                }
                i++;
            }
            while (i < ropePositions.Count - 2);

            oldAngles = angles;

        }
    }



    /* This method uses angles to check
     * whether the rope can be unwrapped */
    private void CheckUnwrap() {


        if (ropePositions.Count > 2) {

            int i = 0;

            do {
                /* If the path to the second last point
                 * is no longer obstructed, unwrap the rope */
                if ((angles.ElementAt(i)[1] < angles.ElementAt(i)[0] && angles.ElementAt(i)[2] == 1)
                    || (angles.ElementAt(i)[1] > angles.ElementAt(i)[0] && angles.ElementAt(i)[2] == -1)) {
                    
                    Debug.Log("UNWRAPPING\n" + i + " = " + angles.ElementAt(i)[0] + ", " + angles.ElementAt(i)[1] + ", " + angles.ElementAt(i)[2] + "\nCONDITION 1: " + (angles.ElementAt(i)[1] < angles.ElementAt(i)[0] && angles.ElementAt(i)[2] == 1) + "\nCONDITION 2: " + (angles.ElementAt(i)[1] > angles.ElementAt(i)[0] && angles.ElementAt(i)[2] == -1));



                    ropePositions.RemoveAt(i+1);
                    holdPoints.RemoveAt(i);
                    angles.RemoveAt(i);
                    angleWraps.RemoveAt(i);
                    GetAngles();

                    Debug.Log(i + " is DISCONNECT");
                    i = -1;
                }

                i++;

                if(i > 10) {
                    Debug.Log("NIGHTMARE2"); Debug.Break(); break;
                }

            }
            while (i < ropePositions.Count - 2);

        }
    }



    /* This method checks if the rope has been
     * obstructed and can wrap around an object */
    private void CheckWrap() {

        RaycastHit2D ropeRaycast;
        int i = 0;

        do {

            Vector2 dir, origin;

            if (i + 1 == ropePositions.Count() - 1) {

                dir = (ropePositions.ElementAt(i) - ropePositions.ElementAt(i+1)).normalized;
                origin = ropePositions.ElementAt(i+1) + dir * 0.1f;
                Debug.DrawRay(origin, dir * (Vector2.Distance(ropePositions.ElementAt(i), ropePositions.ElementAt(i + 1)) - 0.2f), Color.green);  // Reverse ray 
            }
            else {

                dir = (ropePositions.ElementAt(i+1) - ropePositions.ElementAt(i)).normalized;
                origin = ropePositions.ElementAt(i) + dir * 0.1f;
                Debug.DrawRay(origin, dir * (Vector2.Distance(ropePositions.ElementAt(i), ropePositions.ElementAt(i + 1)) - 0.2f), Color.red);  // Forward ray 
            }

            ropeRaycast = Physics2D.Raycast(origin, dir,
                          Vector2.Distance(ropePositions.ElementAt(i), ropePositions.ElementAt(i + 1)) - 0.2f,
                          ropeLayerMask);

            // If rope raycast is interrupted, add a vertex to the line
            if (ropeRaycast) {

                PolygonCollider2D colliderWithVertices = ropeRaycast.collider as PolygonCollider2D;

                if (colliderWithVertices) {

                    // Get collider and find its closest vertex to point of collision
                    Vector2 closestPointToHit = GetClosestColliderPoint(ropeRaycast.point, colliderWithVertices);

                    scalePoint.transform.SetParent(colliderWithVertices.transform);
                    scalePoint.transform.localScale = Vector3.one;
                    scalePoint.transform.localPosition = Vector3.zero;
                    referencePoint.transform.SetParent(scalePoint.transform);
                    referencePoint.transform.localScale = Vector3.one;
                    referencePoint.transform.position = closestPointToHit;
                    scalePoint.transform.localScale += new Vector3(0.05f, 0.05f, 0.05f);
                    referencePoint.transform.SetParent(colliderWithVertices.transform);

                    // Ensure the rope position was not already added recently
                    if (Vector2.Distance(ropePositions.ElementAt(i), (Vector2)referencePoint.transform.position) > 0.03 && Vector2.Distance(ropePositions.ElementAt(i+1), (Vector2)referencePoint.transform.position) > 0.03) {

                        GameObject holdPoint = new GameObject("holdPoint" + (holdPoints.Count()+1));
                        holdPoint.transform.SetParent(scalePoint.transform);
                        holdPoint.transform.position = referencePoint.transform.position;
                        holdPoint.transform.SetParent(colliderWithVertices.transform);
                        holdPoint.transform.localScale = Vector3.one;
                        holdPoints.Add(holdPoint.transform);

                        ropePositions.Insert(i + 1, closestPointToHit);

                        GetAngles();

                        /*

                        double[] newAngle = new double[2];
                        newAngle[0] = Vector2.SignedAngle(ropePositions.ElementAt(i + 1), ropePositions.ElementAt(i + 2) - ropePositions.ElementAt(i + 1));
                        newAngle[1] = Vector2.SignedAngle(ropePositions.ElementAt(i + 1), ropePositions.ElementAt(i + 1) - ropePositions.ElementAt(i));
                        
                        if (newAngle[1] < newAngle[0])
                            angleWraps.Insert(i, -1);
                        else if (newAngle[1] > newAngle[0])
                            angleWraps.Insert(i, 1);*/

                        i++;
                    }
                }
            }

            i++;

            if (i > 10) {
                Debug.Log("NIGHTMARE3"); Debug.Break(); break;
            }
        }
        while (i < ropePositions.Count()-1);

    }


    
    private void MatchHoldPoints() {

        if(ropePositions.Count() > 2) {

            int i = 1;

            do {
                ropePositions.RemoveAt(i);

                ropePositions.Insert(i, holdPoints.ElementAt(i - 1).position);

                if (i > 10) {
                    Debug.Log("NIGHTMARE4"); Debug.Break(); break;
                }
                i++;
            }
            while (i < ropePositions.Count() - 2);
        }

    }



    /* This method tells the rope's line renderer
     * the position of each vertex.
     * This is purely cosmetic. */
    private void UpdateRopeRenderer() {

        ropeRenderer.positionCount = ropePositions.Count;

        // Render a rope through every point
        for (int i = 0; i < ropeRenderer.positionCount; i++)
            ropeRenderer.SetPosition(i, ropePositions[i]);
    }



    public void ResetRope()
    {
        ropePositions = new List<Vector2>();
        angleWraps = new List<int>();
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
        Vector2 result = orderedDictionary.Any() ? orderedDictionary.First().Value : Vector2.zero;
        return result;
    }

}