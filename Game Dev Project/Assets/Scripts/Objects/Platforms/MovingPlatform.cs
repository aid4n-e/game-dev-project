using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

    public bool moving;
    public bool forward = true;
    public Transform start, end;

    // Update is called once per frame
    void FixedUpdate()
    {
        
        if(moving && forward)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, end.position, Time.deltaTime * 1.3f);

            if (Vector2.Distance(this.transform.position, end.position) < 0.1f) {
                forward = false;
            }
        }
        if(moving && !forward) {

            this.transform.position = Vector3.Lerp(this.transform.position, start.position, Time.deltaTime * 0.9f);

            if (Vector2.Distance(this.transform.position, start.position) < 0.1f) {
                moving = false;
                forward = true;
            }
        }






    }


    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.transform.CompareTag("Player"))
        {
            Debug.Log("MOVE");
            moving = true;   
        }
    }
}
