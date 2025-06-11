using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StalkerPanel : MonoBehaviour
{
    public GameObject target;

    public Boolean adjustYDynamically = false;


    public float xdistance;
    public float ydistance;
    public float zdistance;

    private void Update()
    {
        var pos = target.transform.position;

        this.gameObject.transform.position = new Vector3(pos.x - xdistance, !adjustYDynamically ? ydistance : ydistance + pos.y, pos.z - zdistance * 2);
    }
}
