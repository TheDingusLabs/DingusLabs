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

public class EscapeAgent : Agent
{
    public bool levelEnded = false;

    public float maxVelocity;
    public float maxBackVelocity;
    public float jumpForce;
    public float runForce;
    private float numberOfJumps = 0;
    public float jumpLimit = 8;
    public float jumpCounter = 999;
    public float jumpCooldown = 0.3f;
    public float fallingForce;
    public float turnSpeed = 0.65f;

    // Use to check the coliding objects
    public Collider[] hitGroundColliders = new Collider[3];

    EnvironmentParameters m_ResetParams;

    public bool dead = false;
    public bool gameOver = false;

    public EscapeEnvController controller;

    private HashSet<Vector2Int> visitedLocations; // Stores visited coordinates
    private float cellSize = 2f; // Size of the grid cells (1 unit increments)
    Vector2Int previousCell;

    public Vector3 dirVectorToGoal = Vector3.zero;
    public int goalsremaining = 0;
    public bool insideDeathWall = false;
    public bool goalOpen = false;
    private Rigidbody m_AgentRb;
    public float rewardEarnedThisRound = 0;
    private float totaltimeElapsed = 0f;
    private float timeDelayedRewardMulti = 0.0001f;
    private float timeUntilMaxTimeDelayedRewardMulti = 19400f; //this model learns too fast and tries to exploit too quickly, at the start I want to weigh down rewards whilst it explores

    public bool grounded = true;

    public void Start()
    {
        m_AgentRb = GetComponent<Rigidbody>();
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        controller = this.transform.parent.GetComponent<EscapeEnvController>();
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
        Vector2Int currentCell = GetGridCoordinates(this.gameObject.transform.localPosition);

        if(previousCell != null && previousCell != currentCell && visitedLocations.Contains(currentCell))
        {
            //Debug.Log("you've retredead old ground");
            //AddTimeWeightedReward(-0.005f); //you've gone back to a previously explored area, we want to discorage this
        }
        else if (!visitedLocations.Contains(currentCell) && !insideDeathWall)
        {

            AddNonTimeWeightedReward(0.030f);
            visitedLocations.Add(currentCell); // Mark this cell as visited
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
    public void ObservedDied(){
        AddTimeWeightedReward(-1.0f);
    }

    public void ObservedHitHead(){
        AddTimeWeightedReward(-1.0f);
    }

    public void ObservedWon(){
        AddTimeWeightedReward(1f);
    }

    public void observeTimedOut(){
        AddTimeWeightedReward(-0.5f);
    }

    public void observeHitGoal(float reward){
        //Debug.Log($"hit goal, reward: {reward}");
        AddTimeWeightedReward(reward);
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.gameObject.transform.localPosition.y);
        sensor.AddObservation(this.gameObject.GetComponent<Rigidbody>().linearVelocity);
        sensor.AddObservation(this.gameObject.transform.localRotation);
        sensor.AddObservation(grounded);
        sensor.AddObservation(Mathf.Clamp01(jumpCounter / jumpCooldown));
        sensor.AddObservation(Mathf.Clamp01(numberOfJumps / jumpLimit));
        //sensor.AddObservation(dirVectorToGoal);
        sensor.AddObservation(goalsremaining);
        sensor.AddObservation(goalOpen);
        //sensor.AddObservation(controller.currentLevel);
        //sensor.AddObservation(controller.GetNormalizedDistanceToNearestActiveGoal());
    }

    public virtual void MoveAgent(ActionSegment<int> act)
    {
        m_AgentRb.AddForce(Vector3.down * -4.9f, ForceMode.Acceleration);
        if (levelEnded || dead) { return; }
        AddTimeWeightedReward(-0.0004f);
        if(grounded){
            AddTimeWeightedReward(0.00002f);
        }
        else{
            //AddTimeWeightedReward(-0.0004f);
        }

        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;
        var dirToGoForwardAction = act[0];
        var rotateDirAction = act[1];
        var ActionAction = act[2];

        if (dirToGoForwardAction == 1)
            dirToGo = transform.forward;
        else if (dirToGoForwardAction == 2)
            dirToGo = -transform.forward;

        if (rotateDirAction == 1)
            rotateDir = -transform.up;
        else if (rotateDirAction == 2)
            rotateDir = transform.up;

        float dot = Vector3.Dot(this.gameObject.transform.up, Vector3.up);

        if (ActionAction == 1 && jumpCounter >= jumpCooldown && dot >= (1.0f - 0.05f))
        {
            Jump();
            if(controller.escapeDoor != null && controller.escapeDoor.open)
            {
                //AddTimeWeightedReward(-0.1f);
            }
        }

        float turningMoveSpeedMod = rotateDirAction == 0 ? 1 : 0.8f;

        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f * turnSpeed);

        ApplyDrag(); // Now correctly applies a horizontal velocity cap

        // Ensure applied force has no Y component
        Vector3 forceToApply = dirToGo * runForce * 50f;
        forceToApply.y = 0; // Remove Y influence
        m_AgentRb.AddForce(forceToApply, ForceMode.Force);

        jumpCounter += Time.fixedDeltaTime;
    }

    public void ApplyDrag()
    {
        Vector3 velocity = m_AgentRb.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);

        // Determine if the agent is moving backward
        float forwardDot = Vector3.Dot(transform.forward, horizontalVelocity.normalized);
        bool isMovingBackward = forwardDot < 0; // Negative dot means moving opposite to forward direction

        // Apply different velocity caps based on movement direction
        float velocityCap = isMovingBackward ? maxBackVelocity : maxVelocity; // Reduce speed when moving backward

        // if(!isMovingBackward)
        // {
        //     AddTimeWeightedReward(Time.fixedDeltaTime * 0.005f * horizontalVelocity.magnitude/maxVelocity);
        // }

        if (!grounded)
        {
            m_AgentRb.AddForce(Vector3.down * 5f, ForceMode.Force);
            m_AgentRb.angularVelocity *= 0.65f;
            velocityCap *= 0.45f;
            //if(isMovingBackward){AddTimeWeightedReward(-0.01f);} //backwards jumping is super slow and wasteful but I dunno punishing specifically that when the issue is more the constant jumping seems suboptimal
        }

        if (horizontalVelocity.magnitude > velocityCap)
        {
            horizontalVelocity = horizontalVelocity.normalized * velocityCap;
        }

        //if(isMovingBackward){AddTimeWeightedReward(-0.0002f);} //moving backwards is generally a bad idea and only to be done to fix problems

        m_AgentRb.linearVelocity = new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.z);
    }

    public void ApplyAirDrag()
    {
        Vector3 velocity = m_AgentRb.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);

        var airMaxVelocity = maxVelocity * 0.65f;

        if (horizontalVelocity.magnitude > airMaxVelocity)
        {
            horizontalVelocity = horizontalVelocity.normalized * airMaxVelocity;
        }
        m_AgentRb.linearVelocity = new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.z);
    }

    public void Jump()
    {
        numberOfJumps += 1;
        jumpCounter = 0;
        m_AgentRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // Apply jump
        //AddTimeWeightedReward(-0.1f);
    }

    public bool DoRealGroundCheck(){
        RaycastHit hit;
        int layerMask = LayerMask.GetMask("Default"); 
        Physics.Raycast(this.transform.position + new Vector3(0, -0.15f, 0), -Vector3.up, out hit, 0.2f, layerMask);

        if (hit.collider != null && hit.collider.CompareTag("walkableSurface") && hit.normal.y > 0.8f)
        {
            //Debug.Log("on ground");
            return true;
        }
        //Debug.Log("in air");
        return false;
    }

    protected virtual void OnTriggerStay(Collider col)
    {
        if(levelEnded || gameOver || dead){
            return;
        }
        if(col.gameObject.CompareTag("goal"))
        {
            controller.AgentEscaped();
        }
        if(col.gameObject.CompareTag("theDeadZone"))
        {
            controller.AgentHasFallen();
            DieInstantly();
        }
        if(col.gameObject.CompareTag("target"))
        {
            Destroy(col.gameObject);
            //Debug.Log("hit target");
            AddTimeWeightedReward(0.25f);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        float dot = Vector3.Dot(transform.up, Vector3.up); // Compare "up" vectors
        if (collision.gameObject.CompareTag("walkableSurface") && dot < 0)
        {
            HitHead();
        }
    }

    public void HitHead(){
        controller.AgentHitHead();
        DieInstantly();
    }
    
    public void JumpedTooMuch(){
        controller.JumpedTooMuch();
        DieInstantly();
    }

    public virtual void DieInstantly()
    { //Really wondering if we can track who did damage recently and reward them the kill hp! We could use TakeDamage to take the source unit and if it's any of the pushing types give them a reward?
        dead = true;
        if (levelEnded || gameOver || dead) { return; }
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
        discreteActionsOut[2] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    public void BeginGameEnded(){
        gameOver = true;
        NotifyAgentDone(DoneReason.DoneCalled);
    }

    public override void OnEpisodeBegin()
    {
        numberOfJumps = 0;
        rewardEarnedThisRound = 0;
        dead = false;
        levelEnded = false;
        gameOver = false;
        insideDeathWall = false;
        goalOpen = false;
        ResetVisitedLocations();
        previousCell = new Vector2Int(9999,9999);
    }

    private void Update()
    {
        float rampMultiplier = Mathf.Clamp01(totaltimeElapsed / timeUntilMaxTimeDelayedRewardMulti);
        timeDelayedRewardMulti = Mathf.Lerp(0.5f, 1f, rampMultiplier);
        if(!dead && !gameOver){
            if (numberOfJumps >= jumpLimit)
            {
                JumpedTooMuch();
            }
            RewardForNewLocation();
            grounded = DoRealGroundCheck();
        }
    }

    public void AddTimeWeightedReward(float reward){
        var moddedReward = reward * timeDelayedRewardMulti;
        rewardEarnedThisRound += moddedReward;
        AddReward(moddedReward);
    }

    public void AddNonTimeWeightedReward(float reward){
        rewardEarnedThisRound += reward;
        AddReward(reward);
    }
}

   

