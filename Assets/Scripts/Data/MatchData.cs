using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MatchData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsTeamMatch { get; set; }
    public int TournamentId { get; set; }
    public int GroupIndex { get; set; } // index refers to position in group list of tournament
    public int Quarter { get; set; }
    public int Day { get; set; }
    public string Format { get; set; }
    public bool IsDone { get; set; }
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public List<MatchAdvancementData> AdvancementTargets { get; set; }
    public List<int> PointDistribution { get; set; }

    public List<MatchParticipantData> Participants { get; set; }

    // Only used for team matches
    public int NumTeams { get; set; }
    public int NumPlayersPerTeam { get; set; }
    public List<int> TeamPointDistribution { get; set; }
    public List<TeamMatchParticipantData> TeamParticipants { get; set; }
}
