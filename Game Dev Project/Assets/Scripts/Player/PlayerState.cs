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

        playerRb.velocity = Vector2.zero;
        player.position = respawnPos;

        rm.grappleHook.ResetRope();
        rm.hookThrow.ResetThrow();

        rm.audioSrc.PlayOneShot(rm.sounds[0]);
    }

}
