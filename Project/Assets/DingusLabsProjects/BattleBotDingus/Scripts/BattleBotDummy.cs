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

public class BattleBotDummy
{
    public enum Position
    {
        Charger,
        Shooter,
        Generic
    }
    public GameObject healthBar;
    public GameObject healthBarProgress;
    private GameObject AhCamera;


    public GameObject ground;
    public bool levelEnded = false;

    public Rigidbody m_AgentRb;

    public float maxVelocity;
    public float maxBackVelocity;
    public float jumpForce;
    public float runForce;
    public float jumpCounter = 999;
    public float jumpCooldown = 0.3f;

    public float actionCooldown = 0.3f;
    public float actionCounter = 0.3f;
    // This is a downward force applied when falling to make jumps look
    // less floaty
    public float fallingForce;

    public float turnSpeed = 0.65f;

    // Use to check the coliding objects
    public Collider[] hitGroundColliders = new Collider[3];

    EnvironmentParameters m_ResetParams;

    public float maxHP = 100;
    public float hp = 100;

    public BattleBotEnvController.BattleTeam team;
    public List<Material> colours;
    public Material healthBarRegularColour;
    public Material healthBarDecayColour;
    public MeshRenderer displayCube;

    public bool dead = false;
    public bool gameOver = false;
    private string properTagType;


    //bot specific objects
    public GameObject enemyTypeIdentifier;


    public GameObject facingBox;



    public void ObservedTookDamage(float damageAmount){

    }
    public virtual void ObservedDidDamage(float damageAmount){

    }

    public void ObservedDied(){
    }

    public void ObservedWon(){

    }

    public void ObservedAttackingTheDead(float damageAmount){
    }

    public void observeTimedOut(){
    }

    public void observeAttackFailed(float multiplier = 1){
    }


    public virtual void MoveAgent(ActionSegment<int> act)
    {

    }

    public void ApplyDrag(int dirToGoForwardAction){
        //capping reverse speed
        var nonYSpeed = new Vector3(m_AgentRb.linearVelocity.x, 0, m_AgentRb.linearVelocity.z);
        if(dirToGoForwardAction == 2){
            m_AgentRb.linearDamping = nonYSpeed.magnitude / maxBackVelocity;
        }
        else{
            m_AgentRb.linearDamping = nonYSpeed.magnitude / maxVelocity;
        }
    }

    protected virtual void OnCollisionStay(Collision col)
    {

    }

    protected virtual void OnTriggerStay(Collider col)
    {
        if(levelEnded || gameOver || dead){
            return;
        }

    }

    public virtual void DieInstantly(){ //Really wondering if we can track who did damage recently and reward them the kill hp! We could use TakeDamage to take the source unit and if it's any of the pushing types give them a reward?
        if(levelEnded || gameOver || dead){return;}
        var hpLost = hp;
        TakeDamage(hpLost, null);
        //Die();
    }

    public virtual void TakeDamage(float damage, BattleBotAgent sourceAgent){
        if(levelEnded || gameOver){return;}
        // punish hurting the dead?
        if(dead){

        }
        else{
            this.hp -= damage;
            if(this.hp <= 0){Die();}
            //TODO this needs to be updated for a target dummy
            //this.transform.parent.gameObject.GetComponent<BattleBotEnvController>().AnAgentWasHurt(this, sourceAgent, damage);
        }
    }

    public void Die(){

    }



    public void BeginGameEnded(){
        gameOver = true;
    }

    public virtual void TakeEnemyHurtingAction(){
    }


    public virtual void ExecuteAction()
    {

    }

    public virtual void PerformOverTimeActions(){

    }


    private void Update()
    {

    }
}
