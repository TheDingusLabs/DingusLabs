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

public class BattleBotAgentGunslinger : BattleBotAgent
{
    EnvironmentParameters m_ResetParams;

    float tempSlowDuration = 0.6f;
    float tempSlowCounter = 0f;
    private float previousMaxVelocity;
    private float previousRunForce;
    private float previousTurnSpeed;

    public GameObject bulletPrefab;

    public override void Initialize()
    {
        base.Initialize();
        previousMaxVelocity = maxVelocity;
        previousRunForce = runForce;
        previousTurnSpeed = turnSpeed;
    }
    public override void PerformOverTimeActions(){
        tempSlowCounter+= Time.deltaTime;

        if(tempSlowCounter < tempSlowDuration){
            maxVelocity = previousMaxVelocity - 1.2f;
            runForce = previousRunForce - 0.3f;
            turnSpeed = previousTurnSpeed - 0.15f;
        }
        else{
            maxVelocity = previousMaxVelocity;
            runForce = previousRunForce;
            turnSpeed = previousTurnSpeed;
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

    public override void ExecuteAction()
    {
        RewardGoodAim(0.02f, true);
        actionCounter = 0;
        tempSlowCounter = 0f;
        var newBullet = Instantiate(bulletPrefab, this.gameObject.transform.position + this.gameObject.transform.forward * 1, this.transform.rotation, this.gameObject.transform.parent);
        newBullet.GetComponent<BattleBullet>().owner = this.gameObject;
    }

    public override void OnEpisodeBegin(){
        base.OnEpisodeBegin();
        maxVelocity = previousMaxVelocity;
        runForce = previousRunForce;
        turnSpeed = previousTurnSpeed;
    }
}
