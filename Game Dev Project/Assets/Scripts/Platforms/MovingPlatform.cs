using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

    public bool moving;
    public Vector3 direction;

    // Update is called once per frame
    void FixedUpdate()
    {
        
        if(moving)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, this.transform.position + direction, Time.deltaTime);
        }


    }
}
