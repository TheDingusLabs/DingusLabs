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
using Unity.Mathematics;

public class BattleBotAgentBuzzSaw : BattleBotAgent
{
    EnvironmentParameters m_ResetParams;

    public BattleBuzzSaw saw;

    private float drillDuration = 3f;
    private float drillCounter = 3f;

    private float previousMaxVelocity;
    private float previousRunForce;
    public override void Initialize(){
        base.Initialize();

        previousMaxVelocity = maxVelocity;
        previousRunForce = runForce;
    }

    // public override void MoveAgent(ActionSegment<int> act)
    // {
    //     if(drillCounter >= drillDuration){
    //         base.MoveAgent(act);
    //     }
    // }

    public override void ExecuteAction()
    {
        RewardGoodAim(0.2f);
        actionCounter = 0;
        drillCounter = 0;
        //m_AgentRb.AddForce(m_AgentRb.transform.forward * 2500f, ForceMode.Force); 
        saw.BeginSpin(drillDuration);
    }

    public override void PerformOverTimeActions(){
        drillCounter+= Time.deltaTime;

        if(drillCounter < drillDuration){
            maxVelocity = previousMaxVelocity - 1.5f;
            runForce = previousRunForce - 0.15f;
        }
        else{
            maxVelocity = previousMaxVelocity;
            runForce = previousRunForce;
        }

        if(!dead && !gameOver)
        {
            var foes = this.transform.parent.gameObject.GetComponent<BattleBotEnvController>().GetFoes(this);
            float reward = 0;
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
                        reward = math.max(movementTowardsEnemy * 0.05f * speed * Time.deltaTime, reward); // Reward proportional to alignment and speed
                    }
                }
            }

            if(reward > 0){
                AddReward(reward); // Reward proportional to alignment and speed
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        maxVelocity = previousMaxVelocity;
        runForce = previousRunForce;
        drillCounter = 9999f;
    }


}
