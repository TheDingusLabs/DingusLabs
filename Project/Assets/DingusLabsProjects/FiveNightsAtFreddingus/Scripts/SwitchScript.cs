using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchScript : MonoBehaviour
{
    public Material onColour;
    public Material offColour;

    public void ToggleSwitch(bool on)
    {
        Material mat;
        if(on){
            mat = onColour;
        }
        else{
            mat = offColour;
        }
        //Material[] matArray = newMater
        this.gameObject.GetComponent<Renderer>().material = mat;
    }
}
