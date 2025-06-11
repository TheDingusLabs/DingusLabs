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
using UnityEngine.TextCore.LowLevel;

public class BattleBotAgentSisyphus : BattleBotAgent
{
    EnvironmentParameters m_ResetParams;

    public BattleBoulder boulder;

    public BattleExplosionIndicator indicator;

    private bool boulderLost = false;

    private float angleThreshold = 10f; // The angle threshold

    private float chargeDuration = 3f;
    private float chargeCounter = 3f;
    private float previousRunForce;

    private float previousTurnSpeed;
    private bool charging = false;

    public GameObject buffBod;
    //private float initialpushReward = 0;

    void Start(){
        boulder.transform.parent = boulder.transform.parent.parent;

        previousRunForce = runForce;
        previousTurnSpeed = turnSpeed;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
        sensor.AddObservation(boulder.gameObject.transform.localPosition);
        sensor.AddObservation(Vector3.Distance(this.transform.position, boulder.transform.position));
        sensor.AddObservation(boulderLost);
        sensor.AddObservation(IsLookingAtTarget(boulder.gameObject, angleThreshold));
        sensor.AddObservation(charging);
        sensor.AddObservation(boulder.transform.position - transform.position); // Direction to boulder
        sensor.AddObservation(boulder.GetComponent<Rigidbody>().linearVelocity); // Boulder velocity
    }

    bool IsLookingAtTarget(GameObject target, float threshold)
    {
        if (target == null) return false;

        // Get the vector pointing from this object to the target
        Vector3 directionToTarget = target.transform.position - transform.position;
        directionToTarget.Normalize();

        // Compare this object's forward direction with the direction to the target
        float angle = Vector3.Angle(transform.forward, directionToTarget);

        // Check if the angle is within the threshold
        return angle <= threshold;
    }

    public override void ExecuteAction()
    {
        m_AgentRb.AddForce(m_AgentRb.transform.forward * 100f, ForceMode.Force); 
        chargeCounter = 0;
        actionCounter = 0;
        charging = true;
        runForce = previousRunForce * 1.8f;
        turnSpeed = previousTurnSpeed * 1.3f;
    }

    public override void PerformOverTimeActions()
    {
        base.PerformOverTimeActions();
        chargeCounter += Time.deltaTime;
        SetHandColour();

        if(chargeCounter > chargeDuration){
            runForce = previousRunForce;
            turnSpeed = previousTurnSpeed;
            charging = false;
        }

        if(dead || gameOver || boulderLost){
            runForce = previousRunForce;
            turnSpeed = previousTurnSpeed;
            charging = false;
            return;
            }

        if(!boulderLost && !dead && !gameOver){
            float maxAllowedDistance = 4f;

            float distanceToBoulder = Vector3.Distance(this.gameObject.transform.position, boulder.transform.position);
            float boulderSpeed = boulder.gameObject.GetComponent<Rigidbody>().linearVelocity.magnitude;
            Vector3 directionToBoulder = (boulder.transform.position - this.gameObject.transform.position).normalized;
            float speedTowardsBoulder = Vector3.Dot(this.gameObject.GetComponent<Rigidbody>().linearVelocity, directionToBoulder);

            //Debug.Log(speedTowardsBoulder);
            AddReward(Time.deltaTime * speedTowardsBoulder * 0.012f); // Reward for moving towards the boulder  
            //AddReward(Time.deltaTime * boulderSpeed * 0.012f);

            // Define bounds
            float minDist = maxAllowedDistance / 2f;
            float maxDist = maxAllowedDistance * 3f;
            // Linearly interpolate reward from 1 to -1
            float t = Mathf.InverseLerp(minDist, maxDist, distanceToBoulder); // t = 0 (close) to 1 (far)
            float proximityReward = Mathf.Lerp(1f, -1f, t);
            // Clamp to make sure we never go above 1 or below -1
            proximityReward = Mathf.Clamp(proximityReward, -1f, 1f);
            AddReward(Time.deltaTime * proximityReward*0.15f);


            if (this.gameObject.GetComponent<Rigidbody>().linearVelocity.magnitude < 0.5f)
            {
                AddReward(Time.deltaTime * -0.02f); // Penalty for idling
            }

            var foes = this.transform.parent.gameObject.GetComponent<BattleBotEnvController>().GetFoes(this);
            float reward = 0f;
            if(foes.Count > 0){
                foreach(var foe in foes){
                    if(foe.dead){continue;}
                    Vector3 toOpponent = foe.gameObject.transform.position - this.gameObject.transform.position;
                    Vector3 toBoulder = boulder.transform.position - this.gameObject.transform.position;

                    var doot = Vector3.Dot(toOpponent.normalized, toBoulder.normalized);
                    if(doot > 0.5f)
                    {
                        reward = math.max(reward, Time.deltaTime * boulderSpeed * doot * 0.2f);
                    }
                    // if (Vector3.Dot(toOpponent.normalized, toBoulder.normalized) > 0.8f) // Alignment check
                    // {
                    //     reward = math.max(reward, Time.deltaTime * boulderSpeed * 0.20f);
                    // }
                }
            }
            if(reward > 0){
                AddReward(reward * 0.1f);
            }
        }   
    }

    private void FixedUpdate()
    {
        if(!DoRealGroundCheck()){
            m_AgentRb.AddForce(-1f * m_AgentRb.transform.up * 60f, ForceMode.Force);
        }

    }

    public void BoulderFellDownTheWell(){
        if(!dead && !gameOver && !levelEnded && !boulderLost){
            //AddReward(-0.1f); //don't lose your boulder idiot
            boulderLost = true;

            StartCoroutine(
                BringBackBoulder(3)
            );
        }
    }

    IEnumerator BringBackBoulder(float time)
    {
        yield return new WaitForSeconds(time);

        if(!gameOver && !levelEnded && !dead && boulderLost){
            TelportBoulder();
            boulderLost = false;
        }
    }

    protected override void OnCollisionStay(Collision col){
        base.OnCollisionStay(col);
        if(levelEnded || dead){return;}
        else if(col.gameObject.CompareTag("boulder"))
        {
            boulder.timeMostRecentlyTouched = 0f;
            //touching boulder is good
            AddReward(Time.deltaTime * 0.01f);
        }
    }

    public void SetHandColour(){
        if(chargeCounter < chargeDuration && charging){
            buffBod.SetActive(true);
        }
        else{
            buffBod.SetActive(false);
        }
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        boulderLost = false;
        chargeCounter = chargeDuration;
        charging = false;

        TelportBoulder(true);
        //initialpushReward = 0;
    }

    public void TelportBoulder(bool starting = false){
        boulder.gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        if(starting){
            boulder.transform.position = this.transform.position + this.transform.forward * 1.2f;
        }
        else{
            boulder.transform.position = this.transform.position + this.transform.forward * 1.5f + this.transform.up * 1f;
        }
    }
}
