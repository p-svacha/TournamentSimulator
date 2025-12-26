using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

/// <summary>
/// Wrapper that contains info about a medal a player/country received.
/// </summary>
public class MedalInfo
{
    public Player Player { get; private set; }
    public Team Team { get; private set; }
    public Medal Medal { get; private set; }
    public TournamentType TournamentType { get; private set; }
    public string Text { get; private set; }

    public MedalInfo (Player player, Medal medal, TournamentType type, string text)
    {
        Player = player;
        Init(medal, type, text);
    }

    public MedalInfo (Team team, Medal medal, TournamentType type, string text)
    {
        Team = team;
        Init(medal, type, text);
    }

    private void Init(Medal medal, TournamentType type, string text)
    {
        Medal = medal;
        TournamentType = type;
        Text = text;
    }
}
