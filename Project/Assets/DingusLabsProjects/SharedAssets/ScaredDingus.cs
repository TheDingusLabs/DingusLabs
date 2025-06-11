using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaredDingus : MonoBehaviour
{
    // Start is called before the first frame update
    float switchFrequency = 0.1f;
    float count = 0f;
    float dir = 1;
    float verdir = 1;
    float vircount = 0f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var amount = Time.deltaTime;

        count += Time.deltaTime;
        vircount += Time.deltaTime;

        if(count > switchFrequency){
            count = 0f;
            dir = dir * -1;
        }

        if(vircount > switchFrequency * 4f){
            vircount = 0f;
            verdir = verdir * -1;
        }

        this.transform.Rotate(10f * amount * -verdir, 40f * amount * dir, 0f * amount * dir);
    }
}
