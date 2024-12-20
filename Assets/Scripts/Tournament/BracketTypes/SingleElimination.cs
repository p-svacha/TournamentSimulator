using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleElimination : MonoBehaviour
{
    public static int GetFirstMatchId(int bracketSeed, int totalPlayers)
    {
        if ((totalPlayers & (totalPlayers - 1)) != 0)
        {
            throw new System.ArgumentException("Total players must be a power of 2.");
        }

        // Validate seed
        if (bracketSeed < 0 || bracketSeed >= totalPlayers)
        {
            throw new System.ArgumentException("Seed must be between 0 and totalPlayers - 1.");
        }

        // Adjust seed to 1-based for calculations
        int adjustedSeed = bracketSeed + 1;

        // Calculate the initial match for the given seed
        int pairedSeed = totalPlayers + 1 - adjustedSeed;
        int matchId = Mathf.Min(adjustedSeed, pairedSeed) - 1; // Adjust back to 0-based

        return matchId;
    }

    public static int GetSeedWithininFirstMatch(int bracketSeed, int totalPlayers)
    {
        if ((totalPlayers & (totalPlayers - 1)) != 0)
        {
            throw new System.ArgumentException("Total players must be a power of 2.");
        }

        // Validate seed
        if (bracketSeed < 0 || bracketSeed >= totalPlayers)
        {
            throw new System.ArgumentException("Seed must be between 0 and totalPlayers - 1.");
        }

        // Adjust seed to 1-based for calculations
        int adjustedSeed = bracketSeed + 1;

        // Calculate paired seed
        int pairedSeed = totalPlayers + 1 - adjustedSeed;

        // Return 0 if seed is the lower seed in the match, 1 if it is the higher seed
        return adjustedSeed < pairedSeed ? 0 : 1;
    }

    public static int GetGroupForSeed(int qualifierSeed, int totalPlayers, int groups)
    {
        if (groups <= 0 || totalPlayers % groups != 0)
        {
            throw new System.ArgumentException("Total players must be evenly divisible by the number of groups.");
        }

        int playersPerGroup = totalPlayers / groups;

        // Validate qualifier seed
        if (qualifierSeed < 0 || qualifierSeed >= totalPlayers)
        {
            throw new System.ArgumentException("Qualifier seed must be between 0 and totalPlayers - 1.");
        }

        // Fairly distribute seeds across groups using balanced seeding
        int roundRobinPosition = qualifierSeed % groups;
        int round = qualifierSeed / groups;
        int group = (round % 2 == 0) ? roundRobinPosition : (groups - 1 - roundRobinPosition);

        return group;
    }

    public static int GetSeedWithinGroup(int qualifierSeed, int totalPlayers, int groups)
    {
        if (groups <= 0 || totalPlayers % groups != 0)
        {
            throw new System.ArgumentException("Total players must be evenly divisible by the number of groups.");
        }

        int playersPerGroup = totalPlayers / groups;

        // Validate qualifier seed
        if (qualifierSeed < 0 || qualifierSeed >= totalPlayers)
        {
            throw new System.ArgumentException("Qualifier seed must be between 0 and totalPlayers - 1.");
        }

        // Calculate the seed within the group
        return qualifierSeed / groups;
    }
}
