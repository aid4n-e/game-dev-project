using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HookThrow : MonoBehaviour {

    public GrappleHook gh;
    public Rigidbody2D hookRb;

    public float gravity;
    public bool fire;
    public bool thrown;
    public float chargeScalar;
    public float chargeTime;

    private bool stuck;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void FixedUpdate() {

        if(!stuck)
            hookRb.AddForce((Vector2.down * (gravity*100)) * Time.fixedDeltaTime, ForceMode2D.Force);
        
        if (fire) {

            ResetThrow();

            fire = false;
            thrown = true;
            stuck = false;

            //hookRb.GetComponent<GameObject>().SetActive(true);

            hookRb.bodyType = RigidbodyType2D.Dynamic;
            hookRb.velocity = Vector2.zero;
            hookRb.position = gh.player.position;

            Throw(chargeTime);
        }


        if(Vector2.Distance(gh.player.position, gh.hook.position) > gh.maxLength + 0.1f) {

            Debug.Log("BROKEN; Distance = " + Vector2.Distance(gh.player.position, gh.hook.position));
            ResetThrow();
        }



    }



    void Throw(float charge) {

        //float strength = chargeScalar * (Mathf.Pow(0.3f * charge, 6) + Mathf.Pow(2 * charge, 0.15f) + 0.3f);           
        //float strength = chargeScalar * (6 * Mathf.Pow(charge, 4.5) + Mathf.Pow(charge, 0.3f) + 1);
        float strength = chargeScalar * (10 * Mathf.Pow(charge, 5) + 0.7f * Mathf.Pow(charge, 0.5f) + 1);

        Vector2 force = new Vector2(1, 1) * strength;
        Debug.Log(strength);

        hookRb.AddForce(force, ForceMode2D.Impulse);
    }



    void Attach() {

        stuck = true;
        hookRb.bodyType = RigidbodyType2D.Static;
        gh.distanceJoint.enabled = true;
    }



    void ResetThrow() {

        gh.ResetRope();

        stuck = false;
        thrown = false;
        fire = false;

        //hookRb.GetComponent<GameObject>().SetActive(false);
        hookRb.velocity = Vector2.zero;
    }



    private void OnCollisionEnter2D(Collision2D collision) {

        Debug.Log("HIT");
        Attach();
    }

}
