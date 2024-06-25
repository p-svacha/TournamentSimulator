using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player
{
    public TournamentSimulator Sim;

    public int Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Country Country { get; private set; }
    public string Sex { get; private set; }
    public int Elo { get; private set; }
    public LeagueType LeagueType { get; private set; }
    public Dictionary<SkillId, int> Skills { get; private set; }

    public League League => Sim.GetCurrentLeague(LeagueType);
    public int CurrentLeaguePoints => League.Standings[this];
    public int LeagueRank => League.GetRankOf(this);
    public Sprite LeagueIcon => League == null ? ColorManager.Singleton.NoLeagueIcon : League.Icon;
    public Color LeagueColor => League == null ? ColorManager.Singleton.NoLeagueColor : League.Color;
    public Sprite FlagSprite => Resources.Load<Sprite>("Icons/Flags/" + Country.Name.Replace(" ", "-").ToLower());
    public int Age => 20 + (Sim.Season - Database.Leagues.Values.Where(x => x.Players.Contains(this)).Min(x => x.Season));
    public int WorldRank => Database.Players.Values.OrderByDescending(x => x.Elo).ToList().IndexOf(this) + 1;

    // New player
    public Player(TournamentSimulator sim, string firstName, string lastName, Country country, string sex, int elo, Dictionary<SkillId, int> skills)
    {
        Id = Database.GetNewPlayerId();

        Sim = sim;
        FirstName = firstName;
        LastName = lastName;
        Sex = sex;
        Country = country;
        Elo = elo;
        Skills = skills;

        LeagueType = LeagueType.None;
    }

    public void SetElo(int value)
    {
        Elo = value;
    }
    public void SetLeague(LeagueType league)
    {
        LeagueType = (LeagueType)league;
    }


    public override string ToString() => Name + " (" + Id + ")";
    public string Name => FirstName + " " + LastName;

    #region Save / Load

    public PlayerData ToData()
    {
        PlayerData data = new PlayerData();
        data.Id = Id;
        data.FirstName = FirstName;
        data.LastName = LastName;
        data.CountryId = Country.Id;
        data.Sex = Sex;
        data.Elo = Elo;
        data.LeagueType = (int)LeagueType;
        data.Skills = Skills.Select(x => new PlayerSkillData() { SkillId = (int)x.Key, Value = x.Value }).ToList();
        return data;
    }

    public Player(TournamentSimulator sim, PlayerData data)
    {
        Sim = sim;

        Id = data.Id;
        FirstName = data.FirstName;
        LastName = data.LastName;
        Country = Database.Countries[data.CountryId];
        Sex = data.Sex;
        Elo = data.Elo;
        LeagueType = (LeagueType)data.LeagueType;
        Skills = data.Skills.ToDictionary(x => (SkillId)x.SkillId, x => x.Value);
    }

    #endregion

}
