using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player
{
    public int Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Country Country { get; private set; }
    public string Sex { get; private set; }
    public Dictionary<DisciplineDef, int> Elo { get; private set; }
    public TournamentType LeagueType { get; private set; }
    public Dictionary<SkillDef, Skill> Skills { get; private set; }

    public League League => Database.GetCurrentLeague(LeagueType);
    public int CurrentLeaguePoints => League.Standings[this];
    public int LeagueRank => League.GetRankOf(this);
    public Sprite LeagueIcon => League == null ? ColorManager.Singleton.NoLeagueIcon : League.Icon;
    public Color LeagueColor => League == null ? ColorManager.Singleton.NoLeagueColor : League.Color;
    public Sprite FlagBig => Country.FlagBig;
    public Sprite FlagSmall => Country.FlagSmall;
    public int Age { get; set; }

    public static string MISTAKE_MODIFIER = "Mistake";

    // Stats
    private List<Match> Matches = new List<Match>();
    public int NumCompletedMatches => Matches.Where(m => m.IsDone).Count();
    public int GetWorldRank(DisciplineDef discipline) => Database.AllPlayers.OrderByDescending(x => x.Elo[discipline]).ToList().IndexOf(this) + 1;

    // New player
    public Player(string firstName, string lastName, Country country, string sex, Dictionary<SkillDef, Skill> skills)
    {
        Id = Database.GetNewPlayerId();

        FirstName = firstName;
        LastName = lastName;
        Sex = sex;
        Country = country;
        Age = 20;
        Skills = new Dictionary<SkillDef, Skill>(skills);

        Elo = new Dictionary<DisciplineDef, int>();
        foreach (DisciplineDef discipline in DefDatabase<DisciplineDef>.AllDefs)
        {
            Elo.Add(discipline, TournamentSimulator.DEFAULT_RATING);
        }

        LeagueType = TournamentType.None;
    }

    public void AddMatch(Match m) => Matches.Add(m);
    public List<Match> GetMatches() => Matches;

    public float GetSkillBaseValue(SkillDef skillDef) => Skills[skillDef].BaseValue;

    /// <summary>
    /// Returns a match round result value for a specific skill, given the match state.
    /// </summary>
    public PlayerGameRound GetMatchRoundResult(SkillDef skillDef)
    {
        Skill skill = Skills[skillDef];
        return skill.GetGameRoundResult(this);
    }
    


    public void SetElo(DisciplineDef discipline, int value)
    {
        Elo[discipline] = value;
    }
    public void SetLeague(TournamentType league)
    {
        LeagueType = league;
        Debug.Log("League of " + Name + " has been changed to " + league.ToString());
    }

    /// <summary>
    /// Adjusts all values in all skills by a tiny value.
    /// </summary>
    public void AdjustEndOfGameSkills()
    {
        foreach (SkillDef skillDef in DefDatabase<SkillDef>.AllDefs)
        {
            Skills[skillDef].AdjustBaseValue(PlayerGenerator.GetRandomEndOfGameSkillAdjustment());
            Skills[skillDef].AdjustInconsistency(PlayerGenerator.GetRandomEndOfGameInconsistencyAdjustment());
            Skills[skillDef].AdjustMistakeChance(PlayerGenerator.GetRandomEndOfGameMistakeChanceAdjustment());
        }
    }

    /// <summary>
    /// Adjusts all values in all skills by a significant value.
    /// </summary>
    public void AdjustEndOfSeasonSkills()
    {
        foreach (SkillDef skillDef in DefDatabase<SkillDef>.AllDefs)
        {
            Skills[skillDef].AdjustBaseValue(PlayerGenerator.GetRandomEndOfSeasonSkillAdjustment());
            Skills[skillDef].AdjustInconsistency(PlayerGenerator.GetRandomEndOfSeasonInconsistencyAdjustment());
            Skills[skillDef].AdjustMistakeChance(PlayerGenerator.GetRandomEndOfSeasonMistakeChanceAdjustment());
        }
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
        data.Age = Age;
        data.Elos = Elo.Select(x => new EloData(x.Key.DefName, x.Value)).ToList();
        data.LeagueType = (int)LeagueType;
        data.Skills = Skills.Select(x => x.Value.ToData()).ToList();
        return data;
    }

    public Player(PlayerData data)
    {
        Id = data.Id;
        FirstName = data.FirstName;
        LastName = data.LastName;
        Country = Database.GetCountry(data.CountryId);
        Sex = data.Sex;
        Age = data.Age;
        LeagueType = (TournamentType)data.LeagueType;
        Skills = data.Skills.ToDictionary(x => DefDatabase<SkillDef>.GetNamed(x.Skill), x => new Skill(x));

        // Elos
        Elo = new Dictionary<DisciplineDef, int>();
        foreach (DisciplineDef discipline in DefDatabase<DisciplineDef>.AllDefs)
        {
            EloData disciplineData = data.Elos.First(x => x.Discipline == discipline.DefName);
            if (disciplineData != null) Elo.Add(discipline, disciplineData.Elo);
            else Elo.Add(discipline, TournamentSimulator.DEFAULT_RATING);
        }
    }

    #endregion

}
