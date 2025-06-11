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
using UnityEngine.PlayerLoop;
using Unity.Mathematics;

public class BattleBotAgentPlumber : BattleBotAgent
{
    EnvironmentParameters m_ResetParams;

    public HurtBox stompbox;

    private float upCounter = 5f;
    private float upMax = 0.2f;

    public override void ExecuteAction()
    {
        //RewardGoodAim(0.2f, true);
        //Debug.Log("Plumber tried to jump");
        if(upCounter > upMax && DoRealGroundCheck()){
            // Debug.Log("Plumber jumped");
            actionCounter = 0;
            upCounter = 0f;
            m_AgentRb.AddForce(m_AgentRb.transform.up * 38.5f, ForceMode.Impulse); 
            stompbox.enabled = false;
        }
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
                        reward = math.max(movementTowardsEnemy * 0.04f * speed * Time.deltaTime, reward); // Reward proportional to alignment and speed
                    }
                }
            }

            if(reward > 0){
                AddReward(reward); // Reward proportional to alignment and speed
            }
        }
    }

    private void FixedUpdate()
    {
        if(upCounter < upMax)
        {
            upCounter += Time.deltaTime;
            m_AgentRb.AddForce(m_AgentRb.transform.up * 1f, ForceMode.Force); 
            
        }

        else if(upCounter > upMax && !stompbox.enabled){
            stompbox.enabled = true;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);

        sensor.AddObservation(DoRealGroundCheck());
        sensor.AddObservation(this.transform.localPosition.y);
        sensor.AddObservation(this.gameObject.GetComponent<Rigidbody>().linearVelocity.y);
    }

    public override void TakeEnemyHurtingAction(){
        upCounter = 0.1f;
        m_AgentRb.AddForce(m_AgentRb.transform.up * 30f, ForceMode.Impulse); 
    }

    private void FoeStomped(){

    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();

        upCounter = 5;
    }
}
