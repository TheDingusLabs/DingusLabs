using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using UnityEngine;
using Random = UnityEngine.Random;

public class BattleBotEnvController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerInfo
    {
        public BattleBotAgent agent;
        
        [HideInInspector]
        public Vector3 startingPos;
        [HideInInspector]
        public Quaternion startingRot;
        [HideInInspector]
        public Rigidbody Rb;
    }

    [System.Serializable]
    public class BattleStats
    {
        public string winner;
        
        // public string highestDamager;
        // public float highestDamage;
        public List<AgentDamageStat> damageStats;
    }

    [System.Serializable]
    public class AgentDamageStat
    {
        public string name;
        
        public float damage;

        public AgentDamageStat(string n, float d)
        {
            name = n;
            damage = d;
        }
    }

    public enum BattleTeam
    {
        Blue = 0,
        Red = 1,
        Green = 2,
        Yellow = 3,
        Purple = 4,
        Orange = 5,
        Grey = 6,
        Teal = 7,
        Brown = 8,
        Forest = 9,
        DeepPink = 10,
        DeepPurple = 11,
    }

    /// <summary>
    /// Max Academy steps before this platform resets
    /// </summary>
    /// <returns></returns>
    //[Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;

    public float battleTime = 0f;
    public float maxBattleTime = 60f;

    private float totalTrainingTime = 0f;

    /// <summary>
    /// The area bounds.
    /// </summary>

    /// <summary>
    /// We will be changing the ground material based on success/failue
    /// </summary>

    //List of Agents On Platform
    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();
    private List<float> AgentsHealths = new List<float>();
    public List<GameObject> startinglocations = new List<GameObject>();
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
    public TextMeshPro winnersText;
    public TextMeshPro damagersText;

    public List<BattleStats> battleStats = new List<BattleStats>();
    //public BattleStats currentBattleStats;
    public Dictionary<GameObject,float> battleStatsDictionary = new Dictionary<GameObject, float>();

    public int randomTeamSizes = 1;

    public bool randomiseTeams = false;

    public bool isTournament = false;
    public int tournamentSize;
    public int tournamentMode;
    public Tournament tournament;
    public float tournamentBeginCountdown = 0f;

    public List<TournamentTeam> tournamentTeams = new List<TournamentTeam>();
    public Battle nextTournamentBattle;
    private Battle previousBattle;
    public TextMeshPro tournamentBoard;
    public int tournamentWinsRequired = 1;


    void Start()
    {
        //Debug.Log("debug logs are working");
        if(randomTeamSizes < 1){randomTeamSizes = 1;}

        m_SoccerSettings = FindObjectOfType<SoccerSettings>();
        // Initialize TeamManagers
        //InitInitialTeams();

        startinglocations = this.gameObject.transform.Cast<Transform>()
            .Where(child => child.CompareTag("spawner"))
            .Select(child => child.gameObject)
            .ToList();

        AgentsList = new List<PlayerInfo>();
        var agents = GetComponentsInChildren<Agent>().ToList();
        foreach(var agent in agents){
            PlayerInfo item = new PlayerInfo();
            item.agent = agent.GetComponent<BattleBotAgent>();
            item.startingPos = agent.transform.position;
            item.startingRot = agent.transform.rotation;
            item.Rb = agent.GetComponent<Rigidbody>();
            AgentsList.Add(item);
        }

        var newList = new List<float>();
        for (int i = 0; i < AgentsList.Count; i++){
            if(i < AgentsList.Count){
                newList.Add(AgentsList[i].agent.hp);
            }
            else{
                newList.Add(0);
            }
        }
        AgentsHealths = newList;
        foreach (var item in AgentsList)
        {
            item.agent.UpdateAgentHealths(AgentsHealths);
        }

        if(isTournament){
            CreateNewTournament();
        }

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
    void AssignInitialTeams(PlayerInfo item){
        if (item.agent.team == BattleTeam.Blue){ m_BlueAgentGroup.RegisterAgent(item.agent);}
        else if (item.agent.team == BattleTeam.Red){m_RedAgentGroup.RegisterAgent(item.agent);}
        else if (item.agent.team == BattleTeam.Yellow){m_YellowAgentGroup.RegisterAgent(item.agent);}
        else if (item.agent.team == BattleTeam.Green){m_GreenAgentGroup.RegisterAgent(item.agent);}
        else if (item.agent.team == BattleTeam.Purple){m_PurpleAgentGroup.RegisterAgent(item.agent);}
        else if (item.agent.team == BattleTeam.Orange){m_OrangeAgentGroup.RegisterAgent(item.agent);}
        else if (item.agent.team == BattleTeam.Grey){m_GreyAgentGroup.RegisterAgent(item.agent);}
        else if (item.agent.team == BattleTeam.Teal){m_TealAgentGroup.RegisterAgent(item.agent);}
        else if (item.agent.team == BattleTeam.Brown){m_BrownAgentGroup.RegisterAgent(item.agent);}
        else if (item.agent.team == BattleTeam.Forest){m_ForestAgentGroup.RegisterAgent(item.agent);}
    }
    

    // Method to check if a certain index exists in the list
    bool IndexExists<T>(List<T> list, int index)
    {
        return index >= 0 && index < list.Count;
    }

    void Update(){
        totalTrainingTime += Time.deltaTime;
        tournamentBeginCountdown -= Time.deltaTime;

        var newList = new List<float>();
        for (int i = 0; i < AgentsList.Count; i++){
            if(i < AgentsList.Count){
                newList.Add(AgentsList[i].agent.hp);
            }
            else{
                newList.Add(0);
            }
        }
        AgentsHealths = newList;
        foreach(var agent in AgentsList)
        {
            agent.agent.UpdateAgentHealths(AgentsHealths);
        }

        battleTime += Time.deltaTime;

        // Convert seconds to minutes and seconds
        int minutes = Mathf.FloorToInt(totalTrainingTime / 60);
        int seconds = Mathf.FloorToInt(totalTrainingTime % 60);
        // Format and display the time as "MM:SS"
        string timeFormatted = string.Format("{0:00}:{1:00}", minutes, seconds);
        totalTrainingText.text = "Training Time:" + timeFormatted;


        CalculateIsGameOver();

        if(isTournament){
            string tournamentText = "";
            if(tournamentBeginCountdown > 0){
                tournamentText = $"NEW TOURNAMENT IN: {tournamentBeginCountdown.ToString("F0")}s\n";
            }
            // tournamentText += $"{tournament.RequiredWinsPerBattle} WINS TO PROGRESS\n---------------------------\nTEAMS\n---------------------------";
            tournamentText += $"{tournament.RequiredWinsPerBattle} WINS TO PROGRESS";
            tournamentText += $"\n--------------TEAMS--------------";
            foreach(var tim in tournament.AllTeams){
                tournamentText += $"\n{tim.Name}{(tim.IsEliminated ? " - eliminated" : tim.IsInLosersBracket ? " - loser bracket" : "")}";
                if(!tim.IsInLosersBracket && !tim.IsEliminated && tournament.IsComplete()){
                    tournamentText += " - winner!";
                }
            }
            tournamentBoard.text = tournamentText;
        }

        if(isGameOver){return;}

        gametimeText.text = "Time: " + (maxBattleTime - battleTime).ToString("0");
    }

    void CalculateIsGameOver() {
        if(isGameOver) {return;}
        //need to make this work with teams of size > 1
        //GameObject winner = null;
        if (battleTime > maxBattleTime)
        {
            isGameOver = true;
            foreach(var agent in AgentsList){
                if (agent.agent.hp > 0){
                    agent.agent.observeTimedOut();
                }
            }

            InitGameOver();
            levelEndText.text = "Time's up!";
            return;
        }

        List<BattleTeam> aliveTeams = new List<BattleTeam>();

        int livePlayers = 0;
        int liveNonTargetDummyPlayers = 0;
        foreach(var agent in AgentsList){
            if (agent.agent.hp > 0){
                livePlayers++;
                aliveTeams.Add(agent.agent.team);
                if(agent.agent.GetComponent<BehaviorParameters>().BehaviorType != BehaviorType.InferenceOnly){
                    liveNonTargetDummyPlayers++;
                }
            }
        }

        if(aliveTeams.Count > 0 && aliveTeams.All(value => value == aliveTeams[0])){
            isGameOver = true;
            string winnernames = "";
            //awful code but I'm tired
            int currentwinnernum = 0;
            foreach(var agent in AgentsList){
                if (currentwinnernum != 0){break;}
                if(agent.agent.hp > 0){
                    if(isTournament)
                    {
                        winnernames = tournament.AllTeams.FirstOrDefault(team => team.Name.Contains(agent.agent.gameObject.name, System.StringComparison.OrdinalIgnoreCase))?.Name;
                        //Debug.Log($"for some deranged reason I believe {agent.agent.name} won the last fight");

                        currentwinnernum ++;
                        if(isTournament){
                            tournament.ProcessBattleResult(winnernames);
                        }
                    }
                    else{
                        winnernames = tournamentTeams.FirstOrDefault(team => team.Name.Contains(agent.agent.gameObject.name, System.StringComparison.OrdinalIgnoreCase))?.Name;
                    }
                }
            }  
            levelEndText.text = "Winner: " + winnernames;
            InitGameOver(winnernames); 
        } else if (livePlayers == 0){
            isGameOver = true;
            InitGameOver();
            levelEndText.text = "Time's up!";
            return;
        } else if(liveNonTargetDummyPlayers <= 0){
            isGameOver = true;
            InitGameOver();
            levelEndText.text = "Only ghosts remain....";
        }
    }

    void RewardIndividualsInTeam(BattleTeam team, float score){
        foreach(var agent in AgentsList){
            if(agent.agent.team == team){
                agent.agent.AddReward(score);
            }
        }
    }

    void RewardTeam(BattleTeam team, float score) {
        if(team == BattleTeam.Blue){
            m_BlueAgentGroup.AddGroupReward(score);
        }else if(team == BattleTeam.Red){
            m_RedAgentGroup.AddGroupReward(score);
        }else if(team == BattleTeam.Green){
            m_GreenAgentGroup.AddGroupReward(score);
        }else if(team == BattleTeam.Yellow){
            m_YellowAgentGroup.AddGroupReward(score);
        } else if(team == BattleTeam.Purple){
            m_PurpleAgentGroup.AddGroupReward(score);
        }else if(team == BattleTeam.Orange){
            m_OrangeAgentGroup.AddGroupReward(score);
        }else if(team == BattleTeam.Grey){
            m_GreyAgentGroup.AddGroupReward(score);
        }else if(team == BattleTeam.Teal){
            m_TealAgentGroup.AddGroupReward(score);
        }else if(team == BattleTeam.Brown){
            m_BrownAgentGroup.AddGroupReward(score);
        }else if(team == BattleTeam.Forest){
            m_ForestAgentGroup.AddGroupReward(score);
        }

        //if PPO
    }

    void InitGameOver(/*GameObject winner = null*/string winner = null){
        
        var currentRoundBattleStats = new BattleStats();
        currentRoundBattleStats.damageStats = new List<AgentDamageStat>();
        currentRoundBattleStats.winner = winner != null ? winner : "-";
        foreach(var agent in battleStatsDictionary){
            currentRoundBattleStats.damageStats.Add(new AgentDamageStat(agent.Key.name, agent.Value) );
        }
        currentRoundBattleStats.damageStats.Sort((a, b) => b.damage.CompareTo(a.damage));
        battleStats.Add(currentRoundBattleStats);
        UpdateScoreDisplay();
        foreach(var agent in AgentsList){
            agent.agent.BeginGameEnded();
        }

        StartCoroutine(
            LevelFinishCountdown(5, winner == null)
        );
    }

    public void UpdateScoreDisplay(){
        if(isTournament && previousBattle != null && nextTournamentBattle != null && !tournament.BattlesHaveSameTeams(previousBattle, nextTournamentBattle) && battleStats.Count > 0){
            var topStat = battleStats[battleStats.Count() - 1];
            battleStats = new List<BattleStats>();
            battleStats.Add(topStat);
        }
        if(battleStats.Count > 0){
            string winnerDisplayText = "Winner:"+ System.Environment.NewLine;
            string damageDisplayText = "Max Damage:"+ System.Environment.NewLine;
            int recentCount = 5;
            int startIndex = Mathf.Max(battleStats.Count - recentCount, 0);
            // Iterate through the recent entries
            for (int i = startIndex; i < battleStats.Count; i++)
            {
                winnerDisplayText += battleStats[i].winner;
                if (battleStats[i].damageStats.Count > 0)
                {
                    damageDisplayText += battleStats[i].damageStats[0].name + " " + battleStats[i].damageStats[0].damage.ToString("F0");
                }
                else{
                    damageDisplayText += "-";
                }
                damageDisplayText += System.Environment.NewLine;
                winnerDisplayText += System.Environment.NewLine;

            }

            winnersText.text = winnerDisplayText;
            damagersText.text = damageDisplayText;
        }
    }

    public void AnAgentWasHurt(BattleBotAgent hurtAgent, BattleBotAgent hurterAgent, float damageAmount){
        //performAgentTeamRewards(hurtAgent, hurterAgent, damageAmount);
        
        if(hurterAgent != null && hurterAgent.gameObject != hurtAgent.gameObject && hurterAgent.team != hurtAgent.team){
            if(battleStatsDictionary.ContainsKey(hurterAgent.gameObject)){
                battleStatsDictionary[hurterAgent.gameObject] = battleStatsDictionary[hurterAgent.gameObject] + damageAmount;
            }
            else{
                battleStatsDictionary.Add(hurterAgent.gameObject, damageAmount);   
            }
        }

        //reward damaging an enemy if the enemy wasn't yourself or an ally
        if(hurterAgent != null && hurterAgent.gameObject != hurtAgent.gameObject && hurterAgent.team != hurtAgent.team){
            //Debug.Log($"rewarding {hurterAgent.name} for hurting {hurtAgent.name}");
            hurterAgent.ObservedDidDamage(damageAmount);
        } //you hurt yourself idiot, we use like 5 times less punishment for taking damage so self injuring behaviour needs to be penalised properly
        else if(hurterAgent != null && hurterAgent.gameObject == hurtAgent.gameObject){
            //Debug.Log($"punishing {hurterAgent.name} for hurting itself");
            hurtAgent.ObservedTookDamage(damageAmount*2);
        } // team friendly fire, punish this, they really want to hurt each other, gotta make this punishment really big
        else if(hurterAgent != null && hurterAgent.team == hurtAgent.team){
            //Debug.Log($"punishing {hurterAgent.name} for hurting its ally {hurtAgent.name}");
            //I think we should be punishing hurting allies but it may cause confusion during training, so lets see what happens if we don't
            hurterAgent.ObservedTookDamage(damageAmount*4);
        }

        //punish taking damage
        hurtAgent.ObservedTookDamage(damageAmount);
    }


    public void performAgentTeamRewards(BattleBotAgent hurtAgent, BattleBotAgent hurterAgent, float damageAmount){
        //todo, if I made each agent group into an array that has the same value for entries as the team enums I could just use the team enum's number as an index into the correct array item, kinda jank but very readable and mistake proof!
        if(hurterAgent != null){
            if(hurterAgent.team == BattleTeam.Blue && hurterAgent.team != hurtAgent.team){
                m_BlueAgentGroup.AddGroupReward(damageAmount/400);
            }else if(hurterAgent.team == BattleTeam.Red && hurterAgent.team != hurtAgent.team){
                m_RedAgentGroup.AddGroupReward(damageAmount/400);
            }else if(hurterAgent.team == BattleTeam.Green && hurterAgent.team != hurtAgent.team){
                m_GreenAgentGroup.AddGroupReward(damageAmount/400);
            }else if(hurterAgent.team == BattleTeam.Yellow && hurterAgent.team != hurtAgent.team){
                m_YellowAgentGroup.AddGroupReward(damageAmount/400);
            } else if(hurterAgent.team == BattleTeam.Purple && hurterAgent.team != hurtAgent.team){
                m_PurpleAgentGroup.AddGroupReward(damageAmount/400);
            }else if(hurterAgent.team == BattleTeam.Orange && hurterAgent.team != hurtAgent.team){
                m_OrangeAgentGroup.AddGroupReward(damageAmount/400);
            }else if(hurterAgent.team == BattleTeam.Grey && hurterAgent.team != hurtAgent.team){
                m_GreyAgentGroup.AddGroupReward(damageAmount/400);
            }else if(hurterAgent.team == BattleTeam.Teal && hurterAgent.team != hurtAgent.team){
                m_TealAgentGroup.AddGroupReward(damageAmount/400);
            }else if(hurterAgent.team == BattleTeam.Brown && hurterAgent.team != hurtAgent.team){
                m_BrownAgentGroup.AddGroupReward(damageAmount/400);
            }else if(hurterAgent.team == BattleTeam.Forest && hurterAgent.team != hurtAgent.team){
                m_ForestAgentGroup.AddGroupReward(damageAmount/400);
            }

            if(hurtAgent.team == BattleTeam.Blue){
                m_BlueAgentGroup.AddGroupReward(-damageAmount/800);
            }else if(hurtAgent.team == BattleTeam.Red){
                m_RedAgentGroup.AddGroupReward(-damageAmount/800);
            }else if(hurtAgent.team == BattleTeam.Green){
                m_GreenAgentGroup.AddGroupReward(-damageAmount/800);
            }else if(hurtAgent.team == BattleTeam.Yellow){
                m_YellowAgentGroup.AddGroupReward(-damageAmount/800);
            }else if(hurtAgent.team == BattleTeam.Purple){
                m_PurpleAgentGroup.AddGroupReward(-damageAmount/800);
            }else if(hurtAgent.team == BattleTeam.Orange){
                m_OrangeAgentGroup.AddGroupReward(-damageAmount/800);
            }else if(hurtAgent.team == BattleTeam.Grey){
                m_GreyAgentGroup.AddGroupReward(-damageAmount/800);
            }else if(hurtAgent.team == BattleTeam.Teal){
                m_TealAgentGroup.AddGroupReward(-damageAmount/800);
            }else if(hurtAgent.team == BattleTeam.Brown){
                m_BrownAgentGroup.AddGroupReward(-damageAmount/800);
            }else if(hurtAgent.team == BattleTeam.Forest){
                m_ForestAgentGroup.AddGroupReward(-damageAmount/800);
            }
        }
        else{ //this likely means they died to stage damage, though that can still be the result of another's actions, perhaps we can implement an attribution method using the last damager to still reward the damager here!
            if(hurtAgent.team == BattleTeam.Blue || ( hurterAgent != null && hurterAgent.team == hurtAgent.team)){
                    m_BlueAgentGroup.AddGroupReward(-damageAmount/800);
                }else if(hurtAgent.team == BattleTeam.Red || ( hurterAgent != null && hurterAgent.team == hurtAgent.team)){
                    m_RedAgentGroup.AddGroupReward(-damageAmount/800);
                }else if(hurtAgent.team == BattleTeam.Green || ( hurterAgent != null && hurterAgent.team == hurtAgent.team)){
                    m_GreenAgentGroup.AddGroupReward(-damageAmount/800);
                }else if(hurtAgent.team == BattleTeam.Yellow || ( hurterAgent != null && hurterAgent.team == hurtAgent.team)){
                    m_YellowAgentGroup.AddGroupReward(-damageAmount/800);
                }else if(hurtAgent.team == BattleTeam.Purple || ( hurterAgent != null && hurterAgent.team == hurtAgent.team)){
                    m_PurpleAgentGroup.AddGroupReward(-damageAmount/800);
                }else if(hurtAgent.team == BattleTeam.Orange || ( hurterAgent != null && hurterAgent.team == hurtAgent.team)){
                    m_OrangeAgentGroup.AddGroupReward(-damageAmount/800);
                }else if(hurtAgent.team == BattleTeam.Grey || ( hurterAgent != null && hurterAgent.team == hurtAgent.team)){
                    m_GreyAgentGroup.AddGroupReward(-damageAmount/800);
                }else if(hurtAgent.team == BattleTeam.Teal || ( hurterAgent != null && hurterAgent.team == hurtAgent.team)){
                    m_TealAgentGroup.AddGroupReward(-damageAmount/800);
                }else if(hurtAgent.team == BattleTeam.Brown || ( hurterAgent != null && hurterAgent.team == hurtAgent.team)){
                    m_BrownAgentGroup.AddGroupReward(-damageAmount/800);
                }else if(hurtAgent.team == BattleTeam.Forest || ( hurterAgent != null && hurterAgent.team == hurtAgent.team)){
                    m_ForestAgentGroup.AddGroupReward(-damageAmount/800);
                }
        }
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
        //EndLevelAgentGroups(time, isEveryoneDead);
        if(isTournament && tournament != null && tournament.IsComplete()){
            CreateNewTournament();
            yield return new WaitForSeconds(tournamentBeginCountdown+1f);
        }
        EndScene();

        //TODO agents don't start the next episode on the correct team because we call end epsiode before we run begin scene which assigns new teams, this is porbably fine because they update realtime but I'd like to fix in the future
        foreach(var agent in AgentsList){
            agent.agent.EndEpisode();
        }

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
        //AssignTeams();
        if(!isTournament)
        {
            PerformTeamRandomisation();
        }

        foreach (var item in AgentsList)
        {
            item.agent = item.agent.GetComponent<BattleBotAgent>();
            item.startingPos = item.agent.transform.position;
            item.startingRot = item.agent.transform.rotation;
            item.Rb = item.agent.GetComponent<Rigidbody>();
        }
        //UnregisterAgents();
    }

    public void CreateNewTournament(){
        PerformTeamRandomisation();
        tournament = new Tournament(tournamentTeams, tournamentSize, tournamentWinsRequired);
        tournamentBeginCountdown = 30f;
    }

    public void PerformTeamRandomisation()
    {
        tournamentTeams = new List<TournamentTeam>();

        if (randomTeamSizes > 0 || randomiseTeams)
        {
            // Randomize agent list and team types
            RandomizeList(AgentsList);
            List<BattleTeam> teamTypes = new List<BattleTeam>((BattleTeam[])BattleTeam.GetValues(typeof(BattleTeam)));

            int currentTeamIndex = 0;

            foreach (var agent in AgentsList)
            {
                // Assign agent to the current team
                agent.agent.team = teamTypes[currentTeamIndex];

                // Move to the next team after every agent for team size 1, or after filling the team
                if (randomTeamSizes == 1 || AgentsList.IndexOf(agent) % randomTeamSizes == randomTeamSizes - 1)
                {
                    currentTeamIndex = (currentTeamIndex + 1) % teamTypes.Count;
                }
            }
        }

        // Group agents by their teams
        var teamsDictionary = AgentsList
            .GroupBy(agent => agent.agent.team)
            .ToDictionary(group => group.Key, group => group.Select(agent => agent.agent.gameObject).ToList());

        // Create tournament teams
        foreach (var team in teamsDictionary)
        {
            string teamName = string.Join(" & ", team.Value.Select(member => member.name));
            tournamentTeams.Add(new TournamentTeam(team.Value, team.Key, teamName));
        }
    }

    // public void PerformTeamRandomisation(){
    //     tournamentTeams = new List<TournamentTeam>();
    //     if (randomTeamSizes > 0 || randomiseTeams)
    //     {
    //         RandomizeList(AgentsList);

    //         int currentTeamSize = 0; // Tracks how many agents are in the current team
    //         int currentTeamIndex = 0; // Tracks the current team in teamTypes
    //         List<BattleTeam> teamTypes = new List<BattleTeam>((BattleTeam[])BattleTeam.GetValues(typeof(BattleTeam)));
    //         //RandomizeList(teamTypes);
    //         var agentsListtoRandomise = AgentsList;
    //         RandomizeList(agentsListtoRandomise);
    //         for (int i = 0; i < agentsListtoRandomise.Count; i++)
    //         {
    //             // Assign the current agent to the current team
    //             agentsListtoRandomise[i].agent.team = teamTypes[currentTeamIndex];

    //             // Increment the team size
    //             currentTeamSize++;

    //             // If the current team is full, move to the next team
    //             if (currentTeamSize >= randomTeamSizes)
    //             {
    //                 currentTeamSize = 0;
    //                 currentTeamIndex++;

    //                 // Wrap around if we run out of teams
    //                 if (currentTeamIndex >= teamTypes.Count)
    //                 {
    //                     currentTeamIndex = 0;
    //                 }
    //             }
    //         }
    //     }

    //     Dictionary<BattleTeam, List<GameObject>> teamsDictionary = new Dictionary<BattleTeam, List<GameObject>>();

    //     foreach (var agent in AgentsList)
    //     {
    //         // Get the team color of the agent.
    //         BattleTeam teamColor = agent.agent.team;

    //         // Group agents by their team color.
    //         if (!teamsDictionary.ContainsKey(teamColor))
    //         {
    //             teamsDictionary[teamColor] = new List<GameObject>();
    //         }

    //         teamsDictionary[teamColor].Add(agent.agent.gameObject);
    //     }

    //     // Create TournamentTeam objects from the grouped agents.
    //     foreach (var team in teamsDictionary)
    //     {
    //         var timname = string.Join(" & ", team.Value.Select(member => member.name));
    //         TournamentTeam tournamentTeam = new TournamentTeam(team.Value, team.Key, timname);
    //         tournamentTeams.Add(tournamentTeam);
    //     }
    // }

    public void EndScene()
    {
        if(isTournament && tournamentBeginCountdown <= 0){
            if(nextTournamentBattle != null){previousBattle = nextTournamentBattle;}
            nextTournamentBattle = tournament.GetNextBattle();
        }
        else if (isTournament && tournamentBeginCountdown > 0){
            StartCoroutine(
                LevelFinishCountdown(tournamentBeginCountdown+1f, false)
            );
            return;
        }
        levelEndText.text = "";
        battleTime = 0;
        isGameOver = false;

        RandomizeList(startinglocations);

        if(!isTournament){
            for(int i = 0; i < AgentsList.Count; i++){
                //AgentsList[i].agent.gameObject.SetActive(true);

                AgentsList[i].agent.transform.position = startinglocations[i].transform.position;
                AgentsList[i].agent.transform.LookAt(centreLocation.transform.position);

                AgentsList[i].Rb.linearVelocity = Vector3.zero;
                AgentsList[i].Rb.angularVelocity = Vector3.zero;
            }
        }
        else{
            //not in this round? then get out
            for(int j = 0; j < AgentsList.Count; j++){
                //AgentsList[i].agent.gameObject.SetActive(true);

                AgentsList[j].agent.transform.localPosition = new Vector3(0, -10f, 0);
            }

            // Loop through each team in the next tournament battle
            int i = 0;
            bool shouldChangeTeams = true;

            // Check if team colors need to be updated
            if (previousBattle != null)
            {
                shouldChangeTeams = !tournament.BattlesHaveSameTeams(previousBattle, nextTournamentBattle);
            }

            // Get the list of unique team colors to assign
            var availableColors = Enum.GetValues(typeof(BattleTeam)).Cast<BattleTeam>().ToList();

            // Ensure Blue and Red are prioritized if there are only two teams
            if (nextTournamentBattle.Teams.Count == 2)
            {
                availableColors.Remove(BattleTeam.Blue);
                availableColors.Remove(BattleTeam.Red);
                availableColors.Insert(0, BattleTeam.Red);
                availableColors.Insert(0, BattleTeam.Blue);
            }

            // Assign colors to teams
            var teamColorAssignments = new Dictionary<TournamentTeam, BattleTeam>();
            int colorIndex = 0;

            foreach (var team in nextTournamentBattle.Teams)
            {
                // Assign a unique color from the available colors
                BattleTeam assignedColor = availableColors[colorIndex % availableColors.Count];
                teamColorAssignments[team] = assignedColor;
                colorIndex++;
            }

            // Position fighters and assign their team colors
            foreach (var team in nextTournamentBattle.Teams)
            {
                BattleTeam assignedColor = teamColorAssignments[team];

                foreach (var fighter in team.Members)
                {
                    // Position and orient fighter
                    fighter.transform.position = startinglocations[i].transform.position;
                    fighter.transform.LookAt(centreLocation.transform.position);

                    // Reset fighter's velocities
                    var rigidbody = fighter.GetComponent<Rigidbody>();
                    rigidbody.linearVelocity = Vector3.zero;
                    rigidbody.angularVelocity = Vector3.zero;

                    // Update the fighter's team color if required
                    if (shouldChangeTeams)
                    {
                        fighter.GetComponent<BattleBotAgent>().team = assignedColor;
                    }

                    i++;
                }
            }
        }

        battleStatsDictionary = new Dictionary<GameObject, float>();
    }

    public void AssignTeams(){
        foreach (var item in AgentsList)
        {
            if (item.agent.team == BattleTeam.Blue){ m_BlueAgentGroup.UnregisterAgent(item.agent);}
            else if (item.agent.team == BattleTeam.Red){m_RedAgentGroup.UnregisterAgent(item.agent);}
            else if (item.agent.team == BattleTeam.Yellow){m_YellowAgentGroup.UnregisterAgent(item.agent);}
            else if (item.agent.team == BattleTeam.Green){m_GreenAgentGroup.UnregisterAgent(item.agent);}
            else if (item.agent.team == BattleTeam.Purple){m_PurpleAgentGroup.UnregisterAgent(item.agent);}
            else if (item.agent.team == BattleTeam.Orange){m_OrangeAgentGroup.UnregisterAgent(item.agent);}
            else if (item.agent.team == BattleTeam.Grey){m_GreyAgentGroup.UnregisterAgent(item.agent);}
            else if (item.agent.team == BattleTeam.Teal){m_TealAgentGroup.UnregisterAgent(item.agent);}
            else if (item.agent.team == BattleTeam.Brown){m_BrownAgentGroup.UnregisterAgent(item.agent);}
            else if (item.agent.team == BattleTeam.Forest){m_ForestAgentGroup.UnregisterAgent(item.agent);}
        }
    }

    public void UnregisterAgents(){
        foreach (var item in AgentsList)
        {
            if (item.agent.team == BattleTeam.Blue){ m_BlueAgentGroup.RegisterAgent(item.agent);}
            else if (item.agent.team == BattleTeam.Red){m_RedAgentGroup.RegisterAgent(item.agent);}
            else if (item.agent.team == BattleTeam.Yellow){m_YellowAgentGroup.RegisterAgent(item.agent);}
            else if (item.agent.team == BattleTeam.Green){m_GreenAgentGroup.RegisterAgent(item.agent);}
            else if (item.agent.team == BattleTeam.Purple){m_PurpleAgentGroup.RegisterAgent(item.agent);}
            else if (item.agent.team == BattleTeam.Orange){m_OrangeAgentGroup.RegisterAgent(item.agent);}
            else if (item.agent.team == BattleTeam.Grey){m_GreyAgentGroup.RegisterAgent(item.agent);}
            else if (item.agent.team == BattleTeam.Teal){m_TealAgentGroup.RegisterAgent(item.agent);}
            else if (item.agent.team == BattleTeam.Brown){m_BrownAgentGroup.RegisterAgent(item.agent);}
            else if (item.agent.team == BattleTeam.Forest){m_ForestAgentGroup.RegisterAgent(item.agent);}
        }
    }


    // Generic method to randomize the contents of a list
    void RandomizeList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            // Generate a random index between i and the end of the list
            int randomIndex = Random.Range(i, list.Count);

            // Swap the elements at i and randomIndex
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public float GetUnitDistanceFromNearestFoe(BattleBotAgent unit){
        float furthestDistance = 99f;
        foreach(var agent in AgentsList){
            if(unit.gameObject != agent.agent.gameObject && unit.team != agent.agent.team){
                Vector3 point1 = unit.gameObject.transform.position;
                Vector3 point2 = agent.agent.gameObject.transform.position;
                float distance = Vector3.Distance(new Vector3(point1.x, 0, point1.z), new Vector3(point2.x, 0, point2.z));
                if(distance < furthestDistance)
                {
                    furthestDistance = distance;
                }
            }
        }

        return furthestDistance;
    }

    public List<BattleBotAgent> GetFoes(BattleBotAgent unit){
        List<BattleBotAgent> foes = new List<BattleBotAgent>();
        foreach(var agent in AgentsList){
            if(unit.gameObject != agent.agent.gameObject && unit.team != agent.agent.team){
                foes.Add(agent.agent);
            }
        }

        return foes;
    }

    public List<BattleBotAgent> GetAllies(BattleBotAgent unit){
        List<BattleBotAgent> foes = new List<BattleBotAgent>();
        foreach(var agent in AgentsList){
            if(unit.gameObject != agent.agent.gameObject && unit.team == agent.agent.team){
                foes.Add(agent.agent);
            }
        }

        return foes;
    }
}
