using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : IPlayer
{
    public int Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Country Country { get; private set; }
    public string Sex { get; private set; }
    public Dictionary<DisciplineDef, int> Elo { get; private set; }
    public TournamentType LeagueType { get; private set; }
    private Dictionary<SkillDef, int> Skills { get; set; }
    public float Inconsistency { get; private set; }
    public float TiebreakerScore { get; private set; }
    public float MistakeChance { get; private set; }

    public League League => Database.GetCurrentLeague(LeagueType);
    public int CurrentLeaguePoints => League.Standings[this];
    public int LeagueRank => League.GetRankOf(this);
    public Sprite LeagueIcon => League == null ? ColorManager.Singleton.NoLeagueIcon : League.Icon;
    public Color LeagueColor => League == null ? ColorManager.Singleton.NoLeagueColor : League.Color;
    public Sprite FlagBig => Country.FlagBig;
    public Sprite FlagSmall => Country.FlagSmall;
    public int Age => 20 + (Database.Season - Database.AllLeagues.Where(x => x.Players.Contains(this)).Min(x => x.Season));

    public static string MISTAKE_MODIFIER = "Mistake";

    // Stats
    public List<Match> Matches = new List<Match>();
    public int NumCompletedMatches => Matches.Where(m => m.IsDone).Count();
    public int GetWorldRank(DisciplineDef discipline) => Database.AllPlayers.OrderByDescending(x => x.Elo[discipline]).ToList().IndexOf(this) + 1;

    // New player
    public Player(string firstName, string lastName, Country country, string sex, Dictionary<SkillDef, int> skills, float inconsistecy, float tiebreakerScore, float mistakeChance)
    {
        Id = Database.GetNewPlayerId();

        FirstName = firstName;
        LastName = lastName;
        Sex = sex;
        Country = country;
        Skills = skills;
        Inconsistency = inconsistecy;
        TiebreakerScore = tiebreakerScore;
        MistakeChance = mistakeChance;

        Elo = new Dictionary<DisciplineDef, int>();
        foreach (DisciplineDef discipline in DefDatabase<DisciplineDef>.AllDefs)
        {
            Elo.Add(discipline, TournamentSimulator.DEFAULT_RATING);
        }

        LeagueType = TournamentType.None;
    }

    public int GetSkillBaseValue(SkillDef skillDef) => Skills[skillDef];

    /// <summary>
    /// Returns a match round result value for a specific skill, given the match state.
    /// </summary>
    public PlayerGameRound GetMatchRoundResult(SkillDef skillDef)
    {
        List<string> modifiers = new List<string>();

        // Score
        int score;
        if (Random.value < MistakeChance) // Mistake
        {
            score = 0;
            modifiers.Add(MISTAKE_MODIFIER);
        }
        else // Inconsistency
        {
            score = Mathf.RoundToInt(HelperFunctions.RandomGaussian(minValue: Skills[skillDef] - Inconsistency, maxValue: Skills[skillDef] + Inconsistency));
            if (score < 0) score = 0;
        }
        return new PlayerGameRound(this, score, modifiers);
    }
    
    /// <summary>
    /// Changes the specified skill by the specified value.
    /// </summary>
    public void AdjustSkill(SkillDef skillDef, int adjustmentValue)
    {
        Skills[skillDef] += adjustmentValue;
        if (Skills[skillDef] < 0) Skills[skillDef] = 0;
    }
    public void AdjustInconsistency(float adjustmentValue)
    {
        Inconsistency += adjustmentValue;
        Inconsistency = Mathf.Clamp(Inconsistency, 0f, 20f);
    }
    public void AdjustMistakeChance(float adjustmentValue)
    {
        MistakeChance += adjustmentValue;
        MistakeChance = Mathf.Clamp01(MistakeChance);
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
        data.Elos = Elo.Select(x => new EloData(x.Key.DefName, x.Value)).ToList();
        data.LeagueType = (int)LeagueType;
        data.Skills = Skills.Select(x => new PlayerSkillData() { Skill = x.Key.DefName, Value = x.Value }).ToList();
        data.Inconsistency = Inconsistency;
        data.TiebreakerScore = TiebreakerScore;
        data.MistakeChance = MistakeChance;
        return data;
    }

    public Player(PlayerData data)
    {
        Id = data.Id;
        FirstName = data.FirstName;
        LastName = data.LastName;
        Country = Database.GetCountry(data.CountryId);
        Sex = data.Sex;
        LeagueType = (TournamentType)data.LeagueType;
        Skills = data.Skills.ToDictionary(x => DefDatabase<SkillDef>.AllDefs.First(s => s.DefName == x.Skill), x => x.Value);
        Inconsistency = data.Inconsistency;
        TiebreakerScore = data.TiebreakerScore;
        MistakeChance = data.MistakeChance;

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
