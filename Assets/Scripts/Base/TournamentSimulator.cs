using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TournamentSimulator : MonoBehaviour
{
    public const int DEFAULT_RATING = 5000;

    [HideInInspector]
    public UI_Base UI;

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

    #region Init

    // Start is called before the first frame update
    void Start()
    {
        Database.LoadData();
        PlayerGenerator.InitGenerator(Database.Countries.Values.ToList());

        Database.ListCountriesByPlayerAmount();
        AddMissingCountryTeams();

        /* Test Relative <-> Absolute Dates
        int absoluteDay = Database.CurrentDayAbsolute;
        Debug.Log("absDay: " + absoluteDay);
        int season = Database.ToRelativeSeason(absoluteDay);
        int qu = Database.ToRelativeQuarter(absoluteDay);
        int da = Database.ToRelativeDay(absoluteDay);
        Debug.Log(season + "|" + qu + "|" + da);
        */

        UI = GetComponent<UI_Base>();
        UI.Init(this);
        UpdateUI();

        //StartTestMatch();
        //StartTestTeamMatch();
    }

    private void AddMissingCountryTeams()
    {
        foreach(Country c in Database.Countries.Values)
        {
            if (Database.Teams.Values.Where(x => x.Country == c).Count() > 0) continue; // Country team already exists
            if (Database.Players.Values.Where(x => x.Country == c).Count() == 0) continue; // No player with that country exists yet

            CreateNationalTeam(c);
        }
    }

    private void CreateNationalTeam(Country c)
    {
        Team newCountryTeam = new Team(c);
        Database.Teams.Add(newCountryTeam.Id, newCountryTeam);
    }

    #endregion

    public void UpdateUI()
    {
        UI.UpdateTime();
        UI.DashboardScreen.Refresh();
    }


    public void GoToNextDay()
    {
        if(Database.Season == 0 && Database.Quarter == 0 && Database.Day == 0)
        {
            Database.Season = 1;
            Database.Quarter = 1;
            Database.Day = 1;
            StartSeason();
        }

        else if (Database.Quarter == Database.QUARTERS_PER_SEASON && Database.Day == Database.DAYS_PER_QUARTER)
        {
            Database.Season++;
            Database.Quarter = 1;
            Database.Day = 1;
            EndSeason();
            StartSeason();
        }
        else if (Database.Day == Database.DAYS_PER_QUARTER)
        {
            Database.Quarter++;
            Database.Day = 1;
        }
        else Database.Day++;

        OnDayStart();
        Save();
        UpdateUI();
    }
    public bool CanGoToNextDay()
    {
        return Database.Matches.Values.Where(x => x.Tournament.Season == Database.Season && x.Quarter == Database.Quarter && x.Day == Database.Day && !x.IsDone).Count() == 0;
    }

    /// <summary>
    /// Gets executed whenever a new day has started.
    /// </summary>
    private void OnDayStart()
    {
        foreach (Match m in Database.Matches.Values.Where(x => x.IsToday)) m.OnDayStart();
    }

    #region Season Transition

    private void StartSeason()
    {
        // Create seasonal leagues
        List<Player> grandLeaguePlayers = new List<Player>();
        List<Player> challengeLeaguePlayers = new List<Player>();
        List<Player> openLeaguePlayers = new List<Player>();
        foreach (Player p in Database.Players.Values)
        {
            if (p.LeagueType == TournamentType.GrandLeague) grandLeaguePlayers.Add(p);
            else if (p.LeagueType == TournamentType.ChallengeLeague) challengeLeaguePlayers.Add(p);
            else if (p.LeagueType == TournamentType.OpenLeague) openLeaguePlayers.Add(p);
        }
        AddNewLeague("Grand League", Database.Season, 0, grandLeaguePlayers);
        AddNewLeague("Challenger League", Database.Season, 1, challengeLeaguePlayers);
        AddNewLeague("Open League", Database.Season, 2, openLeaguePlayers);

        // Create league tournaments
        ScheduleTournament(TournamentType.GrandLeague, 1, 10);
        ScheduleTournament(TournamentType.GrandLeague, 2, 10);
        ScheduleTournament(TournamentType.GrandLeague, 3, 10);
        ScheduleTournament(TournamentType.GrandLeague, 3, 14);
        ScheduleTournament(TournamentType.GrandLeague, 4, 4);

        ScheduleTournament(TournamentType.ChallengeLeague, 1, 6);
        ScheduleTournament(TournamentType.ChallengeLeague, 2, 6);
        ScheduleTournament(TournamentType.ChallengeLeague, 2, 14);
        ScheduleTournament(TournamentType.ChallengeLeague, 3, 6);
        ScheduleTournament(TournamentType.ChallengeLeague, 4, 3);

        ScheduleTournament(TournamentType.OpenLeague, 1, 2);
        ScheduleTournament(TournamentType.OpenLeague, 1, 14);
        ScheduleTournament(TournamentType.OpenLeague, 2, 2);
        ScheduleTournament(TournamentType.OpenLeague, 3, 2);
        ScheduleTournament(TournamentType.OpenLeague, 4, 2);

        // Season Cup
        ScheduleTournament(TournamentType.SeasonCup);

        // World Cup
        ScheduleTournament(TournamentType.WorldCup, numPlayersPerTeam: 2);

        // Switch season view
        UI.DashboardScreen.SelectedSeason = Database.Season;

        Save();
    }

    private void AddNewLeague(string name, int season, int formatId, List<Player> players)
    {
        League newLeague = new League(name, season, formatId, players);
        Database.Leagues.Add(newLeague.Id, newLeague);
    }

    public void ScheduleTournament(TournamentType type, int quarter = 0, int day = 0, int numPlayersPerTeam = 0)
    {
        int season = Database.Season;

        Tournament newTournament;
        if (type == TournamentType.GrandLeague) newTournament = new Format_GrandLeague(season, quarter, day, Database.CurrentGrandLeague);
        else if (type == TournamentType.ChallengeLeague) newTournament = new Format_ChallengeLeague(season, quarter, day, Database.CurrentChallengeLeague);
        else if (type == TournamentType.OpenLeague) newTournament = new Format_OpenLeague(season, quarter, day, Database.CurrentOpenLeague);
        else if (type == TournamentType.SeasonCup) newTournament = new Format_SeasonCup(season);
        else if (type == TournamentType.WorldCup) newTournament = new Format_WorldCup(season, numPlayersPerTeam);
        else throw new System.Exception("TournamentType " + type.ToString() + " not handled.");

        Database.Tournaments.Add(newTournament.Id, newTournament);
        foreach (Match m in newTournament.Matches) Database.Matches.Add(m.Id, m);
    }

    public void EndSeason()
    {
        // Skill and attribute shuffle
        Debug.Log("Shuffling skills and attributes of all players.");
        ShuffleSkillsAndAttributes();

        // Relegations
        Debug.Log("Performing relegations.");
        List<Player> grandLegueRanking = Database.GetLeague(TournamentType.GrandLeague, Database.Season - 1).Ranking;
        for(int i = 19; i < grandLegueRanking.Count; i++) grandLegueRanking[i].SetLeague(TournamentType.ChallengeLeague);

        List<Player> challengeLegueRanking = Database.GetLeague(TournamentType.ChallengeLeague, Database.Season - 1).Ranking;
        for(int i = 0; i < 5; i++) challengeLegueRanking[i].SetLeague(TournamentType.GrandLeague);
        for (int i = 19; i < challengeLegueRanking.Count; i++) challengeLegueRanking[i].SetLeague(TournamentType.OpenLeague);

        List<Player> openLegueRanking = Database.GetLeague(TournamentType.OpenLeague, Database.Season - 1).Ranking;
        for (int i = 0; i < 5; i++) openLegueRanking[i].SetLeague(TournamentType.ChallengeLeague);
        List<Player> eliminatedPlayers = new List<Player>();
        for (int i = openLegueRanking.Count - 5; i < openLegueRanking.Count; i++)
        {
            eliminatedPlayers.Add(openLegueRanking[i]);
            openLegueRanking[i].SetLeague(TournamentType.None);
        }

        // Revive 2 inactive players and put them into open league (only ones that haven't just been eliminated)
        Debug.Log("Reviving random inactive players.");
        for (int i = 0; i < 2; i++) ReviveRandomPlayer(eliminatedPlayers);

        // Add 6 completely new players to the open league
        Debug.Log("Generating new players.");
        for (int i = 0; i < 7; i++) AddRandomPlayer();

        // Save
        Save();
    }

    public void ShuffleSkillsAndAttributes()
    {
        foreach(Player p in Database.Players.Values)
        {
            foreach(SkillDef skillDef in SkillDefs)
                p.AdjustSkill(skillDef, PlayerGenerator.GetRandomSkillAdjustment());

            p.AdjustInconsistency(PlayerGenerator.GetRandomInconsistencyAdjustment());
            p.AdjustMistakeChance(PlayerGenerator.GetRandomMistakeChanceAdjustment());
        }
    }

    public void ReviveRandomPlayer(List<Player> excludedPlayers)
    {
        List<Player> candidates = Database.Players.Values.Where(x => x.LeagueType == TournamentType.None && !excludedPlayers.Contains(x)).ToList();
        Player playerToRevive = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        playerToRevive.SetLeague(TournamentType.OpenLeague);
        Debug.Log(playerToRevive.ToString() + " has been revived.");
    }

    /// <summary>
    /// Generates a new random player with default rating and adds them to the lowest league with space.
    /// </summary>
    public void AddRandomPlayer(string region = "", string continent = "", int rating = DEFAULT_RATING)
    {
        Player newPlayer = PlayerGenerator.GenerateRandomPlayer(region, continent, rating);

        int league = (Database.Players.Count / 24);
        if (league > 2) league = 2;
        newPlayer.SetLeague((TournamentType)league);

        Database.Players.Add(newPlayer.Id, newPlayer);

        // Add to country team
        Team nationalTeam = Database.GetNationalTeam(newPlayer.Country);
        if (nationalTeam == null) CreateNationalTeam(newPlayer.Country);
        else nationalTeam.AddPlayer(newPlayer);

        Debug.Log(newPlayer.ToString() + " has been generated.");
    }

    #endregion

    #region Test

    private void StartTestMatch()
    {
        Match testMatch = new Match("Test", Database.Tournaments.Values.Last(), Database.Quarter, Database.Day, numPlayers: 3, new List<int>() { 5, 3, 1 });
        testMatch.AddPlayerToMatch(Database.Players[0], 0);
        testMatch.AddPlayerToMatch(Database.Players[1], 0);
        testMatch.AddPlayerToMatch(Database.Players[2], 0);
        UI.StartMatchSimulation(testMatch, 1.5f);
    }

    private void StartTestTeamMatch()
    {
        TeamMatch testMatch = new TeamMatch("Test Match", Database.Tournaments.Values.Last(), Database.Quarter, Database.Day, numTeams: 2, numPlayersPerTeam: 2, new List<int>() { 1, 0 }, new List<int>() { 4, 3, 2, 1 });
        testMatch.AddTeamToMatch(Database.GetNationalTeam(Database.GetCountry("Pakistan")), 0);
        testMatch.AddTeamToMatch(Database.GetNationalTeam(Database.GetCountry("Guatemala")), 0);
        //Database.Matches.Add(testMatch.Id, testMatch); // only uncomment if you want to save the match
        UI.StartMatchSimulation(testMatch, 0.05f);
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
        int rng = (int)(UnityEngine.Random.value * 15);
        string backupName = "simulation_data_backup_" + rng;
        JsonUtilities.SaveData<SimulationData>(ToData(), backupName);
    }

    public SimulationData ToData()
    {
        SimulationData data = new SimulationData();
        data.CurrentSeason = Database.Season;
        data.CurrentQuarter = Database.Quarter;
        data.CurrentDay = Database.Day;
        data.Players = Database.Players.Select(x => x.Value.ToData()).ToList();
        data.Teams = Database.Teams.Select(x => x.Value.ToData()).ToList();
        data.Leagues = Database.Leagues.Select(x => x.Value.ToData()).ToList();
        data.Tournaments = Database.Tournaments.Select(x => x.Value.ToData()).ToList();
        data.Matches = Database.Matches.Select(x => x.Value.ToData()).ToList();
        return data;
    }

    #endregion
}