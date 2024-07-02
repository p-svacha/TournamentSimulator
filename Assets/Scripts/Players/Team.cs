using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Team
{
    public int Id { get; private set; }
    public string Name { get; protected set; }
    public List<Player> Players { get; protected set; }
    public Sprite Image { get; protected set; }
    public Color Color1 { get; protected set; }
    public Color Color2 { get; protected set; }
    public Country Country { get; protected set; } // null if not a country team
    public bool IsCountryTeam => Country != null;

    public int Elo { get; protected set; }

    // New country team
    public Team(Country c)
    {
        Id = Database.GetNewTeamId();

        Name = c.Name;
        Players = Database.Players.Values.Where(x => x.Country == c).ToList();
        Image = c.Flag;
        Country = c;

        Color[] mostCommonFlagColors = HelperFunctions.GetMostCommonColors(c.Flag);
        Color1 = mostCommonFlagColors[0];
        Color2 = mostCommonFlagColors[1];

        Elo = TournamentSimulator.DEFAULT_RATING;
    }

    public void AddPlayer(Player p)
    {
        Players.Add(p);
    }

    /// <summary>
    /// Returns a list of players that will play for this team for the given match when the match is starting.
    /// </summary>
    public List<Player> GetPlayersForMatch(TeamMatch m)
    {
        // Sort players by their elo and take top x for match
        return Players.OrderByDescending(x => x.Elo).Take(m.NumPlayersPerTeam).ToList();
    }

    public void SetElo(int value)
    {
        Elo = value;
    }

    #region Save / Load

    public TeamData ToData()
    {
        TeamData data = new TeamData();
        data.Id = Id;
        data.Name = Name;
        data.Players = Players.Select(x => x.Id).ToList();
        data.Color1 = HelperFunctions.Color2Data(Color1);
        data.Color2 = HelperFunctions.Color2Data(Color2);
        data.CountryId = Country == null ? -1 : Country.Id;
        data.Elo = Elo;
        return data;
    }

    public Team(TeamData data)
    {
        Id = data.Id;
        Name = data.Name;
        Players = data.Players.Select(x => Database.Players[x]).ToList();
        Color1 = HelperFunctions.Data2Color(data.Color1);
        Color2 = HelperFunctions.Data2Color(data.Color2);
        Country = data.CountryId == -1 ? null : Database.Countries[data.CountryId];
        Elo = data.Elo;

        if (IsCountryTeam) Image = Country.Flag;
    }

    #endregion

}
