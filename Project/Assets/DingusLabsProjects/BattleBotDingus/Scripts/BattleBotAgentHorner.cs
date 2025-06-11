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

public class BattleBotAgentHorner : BattleBotAgent
{
    EnvironmentParameters m_ResetParams;
    private bool charging = false;

    public BattleHorn horn;

    public override void MoveAgent(ActionSegment<int> act){
        if(charging)
        {
            m_AgentRb.AddForce(m_AgentRb.transform.forward * 140f, ForceMode.Force); 
        }
        else{
            base.MoveAgent(act);
        }
    }

    public override void TakeDamage(float damage, BattleBotAgent sourceAgent)
    {
        base.TakeDamage(damage, sourceAgent);

        if(damage > 15){
            charging = false;
        }
    }

    public override void ExecuteAction()
    {
        if(charging){return;}
        RewardGoodAim(0.3f);
        actionCounter = 0;
        charging = true;
        horn.BeginHorn();
        m_AgentRb.linearVelocity = m_AgentRb.transform.forward * 2f;
        m_AgentRb.AddForce(m_AgentRb.transform.forward * 200f, ForceMode.Force); 
    }

    public void HitSomethingWhilstCharging(){
        actionCounter = 0;
        charging = false;
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        charging = false;
        horn.EndHorn();
    }
}
