using System.Collections.Generic;
using UnityEngine;

public enum EscapeSwitch
{
    LightSwitch = 0,
    HeavySwitch = 1,
    LightSwitchPressed = 2,
    HeavySwitchPressed = 3
}

public class LevelSwitch : MonoBehaviour
{
    public bool currentlyPressed { get; set; } = false;
    private bool everBeenPressed = false;
    public EscapeSwitch switchType;
    private EscapeSwitch tempSwitchType;
    public List<Material> colours;
    MeshRenderer wendewer;

    public EscapeEnvController controller;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tempSwitchType = switchType;
        wendewer = this.gameObject.GetComponent<MeshRenderer>();
    }

    void Update(){
        if(!this.gameObject.CompareTag(getCorrectTagForState())){
            this.gameObject.tag = getCorrectTagForState();
        }
    }

    private string getCorrectTagForState(){
        switch (tempSwitchType)
        {
            case EscapeSwitch.LightSwitch:
                return "LightSwitch";
            case EscapeSwitch.HeavySwitch:
                return "HeavySwitch";
            case EscapeSwitch.LightSwitchPressed:
                return "LightSwitchPressed";
            case EscapeSwitch.HeavySwitchPressed:
                return "HeavySwitchPressed";
            default:
                return "Not gonna happen. I never mess up";
        }
    }

    public void ResetSwitch(){
        currentlyPressed = false;
        everBeenPressed = false;
    }

    void OnTriggerStay(Collider other){
        if(other.CompareTag("agent") && switchType == EscapeSwitch.LightSwitch && currentlyPressed == false){
            ChangeSwitch(true);
        }
        else if(other.CompareTag("block") && switchType == EscapeSwitch.HeavySwitch && currentlyPressed == false){
            ChangeSwitch(true);
        }
    }

    void OnTriggerExit(Collider other){
        if(other.CompareTag("block") && switchType == EscapeSwitch.HeavySwitch && currentlyPressed == true){
            ChangeSwitch(false);
            controller.TurnedHeavySwitchOff();
        }
    }

    private void ChangeSwitch(bool on) {
        controller.hitSwitch(on && everBeenPressed == false, switchType == EscapeSwitch.HeavySwitch ? true : false);
        //change light colour here
        Material colour;
        if(on){
            colour = colours[1];
            if(switchType == EscapeSwitch.HeavySwitch){
                tempSwitchType = EscapeSwitch.HeavySwitchPressed;
            }
            else{
                tempSwitchType = EscapeSwitch.LightSwitchPressed;
            }
        }
        else{
            colour = colours[0];
            if(switchType == EscapeSwitch.HeavySwitch){
                tempSwitchType = EscapeSwitch.HeavySwitch;
            }
            else{
                tempSwitchType = EscapeSwitch.LightSwitch;
            }
        }

        everBeenPressed = true;
        wendewer.material = colour;
        currentlyPressed = on;
    }
}
