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

public class BattleBotAgentShover : BattleBotAgent
{
    EnvironmentParameters m_ResetParams;

    //public BattleExplosionIndicator indicator;
    public BattleKungFuHand indicator;

    public override void ExecuteAction()
    {
        RewardGoodAim(0.2f, true);
        actionCounter = 0;

        var hand = Instantiate(indicator, transform.position + transform.forward * 1f, transform.rotation);
        hand.owner = this.gameObject;
        m_AgentRb.AddForce(m_AgentRb.transform.forward * 200f *-1, ForceMode.Acceleration); 
    }

    public override void PerformOverTimeActions(){
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
                        reward = math.max(movementTowardsEnemy * 0.02f * speed * Time.deltaTime, reward); // Reward proportional to alignment and speed
                    }
                }
            }

            if(reward > 0){
                AddReward(reward); // Reward proportional to alignment and speed
            }
        }
    }
}
