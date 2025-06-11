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
using NUnit.Framework;
using Unity.Mathematics;

public class BattleBotAgentAssassin : BattleBotAgent
{
    EnvironmentParameters m_ResetParams;
    private float cloakDuration = 5f;
    private float cloakCounter = 0f;

    private float previousMaxVelocity;
    private float previousRunForce;
    private float previousTurnSpeed;
    public float assassinMulti = 0f;
    
    public GameObject assassinBar;
    public GameObject assassinBarProgress;
    public GameObject buffBod;
    public override void Initialize(){
        base.Initialize();

        previousMaxVelocity = maxVelocity;
        previousRunForce = runForce;
        previousTurnSpeed = turnSpeed;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
        sensor.AddObservation(IsCloaked());
        sensor.AddObservation(assassinMulti);
    }

    public override void PerformOverTimeActions(){
        cloakCounter+= Time.deltaTime;

        if(IsCloaked()){
            maxVelocity = previousMaxVelocity + 3.5f;
            runForce = previousRunForce + 2.0f;
            turnSpeed = previousTurnSpeed + 0.85f;
            assassinMulti = assassinMulti <= 1f ? assassinMulti + Time.deltaTime / 6f : 1f;
            if(!assassinBar.activeInHierarchy){
                assassinBar.SetActive(true);
            }
            assassinBar.transform.LookAt(AhCamera.transform);
            assassinBarProgress.transform.LookAt(AhCamera.transform);

            assassinBarProgress.transform.localScale = new Vector3(assassinMulti, assassinBarProgress.transform.localScale.y, assassinBarProgress.transform.localScale.z);
        }
        else{
            maxVelocity = previousMaxVelocity;
            runForce = previousRunForce;
            turnSpeed = previousTurnSpeed;
            assassinMulti = 0;
        }

        if(!dead && !gameOver && IsCloaked() && assassinMulti > 0.3f && assassinMulti < 1f)
        {
            var foes = this.transform.parent.gameObject.GetComponent<BattleBotEnvController>().GetFoes(this);
            float reward = 0;
            if(foes.Count > 0){
                foreach(var foe in foes){
                    if(foe.dead){continue;}
                    // Direction to the enemy
                    Vector3 toEnemy = (foe.gameObject.transform.position - transform.position).normalized;
                    // Agent's current velocity
                    Vector3 agentVelocity = this.gameObject.GetComponent<Rigidbody>().linearVelocity;
                    // Dot product of velocity and direction to enemy
                    float movementTowardsEnemy = Vector3.Dot(agentVelocity.normalized, toEnemy);
                    Vector3 agentForward = transform.forward; // Agent's forward direction
                    float facingAlignment = Vector3.Dot(agentForward, toEnemy); // Alignment with the enemy
                    float speed = agentVelocity.magnitude;
                    speed = speed / maxVelocity;

                    // Get the velocity direction (normalized to ensure it's a direction vector)
                    Vector3 velocityDirection = this.gameObject.GetComponent<Rigidbody>().linearVelocity.normalized;

                    // Get the back direction of the target object
                    Vector3 backDirection = -foe.gameObject.transform.forward ;

                    // Calculate the dot product
                    float dotProduct = Vector3.Dot(velocityDirection, backDirection);

                    // Reward the agent for moving toward the enemy
                    if (movementTowardsEnemy > 0 && facingAlignment > 0.8f) // Positive dot product means moving closer
                    {
                        reward = math.max(movementTowardsEnemy * 0.075f * speed * Time.deltaTime * (1 + dotProduct), reward); // Reward proportional to alignment and speed
                    }
                }
            }

            if(reward > 0){
                AddReward(reward); // Reward proportional to alignment and speed
            }
        }
    }

    public override void DieInstantly()
    {
        base.DieInstantly();
        UnCloak();
    }

    public bool IsCloaked(){
        var iscloaked = cloakCounter > cloakDuration;
        if(iscloaked && !buffBod.activeInHierarchy){
            buffBod.SetActive(true);
        }
        return iscloaked;
    }

    public void UnCloak(){
        cloakCounter = 0f;
        buffBod.SetActive(false);
        assassinMulti = 0;
        if(assassinBar.activeInHierarchy){
            assassinBar.SetActive(false);
        }
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        cloakCounter = 0f;
        maxVelocity = previousMaxVelocity;
        runForce = previousRunForce;
        turnSpeed = previousTurnSpeed;
        cloakCounter = 9999f;
        UnCloak();
    }


}
