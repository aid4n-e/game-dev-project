using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public ReferenceManager rm;

    public Vector2 respawnPos;
    public Transform player;
    public Rigidbody2D playerRb;




    public void Kill() {

        rm.grappleHook.ResetRope();
        rm.hookThrow.ResetThrow();

        playerRb.velocity = Vector2.zero;
        player.position = respawnPos;

        rm.audioSrc.PlayOneShot(rm.sounds[0]);
    }

}
