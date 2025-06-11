using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightScript : MonoBehaviour
{
    public bool on = false;
    float lightMinDuration = 0.5f;
    public float inc = 0;
    private Light theLight;
    public SwitchScript lightSwitch;
    public GameObject darknessPanel;

    void Start(){
        theLight = this.gameObject.GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        if(on){
            inc += Time.deltaTime;

            if(inc >= lightMinDuration){
                
                on = false;
                theLight.intensity = 0;
                inc = 0;
                lightSwitch.ToggleSwitch(on);
                darknessPanel.GetComponent<MeshRenderer>().forceRenderingOff = on;
            }
        }
    }

    public void ToggleLight(){
        if(!on){
            on = true;
            theLight.intensity = 1;
            lightSwitch.ToggleSwitch(on);
            darknessPanel.GetComponent<MeshRenderer>().forceRenderingOff = on;
        }
    }

    public void ResetLight(){
        on = false;
        inc = 0;
    }
}
