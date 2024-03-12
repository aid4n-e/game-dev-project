using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookCollisionCheck : MonoBehaviour
{

    public HookThrow hookThrow;

    void OnCollisionEnter2D(Collision2D col)
    {
        Transform newHookParent = col.transform;
        hookThrow.Attach(newHookParent);
    }
}
