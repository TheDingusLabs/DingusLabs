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

public class BattleBotAgentDummy : BattleBotAgent
{
    EnvironmentParameters m_ResetParams;

    public BattleBuzzSaw saw;

    private float drillDuration = 6f;
    private float drillCounter = 6f;

    private float previousMaxVelocity;
    private float previousRunForce;
    public override void Initialize(){
        base.Initialize();

        previousMaxVelocity = maxVelocity;
        previousRunForce = runForce;
    }

    public override void ExecuteAction()
    {
        actionCounter = 0;
        drillCounter = 0;
        //m_AgentRb.AddForce(m_AgentRb.transform.forward * 2500f, ForceMode.Force); 
        saw.BeginSpin(drillDuration);
    }

    public override void PerformOverTimeActions(){
        drillCounter+= Time.deltaTime;

        if(drillCounter < drillDuration){
            maxVelocity = previousMaxVelocity - 1.5f;
            runForce = previousRunForce - 0.2f;
        }
        else{
            maxVelocity = previousMaxVelocity;
            runForce = previousRunForce;
        }

        if(!dead && !gameOver){
            AddReward(Time.deltaTime * this.gameObject.GetComponent<Rigidbody>().linearVelocity.magnitude * 0.015f);
        }
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        drillCounter = drillDuration;
    }


}
