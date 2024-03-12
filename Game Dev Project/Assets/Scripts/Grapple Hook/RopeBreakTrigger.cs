using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeBreakTrigger : MonoBehaviour
{

    public GrappleHook grappleHook;

    private void OnTriggerEnter(Collider other) {
        grappleHook.snap = true;
    }

}
