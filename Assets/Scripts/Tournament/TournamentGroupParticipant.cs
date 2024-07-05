using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data structure that holds all information for a participant in a tournament group.
/// </summary>
public class TournamentGroupParticipant
{
    public TournamentGroup Group { get; private set; }
    public Player Player { get; private set; }
    public Team Team { get; private set; }

    public int Rank { get; set; }
    public int NumMatches { get; set; }
    public int TotalMatchPointsGained { get; set; } // aka Anzahl Tore
    public int TotalMatchPointsLost { get; set; } // aka Anzahl Gegentore
    public int TotalMatchPointRatio => TotalMatchPointsGained - TotalMatchPointsLost;
    public int GroupPoints { get; set; }

    public TournamentGroupParticipant(TournamentGroup g, Player p)
    {
        Player = p;
    }
    public TournamentGroupParticipant(TournamentGroup g, Team t)
    {
        Team = t;
       
    }
}
