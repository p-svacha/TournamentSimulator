using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TournamentSimulator : MonoBehaviour
{
    public static int PlayerInconsistency = 10; // standard deviation when calculating a player score for an attribute in a match

    public const int DaysPerQuarter = 15;
    public int Season;
    public int Quarter;
    public int Day;

    public const int DEFAULT_RATING = 5000;

    [HideInInspector]
    public UI_Base UI;

    public League CurrentGrandLeague;
    public League CurrentChallengeLeague;
    public League CurrentOpenLeague;

    public static List<SkillDef> SkillDefs = new List<SkillDef>()
    {
        new SkillDef(SkillId.Agility, "Agility", "AGI"),
        new SkillDef(SkillId.BallControl, "Ball Control", "BCT"),
        new SkillDef(SkillId.Dribbling, "Dribbling", "DRB"),
        new SkillDef(SkillId.Jumping, "Jumping", "JMP"),
        new SkillDef(SkillId.Mentality, "Mentality", "MNT"),
        new SkillDef(SkillId.Passing, "Passing", "PAS"),
        new SkillDef(SkillId.Positioning, "Positioning", "POS"),
        new SkillDef(SkillId.Shooting, "Shooting", "SHO"),
        new SkillDef(SkillId.Sprint, "Sprint", "SPR"),
        new SkillDef(SkillId.Stamina, "Stamina", "STM"),
        new SkillDef(SkillId.Strength, "Strength", "STR"),
    };

    // Start is called before the first frame update
    void Start()
    {
        UI = GetComponent<UI_Base>();
        UI.Init(this);

        SimulationData data = Database.LoadData(this);
        LoadState(data);
        PlayerGenerator.InitGenerator(Database.Countries.Values.ToList());

        Debug.Log("Loaded simulation state at " + GetQuarterName(Quarter) + " " + Day + ", Season " + Season);

        CurrentGrandLeague = Database.Leagues.Values.FirstOrDefault(x => x.LeagueType == LeagueType.GrandLeague && x.Season == Season);
        CurrentChallengeLeague = Database.Leagues.Values.FirstOrDefault(x => x.LeagueType == LeagueType.ChallengeLeague && x.Season == Season);
        CurrentOpenLeague = Database.Leagues.Values.FirstOrDefault(x => x.LeagueType == LeagueType.OpenLeague && x.Season == Season);

        UpdateUI();
    }

    /// <summary>
    /// Generates a new random player with default rating and adds them to the lowest league with space.
    /// </summary>
    public void AddRandomPlayer(string region = "", string continent = "", int rating = DEFAULT_RATING)
    {
        Player newPlayer = PlayerGenerator.GenerateRandomPlayer(this, region, continent, rating);

        int league = (Database.Players.Count / 24);
        if (league > 2) league = 2;
        newPlayer.SetLeague((LeagueType)league);

        Database.Players.Add(newPlayer.Id, newPlayer);
    }

    public void UpdateUI()
    {
        UI.UpdateTime(Season, Quarter, Day);
        UI.UpdatePlayers(Database.Players.Values.ToList());
        UI.UpdateTournaments(Database.Tournaments.Where(x => x.Value.League.Season == Season).Select(x => x.Value).ToList());
        UI.UpdateMedals(GetMedals());
    }



    public void GoToNextDay()
    {
        if(Season == 0 && Quarter == 0 && Day == 0)
        {
            Season = 1;
            Quarter = 1;
            Day = 1;
            StartSeason();
        }

        else if (Quarter == 4 && Day == 15)
        {
            Season++;
            Quarter = 1;
            Day = 1;
            EndSeason();
            StartSeason();
        }
        else if (Day == 15)
        {
            Quarter++;
            Day = 1;
        }
        else Day++;

        Save();
        UpdateUI();
    }
    public bool CanGoToNextDay()
    {
        return Database.Tournaments.Values.Where(x => x.League.Season == Season && x.Quarter == Quarter && x.Day == Day && !x.IsDone).Count() == 0;
    }

    private void StartSeason()
    {
        // Create seasonal leagues
        List<Player> grandLeaguePlayers = new List<Player>();
        List<Player> challengeLeaguePlayers = new List<Player>();
        List<Player> openLeaguePlayers = new List<Player>();
        foreach (Player p in Database.Players.Values)
        {
            if (p.LeagueType == LeagueType.GrandLeague) grandLeaguePlayers.Add(p);
            else if (p.LeagueType == LeagueType.ChallengeLeague) challengeLeaguePlayers.Add(p);
            else if (p.LeagueType == LeagueType.OpenLeague) openLeaguePlayers.Add(p);
        }
        AddNewLeague("Grand League", Season, 0, grandLeaguePlayers);
        AddNewLeague("Challenger League", Season, 1, challengeLeaguePlayers);
        AddNewLeague("Open League", Season, 2, openLeaguePlayers);

        CurrentGrandLeague = Database.Leagues.Values.FirstOrDefault(x => x.LeagueType == LeagueType.GrandLeague && x.Season == Season);
        CurrentChallengeLeague = Database.Leagues.Values.FirstOrDefault(x => x.LeagueType == LeagueType.ChallengeLeague && x.Season == Season);
        CurrentOpenLeague = Database.Leagues.Values.FirstOrDefault(x => x.LeagueType == LeagueType.OpenLeague && x.Season == Season);

        // Create seasonal tournaments from schedule
        List<Tuple<int, int, int>> schedule = Database.ReadSchedule();
        foreach(Tuple<int, int, int> entry in schedule) ScheduleTournament((LeagueType)entry.Item3, entry.Item1, entry.Item2);

        Save();
    }

    private void AddNewLeague(string name, int season, int formatId, List<Player> players)
    {
        League newLeague = new League(name, season, formatId, players);
        Database.Leagues.Add(newLeague.Id, newLeague);
    }

    public void ScheduleTournament(LeagueType type, int quarter, int day)
    {
        Tournament newTournament = Tournament.CreateTournament(this, type, Season, quarter, day, Database.Players.Values.Where(x => x.LeagueType == type).ToList(), Database.Leagues.Values.ToList());
        Database.Tournaments.Add(newTournament.Id, newTournament);
        foreach (Match m in newTournament.Matches) Database.Matches.Add(m.Id, m);
    }

    public void EndSeason()
    {
        // Revive 2 inactive players and put them into open league
        for (int i = 0; i < 2; i++) ReviveRandomPlayer();

        // Relegations
        List<Player> grandLegueRanking = CurrentGrandLeague.Ranking;
        for(int i = 19; i < grandLegueRanking.Count; i++) grandLegueRanking[i].SetLeague(LeagueType.ChallengeLeague);

        List<Player> challengeLegueRanking = CurrentChallengeLeague.Ranking;
        for(int i = 0; i < 5; i++) challengeLegueRanking[i].SetLeague(LeagueType.GrandLeague);
        for (int i = 19; i < challengeLegueRanking.Count; i++) challengeLegueRanking[i].SetLeague(LeagueType.OpenLeague);

        List<Player> openLegueRanking = CurrentOpenLeague.Ranking;
        for (int i = 0; i < 5; i++) openLegueRanking[i].SetLeague(LeagueType.ChallengeLeague);
        for (int i = openLegueRanking.Count - 5; i < openLegueRanking.Count; i++) openLegueRanking[i].SetLeague(LeagueType.None);

        // Attribute skill shuffle
        ShuffleSkills();

        // Add 6 completely new players to the open league
        for (int i = 0; i < 7; i++) AddRandomPlayer();

        // Save
        Save();
    }

    public void ShuffleSkills()
    {
        foreach(Player p in Database.Players.Values)
        {
            foreach(SkillDef skillDef in TournamentSimulator.SkillDefs)
            {
                int skillChange = UnityEngine.Random.Range(0, 11) - 5;
                p.Skills[skillDef.Id] += skillChange;
                if (p.Skills[skillDef.Id] < 0) p.Skills[skillDef.Id] = 0;
            }
        }
    }

    public void ReviveRandomPlayer()
    {
        List<Player> candidates = Database.Players.Values.Where(x => x.LeagueType == LeagueType.OpenLeague).ToList();
        Player playerToRevive = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        playerToRevive.SetLeague(LeagueType.OpenLeague);
    }

    #region Getters

    public static string GetQuarterName(int quarter)
    {
        if (quarter == 1) return "Spring";
        if (quarter == 2) return "Summer";
        if (quarter == 3) return "Autumn";
        if (quarter == 4) return "Winter";
        return "???";
    }
    public League GetCurrentLeague(LeagueType leagueType)
    {
        if (leagueType == LeagueType.GrandLeague) return CurrentGrandLeague;
        if (leagueType == LeagueType.ChallengeLeague) return CurrentChallengeLeague;
        if (leagueType == LeagueType.OpenLeague) return CurrentOpenLeague;
        return null;
    }
    private List<System.Tuple<Player, int, int, int>> GetMedals()
    {
        List<System.Tuple<Player, int, int, int>> medals = new List<Tuple<Player, int, int, int>>();
        Dictionary<Player, int> goldMedals = new Dictionary<Player, int>();
        Dictionary<Player, int> silverMedals = new Dictionary<Player, int>();
        Dictionary<Player, int> bronzeMedals = new Dictionary<Player, int>();
        Dictionary<Player, int> medalScore = new Dictionary<Player, int>();

        foreach (Player p in Database.Players.Values)
        {
            goldMedals.Add(p, 0);
            silverMedals.Add(p, 0);
            bronzeMedals.Add(p, 0);
            medalScore.Add(p, 0);
        }

        foreach (League l in Database.Leagues.Values.Where(x => x.Season < Season && x.LeagueType == LeagueType.GrandLeague))
        {
            List<Player> ranking = l.Ranking;
            goldMedals[ranking[0]]++;
            medalScore[ranking[0]] += 3;
            silverMedals[ranking[1]]++;
            medalScore[ranking[1]] += 2;
            bronzeMedals[ranking[2]]++;
            medalScore[ranking[2]] += 1;
        }

        medalScore = medalScore.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        foreach (KeyValuePair<Player, int> kvp in medalScore)
        {
            if (kvp.Value > 0)
            {
                Player p = kvp.Key;
                medals.Add(new Tuple<Player, int, int, int>(p, goldMedals[p], silverMedals[p], bronzeMedals[p]));
            }
        }

        return medals;
    }

    /// <summary>
    /// Returns an ordered dictionary representing the country leaderboard of a given country based on elo rating.
    /// </summary>
    public static Dictionary<Player, int> GetCountryRanking(string country)
    {
        return Database.Players.Values.Where(x => x.Country.Name == country).OrderByDescending(x => x.Elo).ToDictionary(x => x, x => x.Elo);
    }
    /// <summary>
    /// Returns an ordered dictionary representing the region leaderboard of a given region based on elo rating.
    /// </summary>
    public static Dictionary<Player, int> GetRegionRanking(string region)
    {
        return Database.Players.Values.Where(x => x.Country.Region == region).OrderByDescending(x => x.Elo).ToDictionary(x => x, x => x.Elo);
    }
    /// <summary>
    /// Returns an ordered dictionary representing the region leaderboard of a given region based on elo rating.
    /// </summary>
    public static Dictionary<Player, int> GetContinentRanking(string continent)
    {
        return Database.Players.Values.Where(x => x.Country.Continent == continent).OrderByDescending(x => x.Elo).ToDictionary(x => x, x => x.Elo);
    }

    #endregion


    #region Save / Load

    /// <summary>
    /// Saves the current simulation state
    /// </summary>
    public void Save()
    {
        JsonUtilities.SaveData<SimulationData>(ToData(), "simulation_data");

        // Save a backup
        int rng = (int)(UnityEngine.Random.value * 10);
        string backupName = "simulation_data_backup_" + rng;
        JsonUtilities.SaveData<SimulationData>(ToData(), backupName);
    }

    public SimulationData ToData()
    {
        SimulationData data = new SimulationData();
        data.CurrentSeason = Season;
        data.CurrentQuarter = Quarter;
        data.CurrentDay = Day;
        data.Players = Database.Players.Select(x => x.Value.ToData()).ToList();
        data.Leagues = Database.Leagues.Select(x => x.Value.ToData()).ToList();
        data.Tournaments = Database.Tournaments.Select(x => x.Value.ToData()).ToList();
        data.Matches = Database.Matches.Select(x => x.Value.ToData()).ToList();
        return data;
    }

    public void LoadState(SimulationData data)
    {
        Season = data.CurrentSeason;
        Quarter = data.CurrentQuarter;
        Day = data.CurrentDay;
        Database.Players = data.Players.Select(x => new Player(this, x)).ToDictionary(x => x.Id, x => x);
        Database.Leagues = data.Leagues.Select(x => new League(x)).ToDictionary(x => x.Id, x => x);
        Database.Tournaments = data.Tournaments.Select(x => Tournament.LoadTournament(x)).ToDictionary(x => x.Id, x => x);
        Database.Matches = data.Matches.Select(x => new Match(this, x)).ToDictionary(x => x.Id, x => x);
    }

    #endregion
}
