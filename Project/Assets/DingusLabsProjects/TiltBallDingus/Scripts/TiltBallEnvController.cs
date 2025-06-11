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

public class TiltBallEnvController : MonoBehaviour
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
    private SoccerSettings m_SoccerSettings;

    //private SimpleMultiAgentGroup m_BlueAgentGroup;

    private int m_ResetTimer;
    private bool isGameOver = false;

    public TextMeshPro gametimeText;
    public TextMeshPro totalTrainingText;
    public TextMeshPro levelEndText;
    private bool entireGameWon = false;
    public TextMeshPro entireGameWonText;
    public TextMeshPro attemptsText;
    public List<TiltBallGoal> goals = new List<TiltBallGoal>();
    public List<TiltBallBoulder> balls = new List<TiltBallBoulder>();
    public TiltBallAgent agent;
    private Vector3 boulderSpawnLoc;

    public List<GameObject> levelPrefabs; // List of all level prefabs
    private Dictionary<GameObject, bool> levelStates; // Tracks whether each level is completed
    public GameObject currentLevel; // The current level being played
    private GameObject spawnedLevel;
    private Dictionary<GameObject, float> initialDistances; // Tracks initial distances to goals
    private Dictionary<GameObject, float> rewardsEarned; // Tracks rewards earned for each goal

    private float timeRewardpool = 10f;

    private float maxCompletion = 0f;

    private float numberOfAttempts = -1;

    void Start()
    {

        m_SoccerSettings = FindObjectOfType<SoccerSettings>();
        goals = GetComponentsInChildren<TiltBallGoal>().ToList();//GetChildrenByName(transform, "Goal");
        agent = GetComponentsInChildren<TiltBallAgent>().FirstOrDefault();
        balls = GetComponentsInChildren<TiltBallBoulder>().ToList();

        InitializeLevelStates();

        var newList = new List<float>();
        EndScene();
        BeginScene();
    }

    // void InitInitialTeams(){
    //     m_BlueAgentGroup = new SimpleMultiAgentGroup();

    // }
    // void AssignInitialTeams(Agent item){
    //     m_BlueAgentGroup.RegisterAgent(item);
    // }
    

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

        CalculateIsGameOver();
        if(isGameOver){return;}

        RewardNearingGoals();

        gametimeText.text = "Time: " + battleTime.ToString("0");
        if(!isGameOver){
            //agent.AddTimeWeightedReward(Time.deltaTime * -0.002f);
            agent.dirVectorToGoal = GetNormalizedDirectionToNearestActiveGoal();
            agent.goalsremaining = goals.Count();

            timeRewardpool -= Time.deltaTime;
            
            if(timeRewardpool > 0){
                if(balls[0] != null & balls[0].gameObject.GetComponent<Rigidbody>().linearVelocity.magnitude > 1.5f ){
                    agent.AddTimeWeightedReward(Time.deltaTime * 0.002f * math.min(balls[0].gameObject.GetComponent<Rigidbody>().linearVelocity.magnitude, 2f));
                } else if (balls[0] != null & balls[0].gameObject.GetComponent<Rigidbody>().linearVelocity.magnitude < 1f){
                    agent.AddTimeWeightedReward(Time.deltaTime * -0.02f);
                }
            }
        }
    }

    void CalculateIsGameOver() {
        if(isGameOver) {return;}
        if (battleTime <= 0)
        {
            isGameOver = true;
            InitGameOver();
            agent.observeTimedOut();
            levelEndText.text = "Time's up!";
            return;
        }

        int deadBalls = 0;
        foreach(var ball in balls){
            if (ball.dead){
                deadBalls++;
            }
        }

        if (deadBalls > 0){
            isGameOver = true;
            InitGameOver();
            levelEndText.text = "He's gone....";
            agent.ObservedDied(battleTime/maxBattleTime * 0.5f);
            return;
        }
        if(calculateCompletionPercent() >= 100){
            isGameOver = true;
            InitGameOver(true);
            levelEndText.text = "Winner winner, chicken dinner";
            return;
        }
    }
    public void hitGoal(){
        //Debug.Log($"A goal was hit, goals count: {goals.Count()}");
        agent.observeHitGoal(1f/(float)goals.Count()*8f);
        agent.observeHitGoal(0.05f);
        //agent.AddTimeWeightedReward(1f/goals.Count()*5f);
        //agent.AddTimeWeightedReward(battleTime/maxBattleTime/(float)goals.Count()/10f);
        //battleTime = maxBattleTime/goals.Count();
    }

    public void hitWall(){
        agent.AddTimeWeightedReward(-Time.deltaTime * 0.05f);
        //Debug.Log("hitwall");
    }

    public void insideDeathWall(){
        agent.AddTimeWeightedReward(-Time.deltaTime * 0.1f);
        agent.insideDeathWall = true;
        //Debug.Log("hitwall");
    }

    public void enteredOutofBounds(){
        agent.AddTimeWeightedReward(-0.1f);
        //Debug.Log("hitwall");
    }

    public void TheBallHasFallen(){
        if(isGameOver) {return;}
        isGameOver = true;
        levelEndText.text = "The Boulder has fallen!";
        InitGameOver();
    }

    void InitGameOver(bool won = false){
        if(won){
            CompleteLevel();
            if(entireGameWon == false && AreAllLevelsCompleted()){
                entireGameWon = true;
                int minutes = Mathf.FloorToInt(totalTrainingTime / 60f);
                int seconds = Mathf.FloorToInt(totalTrainingTime % 60f);    
                entireGameWonText.text = $"Finished at {minutes:D2}:{seconds:D2} with {numberOfAttempts} attempts";
            }
            agent.ObservedWon();
            agent.AddTimeWeightedReward(battleTime/maxBattleTime*2.5f);
        }
        else{
            FailLevel();
            //agent.ObservedDied();
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
        if(isGameOver) {return;}
        foreach (var goal in goals)
        {
            if(goal != null && balls[0] != null && initialDistances.ContainsKey(goal.gameObject))
            {
                float currentDistance = Vector3.Distance(balls[0].gameObject.transform.position, goal.gameObject.transform.position);
                float initialDistance = initialDistances[goal.gameObject];

                // Calculate progress as a percentage of the initial distance
                float progress = Mathf.Clamp01(1 - (currentDistance / initialDistance));

                // Calculate the incremental reward for this step
                float rewardForThisStep = progress - rewardsEarned[goal.gameObject];

                // if (rewardForThisStep > 0 && !isGameOver) // Ensure rewards only increase
                // {
                //     var rewardmulti = !agent.insideDeathWall ? 2f : -1.5f;
                //     agent.AddTimeWeightedReward(rewardForThisStep * 2f * rewardmulti / (goals.Count > 0 ? goals.Count : 1f) ); // Add to the agent's reward
                //     agent.AddTimeWeightedReward(rewardForThisStep * 0.035f * rewardmulti * balls[0].GetComponent<Rigidbody>().linearVelocity.magnitude / (goals.Count > 0 ? goals.Count : 1f));
                //     rewardsEarned[goal.gameObject] = progress; // Update the reward tracker
                // }
                if (rewardForThisStep > 0 && !isGameOver && agent.insideDeathWall) // Ensure rewards only increase
                {
                    var rewardmulti = -1.0f;
                    agent.AddTimeWeightedReward(rewardForThisStep * 1f * rewardmulti / (goals.Count > 0 ? goals.Count : 1f) ); // Add to the agent's reward
                    rewardsEarned[goal.gameObject] = progress; // Update the reward tracker
                }
            }
        }
    }

    public float GetNormalizedDistanceToNearestActiveGoal()
    {
        float maxDistance = 15.0f;
        float nearestDistance = float.MaxValue;

        foreach (var goal in goals)
        {
            if (goal.gameObject.activeInHierarchy && balls[0] != null) // Check if the goal is active
            {
                float distance = Vector3.Distance(balls[0].gameObject.transform.position, goal.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                }
            }
        }

        if (nearestDistance == float.MaxValue || nearestDistance > maxDistance)
        {
            return 1.0f; // If no active goals, return the "maximum" normalized distance
        }

        return Mathf.Clamp01(nearestDistance / maxDistance); // Normalize and clamp to [0, 1]
    }

    public Vector3 GetNormalizedDirectionToNearestActiveGoal()
    {
        Vector3 directionToGoal = Vector3.zero; // Default direction when no valid goal is found
        float nearestDistance = float.MaxValue;

        if (balls[0] == null)
        {
            return directionToGoal; // Return zero vector if ball doesn't exist
        }

        foreach (var goal in goals)
        {
            if (goal.gameObject.activeInHierarchy) // Check if the goal is active
            {
                float distance = Vector3.Distance(balls[0].gameObject.transform.position, goal.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    directionToGoal = (goal.transform.position - balls[0].gameObject.transform.position).normalized; // Calculate the normalized direction
                }
            }
        }

        return directionToGoal; // Return the normalized direction vector to the nearest active goal
    }

    /// <summary>
    /// Displays attempt status.
    /// </summary>
    /// <returns>The Enumerator to be used in a Coroutine.</returns>
    /// <param name="time">The time until new level start material will remain.</param>
    IEnumerator LevelFinishCountdown(float time)
    {
        yield return new WaitForSeconds(time);
        EndScene();
        BeginScene();
        agent.EndEpisode();
    }

    public void BeginScene()
    {
        var nextLevel = GetNextLevel();
        spawnedLevel = GameObject.Instantiate(nextLevel, agent.tiltBallStage.transform);
        goals = spawnedLevel.GetComponentsInChildren<TiltBallGoal>(includeInactive: true).ToList();
        agent = GetComponentsInChildren<TiltBallAgent>().FirstOrDefault();
        balls.Clear();
        balls = spawnedLevel.GetComponentsInChildren<TiltBallBoulder>().ToList();
        var spawnPoints = spawnedLevel.GetComponentsInChildren<TiltBallSpawnPoint>().ToList();
        var randomYRotation = Random.Range(0, 8) * 45f;
        spawnedLevel.transform.rotation = Quaternion.Euler(0, randomYRotation, 0); //I'm wondering if random rotation will help with learning
        attemptsText.text = $"Attempts: {numberOfAttempts}";

        balls[0].transform.position = spawnPoints[Random.Range(0, spawnPoints.Count())].gameObject.transform.position;
        battleTime = maxBattleTime;

        foreach(var item in goals){
            item.gameObject.SetActive(true);
        }
        foreach(var ball in balls){
            //ball.ReturnToStartingPos();
        }

        initialDistances = new Dictionary<GameObject, float>();
        rewardsEarned = new Dictionary<GameObject, float>();

        foreach (var goal in goals)
        {
            float distance = Vector3.Distance(balls[0].gameObject.transform.position, goal.gameObject.transform.position);
            initialDistances[goal.gameObject] = distance;
            rewardsEarned[goal.gameObject] = 0f; // Reset rewards for this episode


        }
        isGameOver = false;
        timeRewardpool = 30f;
    }

    public void EndScene()
    {
        GameObject.Destroy(spawnedLevel);
        levelEndText.text = "";
        //RandomizeList(spawnPoints);
    }

    // Initialize or reset all levels to incomplete
    private void InitializeLevelStates()
    {
        levelStates = new Dictionary<GameObject, bool>();
        foreach (GameObject level in levelPrefabs)
        {
            levelStates[level] = false; // All levels start as incomplete
        }
    }

    // Get the next level to play
    public GameObject GetNextLevel()
    {
        numberOfAttempts++;
        // Check if all levels are completed
        if (AreAllLevelsCompleted())
        {
            InitializeLevelStates();
        }

        // Separate levels into completed and incomplete lists
        List<GameObject> incompleteLevels = new List<GameObject>();
        List<GameObject> completedLevels = new List<GameObject>();

        foreach (var level in levelStates)
        {
            if (!level.Value) // Incomplete levels
            {
                incompleteLevels.Add(level.Key);
            }
            else // Completed levels
            {
                completedLevels.Add(level.Key);
            }
        }

        // Decide whether to return a completed or incomplete
        bool selectCompleteLevel = Random.Range(0, 5) == 0; 

        if (selectCompleteLevel && completedLevels.Count > 0)
        {
            // Choose a random level from the completed pool
            int randomIndex = Random.Range(0, completedLevels.Count);
            currentLevel = completedLevels[randomIndex];
        }
        else if (incompleteLevels.Count > 0)
        {
            // Choose a random level from the incomplete pool
            int randomIndex = Random.Range(0, incompleteLevels.Count);
            currentLevel = incompleteLevels[randomIndex];
        }
        else
        {
            // If no levels are available, reset all levels
            InitializeLevelStates();
            return GetNextLevel();
        }

        return currentLevel;
    }

    // Mark the current level as failed
    public void FailLevel()
    {
        if (levelStates.ContainsKey(currentLevel))
        {
            levelStates[currentLevel] = false; // Explicitly ensure it's marked incomplete
        }
    }

    // Mark the current level as completed
    public void CompleteLevel()
    {
        if (levelStates.ContainsKey(currentLevel))
        {
            levelStates[currentLevel] = true; // Mark the level as completed
        }
    }

    // Check if all levels are completed
    private bool AreAllLevelsCompleted()
    {
        foreach (var levelState in levelStates.Values)
        {
            if (!levelState) // If any level is incomplete
            {
                return false;
            }
        }
        return true; // All levels are completed
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
        float numGoals = goals.Count;
        float numCompletedgoals = 0f;
        foreach(var goal in goals){
            if(goal != null && !goal.gameObject.activeInHierarchy){
                numCompletedgoals += 1;
            }
        }
        //Debug.Log($"completion percent is {numCompletedgoals/numGoals*100}%");
        return numCompletedgoals/numGoals*100;
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
