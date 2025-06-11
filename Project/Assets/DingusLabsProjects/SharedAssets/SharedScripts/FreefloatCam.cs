using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreefloatCam : MonoBehaviour
{
    public float movespeed;

    private void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            var pos = this.transform.position;
            this.transform.position = new Vector3(pos.x + movespeed*Time.deltaTime,pos.y,pos.z);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            var pos = this.transform.position;
            this.transform.position = new Vector3(pos.x,pos.y,pos.z + movespeed*Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            var pos = this.transform.position;
            this.transform.position = new Vector3(pos.x - movespeed*Time.deltaTime,pos.y,pos.z);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            var pos = this.transform.position;
            this.transform.position = new Vector3(pos.x,pos.y,pos.z - movespeed*Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.F)){
            var cam = this.gameObject.GetComponent<Camera>();
            if(cam.depth == -3){
                cam.depth = 0;
            }
            else{
                cam.depth = -3;
            }
        }
    }
}
