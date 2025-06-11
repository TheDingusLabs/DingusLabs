using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinny : MonoBehaviour
{
    private float spinRateMax = 0.1f;
    private float spinRateMin = 0.4f;
    float movementSpeed = 0.5f;
    private float spinRate;
    public bool spinClockwise = true;

    private void Start()
    {
        spinRate = Random.Range(spinRateMin, spinRateMax);
        spinClockwise = Random.Range(0, 2) == 0 ? true : false;
        transform.Rotate(transform.up * (spinClockwise ? -1f : 1f), Random.Range(0f, 180f));
        Destroy(this, 25f);
    }
    private void FixedUpdate()
    {
    }
    
    private void Update()
    {
        transform.Rotate(transform.up * (spinClockwise ? -1f : 1f), Time.deltaTime * 300f * spinRate);
        transform.position += Vector3.up * movementSpeed;
    }
}
