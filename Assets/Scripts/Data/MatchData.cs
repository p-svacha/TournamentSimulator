using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MatchData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int TournamentId { get; set; }
    public int Quarter { get; set; }
    public int Day { get; set; }
    public bool IsDone { get; set; }
    public int NumPlayers { get; set; }
    public List<int> TargetMatchIndices { get; set; }
    public List<int> PointDistribution { get; set; }

    public List<MatchParticipantData> Participants { get; set; }
    public List<MatchRoundData> Rounds { get; set; }
}
