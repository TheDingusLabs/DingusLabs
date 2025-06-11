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

public class BattleBotAgentBomber : BattleBotAgent
{
    EnvironmentParameters m_ResetParams;

    public BattleBomb battleBomb;


    public override void ExecuteAction()
    {
        actionCounter = 0;
        var ball = Instantiate(battleBomb, this.gameObject.transform.position + (this.gameObject.transform.forward * -1.5f ), this.transform.rotation, this.gameObject.transform.parent);
        ball.GetComponent<BattleBomb>().owner = this.gameObject;
        //ball.transform.GetComponent<Rigidbody>().velocity = /*m_AgentRb.velocity +*/ (this.transform.forward * 12);    
    }
}
