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

public class TiltBallAgent : Agent
{
    public bool levelEnded = false;

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

    public TiltBallEnvController controller;
    private GameObject ball;

    private HashSet<Vector2Int> visitedLocations; // Stores visited coordinates
    private float cellSize = 3f; // Size of the grid cells (1 unit increments)
    Vector2Int previousCell;

    public Vector3 dirVectorToGoal = Vector3.zero;
    public int goalsremaining = 0;
    public GameObject tiltBallStage;
    public bool insideDeathWall = false;
    private float timeElapsed = 0f;
    private float totaltimeElapsed = 0f;
    private float timeDelayedRewardMulti = 0.0001f;
    private float timeUntilMaxTimeDelayedRewardMulti = 7200f; //this model learns too fast and tries to exploit too quickly, at the start I want to weigh down rewards whilst it explores

    public void Start()
    {
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        controller = this.transform.parent.GetComponent<TiltBallEnvController>();
        ResetVisitedLocations();
        //ball = this.transform.parent.GetComponentsInChildren<TiltBallBoulder>()[0].gameObject;
    }

    public void ResetVisitedLocations()
    {
        visitedLocations = new HashSet<Vector2Int>();
    }

    // Call this function every step to reward exploration
    public void RewardForNewLocation()
    {
        // Get the ball's current position rounded to the nearest grid cell
        Vector2Int currentCell = GetGridCoordinates(ball.transform.localPosition);

        if(previousCell != null && previousCell != currentCell && visitedLocations.Contains(currentCell))
        {
            //Debug.Log("you've retredead old ground");
            AddReward(-0.1f); //exempting this from the multiplier to further discourage going back to old spots 
        }
        else if (!visitedLocations.Contains(currentCell) && !insideDeathWall)
        {
            // Reward the agent for exploring a new location
            AddReward(0.2f); //exempting this from the multiplier to further encourage exploration 
            visitedLocations.Add(currentCell); // Mark this cell as visited
        }
        else if (!visitedLocations.Contains(currentCell) && insideDeathWall)
        {
            // Reward the agent for exploring a new location
            //AddTimeWeightedReward(-0.2f);
            visitedLocations.Add(currentCell); // Mark this cell as visited
        }
        else 
        {
            //AddTimeWeightedReward(Time.deltaTime * -0.005f);
        }
        previousCell = currentCell;
    }

    // Convert the ball's position to grid coordinates
    private Vector2Int GetGridCoordinates(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / cellSize);
        int z = Mathf.FloorToInt(position.z / cellSize);
        return new Vector2Int(x, z);
    }

    //we punish for damage already, I'm concerned about the strange lessons it may learn for taking more damage when it takes damage sometime!
    public void ObservedDied(float multiplier){
        AddTimeWeightedReward(-1.0f - multiplier);
    }

    public void ObservedWon(){
        AddTimeWeightedReward(1.0f);
    }

    public void observeTimedOut(){
        AddTimeWeightedReward(-1.250f);
    }

    public void observeHitGoal(float reward){
        //Debug.Log($"hit goal, reward: {reward}");
        AddTimeWeightedReward(reward);
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(ball != null ? ball.transform.localPosition : Vector3.zero);
        sensor.AddObservation(ball != null ? ball.GetComponent<Rigidbody>().linearVelocity : Vector3.zero);
        sensor.AddObservation(ball != null ? ball.GetComponent<Rigidbody>().angularVelocity : Vector3.zero);
        sensor.AddObservation(tiltBallStage != null ? tiltBallStage.transform.rotation : Quaternion.identity);
        //sensor.AddObservation(timeElapsed/30f);
        sensor.AddObservation(dead);
        sensor.AddObservation(gameOver);
        //sensor.AddObservation(dirVectorToGoal);
        sensor.AddObservation(goalsremaining);
        sensor.AddObservation(insideDeathWall);
        //sensor.AddObservation(controller.currentLevel);
        //sensor.AddObservation(controller.GetNormalizedDistanceToNearestActiveGoal());
        //sensor.AddObservation(visitedLocations.Contains(GetGridCoordinates(ball.transform.localPosition)));
        //sensor.AddObservation(ball != null ? GetGridCoordinates(ball.transform.localPosition) : Vector2Int.zero);
        insideDeathWall = false;
    }

    public virtual void MoveAgent(ActionBuffers actionBuffers)
    {
        if(tiltBallStage == null || timeElapsed <= 0.5f){return;}
        Transform stage = tiltBallStage.transform; // The transform of the stage to tilt
        float maxTiltX = 40f; // Maximum tilt angle for X (side to side)
        float maxTiltZ = 40f; // Maximum tilt angle for Z (forward/backward)
        float tiltSpeed = 5f; // Speed at which the stage tilts
        Quaternion targetRotation; // Target rotation for the stage

        var continuousActions = actionBuffers.ContinuousActions;

        var sideTilt = continuousActions[0];
        var forwardTilt = continuousActions[1];

        Vector2 input = new Vector2(forwardTilt,sideTilt); // Controller input
        
        if(levelEnded || dead || gameOver){ //if dead we want the stage rotation to normalize
            input.y = 0;
            input.x = 0;
        }

        // Calculate target tilt angles
        float targetX = input.y * maxTiltX; // Forward/backward tilt (Z-axis rotation)
        float targetZ = -input.x * maxTiltZ; // Side-to-side tilt (X-axis rotation)

        // Update the target rotation
        targetRotation = Quaternion.Euler(targetX, 0f, targetZ);

        // Smoothly interpolate to the target rotation
        stage.rotation = Quaternion.Lerp(stage.rotation, targetRotation, Time.deltaTime * tiltSpeed);
    }


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var contActionsOut = actionsOut.ContinuousActions;

        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        contActionsOut[0] = vertical;
        contActionsOut[1] = horizontal;
    }

    public void BeginGameEnded(){
        gameOver = true;
        NotifyAgentDone(DoneReason.DoneCalled);
    }

    public override void OnEpisodeBegin()
    {
        timeElapsed = 0f;
        dead = false;
        levelEnded = false;
        gameOver = false;
        insideDeathWall = false;
        ball = controller.balls[0].gameObject;
        ResetVisitedLocations();
        previousCell = new Vector2Int(9999,9999);
    }

    private void Update()
    {
        float rampMultiplier = Mathf.Clamp01(totaltimeElapsed / timeUntilMaxTimeDelayedRewardMulti);
        timeDelayedRewardMulti = Mathf.Lerp(0.2f, 1f, rampMultiplier);
        if(ball != null && tiltBallStage != null){
            this.transform.position = ball.transform.position;
            this.transform.rotation = tiltBallStage.transform.rotation;
        }
        if(!dead && !gameOver){
            timeElapsed += Time.deltaTime;
            totaltimeElapsed += Time.deltaTime;
            AddTimeWeightedReward(Time.deltaTime * 0.05f);
            RewardForNewLocation();
            if(ball != null){
                //AddTimeWeightedReward(Time.deltaTime * 0.01f * math.log(1+ ball.GetComponent<Rigidbody>().linearVelocity.magnitude + ball.GetComponent<Rigidbody>().angularVelocity.magnitude));
            }
        }
    }

    public void AddTimeWeightedReward(float reward){
        AddReward(reward * timeDelayedRewardMulti);
    }
}

   

