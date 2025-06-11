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

public class BattleBotAgentShooter : BattleBotAgent
{
    EnvironmentParameters m_ResetParams;

    public BattleBall battleBall;

    private List<GameObject> allBalls;

    public override void Initialize()
    {
        base.Initialize();

        allBalls = new List<GameObject>();
    }

    public override void ExecuteAction()
    {
        RewardGoodAim(0.2f);
        actionCounter = 0;
        var ball = Instantiate(battleBall, this.gameObject.transform.position + this.gameObject.transform.forward * 1, this.transform.rotation, this.gameObject.transform.parent);
        ball.GetComponent<BattleBall>().owner = this.gameObject;
        ball.transform.GetComponent<Rigidbody>().linearVelocity = /*m_AgentRb.velocity +*/ (this.transform.forward * 6f);    
        allBalls.Add(ball.gameObject);
    }

    public override void OnEpisodeBegin(){
        base.OnEpisodeBegin();
        foreach(var ball in allBalls){
            if(ball != null){
                Destroy(ball);
            }
        }

        allBalls = new List<GameObject>();
    }
}
