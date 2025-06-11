using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashDestroyer : MonoBehaviour
{
    public GameObject Agent;

    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.CompareTag("squareTrash") || col.gameObject.CompareTag("cylinderTrash") || col.gameObject.CompareTag("sphereTrash"))
        {
            if(col.gameObject.GetComponent<Trash>().dingusTouched >= 1)
            {
                Agent.GetComponent<TrashManAgent>().YouDidAGood();
            }
            Destroy(col.gameObject);
        }
    }
}
