using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class providing general seeding functionality.
/// </summary>
public static class Seeder
{
    /// <summary>
    /// Fills a globally seeded list of players in a set of initial matches by applying a fair snake seed. (1 -> 2 -> 3 -> 4 -> 4 -> 3 -> 2 -> 1 -> 1 -> 2 -> 3 ... etc.)
    /// </summary>
    public static void SnakeSeedSoloTournament(List<Player> tournamentParticipants, List<Match> initialMatches)
    {
        int currentMatchId = 0;
        int currentMatchSeed = 0;
        bool snakeForward = true;

        for (int i = 0; i < tournamentParticipants.Count; i++)
        {
            int globalSeed = i;
            initialMatches[currentMatchId].AddPlayerToMatch(tournamentParticipants[i], currentMatchSeed);

            // Advance snake seeding
            if (snakeForward)
            {
                if (currentMatchId < initialMatches.Count - 1) currentMatchId++;
                else
                {
                    currentMatchSeed++;
                    snakeForward = false;
                }
            }
            else
            {
                if (currentMatchId > 0) currentMatchId--;
                else
                {
                    currentMatchSeed++;
                    snakeForward = true;
                }
            }
        }
    }


    /// <summary>
    /// Generates and sets the advancement information in all source matches, given how many in each of those matches advance to the group of target matches. Applies even and fair seeding.
    /// </summary>
    /// <param name="sourceMatches">Matches that the players advance from.</param>
    /// <param name="targetMatches">Matches that the players advance to.</param>
    /// <param name="numAdvancements">How many players in each source match advances to the phase with the target matches. If -1, advancements for all remaining players are generated.</param>
    /// <param name="advancementOffset">At what rank the advancements start. Useful for advancements into loser brackets.</param>
    /// <param name="targetSeedOffset">At what seed in the target matches it starts.</param>
    public static void CreateSnakeSeededAdvancements(List<Match> sourceMatches, List<Match> targetMatches, int numAdvancements, int advancementOffset = 0, int targetSeedOffset = 0)
    {
        // --- VALIDATION START ---
        // Calculate the total number of players advancing in this specific batch
        int totalAdvancingPlayers = 0;
        foreach (var m in sourceMatches)
        {
            int limit = numAdvancements == -1 ? m.MaxPlayers : advancementOffset + numAdvancements;
            int count = Mathf.Max(0, limit - advancementOffset);
            totalAdvancingPlayers += count;
        }

        // The Critical Check: 
        // We must ensure the players distribute evenly across targets (Rectangular Fill).
        // If they don't (e.g., 21 players into 2 matches), one match will have more seeds filled than the other.
        // This makes 'targetSeedOffset' dangerous for subsequent batches merging into the same matches.
        if (totalAdvancingPlayers % targetMatches.Count != 0)
        {
            // Exception: If this is the Grand Final (1 target match), uneven numbers are always fine.
            if (targetMatches.Count > 1)
            {
                Debug.LogError($"[Seeder] CRITICAL SETUP ERROR: Uneven distribution detected!");
                Debug.LogError($"Trying to move {totalAdvancingPlayers} players into {targetMatches.Count} matches.");
                Debug.LogError($"This leaves a remainder of {totalAdvancingPlayers % targetMatches.Count}.");
                Debug.LogError($"Source: {sourceMatches[0].Name} | Target: {targetMatches[0].Name}");
                throw new System.Exception("Advancement setup resulted in uneven bracket fill. This breaks 'targetSeedOffset' logic.");
            }
        }
        // --- VALIDATION END ---


        // Group all advancing slots by Source Match to assign them later
        Dictionary<Match, List<MatchAdvancementTarget>> distribution = new Dictionary<Match, List<MatchAdvancementTarget>>();
        foreach (Match m in sourceMatches) distribution.Add(m, new List<MatchAdvancementTarget>());

        // Iterate "Global Ranks" (Rank 0 of Match 0, Rank 0 of Match 1, etc.)
        // This ensures purely even distribution across all targets.

        // Find maximum players to iterate safely if matches have uneven counts
        int maxPlayersInSource = 0;
        foreach (var m in sourceMatches) maxPlayersInSource = Mathf.Max(maxPlayersInSource, m.MaxPlayers);

        int rankLimit = numAdvancements == -1 ? maxPlayersInSource : advancementOffset + numAdvancements;

        for (int rank = advancementOffset; rank < rankLimit; rank++)
        {
            for (int sourceIdx = 0; sourceIdx < sourceMatches.Count; sourceIdx++)
            {
                Match sourceMatch = sourceMatches[sourceIdx];

                // Skip if this specific match doesn't have enough players for this rank
                int matchLimit = numAdvancements == -1 ? sourceMatch.MaxPlayers : advancementOffset + numAdvancements;
                if (rank >= matchLimit) continue;

                // Calculate a unique sequential ID for this player across the whole round
                // relativeRank 0 = First players from all matches
                int relativeRank = rank - advancementOffset;
                int globalSequence = (relativeRank * sourceMatches.Count) + sourceIdx;

                // Map Sequence -> Target Match (Snaking the Match Index)
                int numTargets = targetMatches.Count;
                int cyclePos = globalSequence % (numTargets * 2); // A full snake cycle is 2*N (There and back)

                int targetMatchIdx;
                if (cyclePos < numTargets) targetMatchIdx = cyclePos; // Forward (0 -> 1 -> 2)
                else targetMatchIdx = (2 * numTargets - 1) - cyclePos; // Backward (2 -> 1 -> 0)

                // Map Sequence -> Target Seed
                int targetSeed = (globalSequence / numTargets) + targetSeedOffset;

                // Add to distribution
                Match targetMatch = targetMatches[targetMatchIdx];
                distribution[sourceMatch].Add(new MatchAdvancementTarget(sourceMatch, rank, targetMatch, targetSeed));
            }
        }

        // Apply to matches
        foreach (var kvp in distribution)
        {
            kvp.Key.SetAdvancements(kvp.Value);
        }
    }
}
