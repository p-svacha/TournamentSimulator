using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A 1v1v1v1v1v1......v1 match
/// </summary>
public class SoloMatch : Match
{
    public new List<SoloGame> Games => base.Games.Select(g => (SoloGame)g).ToList();

    public SoloMatch(string name, Tournament tournament, int quarter, int day, MatchFormatDef format, int numPlayers, List<int> pointDistribution, TournamentGroup group = null)
        : base(name, tournament, quarter, day, format, numPlayers, pointDistribution, group)
    {
        IsTeamMatch = false;
    }

    public override int NumParticipants => NumPlayers;

    protected override Game CreateGame(int index, List<GameModifierDef> gameModifiers)
    {
        return new SoloGame(this, index, gameModifiers);
    }

    protected override void SetDone()
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

    public SoloMatch(MatchData data) : base(data) { }

    #endregion
}
