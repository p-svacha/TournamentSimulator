using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TournamentFormatDefs
{
    public static List<TournamentFormatDef> Defs => new List<TournamentFormatDef>()
    {
        new TournamentFormatDef()
        {
            DefName = "SeparateSingleElimQualifier16To1Bo5",
            Label = "Multiple separate 16->1 Single Elimination Brackets",
            Description = "Seperate single eleminiation brackets with 16 players in each. 1 qualifier per bracket. All matches are Best of 5.",
            FormatClass = typeof(TournamentFormat_SingleElimQualifier64To4),
        },

        new TournamentFormatDef()
        {
            DefName = "Groups4x4Into12Playoffs",
            Label = "Warlords format",
            Description = "4 groups of 4 players with round robin. First place advances to quarters, second and third to Round of 12. Group stage bo5, Playoffs bo7 with bo9 final.",
            FormatClass = typeof(TournamentFormat_SingleElimQualifier64To4),
        },
    };
}
