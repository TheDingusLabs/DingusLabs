using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class BattleCam : MonoBehaviour
{
    public List<GameObject> camLocations;
    private int currentCam = 0;

    private bool autoswitchEnabled = false;
    private float autoSwitchCounter = 0;
    private float autoSwitchLimit = 200f;

    public TextMeshProUGUI autoswitchText;

    void Initialize(){

    }
    void Start()
    {
        string targetName = "camera location cube";
        GameObject[] allObjects = FindObjectsOfType<GameObject>(); // Get all GameObjects in the scene
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == targetName)
            {
                //Debug.Log($"Found object: {obj.name} at position {obj.transform.position}");
                camLocations.Add(obj);
            }
        }

        this.transform.position = camLocations[0].transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.C)){
            switchCam();
        }
        else if(Input.GetKeyUp(KeyCode.N) && camLocations.Count > 1){
            autoswitchEnabled = !autoswitchEnabled;
            autoSwitchCounter = 0;
        }
        autoSwitchCounter += Time.deltaTime;
        if(autoswitchEnabled && autoSwitchCounter > autoSwitchLimit){
            autoSwitchCounter = 0;
            switchCam();     
        }

        autoswitchText.text = autoswitchEnabled ? "Swapping in : " + (autoSwitchLimit - autoSwitchCounter).ToString("F0") : autoswitchText.text = "";

    }

    void switchCam(){
        if(camLocations.Count > 1 && currentCam < camLocations.Count - 1){
            currentCam++;
        }
        else{
            currentCam = 0;
        }
        this.transform.position = camLocations[currentCam].transform.position;
    }
}
