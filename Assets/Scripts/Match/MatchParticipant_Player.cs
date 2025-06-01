using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchParticipant_Player
{
    public Match Match { get; private set; }
    public Player Player { get; private set; }
    public int Seed { get; private set; }
    public Team Team { get; private set; }
    public int MatchScore { get; private set; }
    public int EloBeforeMatch { get; private set; }
    public int EloAfterMatch { get; private set; }
    public int LeaguePointsBeforeMatch { get; private set; }

    public MatchParticipant_Player(Match match, Player player, int seed, Team team)
    {
        Match = match;
        Player = player;
        Seed = seed;
        Team = team;
    }

    public void SetPreMatchStats()
    {
        EloBeforeMatch = Player.Elo[Match.Discipline];
        LeaguePointsBeforeMatch = Player.League == null ? -1 : Player.CurrentLeaguePoints;
    }
    public void SetEloAfterMatch(int eloAfterMatch)
    {
        EloAfterMatch = eloAfterMatch;
    }


    #region Save / Load

    public MatchParticipantData ToData()
    {
        MatchParticipantData data = new MatchParticipantData();
        data.PlayerId = Player.Id;
        data.Seed = Seed;
        data.Team = Team == null ? - 1 : Team.Id;
        data.MatchScore = MatchScore;
        data.EloBeforeMatch = EloBeforeMatch;
        data.EloAfterMatch = EloAfterMatch;
        data.LeaguePointsBeforeMatch = LeaguePointsBeforeMatch;
        return data;
    }

    public MatchParticipant_Player(Match match, MatchParticipantData data)
    {
        Match = match;
        Player = Database.GetPlayer(data.PlayerId);
        Seed = data.Seed;
        Team = data.Team == -1 ? null : Database.GetTeam(data.Team);
        MatchScore = data.MatchScore;
        EloBeforeMatch = data.EloBeforeMatch;
        EloAfterMatch = data.EloAfterMatch;
        LeaguePointsBeforeMatch = data.LeaguePointsBeforeMatch;
    }

    #endregion
}
