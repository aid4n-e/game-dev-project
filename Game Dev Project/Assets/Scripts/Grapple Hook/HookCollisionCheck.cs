using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookCollisionCheck : MonoBehaviour
{

    public HookThrow hookThrow;

    // Update is called once per frame
    void onCollisionEnter2D()
    {
        hookThrow.Attach();
    }
}
