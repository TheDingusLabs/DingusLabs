using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using Unity.MLAgents;
using UnityEngine;

public class SisyphusEnvController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerInfo
    {
        public SisyphusAgent agent;
        
        [HideInInspector]
        public Vector3 startingPos;
        [HideInInspector]
        public Quaternion startingRot;
        [HideInInspector]
        public Rigidbody Rb;
    }

    /// <summary>
    /// Max Academy steps before this platform resets
    /// </summary>
    /// <returns></returns>
    //[Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;

    public float battleTime = 0f;
    public float maxBattleTime = 60f;
    private float currentMaxBattleTime;

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


    private SimpleMultiAgentGroup m_BlueAgentGroup;
    private SimpleMultiAgentGroup m_YellowAgentGroup;
    private SimpleMultiAgentGroup m_RedAgentGroup;
    private SimpleMultiAgentGroup m_GreenAgentGroup;
    private SimpleMultiAgentGroup m_PurpleAgentGroup;
    private SimpleMultiAgentGroup m_OrangeAgentGroup;
    private SimpleMultiAgentGroup m_GreyAgentGroup;
    private SimpleMultiAgentGroup m_TealAgentGroup;
    private SimpleMultiAgentGroup m_BrownAgentGroup;
    private SimpleMultiAgentGroup m_ForestAgentGroup;

    private int m_ResetTimer;
    private bool isGameOver = false;

    public TextMeshPro gametimeText;
    public TextMeshPro totalTrainingText;
    public TextMeshPro levelEndText;
    public List<GameObject> goals = new List<GameObject>();
    public List<GameObject> spawnPoints = new List<GameObject>();
    public List<SisyphusAgent> agents = new List<SisyphusAgent>();
    public GameObject boulder;
    private Vector3 boulderSpawnLoc;

    private float maxCompletion = 0f;

    void Start()
    {

        m_SoccerSettings = FindObjectOfType<SoccerSettings>();
        goals = GetChildrenByName(transform, "goal");
        spawnPoints = GetChildrenByName(transform, "SpawnPoint");
        agents = GetComponentsInChildren<SisyphusAgent>().ToList();
        boulderSpawnLoc = boulder.transform.position;
        currentMaxBattleTime = maxBattleTime/goals.Count();
        InitInitialTeams();

        foreach (var item in spawnPoints)
        {
            item.GetComponent<MeshRenderer>().enabled = false;
        }

        foreach (var item in goals)
        {
            item.GetComponent<MeshRenderer>().enabled = false;
        }

        foreach (var item in agents)
        {
            AssignInitialTeams(item);
        }
        var newList = new List<float>();
        EndScene();
        BeginScene();
    }

    void InitInitialTeams(){
        m_BlueAgentGroup = new SimpleMultiAgentGroup();
        m_YellowAgentGroup = new SimpleMultiAgentGroup();
        m_RedAgentGroup = new SimpleMultiAgentGroup();
        m_GreenAgentGroup = new SimpleMultiAgentGroup();
        m_PurpleAgentGroup = new SimpleMultiAgentGroup();
        m_OrangeAgentGroup = new SimpleMultiAgentGroup();
        m_GreyAgentGroup = new SimpleMultiAgentGroup();
        m_TealAgentGroup = new SimpleMultiAgentGroup();
        m_BrownAgentGroup = new SimpleMultiAgentGroup();
        m_ForestAgentGroup = new SimpleMultiAgentGroup(); 
    }
    void AssignInitialTeams(Agent item){
        m_BlueAgentGroup.RegisterAgent(item);
        // if (item.agent.team == BattleTeam.Blue){ m_BlueAgentGroup.RegisterAgent(item.agent);}
        // else if (item.agent.team == BattleTeam.Red){m_RedAgentGroup.RegisterAgent(item.agent);}
        // else if (item.agent.team == BattleTeam.Yellow){m_YellowAgentGroup.RegisterAgent(item.agent);}
        // else if (item.agent.team == BattleTeam.Green){m_GreenAgentGroup.RegisterAgent(item.agent);}
        // else if (item.agent.team == BattleTeam.Purple){m_PurpleAgentGroup.RegisterAgent(item.agent);}
        // else if (item.agent.team == BattleTeam.Orange){m_OrangeAgentGroup.RegisterAgent(item.agent);}
        // else if (item.agent.team == BattleTeam.Grey){m_GreyAgentGroup.RegisterAgent(item.agent);}
        // else if (item.agent.team == BattleTeam.Teal){m_TealAgentGroup.RegisterAgent(item.agent);}
        // else if (item.agent.team == BattleTeam.Brown){m_BrownAgentGroup.RegisterAgent(item.agent);}
        // else if (item.agent.team == BattleTeam.Forest){m_ForestAgentGroup.RegisterAgent(item.agent);}
    }
    

    // Method to check if a certain index exists in the list
    bool IndexExists<T>(List<T> list, int index)
    {
        return index >= 0 && index < list.Count;
    }

    void Update(){
        totalTrainingTime += Time.deltaTime;
        battleTime += Time.deltaTime;

        // Convert seconds to minutes and seconds
        int minutes = Mathf.FloorToInt(totalTrainingTime / 60);
        int seconds = Mathf.FloorToInt(totalTrainingTime % 60);
        // Format and display the time as "MM:SS"
        string timeFormatted = string.Format("{0:00}:{1:00}", minutes, seconds);
        totalTrainingText.text = "Training Time:" + timeFormatted;

        CalculateIsGameOver();
        if(isGameOver){return;}

        gametimeText.text = "Time: " + (currentMaxBattleTime - battleTime).ToString("0");
        if(!isGameOver){
            m_BlueAgentGroup.AddGroupReward(Time.deltaTime * -0.005f);
        }
    }

    void CalculateIsGameOver() {
        if(isGameOver) {return;}
        if (battleTime > currentMaxBattleTime)
        {
            isGameOver = true;
            InitGameOver();
            levelEndText.text = "Time's up!";
            return;
        }

        int livePlayers = 0;
        foreach(var agent in agents){
            if (!agent.dead){
                livePlayers++;
            }
        }

        if (livePlayers <= 3){
            isGameOver = true;
            InitGameOver();
            levelEndText.text = "Too Many dead";
            return;
        }
        if(calculateCompletionPercent() >= 100){
            m_BlueAgentGroup.AddGroupReward(2.5f);
            isGameOver = true;
            InitGameOver(); 
            levelEndText.text = "Winner winner, chicken dinner";
            return;
        }
    }
    public void hitGoal(){
        m_BlueAgentGroup.AddGroupReward(0.075f);
        m_BlueAgentGroup.AddGroupReward(battleTime/currentMaxBattleTime/20);
        battleTime = currentMaxBattleTime - maxBattleTime/goals.Count();
        foreach(var agent in agents){
            var boulderDistance = Vector3.Distance( new Vector3(agent.transform.position.x,0,agent.transform.position.z), new Vector3(boulder.transform.position.x,0,boulder.transform.position.z));
            if(boulderDistance < 5){
                agent.AddReward(0.05f);
                m_BlueAgentGroup.AddGroupReward(0.004f);
            }
        }
        
    }
    public void AnAgentIsCloseToBouldy(float score){
        m_BlueAgentGroup.AddGroupReward(score);
    }
    public void AnAgentHasFallen(){
        //m_BlueAgentGroup.AddGroupReward(-0.025f);
    }

    public void TheBoulderHasFallen(){
        if(isGameOver) {return;}
        isGameOver = true;
        levelEndText.text = "The Boulder has fallen!";
        InitGameOver();
        m_BlueAgentGroup.AddGroupReward(-0.1f);
    }

    void RewardTeam(float score) {

        m_BlueAgentGroup.AddGroupReward(score);
        // if(team == BattleTeam.Blue){
        //     m_BlueAgentGroup.AddGroupReward(score);
        // }else if(team == BattleTeam.Red){
        //     m_RedAgentGroup.AddGroupReward(score);
        // }else if(team == BattleTeam.Green){
        //     m_GreenAgentGroup.AddGroupReward(score);
        // }else if(team == BattleTeam.Yellow){
        //     m_YellowAgentGroup.AddGroupReward(score);
        // } else if(team == BattleTeam.Purple){
        //     m_PurpleAgentGroup.AddGroupReward(score);
        // }else if(team == BattleTeam.Orange){
        //     m_OrangeAgentGroup.AddGroupReward(score);
        // }else if(team == BattleTeam.Grey){
        //     m_GreyAgentGroup.AddGroupReward(score);
        // }else if(team == BattleTeam.Teal){
        //     m_TealAgentGroup.AddGroupReward(score);
        // }else if(team == BattleTeam.Brown){
        //     m_BrownAgentGroup.AddGroupReward(score);
        // }else if(team == BattleTeam.Forest){
        //     m_ForestAgentGroup.AddGroupReward(score);
        // }
    }

    void InitGameOver(/*GameObject winner = null*/string winner = null){
        var currentCompletion = calculateCompletionPercent();
        if(currentCompletion > maxCompletion){maxCompletion = currentCompletion;}
        UpdateScoreDisplay();
        foreach(var agent in agents){
            agent.BeginGameEnded();
        }

        StartCoroutine(
            LevelFinishCountdown(2.5f, winner == null)
        );
    }

    public void UpdateScoreDisplay(){

    }

    public void AHazardWasHurt(BattleBotAgent hurtAgent, BattleBotAgent hurterAgent, float damageAmount){
        //I'm torn about this, there's a risk they'll attack hazards for points
        //AnAgentWasHurt(hurtAgent, hurterAgent, damageAmount);
    }
    
    /// <summary>
    /// Displays attempt status.
    /// </summary>
    /// <returns>The Enumerator to be used in a Coroutine.</returns>
    /// <param name="time">The time until new level start material will remain.</param>
    IEnumerator LevelFinishCountdown(float time, bool isEveryoneDead)
    {
        yield return new WaitForSeconds(time);
        boulder.transform.position = boulderSpawnLoc;
        boulder.GetComponent<Rigidbody>().linearVelocity = new Vector3(0,0,0);
        boulder.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        EndScene();

        //TODO agents don't start the next episode on the correct team because we call end epsiode before we run begin scene which assigns new teams, this is porbably fine because they update realtime but I'd like to fix in the future
        foreach(var agent in agents){
            agent.EndEpisode();
        }
        EndLevelAgentGroups(time, isEveryoneDead);
        BeginScene();
    }

    public void EndLevelAgentGroups(float time, bool isEveryoneDead){
        if(isEveryoneDead){
            m_BlueAgentGroup.EndGroupEpisode();
            m_RedAgentGroup.EndGroupEpisode();
            m_YellowAgentGroup.EndGroupEpisode();
            m_GreenAgentGroup.EndGroupEpisode();
            m_PurpleAgentGroup.EndGroupEpisode();
            m_OrangeAgentGroup.EndGroupEpisode();
            m_GreyAgentGroup.EndGroupEpisode();
            m_TealAgentGroup.EndGroupEpisode();
            m_BrownAgentGroup.EndGroupEpisode();
            m_ForestAgentGroup.EndGroupEpisode();
        }
        else{
            m_BlueAgentGroup.GroupEpisodeInterrupted();
            m_RedAgentGroup.GroupEpisodeInterrupted();
            m_YellowAgentGroup.GroupEpisodeInterrupted();
            m_GreenAgentGroup.GroupEpisodeInterrupted();
            m_PurpleAgentGroup.GroupEpisodeInterrupted();
            m_OrangeAgentGroup.GroupEpisodeInterrupted();
            m_GreyAgentGroup.GroupEpisodeInterrupted();
            m_TealAgentGroup.GroupEpisodeInterrupted();
            m_BrownAgentGroup.GroupEpisodeInterrupted();
            m_ForestAgentGroup.GroupEpisodeInterrupted();
        }
    }

    public void BeginScene()
    {
        //Debug.Log($"max completion is {maxCompletion}%");
        //AssignTeams();
        //UnregisterAgents();
        foreach(var item in goals){
            item.SetActive(true);
        }

        battleTime = 0;
        isGameOver = false;
    }

    public void EndScene()
    {
        levelEndText.text = "";
        RandomizeList(spawnPoints);

        for(int i = 0; i < agents.Count; i++){
            //AgentsList[i].agent.gameObject.SetActive(true);

            agents[i].gameObject.transform.position = spawnPoints[i].transform.position;
            agents[i].gameObject.transform.LookAt(centreLocation.transform.position);

            agents[i].GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            agents[i].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }

    }

    public void UnregisterAgents(){
        foreach (var item in agents)
        {
            m_BlueAgentGroup.UnregisterAgent(item);
            // if (item.agent.team == BattleTeam.Blue){ m_BlueAgentGroup.UnregisterAgent(item.agent);}
            // else if (item.agent.team == BattleTeam.Red){m_RedAgentGroup.UnregisterAgent(item.agent);}
            // else if (item.agent.team == BattleTeam.Yellow){m_YellowAgentGroup.UnregisterAgent(item.agent);}
            // else if (item.agent.team == BattleTeam.Green){m_GreenAgentGroup.UnregisterAgent(item.agent);}
            // else if (item.agent.team == BattleTeam.Purple){m_PurpleAgentGroup.UnregisterAgent(item.agent);}
            // else if (item.agent.team == BattleTeam.Orange){m_OrangeAgentGroup.UnregisterAgent(item.agent);}
            // else if (item.agent.team == BattleTeam.Grey){m_GreyAgentGroup.UnregisterAgent(item.agent);}
            // else if (item.agent.team == BattleTeam.Teal){m_TealAgentGroup.UnregisterAgent(item.agent);}
            // else if (item.agent.team == BattleTeam.Brown){m_BrownAgentGroup.UnregisterAgent(item.agent);}
            // else if (item.agent.team == BattleTeam.Forest){m_ForestAgentGroup.UnregisterAgent(item.agent);}
        }
    }

    public void AssignTeams(){
        foreach (var item in agents)
        {
            m_BlueAgentGroup.RegisterAgent(item);
            // if (item.agent.team == BattleTeam.Blue){ m_BlueAgentGroup.RegisterAgent(item.agent);}
            // else if (item.agent.team == BattleTeam.Red){m_RedAgentGroup.RegisterAgent(item.agent);}
            // else if (item.agent.team == BattleTeam.Yellow){m_YellowAgentGroup.RegisterAgent(item.agent);}
            // else if (item.agent.team == BattleTeam.Green){m_GreenAgentGroup.RegisterAgent(item.agent);}
            // else if (item.agent.team == BattleTeam.Purple){m_PurpleAgentGroup.RegisterAgent(item.agent);}
            // else if (item.agent.team == BattleTeam.Orange){m_OrangeAgentGroup.RegisterAgent(item.agent);}
            // else if (item.agent.team == BattleTeam.Grey){m_GreyAgentGroup.RegisterAgent(item.agent);}
            // else if (item.agent.team == BattleTeam.Teal){m_TealAgentGroup.RegisterAgent(item.agent);}
            // else if (item.agent.team == BattleTeam.Brown){m_BrownAgentGroup.RegisterAgent(item.agent);}
            // else if (item.agent.team == BattleTeam.Forest){m_ForestAgentGroup.RegisterAgent(item.agent);}
        }
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
            if(!goal.activeInHierarchy){
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
