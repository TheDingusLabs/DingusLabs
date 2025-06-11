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

public class BattleBotAgentDeathWaller : BattleBotAgent
{
    EnvironmentParameters m_ResetParams;

    public DeathWallSpawner deathWallSpawnerPrefab;
    public DeathWallSpawner spawner1;
    public DeathWallSpawner spawner2;

    public DeathWall deathWallPrefab;
    public DeathWall deathwall;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void ObservedDidDamage(float damageAmount){
        if(!dead){
            base.ObservedDidDamage(damageAmount);
        }
    }

    public override void ExecuteAction()
    {
        actionCounter = 0;
        var spawner = Instantiate(deathWallSpawnerPrefab, this.gameObject.transform.position + this.gameObject.transform.forward * 1, this.transform.rotation, this.gameObject.transform.parent);
        spawner.GetComponent<DeathWallSpawner>().owner = this.gameObject;
        spawner.transform.GetComponent<Rigidbody>().linearVelocity = /*m_AgentRb.velocity +*/ (this.transform.forward * 18f);    
        if(spawner2 == null){
            spawner2 = spawner;
        }
        else if(spawner1 == null){
            spawner1 = spawner2;
            spawner2 = spawner;
        }
        else{
            //trying to signifficantly discourage spamming when they already have a perfectly good deathwall
            //AddReward(-0.1f);

            if(deathwall != null){
                Destroy(deathwall.gameObject);
            }
            Destroy(spawner1.gameObject);
            spawner1 = spawner2;
            spawner2 = spawner;
        }

        if(spawner1 != null && spawner2 != null){
            if(Vector3.Distance(spawner1.transform.position, spawner2.transform.position) < 3.0f ){
                //we want big deathwalls in this house
                //AddReward(-0.1f);
            }
            else {
                //AddReward(MathF.Min(Vector3.Distance(spawner1.transform.position, spawner2.transform.position) * 0.05f, 0.2f));
            }
        }

        if(spawner1 == null || spawner2 == null){
            actionCounter = actionCooldown -1f;
        }
    }

    public void SpawnerDied(){
        if(actionCounter < actionCooldown - 1){
            actionCounter = actionCooldown - 1;
        }
        CheckIfWallIsOkayToSpawn();
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);

        sensor.AddObservation(spawner1 == null);
        sensor.AddObservation(spawner2 == null);

        sensor.AddObservation(spawner1 != null ? spawner1.transform.localPosition : new Vector3(0,500,0) );
        sensor.AddObservation(spawner2 != null ? spawner2.transform.localPosition : new Vector3(0,500,0) );
    }

    public void CheckIfWallIsOkayToSpawn(){
        if(spawner1 != null && spawner1.GetComponent<DeathWallSpawner>().readyForSpawning && spawner2 != null && spawner2.GetComponent<DeathWallSpawner>().readyForSpawning){

            if(deathwall != null){ Destroy(deathwall.gameObject); }

            SpawnWall();
        }
        else if(deathwall != null) { Destroy (deathwall.gameObject); }
    }

    public override void PerformOverTimeActions()
    {
        base.PerformOverTimeActions();
        MaintainWall();
    }

    public void SpawnWall(){
        Vector3 startPosition = spawner1.transform.position;
        Vector3 endPosition = spawner2.transform.position;
        Vector3 midpoint = (startPosition + endPosition) / 2f;
        
        // Set position of the stretching object to the midpoint
        var wall = Instantiate(deathWallPrefab, midpoint, quaternion.identity, this.gameObject.transform.parent);
        wall.GetComponent<DeathWall>().owner = this.gameObject;
        deathwall = wall;
        MaintainWall();
    }

    public void MaintainWall(){
        if(spawner1 != null && spawner1.GetComponent<DeathWallSpawner>().readyForSpawning && spawner2 != null && spawner2.GetComponent<DeathWallSpawner>().readyForSpawning){
            // Calculate the midpoint
            Vector3 startPosition = spawner1.transform.position;
            Vector3 endPosition = spawner2.transform.position;
            Vector3 midpoint = (startPosition + endPosition) / 2f;
            
            // Set position of the stretching object to the midpoint
            deathwall.transform.position = midpoint;

            // Calculate direction and rotation
            Vector3 direction = endPosition - startPosition;
            deathwall.transform.rotation = Quaternion.LookRotation(direction);

            // Set the scale based on the distance
            float distance = direction.magnitude;
            deathwall.transform.localScale = new Vector3(deathwall.transform.localScale.x, deathwall.transform.localScale.y, distance);
        }
    }

    public override void OnEpisodeBegin(){
        base.OnEpisodeBegin();
        if(deathwall != null){
            Destroy(deathwall.gameObject);
        }
        if(spawner1 != null){
            Destroy(spawner1.gameObject);
        }
        if(spawner2 != null){
            Destroy(spawner2.gameObject);
        }
        deathwall = null;
        spawner1 = null;
        spawner2 = null;

        actionCounter = actionCooldown -1f;
    }
}
