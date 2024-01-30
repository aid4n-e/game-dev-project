using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class GrappleHook : MonoBehaviour
{

    public Transform player;
    


    public LineRenderer ropeRenderer;
    private List<Vector2> ropePositions = new List<Vector2>();

    private Dictionary<Vector2, int> wrapPointsLookup = new Dictionary<Vector2, int>();

    public LayerMask ropeLayerMask;

    bool distanceSet = false;


    public bool fire;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(fire)
        {
            fire = false;

            RaycastHit2D hit = Physics2D.Raycast(player.position, new Vector2(0, 1), 20, ropeLayerMask);

            // 3
            if (hit.collider != null)
            {
                Debug.Log("HIT");

                //if (!ropePositions.Contains(hit.point))
                {
                    // 4
                    // Jump slightly to distance the player a little from the ground after grappling to something.
                    //transform.GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, 2f), ForceMode2D.Impulse);
                    ropePositions.Add(hit.point);
                    //ropeJoint.distance = Vector2.Distance(player, hit.point);
                    //ropeJoint.enabled = true;
                    //ropeHingeAnchorSprite.enabled = true;
                }
            }

        }

        // 1
        if (ropePositions.Count > 0)
        {
            // 2
            Vector2 lastRopePoint = ropePositions.Last();
            RaycastHit2D playerToCurrentNextHit = Physics2D.Raycast(new Vector2(player.position.x, player.position.y), (lastRopePoint - new Vector2(player.position.x, player.position.y)).normalized, Vector2.Distance(new Vector2(player.position.x, player.position.y), lastRopePoint) - 0.1f, ropeLayerMask);

            // 3
            if (playerToCurrentNextHit)
            {
                PolygonCollider2D colliderWithVertices = playerToCurrentNextHit.collider as PolygonCollider2D;
                if (colliderWithVertices != null)
                {
                    Vector2 closestPointToHit = GetClosestColliderPoint(playerToCurrentNextHit.point, colliderWithVertices);

                    // Make sure this point does not exist already
                    /*if (wrapPointsLookup.ContainsKey(closestPointToHit))
                    {
                        //ResetRope();
                        return;
                    }*/

                    // 5
                    ropePositions.Add(closestPointToHit);
                    wrapPointsLookup.Add(closestPointToHit, 0);
                    distanceSet = false;
                }
            }
        }

        UpdateRopePositions();

    }

    private void UpdateRopePositions()
    {

        // 2
        ropeRenderer.positionCount = ropePositions.Count + 1;

        // Start at the top of the list and go through each rope position
        for (int i = ropeRenderer.positionCount - 1; i >= 0; i--)
        {
            if (i != ropeRenderer.positionCount - 1) // if not the Last point of line renderer
            {
                // Set the position of the corresponding vertex i in the line renderer
                ropeRenderer.SetPosition(i, ropePositions[i]);

                // 4
                if (i == ropePositions.Count - 1 || ropePositions.Count == 1)
                {
                    Vector2 ropePosition = ropePositions[ropePositions.Count - 1];
                    if (ropePositions.Count == 1)
                    {
                        //ropeHingeAnchorRb.transform.position = ropePosition;
                        if (!distanceSet)
                        {
                            //ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                            distanceSet = true;
                        }
                    }
                    else
                    {
                        //ropeHingeAnchorRb.transform.position = ropePosition;
                        if (!distanceSet)
                        {
                            //ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                            distanceSet = true;
                        }
                    }
                }
                // 5
                else if (i - 1 == ropePositions.IndexOf(ropePositions.Last()))
                {
                    Vector2 ropePosition = ropePositions.Last();
                    //ropeHingeAnchorRb.transform.position = ropePosition;
                    if (!distanceSet)
                    {
                        //ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                        distanceSet = true;
                    }
                }
            }
            else
            {
                // 6
                ropeRenderer.SetPosition(i, player.position);
            }
        }
    }


    /* This code is wizardry!! Beware!!
     * Given a position, it will find the nearest vertex
     * of a polygon collider for you */
    private Vector2 GetClosestColliderPoint(Vector2 hit, PolygonCollider2D polyCollider)
    {
        // 2
        var distanceDictionary = polyCollider.points.ToDictionary<Vector2, float, Vector2>(
            position => Vector2.Distance(hit, polyCollider.transform.TransformPoint(position)),
            position => polyCollider.transform.TransformPoint(position));

        // 3
        var orderedDictionary = distanceDictionary.OrderBy(e => e.Key);
        return orderedDictionary.Any() ? orderedDictionary.First().Value : Vector2.zero;
    }
}
