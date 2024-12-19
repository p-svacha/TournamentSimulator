using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A 1v1v1v1v1v1......v1 match
/// </summary>
public class FreeForAllMatch : Match
{
    public FreeForAllMatch(string name, Tournament tournament, int quarter, int day, int numPlayers, List<int> pointDistribution, TournamentGroup group = null)
        : base(name, tournament, quarter, day, numPlayers, pointDistribution, group) { }

    public override void SetDone()
    {
        MarkMatchAsDone();
        AdjustPlayerElos();
        SetPlayerAdvancements();
        TryConcludeParents();
    }

    private void SetPlayerAdvancements()
    {
        for (int i = 0; i < NumAdvancements; i++)
        {
            int rank = i;
            Player advancingPlayer = PlayerRanking[rank];
            Match targetMatch = Tournament.Matches[TargetMatchIndices[rank]];
            Debug.Log(advancingPlayer.ToString() + " is advancing to " + targetMatch.ToString());
            int targetSeed = rank;
            if (TargetMatchSeeds.Count > 0) targetSeed = TargetMatchSeeds[rank];
            Debug.Log($"{advancingPlayer.Name} is advancing to {targetMatch} as seed {targetSeed}.");
            targetMatch.AddPlayerToMatch(advancingPlayer, targetSeed);
        }
    }

    #region Save / Load

    public FreeForAllMatch(MatchData data) : base(data) { }

    #endregion
}
