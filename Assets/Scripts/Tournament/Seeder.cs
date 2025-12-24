using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class providing general seeding functionality.
/// </summary>
public static class Seeder
{
    /// <summary>
    /// Generates and sets the advancement information in all source matches, given how many in each of those matches advance to the group of target matches. Applies even and fair seeding.
    /// </summary>
    /// <param name="sourceMatches">Matches that the players advance from.</param>
    /// <param name="targetMatches">Matches that the players advance to.</param>
    /// <param name="numAdvancements">How many players in each source match advances to the phase with the target matches. If -1, advancements for all remaining players are generated.</param>
    /// <param name="advancementOffset">At what rank the advancements start. Useful for advancements into loser brackets.</param>
    public static void CreateSnakeSeededAdvancements(List<Match> sourceMatches, List<Match> targetMatches, int numAdvancements, int advancementOffset = 0)
    {
        for (int i = 0; i < sourceMatches.Count; i++)
        {
            Match sourceMatch = sourceMatches[i];
            List<MatchAdvancementTarget> targets = new List<MatchAdvancementTarget>();

            int currentTargetMatchId = i; // We offset the start for each source match so no seeds are occupied double.
            int currentTargetSeed = 0;
            bool snakeForward = true;

            int limit = advancementOffset + numAdvancements;
            if (numAdvancements == -1) limit = sourceMatch.MaxPlayers;

            for (int rank = 0; rank < limit; rank++)
            {
                int sourceRank = rank;
                Match targetMatch = targetMatches[currentTargetMatchId];
                int targetSeed = currentTargetSeed;

                // Advance snake seeding
                if (snakeForward)
                {
                    if (currentTargetMatchId < targetMatches.Count - 1) currentTargetMatchId++;
                    else
                    {
                        currentTargetMatchId--;
                        snakeForward = false;
                    }
                }
                else
                {
                    if (currentTargetMatchId > 0) currentTargetMatchId--;
                    else
                    {
                        currentTargetMatchId++;
                        snakeForward = true;
                    }
                }

                // Seed always increases by 1 because next round has same amount of matches
                int seedIncreasePerRank = targetMatches.Count / sourceMatches.Count;
                currentTargetSeed += seedIncreasePerRank;

                targets.Add(new MatchAdvancementTarget(sourceMatch, sourceRank, targetMatch, targetSeed));
            }

            sourceMatch.SetAdvancements(targets);
        }
    }
}
