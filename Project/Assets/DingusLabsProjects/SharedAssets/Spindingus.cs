using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spindingus : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var amount = Time.deltaTime;

        this.transform.Rotate(40f * amount, 40f * amount, 40f * amount);
    }
}
