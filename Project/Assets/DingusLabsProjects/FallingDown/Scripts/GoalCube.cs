using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalCube : MonoBehaviour
{

    float movementSpeed = 1.5f;

    private void FixedUpdate()
    {
        transform.position += Vector3.up * movementSpeed;
    }
}
