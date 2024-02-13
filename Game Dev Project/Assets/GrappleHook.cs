using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.UIElements;

public class GrappleHook : MonoBehaviour {

    // PUBLIC VARIABLES
    public Transform player;
    public Transform anchor;
    public Transform hook;
    public Transform terrainParent;
    public LineRenderer ropeRenderer;
    public DistanceJoint2D distanceJoint;
    public HookThrow hookThrow;


    // Defines which layers the rope can collide with
    public LayerMask ropeLayerMask;

    public float maxLength;

    // PRIVATE VARIABLES
    private Vector2 oldPlayerPos;

    public List<Transform> ropePositions = new List<Transform>();

    /* Stores int 1 or -1 for each rope vertex
     * depending on whether the rope was wrapped
     * clockwise or counter-clockwise */
    private List<int> angleWraps = new List<int>();

    [SerializeField]
    private bool fire, reset;

    private double angle1, angle2, lastAngle1, lastAngle2;
    public List<double[]> angles = new List<double[]>();
    public List<double[]> oldAngles = new List<double[]>();
    public Stack<Transform> tempTransforms = new Stack<Transform>();

    private GameObject scalePoint, referencePoint;

    private void Start() {
        scalePoint = new GameObject("scalePoint");
        referencePoint = new GameObject("referencePoint");
        
        ResetRope();
        oldPlayerPos = player.position;
    }



    private void FixedUpdate() {

        
        if (hookThrow.thrown) {

            hookThrow.thrown = false;
            ropePositions.Add(hook.transform);
            ropePositions.Add(player.transform);
            //distanceJoint.enabled = true;
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

        GetAngles();

        float travelDistance = Vector2.Distance((Vector2)player.position, oldPlayerPos);
        int iterations = (int)Mathf.Floor(travelDistance / 0.2f);

        for (int i = 1; i <= iterations; i++) {

            ropePositions.RemoveAt(ropePositions.Count - 1);
            Transform temp = GetTempTransform(oldPlayerPos + (i * 0.2f) * ((Vector2)player.position - oldPlayerPos).normalized);
            ropePositions.Add(temp);

            CheckWrap();
        }

        /*ropePositions.RemoveAt(ropePositions.Count - 1);
        ropePositions.Add((Vector2)player.position);*/

        CheckWrap();
        GetAngles();
        CheckUnwrap();

        //foreach (double[] a in angles) Debug.Log(a[2]);

        // Set the anchor position
        anchor.position = ropePositions.ElementAt(ropePositions.Count() - 1).position;

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
            distance += Vector2.Distance(ropePositions.ElementAt(i).position, ropePositions.ElementAt(i + 1).position);

        return distance;
    }



    private void GetAngles() {

        if (ropePositions.Count > 2) {

            int i = 0;

            angles = new List<double[]>();

            do {

                double[] newAngle = new double[3];
                newAngle[0] = Vector2.SignedAngle(ropePositions.ElementAt(i + 1).position, ropePositions.ElementAt(i + 2).position - ropePositions.ElementAt(i + 1).position);
                newAngle[1] = Vector2.SignedAngle(ropePositions.ElementAt(i + 1).position, ropePositions.ElementAt(i+1).position - ropePositions.ElementAt(i).position); 

                if(oldAngles.Count() > i) {
                    //Debug.Log("OLD " + i);

                    // Correct Euler angle overwrapping (prevent 180 looping to -180)
                    while (newAngle[0] < oldAngles.ElementAt(i)[0] - 180f) newAngle[0] += 360f;
                    while (newAngle[0] > oldAngles.ElementAt(i)[0] + 180f) newAngle[0] -= 360f;

                    while (newAngle[1] < oldAngles.ElementAt(i)[1] - 180f) newAngle[1] += 360f;
                    while (newAngle[1] > oldAngles.ElementAt(i)[1] + 180f) newAngle[1] -= 360f;
                }

                newAngle[2] = angleWraps.ElementAt(i);
                Debug.Log("\t" + i + " = " + newAngle[0] + ", " + newAngle[1] + ", " + newAngle[2]);

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
                    angles.RemoveAt(i);
                    angleWraps.RemoveAt(i);
                    GetAngles();

                    Debug.Log(i + " is DISCONNECT");
                    i = -1;
                    Debug.Break();
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

                dir = (ropePositions.ElementAt(i).position - ropePositions.ElementAt(i + 1).position).normalized;
                origin = (Vector2)ropePositions.ElementAt(i+1).position + dir * 0.1f;
                Debug.DrawRay(origin, dir * (Vector2.Distance(ropePositions.ElementAt(i).position, ropePositions.ElementAt(i + 1).position) - 0.2f), Color.green);  // Reverse ray 
            }
            else {

                dir = (ropePositions.ElementAt(i+1).position - ropePositions.ElementAt(i).position).normalized;
                origin = (Vector2)ropePositions.ElementAt(i).position + dir * 0.1f;
                Debug.DrawRay(origin, dir * (Vector2.Distance(ropePositions.ElementAt(i).position, ropePositions.ElementAt(i + 1).position) - 0.2f), Color.red);  // Forward ray 
            }

            ropeRaycast = Physics2D.Raycast(origin, dir,
                          Vector2.Distance(ropePositions.ElementAt(i).position, ropePositions.ElementAt(i + 1).position) - 0.2f,
                          ropeLayerMask);

            // If rope raycast is interrupted, add a vertex to the line
            if (ropeRaycast) {



                PolygonCollider2D colliderWithVertices = ropeRaycast.collider as PolygonCollider2D;

                if (colliderWithVertices) {

                    // Get collider and find its closest vertex to point of collision
                    Vector2 closestPointToHit = GetClosestColliderPoint(ropeRaycast.point, colliderWithVertices);

                    /*scalePoint.transform.SetParent(colliderWithVertices.transform);
                    scalePoint.transform.localScale = Vector3.one;
                    scalePoint.transform.localPosition = Vector3.zero;*/
                    referencePoint.transform.SetParent(colliderWithVertices.transform);
                    referencePoint.transform.localScale = Vector3.one;
                    referencePoint.transform.position = closestPointToHit;

                    dir = (closestPointToHit - (Vector2)colliderWithVertices.transform.position).normalized;

                    referencePoint.transform.SetParent(colliderWithVertices.transform);

                    bool valid = true;

                    // Ensure the rope position was not already added recently
                    for (int n = i - 1; n < i + 1; n++)
                        if (n > 0 && n < ropePositions.Count() - 1)
                        {
                            Debug.Log("WRAP: i = " + i + ";  d" + n + " = " + Vector2.Distance(ropePositions.ElementAt(n).position, referencePoint.transform.position));
                            if (Vector2.Distance(ropePositions.ElementAt(n).position, referencePoint.transform.position) < 0.1)
                                valid = false;
                        }


                    if (valid) {

                        Debug.Log("WRAPPING: " + i);
                        Debug.Break();

                        GameObject holdPoint = new GameObject("holdPoint" + (ropePositions.Count()-2));
                        holdPoint.transform.SetParent(colliderWithVertices.transform);
                        holdPoint.transform.position = (Vector2)closestPointToHit + (dir * 0.05f);
                        holdPoint.transform.localScale = Vector3.one;

                        ropePositions.Insert(i + 1, holdPoint.transform);

                        double[] newAngle = new double[2];
                        newAngle[0] = Vector2.SignedAngle(ropePositions.ElementAt(i + 1).position, ropePositions.ElementAt(i + 2).position - ropePositions.ElementAt(i + 1).position);
                        newAngle[1] = Vector2.SignedAngle(ropePositions.ElementAt(i + 1).position, ropePositions.ElementAt(i + 1).position - ropePositions.ElementAt(i).position);
                        
                        if (newAngle[1] < newAngle[0])
                            angleWraps.Insert(i, -1);
                        else if (newAngle[1] > newAngle[0])
                            angleWraps.Insert(i, 1);

                        GetAngles();

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



    /* This method tells the rope's line renderer
     * the position of each vertex.
     * This is purely cosmetic. */
    private void UpdateRopeRenderer() {

        ropeRenderer.positionCount = ropePositions.Count;

        // Render a rope through every point
        for (int i = 0; i < ropeRenderer.positionCount; i++)
            ropeRenderer.SetPosition(i, ropePositions[i].position);
    }



    public void ResetRope()
    {
        ropePositions = new List<Transform>();
        angleWraps = new List<int>();
        distanceJoint.enabled = false;
        UpdateRopeRenderer();
    }



    private Transform GetTempTransform()
    {
        Transform temp = tempTransforms.Pop();
        temp.SetParent(terrainParent);
        return temp;
    }
    private Transform GetTempTransform(Vector2 newPosition)
    {
        Transform temp = tempTransforms.Pop();
        temp.SetParent(terrainParent);
        temp.position = newPosition;
        return temp;
    }



    private void RecycleTransform(Transform temp)
    {
        tempTransforms.Push(temp);
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