using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HookThrow : MonoBehaviour {

    public ReferenceManager rm;

    public Rigidbody2D hookRb;

    public float gravity;
    public bool fire;
    public bool thrown;
    public float chargeScalar;
    public float chargeTime;

    public bool attached;



    private void Awake() {
        ResetThrow();
    }



    // Update is called once per frame
    void FixedUpdate() {

        if(!attached)
            hookRb.AddForce((Vector2.down * (gravity*100)) * Time.fixedDeltaTime, ForceMode2D.Force);
        
        if(rm.grappleHook.ropePositions.Count > 1 && rm.grappleHook.GetDistance(true) > rm.grappleHook.maxLength + 0.5f) {

            //Debug.Log("BROKEN; Distance = " + rm.grappleHook.GetDistance(true));
            ResetThrow();
            rm.grappleHook.ResetRope();
        }

    }



    public void Throw(float charge, Vector2 dir) {

        hookRb.GetComponent<SpriteRenderer>().enabled = true;

        hookRb.position = rm.grappleHook.player.position;

        hookRb.bodyType = RigidbodyType2D.Dynamic;

        fire = false;
        thrown = true;
        attached = false;

        float strength = chargeScalar * Mathf.Clamp((1f * Mathf.Pow(charge, 9f) + 1.5f * Mathf.Pow(charge, 0.5f)),0.5f,2f);  // Desmos: 20x^{4.5}\ +\ 1x^{0.4}\ +\ 0.7  OR  1x^{9}\ +\ 1.5x^{0.5}
        Vector2 force = dir * strength + rm.grappleHook.player.GetComponent<Rigidbody2D>().velocity;
        hookRb.AddForce(force, ForceMode2D.Impulse);

        rm.grappleHook.rope.SetActive(true);
    }



    public void Attach(Transform newHookParent) {

        if(!attached) {

            attached = true;
            hookRb.bodyType = RigidbodyType2D.Static;   
            hookRb.transform.SetParent(newHookParent);
            rm.grappleHook.maxLength = rm.grappleHook.GetDistance(true) + 0.5f;
            rm.grappleHook.distanceJoint.enabled = true;
        }

    }



    public void ResetThrow() {

        hookRb.bodyType = RigidbodyType2D.Static;

        hookRb.transform.SetParent(rm.grappleHook.storageParent);
        hookRb.transform.position = rm.grappleHook.player.position;

        attached = false;
        thrown = false;
        fire = false;
    }



    void OnCollisionEnter2D(Collision2D col) {

        Transform newHookParent = col.transform;
        Attach(newHookParent);
    }

}
