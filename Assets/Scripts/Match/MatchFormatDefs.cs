using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MatchFormatDefs
{
    public static List<MatchFormatDef> Defs => new List<MatchFormatDef>()
    {
        new MatchFormatDef()
        {
            DefName = "SingleGame",
            Label = "single game",
            Description = "A single game is played. The result of that game is equal to the result of the match.",
        },
    };
}
