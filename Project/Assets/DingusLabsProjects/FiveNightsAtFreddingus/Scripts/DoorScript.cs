using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    public GameObject openPos;
    public GameObject closePos;
    public SwitchScript doorLight;
    public bool open = true;
    public int moveState = 0; //should be enum but very lazy, 0 is nothing, 1 is closing, 2 is opening
    private float doorCloseSpeed = 0.5f;
    public float doorInc = 0;

    // public float doorActionDelayInc = 20f;
    // public float doorActionDelayTime = 0.3f;

    // Update is called once per frame
    void Update()
    {
        if(moveState > 0){
            doorInc += Time.deltaTime;
            if(doorInc >= doorCloseSpeed){
                doorInc = doorCloseSpeed;
            }

            var transposition = new Vector3();

            if(moveState == 1){
                transposition = Vector3.Lerp(openPos.transform.position, closePos.transform.position, doorInc/doorCloseSpeed);
            }
            if(moveState == 2){
                transposition = Vector3.Lerp(closePos.transform.position, openPos.transform.position, doorInc/doorCloseSpeed);
            }
            
            transform.position = transposition;

            if(doorInc >= doorCloseSpeed){
                //changing door state logic, will now be closed the moment you toggle it, not when it changes the animation;
                // open = moveState == 2 ? true : false;
                moveState = 0;
                doorInc = 0;
            }
        }
    }

    public void ToggleDoor(){
        if(moveState == 0){
            moveState = open ? 1 : 2;
            doorLight.ToggleSwitch(open);
            open = moveState == 2 ? true : false;
        }
    }

    public bool IsDoorMoving(){
        return moveState != 0;
    }


    public void ResetDoor(){
        open = true;
        moveState = 0;
        doorInc = 0;
        transform.position = openPos.transform.position;
    }
}
