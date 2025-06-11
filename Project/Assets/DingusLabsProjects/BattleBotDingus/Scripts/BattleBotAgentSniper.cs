//Put this script on your blue cube.

using System.Collections;
using UnityEngine;
using Unity.MLAgents;
//using Unity.Barracuda;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgentsExamples;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using System;
using Random = UnityEngine.Random;

public class BattleBotAgentSniper : BattleBotAgent
{
    EnvironmentParameters m_ResetParams;

    public GameObject bulletPathIndicatorPrefab;
    private GameObject bulletPathHighlight;
    public GameObject bulletPrefab;
    private GameObject bullet;

    private float shootCounter = 6;
    private float shootCounterLimit = 1.6f;

    private float baseRunSpeed;
    private float baseTurnSpeed;

    private bool preppingShot = false;

    public override void Initialize()
    {
        base.Initialize();

        baseRunSpeed = runForce;
        baseTurnSpeed = turnSpeed;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);

        sensor.AddObservation(preppingShot);
        sensor.AddObservation(shootCounter < shootCounterLimit ? shootCounter / shootCounterLimit : 1);
    }

    public override void ExecuteAction()
    {
        RewardGoodAim(0.5f);
        actionCounter = 0;
        var bulletPath = Instantiate(bulletPathIndicatorPrefab, this.gameObject.transform.position, this.transform.rotation, this.gameObject.transform.parent);
        bulletPath.GetComponent<BattleLaser>().owner = this.gameObject;
        bulletPath.transform.position += this.transform.forward * bulletPath.transform.localScale.z * 0.5f;
        bulletPath.transform.parent = this.transform;
        bulletPathHighlight = bulletPath;

        runForce = 0;
        turnSpeed = baseTurnSpeed * 0.05f;

        shootCounter = 0;
        preppingShot = true;
    }

    public override void PerformOverTimeActions()
    {
        base.PerformOverTimeActions();

        shootCounter += Time.deltaTime;
        if(shootCounter > shootCounterLimit && preppingShot == true && !dead){
            if(bulletPathHighlight != null){
                Destroy(bulletPathHighlight);
            }

            var newBullet = Instantiate(bulletPrefab, this.gameObject.transform.position + this.gameObject.transform.forward * 1, this.transform.rotation, this.gameObject.transform.parent);
            newBullet.GetComponent<BattleBullet>().owner = this.gameObject;
            bullet = newBullet; 
            runForce = baseRunSpeed;
            turnSpeed = baseTurnSpeed;

            preppingShot = false;
        }
        else if(dead){
                if(bulletPathHighlight != null){
                Destroy(bulletPathHighlight);
            }
        }
    }

    public override void OnEpisodeBegin(){
        base.OnEpisodeBegin();
        runForce = baseRunSpeed;
        turnSpeed = baseTurnSpeed;
        preppingShot = false;

        if(bullet != null){
            Destroy(bullet);
        }
        if(bulletPathHighlight != null){
            Destroy(bulletPathHighlight);
        }
    }
}
