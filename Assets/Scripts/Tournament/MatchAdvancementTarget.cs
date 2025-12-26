using UnityEngine;

/// <summary>
/// Holds information about where a participant advances to (match + seed in that match) within a tournament when reaching a specific rank in a match.
/// </summary>
public class MatchAdvancementTarget
{
    public Match SourceMatch;
    public int SourceRank; // 0-indexed

    public int TargetMatchId;
    public Match TargetMatch;
    public int TargetSeed; // 0-indexed

    public MatchAdvancementTarget(Match sourceMatch, int sourceRank, Match targetMatch, int targetSeed)
    {
        SourceMatch = sourceMatch;
        SourceRank = sourceRank;
        TargetMatch = targetMatch;
        TargetMatchId = targetMatch.Id;
        TargetSeed = targetSeed;
    }

    public MatchAdvancementTarget(Match sourceMatch, int sourceRank, int targetLocalId, int targetSeed)
    {
        SourceMatch = sourceMatch;
        SourceRank = sourceRank;
        TargetMatchId = targetLocalId;
        TargetSeed = targetSeed;
    }

    public MatchAdvancementTarget(Match match, MatchAdvancementData data)
    {
        SourceMatch = match;
        SourceRank = data.SourceRank;
        TargetMatchId = data.TargetMatchId;
        TargetSeed = data.TargetMatchSeed;
    }
    public void ResolveReferences()
    {
        TargetMatch = Database.GetMatch(TargetMatchId);
        TargetMatch.IncomingAdvancements.Add(this);
    }

    public MatchAdvancementData ToData()
    {
        MatchAdvancementData data = new MatchAdvancementData();
        data.SourceRank = SourceRank;
        data.TargetMatchId = TargetMatch.Id;
        data.TargetMatchSeed = TargetSeed;
        return data;
    }
}
