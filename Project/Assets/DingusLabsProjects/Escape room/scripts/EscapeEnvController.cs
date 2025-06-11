using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using Unity.Mathematics;
using Unity.MLAgents;
using UnityEngine;
using Random = UnityEngine.Random;

    [System.Serializable]
    public class GameObjectList
    {
        public List<GameObject> levels = new List<GameObject>(); // Single list of GameObjects
    }

public class EscapeEnvController : MonoBehaviour
{
    /// <summary>
    /// Max Academy steps before this platform resets
    /// </summary>
    /// <returns></returns>
    //[Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;

    public float battleTime = 0f;
    public float maxBattleTime = 60f;
    //public float currentMaxBattleTime;

    private float totalTrainingTime = 0f;

    /// <summary>
    /// The area bounds.
    /// </summary>

    /// <summary>
    /// We will be changing the ground material based on success/failue
    /// </summary>

    //List of Agents On Platform
    public GameObject centreLocation;

    //private SimpleMultiAgentGroup m_BlueAgentGroup;

    private int m_ResetTimer;
    private bool isGameOver = false;

    public TextMeshPro gametimeText;
    public TextMeshPro totalTrainingText;
    public TextMeshPro levelEndText;
    private bool entireGameWon = false;
    public TextMeshPro entireGameWonText;
    private float numberOfAttempts = -1;
    public TextMeshPro attemptsText;
    public TextMeshPro rewardsEarnedText;
    public List<LevelSwitch> switches = new List<LevelSwitch>();
    public List<EscapeBlock> blocks = new List<EscapeBlock>();
    public EscapeAgent agent;
    private List<List<GameObject>> levelGroups = new List<List<GameObject>>(); // List of lists for level prefabs
    private Dictionary<GameObject, bool> levelStates; // Tracks completion state of each level
    
    public GameObject currentLevel; // The current level being played
    private GameObject spawnedLevel;
    private Dictionary<GameObject, float> initialDistances; // Tracks initial distances to goals
    private Dictionary<GameObject, float> rewardsEarned; // Tracks rewards earned for each goal
    private Dictionary<GameObject, float> initialMaxBlockDistances;
    private Dictionary<GameObject, float> minSwitchDistances; // Dictionary to track the minimum recorded distance of any block to each heavy switch
    private float initialGoalDistance;
    private float goalRewardsEarned;
    private GameObject spawnArea;
    //private bool goalUnlocked = false;
    public EscapeDoor escapeDoor;

    private float timeRewardpool = 10f;

    private float maxCompletion = 0f;

    public List<GameObjectList> levelsList = new List<GameObjectList>(); // List of Lists

    private int currentTierIndex = 0;
    public int maxTierIndex = -1;
    public int startingTierIndex = 0;
    public bool repeatAtMaxTier = false;
    private HashSet<GameObject> completedLevelsInTier = new HashSet<GameObject>();

    void Start()
    {
        currentTierIndex = startingTierIndex;
        for(int i = 0; i < levelsList.Count(); i++){
            levelGroups.Add(levelsList[i].levels);
        }

        switches = GetComponentsInChildren<LevelSwitch>().ToList();//GetChildrenByName(transform, "Goal");
        blocks = GetComponentsInChildren<EscapeBlock>().ToList();
        agent = GetComponentsInChildren<EscapeAgent>().FirstOrDefault();

        var newList = new List<float>();
        EndScene();
        BeginScene();
    }

    // Method to check if a certain index exists in the list
    bool IndexExists<T>(List<T> list, int index)
    {
        return index >= 0 && index < list.Count;
    }

    void Update(){
        totalTrainingTime += Time.deltaTime;
        battleTime -= Time.deltaTime;

        // Convert seconds to minutes and seconds
        int minutes = Mathf.FloorToInt(totalTrainingTime / 60);
        int seconds = Mathf.FloorToInt(totalTrainingTime % 60);
        // Format and display the time as "MM:SS"
        string timeFormatted = string.Format("{0:00}:{1:00}", minutes, seconds);
        totalTrainingText.text = "Training Time:" + timeFormatted;
        rewardsEarnedText.text = "Reward: " + agent.rewardEarnedThisRound.ToString("F2");
        CalculateIsGameOver();
        if(isGameOver){return;}

        RewardNearingGoals();
        RewardNearingTheGoalAfterOpeningIt();
        rewardMovingBlocks();

        agent.goalOpen = escapeDoor.open;

        gametimeText.text = "Time: " + battleTime.ToString("0");
        if(!isGameOver){
            //agent.AddTimeWeightedReward(Time.deltaTime * -0.002f);
            //agent.dirVectorToGoal = GetNormalizedDirectionToNearestActiveGoal();
            agent.goalsremaining = switches.Count();

            timeRewardpool -= Time.deltaTime;
            
        }

        //debug for skipping level groups
        if (Input.GetKey(KeyCode.N))
        {

            var currentTier = levelsList[currentTierIndex].levels;

            foreach (var level in currentTier)
            {
                completedLevelsInTier.Add(level);
            }

            Debug.Log($"[Debug] Skipped tier {currentTierIndex}, all levels marked complete.");

            InitGameOver();
        }

    }

    void CalculateIsGameOver() {
        if(isGameOver) {return;}
        if(agent.dead){
            InitGameOver();
            agent.ObservedDied();
            levelEndText.text = "Death is no escape Dingus";
            return;
        }
        if (battleTime <= 0)
        {
            InitGameOver();
            agent.observeTimedOut();
            levelEndText.text = "Time's up!";
            return;
        }
        if(calculateCompletionPercent() >= 100){
            //TODO - add a reward for the first time the door opens, punish closing it again?
            if(escapeDoor.previouslyOpened == false) {
                float distance = Vector3.Distance(agent.gameObject.transform.position, escapeDoor.gameObject.transform.position);
                initialGoalDistance = distance;
                agent.AddTimeWeightedReward(1.5f);
                agent.AddTimeWeightedReward(battleTime/maxBattleTime*2.0f);
            }
            escapeDoor.Open();
            return;
        }
        else{
            if(escapeDoor.open){
                //agent.AddTimeWeightedReward(-0.2f);
                escapeDoor.Close();
            }  
        }
    }

    public void AgentEscaped(){
        if(!isGameOver && !agent.dead){
            InitGameOver(true);
            levelEndText.text = "Escaped!";
        }
    }

    public void hitSwitch(bool firstTurnOn, bool heavy){
        if(firstTurnOn){
            //Debug.Log("congrats on first turn on!");
            //agent.AddTimeWeightedReward(1f/(float)switches.Count() * 2);
            agent.AddTimeWeightedReward(0.75f + (heavy ? 0.125f : 0f));
        }
    }

    public void TurnedHeavySwitchOff(){
        agent.AddTimeWeightedReward(-0.57f);
    }

    public void PlayerPushedBlockToRewardZone(){
        if(isGameOver) {return;}
        agent.AddTimeWeightedReward(0.65f);
    }

    public void BlockOffTheEdge(){
        if(isGameOver) {return;}
        agent.ObservedDied();
        agent.AddTimeWeightedReward(-1f*battleTime/maxBattleTime);
        levelEndText.text = "you needed that Dingus";
        InitGameOver();
    }

    public void AgentHasFallen(){
        if(isGameOver) {return;}
        agent.ObservedDied();
        agent.AddTimeWeightedReward(-1f*battleTime/maxBattleTime);
        levelEndText.text = "The Dingus has fallen!";
        InitGameOver();
    }

    public void AgentHitHead(){
        if(isGameOver) {return;}
        agent.ObservedDied();
        agent.AddTimeWeightedReward(-1f*battleTime/maxBattleTime);
        levelEndText.text = "The Dingus has fallen!";
        InitGameOver();
    }

    public void JumpedTooMuch()
    {
        if(isGameOver) {return;}
        agent.ObservedDied();
        agent.AddTimeWeightedReward(-1f*battleTime/maxBattleTime);
        levelEndText.text = "Dingus died from overjumping!";
        InitGameOver();
    }

    void InitGameOver(bool won = false){
        if(isGameOver) {return;}
        isGameOver = true;
        if(won){
            CompleteLevel();
            agent.ObservedWon();
            agent.AddTimeWeightedReward(battleTime/maxBattleTime*1f);
        }
        else{
            FailLevel();
        }
        var currentCompletion = calculateCompletionPercent();
        if(currentCompletion > maxCompletion){maxCompletion = currentCompletion;}
        UpdateScoreDisplay();
        agent.BeginGameEnded();

        StartCoroutine(
            LevelFinishCountdown(2.5f)
        );
    }

    public void UpdateScoreDisplay(){

    }

    public void RewardNearingGoals()
    {
        if (isGameOver) { return; }
        float rewardmulti = 0.3f;
        //rewardmulti *= agent.grounded ? 1 : 0.3f;

        // ✅ Reward agent for getting closer to LightSwitches (Balanced structure)
        foreach (var sw in switches)
        {
            if (sw != null && sw.switchType == EscapeSwitch.LightSwitch && agent != null && initialDistances.ContainsKey(sw.gameObject))
            {
                float currentDistance = Vector3.Distance(agent.gameObject.transform.position, sw.gameObject.transform.position);
                float initialDistance = initialDistances[sw.gameObject];

                float progress = Mathf.Clamp01(1 - (currentDistance / initialDistance));
                float rewardForThisStep = progress - rewardsEarned[sw.gameObject];

                if (rewardForThisStep > 0 && !isGameOver)
                {
                    //agent.AddTimeWeightedReward(rewardForThisStep * rewardmulti / (switches.Count > 0 ? switches.Count : 1f));
                    agent.AddTimeWeightedReward(rewardForThisStep * rewardmulti * 0.1f);
                    rewardsEarned[sw.gameObject] = progress;
                }
            }
        }

        // ✅ Reward agent for moving any block closer to a HeavySwitch than ever before
        foreach (var sw in switches)
        {
            if (sw != null && sw.switchType == EscapeSwitch.HeavySwitch && minSwitchDistances.ContainsKey(sw.gameObject))
            {
                float closestBlockDistance = float.MaxValue;

                // Find the closest block to this switch
                foreach (var block in blocks)
                {
                    float currentDistance = Vector3.Distance(block.gameObject.transform.position, sw.gameObject.transform.position);
                    if (currentDistance < closestBlockDistance)
                    {
                        closestBlockDistance = currentDistance;
                    }
                }

                float initialDistance = initialMaxBlockDistances[sw.gameObject]; // Start from max block distance

                // Calculate progress as a percentage of initial max block distance
                float progress = Mathf.Clamp01(1 - (closestBlockDistance / initialDistance));
                float rewardForThisStep = progress - rewardsEarned[sw.gameObject];

                if (rewardForThisStep > 0 && !isGameOver)
                {
                    //agent.AddTimeWeightedReward(rewardForThisStep * rewardmulti / (switches.Count > 0 ? switches.Count : 1f));
                    agent.AddTimeWeightedReward(rewardForThisStep * rewardmulti * 5f);
                    rewardsEarned[sw.gameObject] = progress; 
                    minSwitchDistances[sw.gameObject] = closestBlockDistance;
                }
            }
        }
    }

    public void rewardMovingBlocks(){
        foreach (var block in blocks)
        {
            float currentspeed = block.gameObject.GetComponent<Rigidbody>().linearVelocity.magnitude;
            if(block.gameObject.GetComponent<Rigidbody>().linearVelocity.y > -0.01f && currentspeed > 0.1f){
                //Debug.Log($"y speed = {block.gameObject.GetComponent<Rigidbody>().linearVelocity.y }");
                //Debug.Log($"the block is moving at speed {currentspeed}");
                //it is an uphill battle to continue to convince this gremlin to experiement with pushing boxes
                agent.AddTimeWeightedReward(currentspeed/agent.maxVelocity * Time.deltaTime * 0.10f);
            }
        }
    }

    public void RewardNearingTheGoalAfterOpeningIt()
    {
        if(isGameOver) {return;}
        if(escapeDoor != null && escapeDoor.open && agent != null)
        {
            float currentDistance = Vector3.Distance(agent.gameObject.transform.position, escapeDoor.gameObject.transform.position);
            float initialDistance = initialGoalDistance;

            // Calculate progress as a percentage of the initial distance
            float progress = Mathf.Clamp01(1 - (currentDistance / initialDistance));

            // Calculate the incremental reward for this step
            float rewardForThisStep = progress - goalRewardsEarned;

            if (rewardForThisStep > 0 && !isGameOver) // Ensure rewards only increase
            {
                //Debug.Log("rewarding nearing the goal");
                var rewardmulti = 1f;
                //rewardmulti *= agent.grounded ? 1 : 0.3f;
                agent.AddTimeWeightedReward(rewardForThisStep * 1f * rewardmulti); // Add to the agent's reward
                goalRewardsEarned = progress; // Update the reward tracker
            }
        }     
    }
    IEnumerator LevelFinishCountdown(float time)
    {
        yield return new WaitForSeconds(time);
        EndScene();
        BeginScene();
        agent.EndEpisode();
    }

    public void BeginScene()
    {
        numberOfAttempts += 1;
        attemptsText.text = $"Attempts: {numberOfAttempts}";
        var nextLevel = GetNextLevel();
        spawnedLevel = GameObject.Instantiate(nextLevel, this.gameObject.transform);
        switches = spawnedLevel.GetComponentsInChildren<LevelSwitch>(includeInactive: true).ToList();
        blocks = spawnedLevel.GetComponentsInChildren<EscapeBlock>(includeInactive: true).ToList();
        spawnArea = GetChildWithTag(spawnedLevel.transform, "Respawn").gameObject;
        escapeDoor = spawnedLevel.GetComponentsInChildren<EscapeDoor>()[0];

        Vector3 cubeSize = spawnArea.transform.lossyScale; // Get world scale of the cube

        Vector3 randomPosition = new Vector3(
            Random.Range(-cubeSize.x / 2, cubeSize.x / 2),
            spawnArea.transform.position.y -5f,
            Random.Range(-cubeSize.z / 2, cubeSize.z / 2)
        );

        agent.transform.position = spawnArea.transform.position + randomPosition; // Offset by cube position
        agent.transform.rotation = Quaternion.identity;
        agent.gameObject.GetComponent<Rigidbody>().linearVelocity = new Vector3(0f,0f,0f);

        battleTime = maxBattleTime;

        foreach(var levelSwitch in switches){
            levelSwitch.controller = this;
            //levelSwitch.ResetSwitch();
        }

        initialDistances = new Dictionary<GameObject, float>();
        rewardsEarned = new Dictionary<GameObject, float>();
        initialMaxBlockDistances = new Dictionary<GameObject, float>();
        minSwitchDistances = new Dictionary<GameObject, float>(); // Tracks closest block-to-switch distance

        goalRewardsEarned = 0;
        initialGoalDistance = 0.0001f;

        foreach (var sw in switches)
        {
            float agentDistance = Vector3.Distance(agent.gameObject.transform.position, sw.gameObject.transform.position);
            initialDistances[sw.gameObject] = agentDistance;
            rewardsEarned[sw.gameObject] = 0f; // Reset rewards for this episode

            if (sw.switchType == EscapeSwitch.HeavySwitch)
            {
                float maxBlockDistance = 0f;

                // Find the farthest block from this switch
                foreach (var block in blocks)
                {
                    float blockDistance = Vector3.Distance(block.gameObject.transform.position, sw.gameObject.transform.position);
                    if (blockDistance > maxBlockDistance)
                    {
                        maxBlockDistance = blockDistance;
                    }
                }

                initialMaxBlockDistances[sw.gameObject] = maxBlockDistance; // Store max starting distance
                minSwitchDistances[sw.gameObject] = maxBlockDistance; // Start from max distance
            }
        }

        isGameOver = false;
        timeRewardpool = 30f;
    }

    Transform GetChildWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                return child; // Return the first found child with the tag
            }
        }
        return null; // No child with the tag found
    }

    public void EndScene()
    {
        GameObject.Destroy(spawnedLevel);
        levelEndText.text = "";
        //RandomizeList(spawnPoints);
    }

    public GameObject GetNextLevel()
    {
        bool isPastFinalTier = currentTierIndex >= levelsList.Count;
        bool isAtCappedTier = maxTierIndex != -1 && currentTierIndex >= maxTierIndex && completedLevelsInTier.Count >= levelsList[currentTierIndex].levels.Count;
        int finalTier = maxTierIndex != -1 ? maxTierIndex : levelsList.Count-1;

        if (isPastFinalTier || isAtCappedTier)
        {
            if (!entireGameWon)
            {
                entireGameWon = true;
                int minutes = Mathf.FloorToInt(totalTrainingTime / 60f);
                int seconds = Mathf.FloorToInt(totalTrainingTime % 60f);
                entireGameWonText.text = $"Finished at {minutes:D2}:{seconds:D2} with {numberOfAttempts} attempts";
            }
            completedLevelsInTier.Clear();
            currentTierIndex = repeatAtMaxTier ? finalTier : startingTierIndex;
        }

        // Get current tier
        var currentTier = levelsList[currentTierIndex].levels;

        // Advance tier if all levels are completed
        if (completedLevelsInTier.Count >= currentTier.Count)
        {
            if (maxTierIndex != -1 && currentTierIndex >= maxTierIndex)
            {
                // Remain in current tier
                Debug.Log("all levels in tier finished, repeating tier due to max tier limit");
                completedLevelsInTier.Clear();
                return GetNextLevel();
            }
            else
            {
                Debug.Log("all levels in tier finished, changing tier");
                currentTierIndex++;
                completedLevelsInTier.Clear();
                return GetNextLevel();
            }
        }

        // Separate levels into incomplete and completed
        var incompleteLevels = currentTier.Where(level => !completedLevelsInTier.Contains(level)).ToList();
        var completedLevels = currentTier.Where(level => completedLevelsInTier.Contains(level)).ToList();

        // Occasionally allow replay of a completed level (e.g. 1 in 5 chance)
        bool replayCompleted = completedLevels.Count > 0 && Random.Range(0, 5) == 0;

        if (replayCompleted)
        {
            currentLevel = completedLevels[Random.Range(0, completedLevels.Count)];
        }
        else
        {
            currentLevel = incompleteLevels[Random.Range(0, incompleteLevels.Count)];
        }

        return currentLevel;
    }

    public void CompleteLevel()
    {
        if (currentLevel != null)
        {
            completedLevelsInTier.Add(currentLevel);
        }
    }

    public void FailLevel()
    {
        // Optional: do nothing, or remove from completedLevelsInTier if retrying
        //completedLevelsInTier.Remove(currentLevel);
    }

    List<GameObject> GetChildrenByName(Transform parent, string targetName)
    {
        List<GameObject> matchingChildren = new List<GameObject>();

        // Iterate through all child transforms
        foreach (Transform child in parent)
        {
            // Check if the name matches
            if (child.name == targetName)
            {
                matchingChildren.Add(child.gameObject);
            }

            // Recursively search in the child's children
            matchingChildren.AddRange(GetChildrenByName(child, targetName));
        }

        return matchingChildren;
    }

    float calculateCompletionPercent(){
        float numGoals = switches.Count;
        float numCompletedgoals = 0f;
        foreach(var sw in switches){
            if(sw != null && sw.currentlyPressed){
                numCompletedgoals += 1;
            }
        }
        //Debug.Log($"completion percent is {numCompletedgoals/numGoals*100}%");
        return numCompletedgoals/numGoals*100;
    }

    public void BlockPushed(){
        if(!isGameOver){
            //Debug.Log("block pushed");
            agent.AddNonTimeWeightedReward(Time.deltaTime * 0.04f);
        }
    }

    List<GameObject> GetChildrenWithComponent<T>(Transform parent) where T : Component
    {
        List<GameObject> matchingChildren = new List<GameObject>();

        // Iterate through all child transforms
        foreach (Transform child in parent)
        {
            // Check if the child has the specified component
            if (child.GetComponent<T>() != null)
            {
                matchingChildren.Add(child.gameObject);
            }

            // Recursively search in the child's children
            matchingChildren.AddRange(GetChildrenWithComponent<T>(child));
        }

        return matchingChildren;
    }

    // Generic method to randomize the contents of a list
    void RandomizeList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            // Generate a random index between i and the end of the list
            int randomIndex = UnityEngine.Random.Range(i, list.Count);

            // Swap the elements at i and randomIndex
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
