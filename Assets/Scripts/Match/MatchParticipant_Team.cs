using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchParticipant_Team
{
    public Team Team { get; private set; }
    public int Seed { get; private set; }
    public int TotalScore { get; private set; }
    public int EloBeforeMatch { get; private set; }
    public int EloAfterMatch { get; private set; }
    public List<string> Modifiers { get; private set; }

    public MatchParticipant_Team(Team team, int seed)
    {
        Team = team;
        Seed = seed;
    }

    public void IncreaseTotalScore(int totalScore)
    {
        TotalScore += totalScore;
    }

    public void SetPreMatchStats()
    {
        EloBeforeMatch = Team.Elo;
    }
    public void SetEloAfterMatch(int eloAfterMatch)
    {
        EloAfterMatch = eloAfterMatch;
    }
}