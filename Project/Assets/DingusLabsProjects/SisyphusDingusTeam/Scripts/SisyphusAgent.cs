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
using UnityEngine.UIElements;
using Unity.Mathematics;

//what four should I make?
//shooter? doneish
//sisyphus? pushes boulder? pushes enemies?
//jedi dingus? swings a sword? can deflect attacks?
//charge dingus? doneish
//bomber dingus?

public class SisyphusAgent : Agent
{
    public GameObject ground;
    public bool levelEnded = false;

    public Rigidbody m_AgentRb;

    public float maxVelocity;
    public float maxBackVelocity;
    public float jumpForce;
    public float runForce;
    public float jumpCounter = 999;
    public float jumpCooldown = 0.3f;
    public float fallingForce;
    public float turnSpeed = 0.65f;

    // Use to check the coliding objects
    public Collider[] hitGroundColliders = new Collider[3];

    EnvironmentParameters m_ResetParams;

    public bool dead = false;
    public bool gameOver = false;

    public SisyphusEnvController controller;
    private GameObject boulder;

    public override void Initialize()
    {
        m_AgentRb = GetComponent<Rigidbody>();
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        controller = this.transform.parent.GetComponent<SisyphusEnvController>();
        boulder = this.transform.parent.GetComponentsInChildren<SisyphusBoulder>()[0].gameObject;
    }


    //we punish for damage already, I'm concerned about the strange lessons it may learn for taking more damage when it takes damage sometime!
    public void ObservedDied(){
        //AddReward(-0.5f);
    }

    public void ObservedWon(){
        //AddReward(0.2f);
    }

    public void observeTimedOut(){
        //AddReward(-0.5f);
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(m_AgentRb.linearVelocity);
        sensor.AddObservation(dead);
        sensor.AddObservation(gameOver);
    }

    public virtual void MoveAgent(ActionSegment<int> act)
    {
        if(levelEnded || dead){return;}

        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;
        var dirToGoForwardAction = act[0];
        var rotateDirAction = act[1];
        var ActionAction = act[2];

        if (dirToGoForwardAction == 1)
            dirToGo = 1f * 1f * transform.forward;
        else if (dirToGoForwardAction == 2)
            dirToGo = 1f * -1f * transform.forward;
        if (rotateDirAction == 1)
            rotateDir = transform.up * -1f;
        else if (rotateDirAction == 2)
            rotateDir = transform.up * 1f;

        float turningMoveSpeedMod = rotateDirAction == 0 ? 1 : 0.8f; 

        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f * turnSpeed);

        ApplyDrag(dirToGoForwardAction);

        if(!DoRealGroundCheck()){
            m_AgentRb.AddForce(-1f * new Vector3(0,1,0) * 30f, ForceMode.Force);
        }

        m_AgentRb.AddForce(dirToGo * runForce * 50f, ForceMode.Force);

        jumpCounter += Time.fixedDeltaTime;

        var boulderDistance = Vector3.Distance( new Vector3(transform.position.x,0,transform.position.z), new Vector3(boulder.transform.position.x,0,boulder.transform.position.z));
        //reward moving forwards when in hyper close proximity to boulder!
        if(boulderDistance < 3.45 && dirToGoForwardAction == 1){
            AddReward(0.001f);
            controller.AnAgentIsCloseToBouldy(0.0002f);
        }
    }

    public void ApplyDrag(int dirToGoForwardAction){
        // //capping reverse speed
        // var nonYSpeed = new Vector3(m_AgentRb.linearVelocity.x, 0, m_AgentRb.linearVelocity.z);
        // if(dirToGoForwardAction == 2){
        //     m_AgentRb.linearDamping = nonYSpeed.magnitude / maxBackVelocity;
        // }
        // else{
        //     m_AgentRb.linearDamping = nonYSpeed.magnitude / maxVelocity;
        // }
        if (dirToGoForwardAction != 2 && m_AgentRb.linearVelocity.magnitude > maxVelocity)
        {
            // Clamp the velocity to the maximum magnitude
            //m_AgentRb.linearVelocity = m_AgentRb.linearVelocity.normalized * maxVelocity;

            Vector3 excessVelocity = m_AgentRb.linearVelocity - m_AgentRb.linearVelocity.normalized * maxVelocity;
            m_AgentRb.AddForce(-excessVelocity, ForceMode.VelocityChange);
        }
        else if (dirToGoForwardAction == 2 && m_AgentRb.linearVelocity.magnitude > maxBackVelocity)
        {
            // Clamp the velocity to the maximum magnitude
            m_AgentRb.linearVelocity = m_AgentRb.linearVelocity.normalized * maxBackVelocity;
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[1] = 2;
        }
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
    }

    protected virtual void OnCollisionStay(Collision col)
    {

        if(col.gameObject.CompareTag("wall")){

        }
    }

    protected virtual void OnTriggerStay(Collider col)
    {
        if(levelEnded || gameOver || dead){
            return;
        }
        if(col.gameObject.CompareTag("theDeadZone"))
        {
            AddReward(-0.2f);
            Die();
        }
    }

    public void Die(){
        if(levelEnded || dead || gameOver){return;}
        ObservedDied();
        dead = true;
        controller.AnAgentHasFallen();
        NotifyAgentDone(DoneReason.DoneCalled);
    }



    public void BeginGameEnded(){
        gameOver = true;
    }

    public virtual void TakeEnemyHurtingAction(){
    }

    public override void OnEpisodeBegin()
    {
        dead = false;
        levelEnded = false;
        gameOver = false;
        m_AgentRb.linearVelocity = default(Vector3);
    }

    public bool DoRealGroundCheck(){
        RaycastHit hit;
        Physics.Raycast(transform.position + new Vector3(0, -0.45f, 0), -Vector3.up, out hit, 0.1f);

        if (hit.collider != null && hit.collider.CompareTag("walkableSurface") && hit.normal.y > 0.95f)
        {
            return true;
        }

        return false;
    }

    private void Update()
    {
        if(!dead && !gameOver)
        {
            var boulderDistance = Vector3.Distance( new Vector3(transform.position.x,0,transform.position.z), new Vector3(boulder.transform.position.x,0,boulder.transform.position.z));
            if(boulderDistance < 5){
                controller.AnAgentIsCloseToBouldy(Time.deltaTime * 0.001f);
            }
            //TODO: prolly want to add a reward for being in a certain distance of the boulder, this might not be necessary as we now award agents for being close to the boulder when it hits goals
            // var boulderDistance = Vector3.Distance( new Vector3(transform.position.x,0,transform.position.z), new Vector3(boulder.transform.position.x,0,boulder.transform.position.z));
            // if(boulderDistance < 5){
            //     AddReward(Time.deltaTime * 0.002f);
            //     controller.AnAgentIsCloseToBouldy(Time.deltaTime * 0.0001f);
            // }
            // if(boulderDistance < 10){
            //     AddReward(Time.deltaTime * 0.001f);
            // }

            if(Vector3.Dot(transform.forward, this.gameObject.GetComponent<Rigidbody>().linearVelocity) > 0.2f){
                AddReward(Time.deltaTime * 0.001f);
            }
        }
        else
        {
            //Debug.Log($"somehow {this.gameObject.name} is getting a closest enemy distance of 99f, this is is a bug as it doesn't get distances when dead or the game is over!");
        }
    }
}

   

