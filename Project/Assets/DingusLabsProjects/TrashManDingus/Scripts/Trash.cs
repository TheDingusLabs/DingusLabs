using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trash : MonoBehaviour
{
    public float dingusTouched = 0;
    private TrashManAgent dingus;

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("agent"))
        {
            if(dingusTouched < 1 && col.gameObject.GetComponent<Rigidbody>().linearVelocity.magnitude > 2f)
            {
		        dingus = col.gameObject.GetComponent<TrashManAgent>();
                dingus.touchedTrash();
                dingusTouched++;
            }
        }
    }

    private void FixedUpdate()
    {
	    if(dingusTouched >= 1 && dingusTouched <= 50 && this.gameObject.GetComponent<Rigidbody>().linearVelocity.magnitude > 0.01f)
	    {
		    dingus.touchedTrashIsMoving();
	    }
    }
}
