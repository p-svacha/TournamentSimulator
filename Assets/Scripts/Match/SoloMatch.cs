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

    public SoloMatch(string name, Tournament tournament, int quarter, int day, MatchFormatDef format, int maxPlayers, List<int> pointDistribution = null, int minPlayers = 0, TournamentGroup group = null, bool isKnockout = false, int knockoutStartingLives = 0, int koLiveGainers = 0, int koLiveLosers = 0)
        : base(name, tournament, quarter, day, format, maxPlayers, pointDistribution, minPlayers, group, isKnockout, knockoutStartingLives, koLiveGainers, koLiveLosers)
    {
        IsTeamMatch = false;
    }

    public override int NumParticipants => PlayerParticipants.Count;

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
        for (int i = 0; i < AdvancementsTargets.Count; i++)
        {
            MatchAdvancementTarget advancement = AdvancementsTargets[i];

            Player advancingPlayer = PlayerRanking[advancement.SourceRank];
            Debug.Log(advancingPlayer.ToString() + " is advancing to " + advancement.TargetMatch.ToString());
            int targetSeed = advancement.TargetSeed;
            Debug.Log($"{advancingPlayer.Name} is advancing to {advancement.TargetMatch} as seed {targetSeed}.");
            advancement.TargetMatch.AddPlayerToMatch(advancingPlayer, targetSeed);
        }
    }

    #region Save / Load

    public SoloMatch(MatchData data) : base(data) { }

    #endregion
}
