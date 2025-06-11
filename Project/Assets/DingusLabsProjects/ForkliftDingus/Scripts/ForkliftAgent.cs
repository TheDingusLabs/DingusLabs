//Put this script on your blue cube.

using System.Collections;
using UnityEngine;
using Unity.MLAgents;
//using Unity.Barracuda;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgentsExamples;
using System.Collections.Generic;

public class ForkliftAgent : Agent
{
    public GameObject spawnArea;
    public GameObject ground;
    public GameObject levelMasta;
    private ForkliftLevelmasta levelMastaScript;
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

    public GameObject spikes;
    public GameObject spikesTop;
    public GameObject spikesBottom;

    public float spikesMoveInc = 9999f;
    private float spikesMoveIncCooldown = 1.7f;
    public float spikesMoveTime = 1f;
    public int spikesMovingDirection = 0;
    public int spikesCurrentPos = -1;

    public float torqueMag = 0f;
    private float enemyHitRecency = 0f;
    private bool keepLevel = false;

    // public delegate void DingusSpotted(GameObject spotter);
    // public static event DingusSpotted foundDingus;

    EnvironmentParameters m_ResetParams;

    public override void Initialize()
    {
        //var newRoom = Instantiate(rooms[currentRoomNo], spawnArea.transform.position + new Vector3(0f, 0f,0), spawnArea.transform.rotation, this.transform.parent.parent);
        //currentRoom = newRoom.GetComponent<Room>();
        //m_WallJumpSettings = FindObjectOfType<WallJumpSettings>();
        goalDistTimer = Random.Range(-0.5f, 0.5f);
        levelMastaScript = levelMasta.GetComponent<ForkliftLevelmasta>();

        m_AgentRb = GetComponent<Rigidbody>();
        m_SpawnAreaBounds = spawnArea.GetComponent<Collider>().bounds;

        //todo this was set to false
        spawnArea.SetActive(true);

        m_ResetParams = Academy.Instance.EnvironmentParameters;
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

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var agentPos = m_AgentRb.position - ground.transform.position;

        //todo: ensure this is useful, now detecting velocity of the agent as well
        sensor.AddObservation(m_AgentRb.linearVelocity / 10f);
        sensor.AddObservation(m_AgentRb.angularVelocity / 10f);
        //sensor.AddObservation(m_AgentRb.rotation);
        //these already existed
        sensor.AddObservation(distanceToClosestGoal / 10f);
        sensor.AddObservation(spikesMovingDirection);
        sensor.AddObservation(spikesCurrentPos);
        sensor.AddObservation(spikesMoveInc >= spikesMoveIncCooldown);

        //sensor.AddObservation(agentPos / 20f);
        //sensor.AddObservation(m_AgentRb.rotation);
        //sensor.AddObservation(DoGroundCheck(true) ? 1 : 0);
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        if(levelEnded){
            return;
        }

        //AddReward(-0.00005f);
        var smallGrounded = DoGroundCheck(true);
        var largeGrounded = DoGroundCheck(false);

        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;
        var dirToGoForwardAction = act[0];
        var rotateDirAction = act[1];
        var spikeAction = act[2];

        if (dirToGoForwardAction == 1)
            dirToGo = (largeGrounded ? 1f : 0.1f) * 1f * transform.forward;
        else if (dirToGoForwardAction == 2)
            dirToGo = (largeGrounded ? 1f : 0.1f) * -1f * transform.forward * 0.8f;
        // if (rotateDirAction == 1)
        //     rotateDir = transform.up * -0.5f;
        // else if (rotateDirAction == 2)
        //     rotateDir = transform.up * 0.5f;

        var rotate = new Vector3(0, 0, 0);

        if (rotateDirAction == 1){
            rotate = new Vector3(0, -1.1f, 0);
        }
        else if (rotateDirAction == 2)
        {
            rotate = new Vector3(0, 1.1f, 0);
        }

        rotate = rotate * (dirToGoForwardAction > 0 ? 0.8f : 1f);

        if(this.transform.GetComponent<Rigidbody>().GetAccumulatedTorque().magnitude < 0.2f && this.transform.GetComponent<Rigidbody>().GetAccumulatedForce().magnitude < 0.5f){
            rotate = rotate * 1.1f;
        }

        this.transform.GetComponent<Rigidbody>().AddRelativeTorque(rotate * 2.7f, ForceMode.Impulse);

        if (!largeGrounded)
        {
            m_AgentRb.AddForce(Vector3.down * fallingForce, ForceMode.Acceleration);
        }

        if(spikeAction > 0 && spikesMovingDirection == 0 && spikesMoveInc >= spikesMoveIncCooldown){

            var raytwo = new Ray(m_AgentRb.position + new Vector3(0f,-0.5f,0f), m_AgentRb.transform.forward);
            RaycastHit hittwo;

            if(spikesCurrentPos == -1){
                spikesMoveInc = 0;
                spikesMovingDirection = 1;
                    
                if (Physics.SphereCast(raytwo, 0.4f, out hittwo, 2.25f))
                {
                    if (hittwo.collider != null && hittwo.collider.tag == "loot")
                    {
                        if(THEOne){Debug.Log("okay I'm hitting loot");}
                        //AddReward(-0.3f * Time.deltaTime);
                    }
                    else{
                        if(THEOne){Debug.Log("I am being punished");}
                        AddReward(-0.2f);
                    }
                }
                else{
                    if(THEOne){Debug.Log("I am being punished");}
                    AddReward(-0.2f);
                }

            }
            else{
                spikesMoveInc = 0;
                spikesMovingDirection = -1;


            }
        }

        m_AgentRb.AddForce(dirToGo * runForce, ForceMode.Impulse);

        jumpCounter += Time.fixedDeltaTime;


        var ray = new Ray(m_AgentRb.position, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.tag == "theDeadZone")
            {
                if(THEOne){Debug.Log("see yah boi");}
                AddReward(-0.3f * Time.deltaTime);
            }
            
        }

        //atempting to punish just having the spikes up and toggling them for no reason
        // var raytwo = new Ray(m_AgentRb.position + new Vector3(0f,1.4f,0f), m_AgentRb.transform.forward);
        // RaycastHit hittwo;
        // if (Physics.SphereCast(raytwo, 0.4f, out hittwo, 2f))
        // {
        //     if (hittwo.collider != null && hittwo.collider.tag == "loot")
        //     {
        //         //if(THEOne){Debug.Log("okay I'm hitting loot");}
        //         //AddReward(-0.3f * Time.deltaTime);
        //     }
        //     else if(hittwo.collider != null)
        //     {
        //         //if(THEOne){Debug.Log(hittwo.collider.tag);}
        //         if(spikesCurrentPos == 1){
        //             AddReward(-0.1f);
        //         }
        //     }

        //     // if (hit.collider.tag == "loot")
        //     // {
        //     //     if(THEOne){Debug.Log("okay I'm hitting loot");}
        //     //     //AddReward(-0.3f * Time.deltaTime);
        //     // }
            
        // }
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

    // Detect when the agent hits the death wall
    void OnTriggerStay(Collider col)
    {
        if(levelEnded){
            return;
        }

        if (col.gameObject.CompareTag("enemy"))
        {
            if(enemyHitRecency > 2f){
                enemyHitRecency = 0f;
                AddReward(-0.3f);
            }
        }
        if (col.gameObject.CompareTag("deathWall"))
        {
            InitiateLevelEndPrep("boy played with deadly stuff!",false,-0.5f);
        }
        if (col.gameObject.CompareTag("theDeadZone"))
        { 
            InitiateLevelEndPrep("goodbye Dingus",false,-0.5f);
        }
        //if (col.gameObject.CompareTag("fanBlade"))
        //{
        //    m_AgentRb.AddForce((-transform.forward * Random.Range(2f, 5f) + Vector3.up * Random.Range(2f, 5f)) * 5, ForceMode.VelocityChange);
        //}
    }

    public void LootHitGoal(){
        if(levelEnded){
            return;
        }

        CheckLevelCompletion();
        
        AddReward(2f + 2*(( countDownTime > 1 ? countDownTime : 1) / maxTime));
        
    }

    public float AddLevelInc(){
        return levelMastaScript.currentRoomNo/6;
    }

    public void GreatestLootHeightIncreased(float inc){
        //AddReward(inc/5f);
    }

    public bool CheckLevelCompletion(){
        if(levelEnded){
            return true;
        }

        var currentRoom = levelMastaScript.currentRoom;

        if ((getClosestGoalDistance() >= 7f || (spikesMoveInc >= 0.3f && spikesMovingDirection == -1) ) && currentRoom.isComplete() && maxTime - countDownTime > 3)
        {
            if(timeWinPanel.activeSelf)
            {
                if(successDisplay.text == "" && levelMastaScript.currentRoom.GetComponent<ForkliftRoom>().isComplete() && levelMastaScript.currentRoomNo == levelMastaScript.rooms.Count -1){
                    successDisplay.text = "success at: " + (int)totalTrainingTime + "s";
                }
            }

            AddReward(1.5f);
            InitiateLevelEndPrep("Success!", true);
        }
        return levelEnded;
    }

    public void LootLeftGoal(){
        if(levelEnded){
            return;
        }
        else{
            AddReward(-4f);
        }
    }

    public void LootCloserToGoal(Vector3 lootPos){
        if(levelEnded){
            return;
        }

        var ray = new Ray(m_AgentRb.position, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.tag == "theDeadZone")
            {
                return;
            }
            
        }

        if(Vector3.Distance(lootPos, this.transform.position) <= 3f){
            AddReward(0.01f);
            if(spikesCurrentPos == 1 || spikesMovingDirection == 1)
            {
                if (THEOne){Debug.Log("working as intended");}
                AddReward(0.2f);
            }
        }
    }

    public void PlayerCloserToLoot(){
        //this might be encouraging always sticking close to the loot
        if(levelEnded){
            return;
        }
        //AddReward(0.05f);
    }

    public void FirstTouch(){
        if(levelEnded){
            return;
        }
        AddReward(0.2f);
    }

    public void LeaveItAlone(){
        if(levelEnded){
            return;
        }
        AddReward(-0.2f);
    }

    public void LootIsDead(){
        if(levelEnded){
            return;
        }
        InitiateLevelEndPrep("you needed that Dingus",false,-0.1f);
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
        if(weined && THEOne && levelMastaScript.currentRoomNo < levelMastaScript.rooms.Count - 1 && !keepLevel){
            CallMajorTom();

            // //for farming one level!
            // levelMastaScript.currentRoom.RestartRoom();
            // EndEpisode();
        }
        else{
            levelMastaScript.currentRoom.RestartRoom();
            EndEpisode();
        }
    }

    public void CallMajorTom()
    {
        var agents = this.transform.parent.parent.GetComponentsInChildren<ForkliftAgent>();
            foreach (var agent in agents)
            {   
                if(!agent.THEOne)
                {
                    agent.StartNextLevel();
                }
                
            }
        StartNextLevel();
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
        enemyHitRecency = 0f;
        //m_SpawnAreaBounds = spawnArea.GetComponent<Collider>().bounds;

        // var randomPosX = Random.Range(-m_SpawnAreaBounds.extents.x * 1f,
        //     m_SpawnAreaBounds.extents.x);
        // var randomPosZ = Random.Range(-m_SpawnAreaBounds.extents.z * 1f,
        //     m_SpawnAreaBounds.extents.z);
        //     Debug.Log(randomPosX);
        //     Debug.Log(randomPosZ);
        // transform.localPosition = spawnArea.transform.localPosition + new Vector3(randomPosX, 2f, randomPosZ);
        spawnArea = levelMastaScript.currentRoom.playerSpawnArea;

        Bounds bounds = spawnArea.GetComponent<Collider>().bounds;
        float offsetX = Random.Range(-bounds.extents.x, bounds.extents.x);
        float offsetZ = Random.Range(-bounds.extents.z, bounds.extents.z);
        // Debug.Log(offsetX);
        // Debug.Log(offsetZ);
        transform.position = spawnArea.transform.position +  new Vector3(offsetX, 2f, offsetZ);


        //transform.rotation = spawnArea.transform.rotation;
        //transform.rotation = Quaternion.AngleAxis(levelMastaScript.currentRoom.spawnDir, Vector3.up);
        //after adding code to handle specific spawn directions for each room I decided to randomise it for better training after all
        this.transform.rotation = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);

        m_AgentRb.linearVelocity = default(Vector3);
        //ignore spawndir use the spawn point instead I reckon
        //this.transform.rotation = Quaternion.AngleAxis(Random.Range(spawnDir, spawnDir), Vector3.up);

        spikesCurrentPos = -1;
        spikesMoveInc = 9999f;
        spikesMovingDirection = 0;
        spikes.transform.position = spikesBottom.transform.position;
        

        distanceToClosestGoal = getClosestGoalDistance();
    }

    private void FixedUpdate()
    {
        if(levelEnded){
            return;
        }
    }

    private float getClosestGoalDistance()
    {
        return Vector3.Distance(levelMastaScript.currentRoom.goal.transform.position, transform.position);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.K))
        {
            keepLevel = !keepLevel;
        }

        if (Input.GetKeyUp(KeyCode.N) && THEOne && levelMastaScript.currentRoomNo < levelMastaScript.rooms.Count - 1 && !keepLevel)
        {
            CallMajorTom();
        }

        if (levelEnded){
            return;
        }
        enemyHitRecency += Time.deltaTime;
        countDownTime -= Time.deltaTime;
        totalTrainingTime += Time.deltaTime;
        if(timetrainpanel.activeSelf){
            timeTrainedDisplay.text = "Total Time Training: " + ((int)totalTrainingTime).ToString() +"s";
        }

        if (countDownTime <= 0.999)
        {
            //AddReward(-0.4f);
            InitiateLevelEndPrep("too dang slow Dingus!");
        }
        else
        {
            countDownDisplay.text = "Time: " +((int)countDownTime).ToString();
        }

        torqueMag = this.transform.GetComponent<Rigidbody>().GetAccumulatedTorque().magnitude;

        if(THEOne && countDownTime - maxTime < -1)
        {
            levelMastaScript.ActivateTheOne(THEOne);
        }

        //removed this, I don't necessarily want to teach it to inch towards the closest goal
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

        spikesMoveInc += Time.deltaTime;
        if(spikesMovingDirection != 0){
            var  toPos = spikesBottom.transform.position;
            var fromPos = spikesTop.transform.position;
            if(spikesMovingDirection == 1){
                fromPos = spikesBottom.transform.position;
                 toPos = spikesTop.transform.position;
            }

            if(spikesMoveInc >= spikesMoveTime){
                spikesMoveInc = spikesMoveTime;
                spikesCurrentPos = spikesMovingDirection;
                spikesMovingDirection = 0;
            }
            spikes.transform.position = Vector3.Lerp(fromPos, toPos, spikesMoveInc/spikesMoveTime);
        }

        if (Input.GetKeyDown("h"))
        {
            Debug.Log("n key was pressed");
            timeWinPanel.SetActive(!timeWinPanel.activeSelf);
            timetrainpanel.SetActive(!timetrainpanel.activeSelf);
        }

        CheckLevelCompletion();
    }
}
