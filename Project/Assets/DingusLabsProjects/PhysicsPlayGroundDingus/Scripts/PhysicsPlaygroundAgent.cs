//Put this script on your blue cube.

using System.Collections;
using UnityEngine;
using Unity.MLAgents;
//using Unity.Barracuda;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgentsExamples;
using System.Collections.Generic;

public class PhysicsPlaygroundAgent : Agent
{
    public GameObject spawnArea;
    public GameObject ground;
    public GameObject levelMasta;
    private PhysicsPlaygroundmasta levelMastaScript;
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
    // This is a downward force applied when falling to make jumps look
    // less floaty
    public float fallingForce;
    public float progress = 0.000001f;
    public float bestProgress = 0.000001f;
    public float maxScore = 0.000001f;
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
    public TMPro.TextMeshPro CurrentProgressText;
    public TMPro.TextMeshPro BestProgressText;

    public GameObject alert;

    public GameObject timetrainpanel;
    public GameObject timeWinPanel;

    public Ball ball;

    public List<GameObject> tpPoints;
    private int currentTpPoint = 0;

    // public delegate void DingusSpotted(GameObject spotter);
    // public static event DingusSpotted foundDingus;

    EnvironmentParameters m_ResetParams;

    public override void Initialize()
    {
        //var newRoom = Instantiate(rooms[currentRoomNo], spawnArea.transform.position + new Vector3(0f, 0f,0), spawnArea.transform.rotation, this.transform.parent.parent);
        //currentRoom = newRoom.GetComponent<Room>();
        //m_WallJumpSettings = FindObjectOfType<WallJumpSettings>();
        goalDistTimer = Random.Range(-0.5f, 0.5f);
        levelMastaScript = levelMasta.GetComponent<PhysicsPlaygroundmasta>();

        m_AgentRb = GetComponent<Rigidbody>();
        m_SpawnAreaBounds = spawnArea.GetComponent<Collider>().bounds;

        //todo this was set to false
        spawnArea.SetActive(true);

        m_ResetParams = Academy.Instance.EnvironmentParameters;
    }

    public void OnStart(){
        maxScore = levelMastaScript.currentRoom.GetMaxScore();

        tpPoints = levelMastaScript.currentRoom.GetTPPoints();
        ball = levelMastaScript.currentRoom.GetBall();
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
        //sensor.AddObservation(m_AgentRb.rotation);
        //these already existed
        sensor.AddObservation(distanceToClosestGoal / 10f);
        //sensor.AddObservation(agentPos / 20f);
        //sensor.AddObservation(m_AgentRb.rotation);
        //sensor.AddObservation(DoGroundCheck(true) ? 1 : 0);
    }

    public virtual void MoveAgent(ActionSegment<int> act)
    {
        if(levelEnded){
            return;
        }
        // removing this negative reward, I think it encourages, when really struggling to find extra rewards, quick suicide, instead let's give a time bonus to getting rewards quickly
        //AddReward(-0.0003f);
        //reward survival?
        AddReward(-0.00005f);
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

        transform.Rotate(rotateDir, Time.fixedDeltaTime * 300f * 0.65f);

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

        if (col.gameObject.CompareTag("deathWall"))
        {
            InitiateLevelEndPrep("boy played with deadly stuff!",false,-0.1f);
        }
        if (col.gameObject.CompareTag("theDeadZone"))
        { 
            InitiateLevelEndPrep("Try again Dingus!",false,-0.1f);
        }
        //if (col.gameObject.CompareTag("fanBlade"))
        //{
        //    m_AgentRb.AddForce((-transform.forward * Random.Range(2f, 5f) + Vector3.up * Random.Range(2f, 5f)) * 5, ForceMode.VelocityChange);
        //}
    }

    public void BallDied(){
        InitiateLevelEndPrep("You needed that!",false,-0.1f);
    }

    public void BallHitGoal(){
        if(levelEnded){
            return;
        }

        progress = levelMastaScript.currentRoom.GetCurrentScore();

        if (levelMastaScript.currentRoom.isComplete())
        {
            if(timeWinPanel.activeSelf)
            {
                if(successDisplay.text == "" && levelMastaScript.currentRoom.GetComponent<PhysicsPlaygroundRoom>().isComplete() && levelMastaScript.currentRoomNo == levelMastaScript.rooms.Count -1){
                    successDisplay.text = "success at: " + (int)totalTrainingTime + "s";
                }
            }

            AddReward(2.5f);
            InitiateLevelEndPrep("Success!", true);
        }
        else{
            AddReward(0.5f);
        }
    }

    public void BallHitBoulder(){
        AddReward(-1f);
    }

    public void BallCloserToGoal(){
        if(levelEnded){
            return;
        }
        AddReward(0.1f);
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
        if(weined && THEOne && levelMastaScript.currentRoomNo < levelMastaScript.rooms.Count - 1){
            CallMajorTom();
        }
        else{
            levelMastaScript.currentRoom.RestartRoom();
            EndEpisode();
        }
    }

    public void CallMajorTom()
    {
        this.transform.parent.parent.gameObject.GetComponent<PhysicsPlaygroundMajorTom>().NextLevel();
    }

    public void StartNextLevel()
    {
        levelMastaScript.StartNextLevel();

        spawnArea.transform.position = levelMastaScript.currentRoom.playerSpawnArea.transform.position;
        EndEpisode();
    }

    public override void OnEpisodeBegin()
    {
        currentTpPoint = 0;
        levelEndText.text = "";
        levelEnded = false;
        levelMastaScript.currentRoom.RestartRoom();
        levelMastaScript.ActivateTheOne(THEOne);
        bestProgress = progress >= bestProgress ? progress : bestProgress;
        progress = levelMastaScript.currentRoom.GetCurrentScore();
        countDownTime = maxTime;
        countDownDisplay.text = "Time:" + ((int)countDownTime).ToString();
        var randomPosX = Random.Range(-m_SpawnAreaBounds.extents.x * 1f,
            m_SpawnAreaBounds.extents.x);
        var randomPosZ = Random.Range(-m_SpawnAreaBounds.extents.z * 1f,
            m_SpawnAreaBounds.extents.z);
        transform.localPosition = spawnArea.transform.localPosition + new Vector3(randomPosX, 0f, randomPosZ);
        //transform.rotation = spawnArea.transform.rotation;
        //transform.rotation = Quaternion.AngleAxis(levelMastaScript.currentRoom.spawnDir, Vector3.up);
        //after adding code to handle specific spawn directions for each room I decided to randomise it for better training after all
        this.transform.rotation = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);

        m_AgentRb.linearVelocity = default(Vector3);
        //ignore spawndir use the spawn point instead I reckon
        //this.transform.rotation = Quaternion.AngleAxis(Random.Range(spawnDir, spawnDir), Vector3.up);

        distanceToClosestGoal = getClosestGoalDistance();
        maxScore = levelMastaScript.currentRoom.GetMaxScore();
        if(maxScore < 1f){ //this is because my code sucks
            maxScore = 9999f;
        }
        tpPoints = levelMastaScript.currentRoom.GetTPPoints();
        ball = levelMastaScript.currentRoom.GetBall();
    }

    public bool GotSpotted(GameObject spotter){
        if (!levelEnded && (countDownTime - maxTime < -1))
        {
            AddReward(-0.1f);
            var alertIcon = Instantiate(alert, spotter.transform.position + new Vector3(0f, 1.2f, 0f), Quaternion.identity);
            Destroy(alertIcon, 1f);

            InitiateLevelEndPrep("Busted");
            return true;
        }
        return false;

    }

    private void FixedUpdate()
    {
        if(levelEnded){
            return;
        }

        countDownTime -= Time.deltaTime;
        totalTrainingTime += Time.deltaTime;
        timeTrainedDisplay.text = "Training Time:" + ((int)totalTrainingTime).ToString() +"s";

        CurrentProgressText.text = "Done:" + ((int) (progress/maxScore*100f) ).ToString() +"%";
        BestProgressText.text = "Best:" + ((int) (bestProgress/maxScore*100f) ).ToString() +"%";
        if(timetrainpanel.activeSelf){

        }

        if (countDownTime <= 0.999)
        {
            AddReward(-0.4f);
            InitiateLevelEndPrep("too dang slow Dingus!");
        }
        else
        {
            countDownDisplay.text = "Time:" +((int)countDownTime).ToString();
        }
    }

    private float getClosestGoalDistance()
    {
        float distance = 20f;
        if(levelMastaScript.currentRoom.pointcubes.Count  >= 1)
        {
            foreach(var goal in levelMastaScript.currentRoom.pointcubes)
            {
                float goalDist = Vector3.Distance(goal.transform.position, transform.position);
                if (goalDist < distance)
                {
                    distance = goalDist;
                }
            }
        }

        return distance;
    }

    private void TpTime(){

        if(tpPoints.Count > 0){
            if(tpPoints.Count - 1 < currentTpPoint){
                currentTpPoint++;
            }
            else{
                currentTpPoint = 0;
            }
            Tp(currentTpPoint);
        }
    }

    private void Tp(int tpNo){
        this.transform.position = tpPoints[tpNo].transform.position;
        ball.transform.position = this.transform.position + this.transform.forward * 1f;
    }

    private void Update()
    {
        if(levelEnded){
            return;
        }

        if(THEOne && countDownTime - maxTime < -1)
        {
            levelMastaScript.ActivateTheOne(THEOne);
        }

        //removed this, I don't necessarily want to teach it to inch towards the closest goal
        goalDistTimer += Time.deltaTime;
        if (goalDistTimer > 0.2f)
        {
            var calcGoalDist = getClosestGoalDistance();
            goalDistTimer = Random.Range(-0.5f, 0.5f);
            if (calcGoalDist < distanceToClosestGoal - 1f)
            {
                distanceToClosestGoal = calcGoalDist;
                //removed this, I don't necessarily want to teach it to inch towards the closest goal
                AddReward(0.02f);
            }
        }

        if (Input.GetKeyDown("h"))
        {
            //Debug.Log("n key was pressed");
            timeWinPanel.SetActive(!timeWinPanel.activeSelf);
            timetrainpanel.SetActive(!timetrainpanel.activeSelf);
        }

        if (Input.GetKeyUp("k"))
        {
            //Debug.Log("k key was pressed");
            TpTime();
        }

        progress = levelMastaScript.currentRoom.GetCurrentScore();
    }
}
