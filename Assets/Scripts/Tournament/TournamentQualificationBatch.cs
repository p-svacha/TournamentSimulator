using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TournamentQualificationBatch
{
    /// <summary>
    /// How many participants qualify through this patch.
    /// </summary>
    public int NumParticipants { get; init; }

    /// <summary>
    /// If true, this criteria is an invitation from the top of the elo leaderboard of the discipline.
    /// </summary>
    public bool IsInvitation { get; init; }

    /// <summary>
    /// If not null, the players for this batch will qualify through reaching the top ranks of this tournament.
    /// </summary>
    public TournamentDef Tournament { get; init; }
}
