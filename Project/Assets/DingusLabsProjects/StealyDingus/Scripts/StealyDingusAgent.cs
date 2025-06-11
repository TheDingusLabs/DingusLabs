//Put this script on your blue cube.

using System.Collections;
using UnityEngine;
using Unity.MLAgents;
//using Unity.Barracuda;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgentsExamples;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Sentis.Layers;

public class StealyDingusAgent : Agent
{
    public GameObject spawnArea;
    public GameObject ground;
    public GameObject levelMasta;
    private StealyDingusMasta levelMastaScript;
    private float goalDistTimer = 0f; 
    private float distanceToClosestGoal = 20f;
    public bool THEOne = false;
    public bool levelEnded = false;
    Bounds m_SpawnAreaBounds;

    //public float spawnDir = 270f;
    //private float spawnDir = 0f;

    Rigidbody m_AgentRb;

    public float maxVelocity;
    public float maxBackVelocity;
    public float jumpForce;
    public float runForce;
    public float jumpCounter = 999;
    public float jumpCooldown = 0.3f;
    private float maxYThisRun;
    // This is a downward force applied when falling to make jumps look
    // less floaty
    public float fallingForce;
    // Use to check the coliding objects
    public Collider[] hitGroundColliders = new Collider[3];
    Vector3 m_JumpTargetPos;
    Vector3 m_JumpStartingPos;

    public float maxTime = 20.9f;
    float countDownTime = 20.9f;
    float totalTrainingTime = 0f;
    public TMPro.TextMeshPro countDownDisplay;
    public TMPro.TextMeshPro timeTrainedDisplay;
    public TMPro.TextMeshPro successDisplay;
    public TMPro.TextMeshPro levelEndText;

    public GameObject timetrainpanel;
    public GameObject timeWinPanel;

    // public delegate void DingusSpotted(GameObject spotter);
    // public static event DingusSpotted foundDingus;

    EnvironmentParameters m_ResetParams;

    public override void Initialize()
    {
        goalDistTimer = Random.Range(-0.5f, 0.5f);
        levelMastaScript = levelMasta.GetComponent<StealyDingusMasta>();

        m_AgentRb = GetComponent<Rigidbody>();
        m_SpawnAreaBounds = spawnArea.GetComponent<Collider>().bounds;

        //todo this was set to false
        spawnArea.SetActive(true);

        m_ResetParams = Academy.Instance.EnvironmentParameters;

        maxYThisRun = this.transform.position.y;
    }

    /// <summary>
    /// Does the ground check.
    /// </summary>
    /// <returns><c>true</c>, if the agent is on the ground,
    /// <c>false</c> otherwise.</returns>
    /// <param name="smallCheck"></param>
    public bool DoGroundCheck(bool smallCheck)
    {
        return true;
        // if (!smallCheck)
        // {
        //     hitGroundColliders = new Collider[3];
        //     var o = gameObject;
        //     Physics.OverlapBoxNonAlloc(
        //         o.transform.position + new Vector3(0, -0.05f, 0),
        //         new Vector3(0.95f / 2f, 0.5f, 0.95f / 2f),
        //         hitGroundColliders,
        //         o.transform.rotation);
        //     var grounded = false;
        //     foreach (var col in hitGroundColliders)
        //     {
        //         if (col != null && col.transform != transform &&
        //             (col.CompareTag("walkableSurface") || col.CompareTag("block")) )
        //         {
        //             grounded = true; //then we're grounded
        //             break;
        //         }
        //     }
        //     return grounded;
        // }
        // else
        // {
        //     RaycastHit hit;
        //     Physics.Raycast(transform.position + new Vector3(0, -0.05f, 0), -Vector3.up, out hit, 0.2f);

        //     if (hit.collider != null &&
        //         ( hit.collider.CompareTag("walkableSurface") || hit.collider.CompareTag("block") )
        //         && hit.normal.y > 0.95f)
        //     {
        //         return true;
        //     }

        //     return false;
        // }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var agentPos = m_AgentRb.position - ground.transform.position;

        //todo: ensure this is useful, now detecting velocity of the agent as well
        sensor.AddObservation(m_AgentRb.linearVelocity / 10f);
        sensor.AddObservation(m_AgentRb.rotation.x);
        //these already existed
        sensor.AddObservation(distanceToClosestGoal / 10f);
        sensor.AddObservation(agentPos.y / 20f);
        //sensor.AddObservation(levelMastaScript.currentRoomNo);
        //sensor.AddObservation(levelMastaScript.currentRoom.);
        //sensor.AddObservation(DoGroundCheck(true) ? 1 : 0);
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        if(levelEnded){
            return;
        }

        //AddReward(-0.00005f); //negative reward here will encourage getting rid of themself when they can't find stuff instead of exploring
        var smallGrounded = DoGroundCheck(true);
        var largeGrounded = DoGroundCheck(false);

        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;
        var dirToGoForwardAction = act[0];
        var rotateDirAction = act[1];
        //var jumpAction = act[2];

        if (dirToGoForwardAction == 1)
            dirToGo = (largeGrounded ? 1f : 0.6f) * 1f * transform.forward;
        else if (dirToGoForwardAction == 2)
            dirToGo = (largeGrounded ? 1f : 0.6f) * -1f * transform.forward;
        if (rotateDirAction == 1)
            rotateDir = transform.up * -1f;
        else if (rotateDirAction == 2)
            rotateDir = transform.up * 1f;
        // if (jumpAction == 1)
        // {
        //     if (smallGrounded && jumpCounter >= jumpCooldown)
        //     {
        //         jumpCounter = 0;
        //         m_AgentRb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        //     }
        // }

        transform.Rotate(rotateDir, Time.fixedDeltaTime * 300f * 0.85f);

        //capping reverse speed
        if(dirToGoForwardAction == 2){
            m_AgentRb.linearDamping = (m_AgentRb.linearVelocity.magnitude / maxBackVelocity);
        }
        else{
            m_AgentRb.linearDamping = (m_AgentRb.linearVelocity.magnitude / maxVelocity);
        }

        if (!largeGrounded)
        {
            m_AgentRb.AddForce(Vector3.down * fallingForce, ForceMode.Acceleration);
        }

        m_AgentRb.AddForce(dirToGo * runForce, ForceMode.VelocityChange);

        jumpCounter += Time.fixedDeltaTime;
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
        //no jump for you!
        //discreteActionsOut[2] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    // Detect when the agent hits the death wall
    void OnTriggerStay(Collider col)
    {
        if(levelEnded){
            return;
        }

        //the ball's collision is what matters here
        // if (col.gameObject.CompareTag("goal"))
        // {
        //     distanceToClosestGoal = getClosestGoalDistance();
        //     AddReward(1f);
        //     AddReward(countDownTime/maxTime * 0.5f);
        //     col.gameObject.SetActive(false);
        //     //Debug.Log(currentRoom.GetComponent<Room>().isComplete());
        //     if (levelMastaScript.currentRoom.isComplete())
        //     {
        //         if(timeWinPanel.active)
        //         {
        //             if(successDisplay.text == "" && levelMastaScript.currentRoom.GetComponent<PhysicsPlaygroundRoom>().isComplete() && levelMastaScript.currentRoomNo == levelMastaScript.rooms.Count -1){
        //                 successDisplay.text = "success at: " + (int)totalTrainingTime + "s";
        //             }
        //         }

        //         AddReward(3f);
        //         InitiateLevelEndPrep("Success!", true);
        //     }
        // }

        if(col.gameObject.CompareTag("plus")){
                AddReward(0.1f);
                Destroy(col.gameObject);
        }

        if (col.gameObject.CompareTag("deathWall"))
        {
            InitiateLevelEndPrep("boy played with deadly stuff!",false,-0.1f);
        }
        if (col.gameObject.CompareTag("theDeadZone"))
        { 
            InitiateLevelEndPrep("Dingus fell Down!",false,-0.8f);
        }
        //if (col.gameObject.CompareTag("fanBlade"))
        //{
        //    m_AgentRb.AddForce((-transform.forward * Random.Range(2f, 5f) + Vector3.up * Random.Range(2f, 5f)) * 5, ForceMode.VelocityChange);
        //}
    }

    public void BallHitGoal(){
        if(levelEnded){
            return;
        }

        if (levelMastaScript.currentRoom.isComplete())
        {
            if(timeWinPanel.activeSelf)
            {
                if(successDisplay.text == "" && levelMastaScript.currentRoom.GetComponent<StealyDingusRoom>().isComplete() && levelMastaScript.currentRoomNo == levelMastaScript.rooms.Count -1){
                    successDisplay.text = "success at: " + (int)totalTrainingTime + "s";
                }
            }
            AddReward(0.5f);
            var extraReward = countDownTime/maxTime *0.4f;
            if(levelMastaScript.currentRoomNo > 1){
                extraReward += 0.05f*levelMastaScript.currentRoomNo;
            }
            AddReward(extraReward);
            InitiateLevelEndPrep("Success in: " + (-1*(countDownTime - maxTime)).ToString("0.00") + "s", true);
        }
        else{
            //never actually added in the time dependant reward
            //var extraReward = countDownTime/maxTime *0.2f;
            var extraReward = countDownTime/maxTime *0.1f;
            if(levelMastaScript.currentRoomNo > 1){
                extraReward += 0.05f*levelMastaScript.currentRoomNo;
            }
            AddReward(extraReward);
            AddReward(0.3f);
        }
    }

    public void BallCloserToGoal(){
        if(levelEnded){
            return;
        }
        AddReward(0.03f);
    }

    void InitiateLevelEndPrep(string endText, bool success = false, float yourFinalReward = 0f){
        if(levelEnded){
            return;
        }

        levelEnded = true;
        levelEndText.text = endText;
        AddReward(yourFinalReward);
        //levelEndText.color;
        {
            StartCoroutine(
                LevelFinishCountdown(success ? 2 : 1, success)
            );
        }
    }

    /// <summary>
    /// Displays attempt status.
    /// </summary>
    /// <returns>The Enumerator to be used in a Coroutine.</returns>
    /// <param name="time">The time until new level start material will remain.</param>
    IEnumerator LevelFinishCountdown(float time, bool weined)
    {
        yield return new WaitForSeconds(time);
        // if(weined && THEOne && levelMastaScript.currentRoomNo < levelMastaScript.rooms.Count - 1){
        //     CallMajorTom();
        // } //because it selects from a variety of rooms
        if(weined){
            //CallMajorTom();
            levelMastaScript.MarkRoomCompleted(levelMastaScript.currentRoomNo);
            StartNextLevel();
        }
        else{
            // levelMastaScript.currentRoom.RestartRoom();
            // EndEpisode();
            //CallMajorTom();
            StartNextLevel();
        }
    }

    public void CallMajorTom()
    {
        this.transform.parent.parent.gameObject.GetComponent<StealyDingusMajorTom>().NextLevel();
    }

    public void StartNextLevel()
    {
        levelMastaScript.StartNextLevel();

        spawnArea.transform.position = levelMastaScript.currentRoom.playerSpawnArea.transform.position;
        EndEpisode();
    }

    public override void OnEpisodeBegin()
    {
        levelEndText.text = "";
        levelEnded = false;
        levelMastaScript.currentRoom.RestartRoom();
        levelMastaScript.ActivateTheOne(THEOne);
        countDownTime = maxTime;
        countDownDisplay.text = "Time: " + ((int)countDownTime).ToString();
        var randomPosX = Random.Range(-m_SpawnAreaBounds.extents.x * 1f,
            m_SpawnAreaBounds.extents.x);
        var randomPosZ = Random.Range(-m_SpawnAreaBounds.extents.z * 1f,
            m_SpawnAreaBounds.extents.z);
        transform.localPosition = spawnArea.transform.localPosition + new Vector3(randomPosX, 0f, randomPosZ);
        maxYThisRun = this.transform.position.y;
        //transform.rotation = spawnArea.transform.rotation;
        //transform.rotation = Quaternion.AngleAxis(levelMastaScript.currentRoom.spawnDir, Vector3.up);
        //after adding code to handle specific spawn directions for each room I decided to randomise it for better training after all
        this.transform.rotation = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);

        m_AgentRb.linearVelocity = default(Vector3);
        //ignore spawndir use the spawn point instead I reckon
        //this.transform.rotation = Quaternion.AngleAxis(Random.Range(spawnDir, spawnDir), Vector3.up);

        distanceToClosestGoal = getClosestGoalDistance();
    }

    private void FixedUpdate()
    {

    }

    private float getClosestGoalDistance()
    {
        float distance = 20f;
        if(levelMastaScript.currentRoom.pointcubes.Count  >= 1)
        {
            foreach(var loot in levelMastaScript.currentRoom.lootList)
            {
                float goalDist = Vector3.Distance(loot.transform.position, transform.position);
                if (goalDist < distance)
                {
                    distance = goalDist;
                }
            }
        }

        return distance;
    }

    private void Update()
    {
        if(levelEnded){
            return;
        }

        countDownTime -= Time.deltaTime;
        totalTrainingTime += Time.deltaTime;
        if(timetrainpanel.activeSelf){
            timeTrainedDisplay.text = "Total Time Training: " + ((int)totalTrainingTime).ToString() +"s";
        }

        if (countDownTime <= 0.999)
        {
            //AddReward(-0.4f); //we risk Dingus taking a different way out maybe if we put this penalty in
            InitiateLevelEndPrep("too dang slow Dingus!");
        }
        else
        {
            countDownDisplay.text = "Time: " +((int)countDownTime).ToString();
        }

        if(THEOne && countDownTime - maxTime < -1)
        {
            levelMastaScript.ActivateTheOne(THEOne);
        }

        if(this.transform.position.y > maxYThisRun)
        {
            AddReward( (this.transform.position.y - maxYThisRun) * 0.08f );
            maxYThisRun = this.transform.position.y;
        }

        //this is a reward for the agent moving closer, not the ball itself
        // goalDistTimer += Time.deltaTime;
        // if (goalDistTimer > 0.2f)
        // {
        //     var calcGoalDist = getClosestGoalDistance();
        //     goalDistTimer = Random.Range(-0.5f, 0.5f);
        //     if (calcGoalDist < distanceToClosestGoal - 1f)
        //     {
        //         distanceToClosestGoal = calcGoalDist;
        //         //removed this, I don't necessarily want to teach it to inch towards the closest goal
        //         AddReward(0.02f);
        //     }
        // }

        if (Input.GetKeyDown("h"))
        {
            Debug.Log("n key was pressed");
            timeWinPanel.SetActive(!timeWinPanel.activeSelf);
            timetrainpanel.SetActive(!timetrainpanel.activeSelf);
        }
    }
}
