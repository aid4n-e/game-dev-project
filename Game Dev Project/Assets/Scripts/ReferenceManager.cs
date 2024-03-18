using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class contains references to all the other
 * important classes that need to talk to each other.
 * This allows us to just define the reference manager (rm)
 * in each class and access everything else from there! */

public class ReferenceManager : MonoBehaviour
{

    public PlayerState playerState;
    public PlayerMovement playerMovement;
    public GrappleHook grappleHook;
    public HookThrow hookThrow;

    public LayerMask playerLayer;
    public LayerMask terrainLayer;

    private void Start() {

        if(playerMovement == null)
            playerMovement = GetComponentInChildren<PlayerMovement>();
        if (grappleHook == null)
            grappleHook = GetComponentInChildren<GrappleHook>();
        if (hookThrow == null)
            hookThrow = GetComponentInChildren<HookThrow>();
    }
}
