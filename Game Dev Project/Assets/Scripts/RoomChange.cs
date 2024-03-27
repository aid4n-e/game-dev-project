using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roomchange : MonoBehaviour
{
    // Array for camera positions
    public Transform[] roomCameraPositions;

    // Current room index
    private int currentRoomIndex = 0;

    void Start()
    {
        // Sets the initial camera position
        Camera.main.transform.position = roomCameraPositions[currentRoomIndex].position;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Increment the room index
            currentRoomIndex = (currentRoomIndex + 1) % roomCameraPositions.Length;
            // Set the camera position to the new room
            Camera.main.transform.position = roomCameraPositions[currentRoomIndex].position;
        }
    }
}
