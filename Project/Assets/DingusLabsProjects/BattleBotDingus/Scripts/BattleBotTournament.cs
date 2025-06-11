using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static BattleBotEnvController;

public class TournamentTeam
{
    public List<GameObject> Members { get; private set; }
    public BattleTeam TeamColor { get; private set; }
    public string Name { get; private set; }
    public bool IsEliminated { get; set; }
    public bool IsInLosersBracket { get; set; }

    public TournamentTeam(List<GameObject> members, BattleTeam teamColor, string name)
    {
        Members = members;
        TeamColor = teamColor;
        Name = name;
        IsEliminated = false;
        IsInLosersBracket = false;
    }
}

public class Battle
{
    public List<TournamentTeam> Teams { get; private set; }
    public int RequiredWins { get; private set; }
    private Dictionary<string, int> Victories { get; set; }

    public Battle(List<TournamentTeam> teams, int requiredWins)
    {
        Teams = teams;
        RequiredWins = requiredWins;
        Victories = teams.ToDictionary(t => t.Name, t => 0);
    }

    public void RecordResult(string winnerName)
    {
        var winningTeam = Teams.FirstOrDefault(t => t.Name == winnerName);
        if (winningTeam == null)
        {
            Debug.LogError($"Team with name '{winnerName}' not found in this battle.");
            return;
        }

        if (!Victories.ContainsKey(winnerName))
        {
            Victories[winnerName] = 0;
        }

        Victories[winnerName]++;
    }

    public bool IsBattleComplete()
    {
        return Victories.Any(v => v.Value >= RequiredWins);
    }

    public TournamentTeam GetWinner()
    {
        var winningName = Victories.OrderByDescending(v => v.Value).First().Key;
        return Teams.First(t => t.Name == winningName);
    }
}

public class Tournament
{
    public List<TournamentTeam> AllTeams { get; set; }
    private Queue<Battle> WinnersBracket { get; set; }
    private Queue<Battle> LosersBracket { get; set; }
    private int TeamsPerBattle { get; set; }
    public int RequiredWinsPerBattle { get; set; }
    private bool IsFinalRound { get; set; }
    private bool HasStarted { get; set; }

    public Tournament(List<TournamentTeam> teams, int teamsPerBattle, int requiredWinsPerBattle)
    {
        AllTeams = teams;
        TeamsPerBattle = teamsPerBattle;
        RequiredWinsPerBattle = requiredWinsPerBattle;
        WinnersBracket = new Queue<Battle>();
        LosersBracket = new Queue<Battle>();
        IsFinalRound = false;
        HasStarted = false;

        InitializeFirstRound();
    }

    private void InitializeFirstRound()
    {
        var groups = new List<List<TournamentTeam>>();
        for (int i = 0; i < AllTeams.Count; i += TeamsPerBattle)
        {
            groups.Add(AllTeams.Skip(i).Take(TeamsPerBattle).ToList());
        }

        foreach (var group in groups)
        {
            WinnersBracket.Enqueue(new Battle(group, RequiredWinsPerBattle));
        }
    }

    public Battle GetNextBattle()
    {
        HasStarted = true;

        // Handle the final match explicitly.
        if (IsFinalRound && WinnersBracket.Count > 0)
        {
            return WinnersBracket.Peek();
        }

        if (WinnersBracket.Count > 0)
        {
            return WinnersBracket.Peek();
        }

        if (LosersBracket.Count > 0)
        {
            return LosersBracket.Peek();
        }

        // If brackets are empty, try to generate the next round.
        if (WinnersBracket.Count == 0 && LosersBracket.Count == 0 && !IsFinalRound)
        {
            GenerateNextRound();
        }

        // Recheck brackets after generating.
        if (WinnersBracket.Count > 0)
        {
            return WinnersBracket.Peek();
        }

        if (LosersBracket.Count > 0)
        {
            return LosersBracket.Peek();
        }

        Debug.LogWarning("No battles available. Check tournament state.");
        return null;
    }

    public void ProcessBattleResult(string winnerName)
    {
        Battle currentBattle;

        // Determine the current battle
        if (WinnersBracket.Count > 0)
        {
            currentBattle = WinnersBracket.Peek();
        }
        else if (LosersBracket.Count > 0)
        {
            currentBattle = LosersBracket.Peek();
        }
        else
        {
            Debug.LogError("No battles available to process. Check tournament state.");
            return;
        }

        // Record the result using the team name
        var winningTeam = currentBattle.Teams.FirstOrDefault(t => t.Name == winnerName);
        if (winningTeam == null)
        {
            Debug.LogError($"No team found with the name '{winnerName}' in the current battle.");
            return;
        }

        currentBattle.RecordResult(winningTeam.Name);

        if (currentBattle.IsBattleComplete())
        {
            var battleWinner = currentBattle.GetWinner();
            var losers = currentBattle.Teams.Where(t => t != battleWinner).ToList();

            if (WinnersBracket.Count > 0)
            {
                WinnersBracket.Dequeue();
                foreach (var loser in losers)
                {
                    if (!loser.IsInLosersBracket)
                    {
                        loser.IsInLosersBracket = true;
                    }
                    else
                    {
                        loser.IsEliminated = true;
                    }
                }
            }
            else if (LosersBracket.Count > 0)
            {
                LosersBracket.Dequeue();
                foreach (var loser in losers)
                {
                    loser.IsEliminated = true;
                }
            }
        }
    }

    private void GenerateNextRound()
    {
        var activeTeams = AllTeams.Where(t => !t.IsEliminated).ToList();

        // If only one team is left, the tournament is complete.
        if (activeTeams.Count == 1)
        {
            IsFinalRound = true;
            return;
        }

        // Separate active teams into winners' and losers' brackets.
        var winnersBracketTeams = activeTeams.Where(t => !t.IsInLosersBracket).ToList();
        var losersBracketTeams = activeTeams.Where(t => t.IsInLosersBracket).ToList();

        // Check if we are ready for the final match.
        if (winnersBracketTeams.Count == 1 && losersBracketTeams.Count == 1)
        {
            IsFinalRound = true;
            WinnersBracket.Enqueue(new Battle(
                new List<TournamentTeam> { winnersBracketTeams[0], losersBracketTeams[0] },
                RequiredWinsPerBattle
            ));
            return;
        }

        // Generate Winners Bracket matches.
        for (int i = 0; i < winnersBracketTeams.Count; i += TeamsPerBattle)
        {
            var battleTeams = winnersBracketTeams.Skip(i).Take(TeamsPerBattle).ToList();

            if (battleTeams.Count >= 2)
            {
                WinnersBracket.Enqueue(new Battle(battleTeams, RequiredWinsPerBattle));
            }
            else if (battleTeams.Count == 1)
            {
                // Automatically advance the remaining team if no battle is possible.
                var autoAdvanceTeam = battleTeams[0];
                autoAdvanceTeam.IsInLosersBracket = false;
            }
        }

        // Generate Losers Bracket matches.
        for (int i = 0; i < losersBracketTeams.Count; i += TeamsPerBattle)
        {
            var battleTeams = losersBracketTeams.Skip(i).Take(TeamsPerBattle).ToList();

            if (battleTeams.Count >= 2)
            {
                LosersBracket.Enqueue(new Battle(battleTeams, RequiredWinsPerBattle));
            }
            else if (battleTeams.Count == 1)
            {
                // Automatically eliminate the remaining team if no battle is possible.
                var autoEliminateTeam = battleTeams[0];
                autoEliminateTeam.IsEliminated = true;
            }
        }
    }

    public bool IsComplete()
    {
        // Tournament is complete when the final round is over and brackets are empty.
        return IsFinalRound && WinnersBracket.Count == 0 && LosersBracket.Count == 0;
    }

    /// <summary>
    /// Compares two teams to determine if they contain the same competitors.
    /// </summary>
    /// <param name="team1">The first team to compare.</param>
    /// <param name="team2">The second team to compare.</param>
    /// <returns>True if the teams contain the exact same members, false otherwise.</returns>
    public bool CompareTeams(TournamentTeam team1, TournamentTeam team2)
    {
        if (team1 == null || team2 == null) return false;
        if (team1.Members.Count != team2.Members.Count) return false;

        foreach (var member in team1.Members)
        {
            if (!team2.Members.Contains(member))
            {
                return false;
            }
        }

        foreach (var member in team2.Members)
        {
            if (!team1.Members.Contains(member))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Compares two battles to determine if they contain the same teams with the same members.
    /// </summary>
    /// <param name="battle1">The first battle to compare.</param>
    /// <param name="battle2">The second battle to compare.</param>
    /// <returns>True if the battles contain the same teams with identical members, false otherwise.</returns>
    public bool BattlesHaveSameTeams(Battle battle1, Battle battle2)
    {
        // Ensure neither battle is null
        if (battle1 == null || battle2 == null)
        {
            Debug.LogError("One or both battles are null. Comparison is invalid.");
            return false;
        }

        // Check if both battles have the same number of teams
        if (battle1.Teams.Count != battle2.Teams.Count)
        {
            return false;
        }

        // Create a list to track matched teams
        var matchedTeams = new HashSet<int>();

        // Compare each team in battle1 to a team in battle2
        foreach (var team1 in battle1.Teams)
        {
            bool matchFound = false;

            for (int i = 0; i < battle2.Teams.Count; i++)
            {
                // Skip already matched teams
                if (matchedTeams.Contains(i)) continue;

                if (CompareTeams(team1, battle2.Teams[i]))
                {
                    matchFound = true;
                    matchedTeams.Add(i);
                    break;
                }
            }

            if (!matchFound)
            {
                // If no matching team is found for a team in battle1, battles are not identical
                return false;
            }
        }

        // All teams in battle1 have matching teams in battle2
        return true;
    }
}