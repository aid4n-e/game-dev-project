using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class GrappleHook : MonoBehaviour
{
    public Transform player;
    public LineRenderer ropeRenderer;

    public Vector2 playerPos;
    public LayerMask ropeLayerMask;

    private List<Vector2> ropePositions = new List<Vector2>();

    public bool fire;


    void Update()
    {
        playerPos = player.position;

        if(fire) {

            RaycastHit2D hit = Physics2D.Raycast(playerPos, new Vector2(0, 1), 20, ropeLayerMask);
            if (hit.collider != null)
                ropePositions.Add(hit.point);
        }

        // If the rope is currently active
        if (ropePositions.Count > 0)
            CalculateRopePositions();

        UpdateRopeRenderer();
    }


    private void CalculateRopePositions() {

        // Define last rope point and RayCast to next
        Vector2 lastRopePoint = ropePositions.Last();
        RaycastHit2D playerToLastRopePoint = Physics2D.Raycast(playerPos,
                                                                (lastRopePoint - playerPos).normalized,
                                                                Vector2.Distance(playerPos,
                                                                lastRopePoint) - 0.1f,
                                                                ropeLayerMask);

        if (ropePositions.Count > 1) {

            RaycastHit2D playerToSecondLastHit = Physics2D.Raycast(playerPos,
                                                        (ropePositions.ElementAt(ropePositions.Count - 2) - playerPos).normalized,
                                                        Vector2.Distance(playerPos,
                                                        ropePositions.ElementAt(ropePositions.Count - 2)) - 0.1f,
                                                        ropeLayerMask);

            if (playerToSecondLastHit.collider == null)
                ropePositions.RemoveAt(ropePositions.Count - 1);
        }

        // If rope raycast is interrupted
        if (playerToLastRopePoint) {

            // Get collider and find its closest vertex to point of collision
            PolygonCollider2D colliderWithVertices = playerToLastRopePoint.collider as PolygonCollider2D;

            if (colliderWithVertices != null) {

                Vector2 closestPointToHit = GetClosestColliderPoint(playerToLastRopePoint.point, colliderWithVertices);

                // Add the new rope position
                ropePositions.Add(closestPointToHit);
            }
        }
    }


    private void UpdateRopeRenderer()
    {
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