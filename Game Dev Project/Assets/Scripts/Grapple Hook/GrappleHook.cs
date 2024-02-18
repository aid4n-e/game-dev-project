using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.Image;

public class GrappleHook : MonoBehaviour {

    // PUBLIC VARIABLES
    public Transform player;
    public GameObject rope;
    public Transform anchor;
    public Transform hook;
    public Transform storageParent;
    public LineRenderer ropeRenderer;
    public DistanceJoint2D distanceJoint;
    public HookThrow hookThrow;
    public GameObject holdPointPrefab;

    // Defines which layers the rope can collide with
    public LayerMask ropeLayerMask;

    public float maxLength;
    public bool snap;

    // PRIVATE VARIABLES
    private Vector2 oldPlayerPos;

    private List<Transform> ropePositions = new List<Transform>();

    /* Stores int 1 or -1 for each rope vertex
     * depending on whether the rope was wrapped
     * clockwise or counter-clockwise */
    private List<int> angleWraps = new List<int>();

    [SerializeField]
    private bool fire, reset;

    private List<double[]> angles = new List<double[]>();
    private List<double[]> oldAngles = new List<double[]>();
    private Stack<Transform> tempTransforms = new Stack<Transform>();
    private GameObject referencePoint;



    private void Awake() {
        foreach (Transform temp in storageParent.GetComponentsInChildren<Transform>()) {
            RecycleTransform(temp);
        }
    }



    private void Start() {
        referencePoint = new GameObject("referencePoint");
        
        ResetRope();
        oldPlayerPos = player.position;
    }



    private void FixedUpdate() {

        if (fire) {
            fire = false;
            hookThrow.Throw(0.4f);
            SpawnRope();
        }
        else if (reset || snap) {

            reset = false; snap = false;
            ResetRope();
        }

        // If the rope is currently active
        else if (ropePositions.Count > 0) {
            
            HandleRopePositions();
            SetDistance();
            UpdateRopeRenderer();
        }

        oldPlayerPos = player.position;
    }



    private void SpawnRope() {
        ResetRope();
        //distanceJoint.enabled = true;

        ropePositions.Add(hook.transform);
        ropePositions.Add(player.transform);
        rope.SetActive(true);
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

        ropePositions.RemoveAt(ropePositions.Count - 1);
        ropePositions.Add(player);

        CheckWrap();
        GetAngles();
        CheckUnwrap();

        // Set the anchor position
        anchor.position = ropePositions.ElementAt(ropePositions.Count()-2).position;
    }



    /* Get the distance between each segment of the rope */
    private void SetDistance() {

        float distance = 0;
        for (int i = 0; i < ropePositions.Count() - 1; i++)
            distance += Vector2.Distance(ropePositions.ElementAt(i).position, ropePositions.ElementAt(i + 1).position);

        distanceJoint.distance = (maxLength - distance);
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

                    // Correct Euler angle overwrapping (prevent 180 looping to -180)
                    while (newAngle[0] < oldAngles.ElementAt(i)[0] - 180f) newAngle[0] += 360f;
                    while (newAngle[0] > oldAngles.ElementAt(i)[0] + 180f) newAngle[0] -= 360f;

                    while (newAngle[1] < oldAngles.ElementAt(i)[1] - 180f) newAngle[1] += 360f;
                    while (newAngle[1] > oldAngles.ElementAt(i)[1] + 180f) newAngle[1] -= 360f;
                }

                newAngle[2] = angleWraps.ElementAt(i);
                //Debug.Log("ANGLE: " + i + "\nA>B: " + newAngle[0] + ", B>C: " + newAngle[1] + ", WRAP: " + newAngle[2]);

                // Save the new angle data immediately
                angles.Add(newAngle);

                i++;
            }
            while (i < ropePositions.Count - 2);

            oldAngles = angles;

        }
    }



    /* Checks if the rope has been obstructed
     * and can wrap around an object */
    private void CheckWrap() {

        RaycastHit2D ropeRaycast;
        int i = 0;

        do {

            Vector2[,] raycastInfo = new Vector2[2, 2];  //[x][0] = direction, [x][1] = origin
            Vector2 closestPointToHit;

            raycastInfo[0, 0] = (ropePositions.ElementAt(i).position - ropePositions.ElementAt(i + 1).position).normalized;  // direction 0
            raycastInfo[0, 1] = (Vector2)ropePositions.ElementAt(i + 1).position + (raycastInfo[0, 0] * 0.1f);  // origin 0

            raycastInfo[1, 0] = (ropePositions.ElementAt(i + 1).position - ropePositions.ElementAt(i).position).normalized;  // direction 1
            raycastInfo[1, 1] = (Vector2)ropePositions.ElementAt(i).position + raycastInfo[1, 0] * 0.1f;  // origin 1
            Debug.DrawRay(raycastInfo[0, 1], raycastInfo[0, 0] * (Vector2.Distance(ropePositions.ElementAt(i).position, ropePositions.ElementAt(i + 1).position) - 0.2f), Color.green);  // Reverse ray 
            Debug.DrawRay(raycastInfo[1, 1], raycastInfo[1, 0] * (Vector2.Distance(ropePositions.ElementAt(i).position, ropePositions.ElementAt(i + 1).position) - 0.2f), Color.red);  // Reverse ray 

            for (int j = 0; j < 2; j++) {

                bool valid = true;

                ropeRaycast = Physics2D.Raycast(raycastInfo[j, 1], raycastInfo[j, 0],
                Vector2.Distance(ropePositions.ElementAt(i).position, ropePositions.ElementAt(i + 1).position) - 0.2f,
                ropeLayerMask);

                // If rope raycast is interrupted, add a vertex to the line
                if (ropeRaycast) {

                    PolygonCollider2D colliderWithVertices = ropeRaycast.collider as PolygonCollider2D;

                    if (colliderWithVertices) {

                        // Get collider and find its closest vertex to point of collision
                        closestPointToHit = GetClosestColliderPoint(ropeRaycast.point, colliderWithVertices);
                        Debug.Log("CLOSEST POINT: " + closestPointToHit);

                        referencePoint.transform.SetParent(colliderWithVertices.transform);
                        referencePoint.transform.position = closestPointToHit;

                        Vector2 dir;
                        dir = (closestPointToHit - (Vector2)colliderWithVertices.transform.position).normalized;

                        // Ensure the rope position was not already added recently
                        for (int n = i - 1; n < i + 1; n++) {
                            if (n > 0 && n < ropePositions.Count() - 1) {
                                Debug.Log("WRAP: i = " + i + ";  d" + n + " = " + Vector2.Distance(ropePositions.ElementAt(n).position, referencePoint.transform.position));
                                if (Vector2.Distance(ropePositions.ElementAt(n).position, referencePoint.transform.position) < 0.1)
                                    valid = false;
                            }
                        }

                        if (valid) {

                            Debug.Log("WRAPPING: " + i);
                            //Debug.Break();

                            Transform holdPoint = GetTempTransform();
                            holdPoint.name = "holdPoint" + (ropePositions.Count() - 2);
                            holdPoint.transform.SetParent(colliderWithVertices.transform);
                            holdPoint.transform.position = (Vector2)closestPointToHit + (dir * 0.05f);

                            ropePositions.Insert(i + 1, holdPoint.transform);

                            double[] newAngle = new double[2];
                            newAngle[0] = Vector2.SignedAngle(ropePositions.ElementAt(i + 1).position, ropePositions.ElementAt(i + 2).position - ropePositions.ElementAt(i + 1).position);
                            newAngle[1] = Vector2.SignedAngle(ropePositions.ElementAt(i + 1).position, ropePositions.ElementAt(i + 1).position - ropePositions.ElementAt(i).position);

                            if (newAngle[1] < newAngle[0])
                                angleWraps.Insert(i, -1);
                            else if (newAngle[1] > newAngle[0])
                                angleWraps.Insert(i, 1);

                            //GetAngles();

                            i++;
                            break;
                        }
                    }
                }
            }

            i++;
        }
        while (i < ropePositions.Count() - 1);
    }



    /* Uses angles to check whether
     * the rope can be unwrapped */
    private void CheckUnwrap() {


        if (ropePositions.Count > 2) {

            int i = 0;

            do {
                /* If the path to the second last point
                 * is no longer obstructed, unwrap the rope */
                if ((angles.ElementAt(i)[1] < angles.ElementAt(i)[0] && angles.ElementAt(i)[2] == 1)
                    || (angles.ElementAt(i)[1] > angles.ElementAt(i)[0] && angles.ElementAt(i)[2] == -1)) {

                    Debug.Log("UNWRAPPING: " + i + "\ni = " + angles.ElementAt(i)[0] + ", " + angles.ElementAt(i)[1] + ", " + angles.ElementAt(i)[2] + "\nCONDITION 1: " + (angles.ElementAt(i)[1] < angles.ElementAt(i)[0] && angles.ElementAt(i)[2] == 1) + "\nCONDITION 2: " + (angles.ElementAt(i)[1] > angles.ElementAt(i)[0] && angles.ElementAt(i)[2] == -1));

                    RecycleTransform(ropePositions.ElementAt(i + 1));
                    ropePositions.RemoveAt(i + 1);
                    angles.RemoveAt(i);
                    angleWraps.RemoveAt(i);
                    GetAngles();

                    i = -1;
                    //Debug.Break();
                }

                i++;
            }
            while (i < ropePositions.Count - 2);

        }
    }



    /* Updates the rope's line renderer to display properly */
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
        rope.SetActive(true);
        UpdateRopeRenderer();
    }



    /* Returns a transform from the temp stack */
    private Transform GetTempTransform()
    {
        return (GetTempTransform(new Vector2(0, 0)));
    }



    private Transform GetTempTransform(Vector2 newPosition)
    {
        if(tempTransforms.Count == 0) {
            GameObject o = Instantiate(holdPointPrefab);
            RecycleTransform(o.transform);
        }
        Transform temp = tempTransforms.Pop();
        temp.position = newPosition;
        return temp;
    }



    /* Recycles a transform into the temp stack.
     * Re-using transforms helps with performance. */
    private void RecycleTransform(Transform temp)
    {
        temp.parent = storageParent;
        temp.name = "temp";
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