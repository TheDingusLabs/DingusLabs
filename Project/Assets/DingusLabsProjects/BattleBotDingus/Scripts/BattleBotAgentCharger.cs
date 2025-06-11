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

public class BattleBotAgentCharger : BattleBotAgent
{
    EnvironmentParameters m_ResetParams;

    public BattleDrill drill;

    public override void ExecuteAction()
    {
        RewardGoodAim(0.2f);
        actionCounter = 0;
        m_AgentRb.AddForce(m_AgentRb.transform.forward * 2500f, ForceMode.Force); 
        drill.BeginSpin(1.7f);
    }
}
