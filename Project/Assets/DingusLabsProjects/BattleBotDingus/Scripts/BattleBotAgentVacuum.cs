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

public class BattleBotAgentVacuum : BattleBotAgent
{
    EnvironmentParameters m_ResetParams;

    private float shootCounter = 6;
    private float shootCounterLimit = 1.5f;

    private float baseRunSpeed;
    private float baseTurnSpeed;

    private bool preppingShot = false;
    public GameObject VacuumHighlight;

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
        actionCounter = 0;
        RewardGoodAim(0.3f, true);
        VacuumHighlight.SetActive(true);
        // var bulletPath = Instantiate(bulletPathIndicatorPrefab, this.gameObject.transform.position, this.transform.rotation, this.gameObject.transform.parent);
        // bulletPath.GetComponent<BattleLaser>().owner = this.gameObject;
        // bulletPath.transform.position += this.transform.forward * bulletPath.transform.localScale.z * 0.5f;
        // bulletPath.transform.parent = this.transform;
        // bulletPathHighlight = bulletPath;

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
            runForce = baseRunSpeed;
            turnSpeed = baseTurnSpeed;

            VacuumHighlight.SetActive(false);
        }
        else if(dead){
            VacuumHighlight.SetActive(false);
        }

        if(!dead && !gameOver)
        {
            var foes = this.transform.parent.gameObject.GetComponent<BattleBotEnvController>().GetFoes(this);
            if(foes.Count > 0){
                foreach(var foe in foes){
                    if(foe.dead){continue;}
                    // Direction to the enemy
                    Vector3 toEnemy = (foe.gameObject.transform.position - transform.position).normalized;
                    // Agent's current velocity
                    Vector3 agentVelocity = GetComponent<Rigidbody>().linearVelocity;
                    // Dot product of velocity and direction to enemy
                    float movementTowardsEnemy = Vector3.Dot(agentVelocity.normalized, toEnemy);
                    Vector3 agentForward = transform.forward; // Agent's forward direction
                    float facingAlignment = Vector3.Dot(agentForward, toEnemy); // Alignment with the enemy
                    float speed = agentVelocity.magnitude;
                    speed = speed / maxVelocity;

                    // Reward the agent for moving toward the enemy
                    if (movementTowardsEnemy > 0 && facingAlignment > 0.8f) // Positive dot product means moving closer
                    {
                        AddReward(movementTowardsEnemy * 0.02f * speed * Time.deltaTime); // Reward proportional to alignment and speed
                    }

                }
            }
        }
    }

    public override void OnEpisodeBegin(){
        base.OnEpisodeBegin();
        runForce = baseRunSpeed;
        turnSpeed = baseTurnSpeed;
        preppingShot = false;

        VacuumHighlight.SetActive(false);
    }
}
