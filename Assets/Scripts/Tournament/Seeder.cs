using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class providing general seeding functionality.
/// </summary>
public static class Seeder
{
    /// <summary>
    /// Seeds players into the first round of matches in a 1v1 single elimination tournament bracket.
    /// <br/>Matches pairs like 1 vs 64, 2 vs 63, etc., recursively ordering matches so the highest seeds meet as late as possible.
    /// </summary>
    public static void SeedSingleElimTournament(List<Player> players, List<Match> initialMatches)
    {
        if (players.Count == 0) return;

        // 1. Determine Bracket Size (Power of 2)
        // e.g., if 50 players, we need a bracket of size 64.
        int bracketSize = 2;
        while (bracketSize < players.Count) bracketSize *= 2;

        int numMatches = bracketSize / 2;
        if (initialMatches.Count < numMatches)
        {
            Debug.LogError($"[Seeder] Not enough matches provided. Need {numMatches} but got {initialMatches.Count}.");
            return;
        }

        // 2. Generate Seeding Order
        // The "order" list represents the seed numbers (0-based) in the order they appear in the bracket from top to bottom.
        // Round 1 (2 players): [0, 1]
        // Round 2 (4 players): [0, 3, 1, 2]  <- 1 plays 4 (idx 3), 2 plays 3 (idx 2)
        // Round 3 (8 players): [0, 7, 3, 4, 1, 6, 2, 5] ...
        List<int> seedOrder = new List<int>() { 0, 1 };

        while (seedOrder.Count < bracketSize)
        {
            List<int> nextOrder = new List<int>();
            int currentSize = seedOrder.Count * 2; // Target size for this step (e.g. 4, then 8, then 16)

            // For every existing seed S, the new match is S vs (Size - 1 - S)
            // e.g. converting size 2 [0, 1] to size 4:
            // Match A: 0 (from prev) vs 3 (new)
            // Match B: 1 (from prev) vs 2 (new)
            // But we append them sequentially: Seed 0, New Opponent, Seed 1, New Opponent

            foreach (int seed in seedOrder)
            {
                nextOrder.Add(seed);
                nextOrder.Add((currentSize - 1) - seed);
            }
            seedOrder = nextOrder;
        }

        // 3. Assign Players to Matches
        // The seedOrder list now tells us exactly which global seed sits in which slot.
        // e.g. for 8 players: Match 1 has seeds (0, 7), Match 2 has seeds (3, 4), etc.
        // Note: 'seedOrder' is length 64. 'initialMatches' is length 32. 
        // We iterate matches 0 to 31.
        // Match 0 takes seedOrder[0] and seedOrder[1].
        // Match 1 takes seedOrder[2] and seedOrder[3].

        for (int i = 0; i < numMatches; i++)
        {
            Match m = initialMatches[i];

            // Slot 1 (Home)
            int seedIndex1 = seedOrder[i * 2];
            if (seedIndex1 < players.Count)
            {
                m.AddPlayerToMatch(players[seedIndex1], seedIndex1);
            }

            // Slot 2 (Away)
            int seedIndex2 = seedOrder[(i * 2) + 1];
            if (seedIndex2 < players.Count)
            {
                m.AddPlayerToMatch(players[seedIndex2], seedIndex2);
            }
        }
    }

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
    /// Fills a globally seeded list of teams in a set of groups by applying a fair snake seed. (1 -> 2 -> 3 -> 4 -> 4 -> 3 -> 2 -> 1 -> 1 -> 2 -> 3 ... etc.)
    /// </summary>
    public static void SnakeSeedTeamsIntoGroups(List<Team> tournamentParticipants, List<TournamentGroup> groups)
    {
        List<List<Team>> seededGroups = GetSnakeSeededGroups(tournamentParticipants, groups.Count);
        for (int i = 0; i < groups.Count; i++) groups[i].SetParticipants(seededGroups[i]);
    }

    /// <summary>
    /// Fills a globally seeded list of players in a set of groups by applying a fair snake seed. (1 -> 2 -> 3 -> 4 -> 4 -> 3 -> 2 -> 1 -> 1 -> 2 -> 3 ... etc.)
    /// </summary>
    public static void SnakeSeedPlayersIntoGroups(List<Player> tournamentParticipants, List<TournamentGroup> groups)
    {
        List<List<Player>> seededGroups = GetSnakeSeededGroups(tournamentParticipants, groups.Count);
        for (int i = 0; i < groups.Count; i++) groups[i].SetParticipants(seededGroups[i]);
    }

    private static List<List<T>> GetSnakeSeededGroups<T>(List<T> participants, int numGroups)
    {
        List<List<T>> seededGroups = new List<List<T>>();
        for (int i = 0; i < numGroups; i++) seededGroups.Add(new List<T>());

        int currentGroupId = 0;
        bool snakeForward = true;

        for (int i = 0; i < participants.Count; i++)
        {
            int globalSeed = i;
            seededGroups[currentGroupId].Add(participants[i]);

            // Advance snake seeding
            if (snakeForward)
            {
                if (currentGroupId < numGroups - 1) currentGroupId++;
                else snakeForward = false;
            }
            else
            {
                if (currentGroupId > 0) currentGroupId--;
                else snakeForward = true;
            }
        }

        return seededGroups;
    }


    /// <summary>
    /// Generates advancements using a "Deck of Cards" approach.
    /// <br/>1. Collects all players into a list (Deck), rotating sources based on rank to ensure mixing.
    /// <br/>2. Deals them into target matches using Snake distribution.
    /// </summary>
    public static void CreateSnakeSeededAdvancements(List<Match> sourceMatches, List<Match> targetMatches, int numAdvancements, int advancementOffset = 0, int targetSeedOffset = 0)
    {
        // ---------------------------------------------------------
        // 1. VALIDATION
        // ---------------------------------------------------------
        int totalAdvancingPlayers = 0;
        foreach (var m in sourceMatches)
        {
            int limit = numAdvancements == -1 ? m.MaxPlayers : advancementOffset + numAdvancements;
            int count = Mathf.Max(0, limit - advancementOffset);
            totalAdvancingPlayers += count;
        }

        if (totalAdvancingPlayers % targetMatches.Count != 0 && targetMatches.Count > 1)
        {
            Debug.LogError($"[Seeder] Uneven bracket fill detected: {totalAdvancingPlayers} players -> {targetMatches.Count} matches.");
            throw new System.Exception("Advancement setup resulted in uneven bracket fill.");
        }

        // ---------------------------------------------------------
        // 2. SETUP TRACKING
        // ---------------------------------------------------------

        // Track how many times a Target Match has received a player from a specific Source Match.
        // targetId -> sourceId -> count
        Dictionary<int, Dictionary<int, int>> mixingHistory = new Dictionary<int, Dictionary<int, int>>();
        foreach (var t in targetMatches)
        {
            mixingHistory[t.Id] = new Dictionary<int, int>();
            foreach (var s in sourceMatches) mixingHistory[t.Id][s.Id] = 0;
        }

        // Track seed indices for targets (to ensure we fill seeds 0, 1, 2... sequentially)
        Dictionary<Match, int> seedsAssignedToTarget = new Dictionary<Match, int>();
        foreach (Match m in targetMatches) seedsAssignedToTarget.Add(m, 0);

        // Prepare the final list
        Dictionary<Match, List<MatchAdvancementTarget>> distribution = new Dictionary<Match, List<MatchAdvancementTarget>>();
        foreach (Match m in sourceMatches) distribution.Add(m, new List<MatchAdvancementTarget>());


        // ---------------------------------------------------------
        // 3. GENERATE & ASSIGN
        // ---------------------------------------------------------

        // Determine the highest rank we need to calculate
        int maxPlayersInSource = 0;
        foreach (var m in sourceMatches) maxPlayersInSource = Mathf.Max(maxPlayersInSource, m.MaxPlayers);
        int rankLimit = numAdvancements == -1 ? maxPlayersInSource : advancementOffset + numAdvancements;

        // GLOBAL SNAKE INDEX: Tracks total players assigned across all ranks
        int globalSequence = 0;

        // Process Rank by Rank (Fairness Priority 1)
        for (int rank = advancementOffset; rank < rankLimit; rank++)
        {
            // A. Collect all CANDIDATES (Players) available for this rank
            List<Match> availableSources = new List<Match>();
            foreach (var source in sourceMatches)
            {
                int limit = numAdvancements == -1 ? source.MaxPlayers : advancementOffset + numAdvancements;
                if (rank < limit) availableSources.Add(source);
            }

            // B. Collect all SLOTS (Targets) needed for this rank
            // We determine this by running the Snake Logic for N steps, where N is the number of candidates.
            List<Match> openSlots = new List<Match>();
            for (int i = 0; i < availableSources.Count; i++)
            {
                int numTargets = targetMatches.Count;
                int snakeCycle = numTargets * 2;
                int cyclePos = globalSequence % snakeCycle;

                int targetMatchIdx = (cyclePos < numTargets) ? cyclePos : (snakeCycle - 1) - cyclePos;
                openSlots.Add(targetMatches[targetMatchIdx]);

                globalSequence++;
            }

            // C. MATCHING: Greedy Least-Used Assignment
            // For each candidate source, find the best fit among open slots.

            // Optimization: Shuffle sources slightly or offset them to prevent bias in tie-breaking? 
            // Actually, simply shifting the starting index of the loop per rank helps.
            int shift = rank - advancementOffset;

            for (int i = 0; i < availableSources.Count; i++)
            {
                // Get Source (Rotated iteration to avoid Order Bias)
                Match source = availableSources[(i + shift) % availableSources.Count];

                // Find the best slot for this source
                // Criteria: The target that has seen this source the LEAST.
                Match bestSlot = null;
                int lowestCount = int.MaxValue;
                int bestSlotIndex = -1;

                for (int s = 0; s < openSlots.Count; s++)
                {
                    Match slot = openSlots[s];
                    int count = mixingHistory[slot.Id][source.Id];

                    if (count < lowestCount)
                    {
                        lowestCount = count;
                        bestSlot = slot;
                        bestSlotIndex = s;
                    }
                }

                // Assign
                openSlots.RemoveAt(bestSlotIndex); // Remove slot so it's not used twice

                // Record History
                mixingHistory[bestSlot.Id][source.Id]++;

                // Assign Seed
                int targetSeed = targetSeedOffset + seedsAssignedToTarget[bestSlot];
                seedsAssignedToTarget[bestSlot]++;

                distribution[source].Add(new MatchAdvancementTarget(source, rank, bestSlot, targetSeed));
            }
        }

        // ---------------------------------------------------------
        // 4. APPLY
        // ---------------------------------------------------------
        foreach (var kvp in distribution)
        {
            kvp.Key.SetAdvancements(kvp.Value);
        }
    }
}
