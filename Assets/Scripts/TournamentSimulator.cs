using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TournamentSimulator : MonoBehaviour
{
    public const int DaysPerQuarter = 15;
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

    // Start is called before the first frame update
    void Start()
    {
        Database.LoadData();
        PlayerGenerator.InitGenerator(Database.Countries.Values.ToList());

        UI = GetComponent<UI_Base>();
        UI.Init(this);
        UpdateUI();
    }

    /// <summary>
    /// Generates a new random player with default rating and adds them to the lowest league with space.
    /// </summary>
    public void AddRandomPlayer(string region = "", string continent = "", int rating = DEFAULT_RATING)
    {
        Player newPlayer = PlayerGenerator.GenerateRandomPlayer(region, continent, rating);

        int league = (Database.Players.Count / 24);
        if (league > 2) league = 2;
        newPlayer.SetLeague((LeagueType)league);

        Database.Players.Add(newPlayer.Id, newPlayer);
        Debug.Log(newPlayer.ToString() + " has been generated.");
    }

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

        else if (Database.Quarter == 4 && Database.Day == 15)
        {
            Database.Season++;
            Database.Quarter = 1;
            Database.Day = 1;
            EndSeason();
            StartSeason();
        }
        else if (Database.Day == 15)
        {
            Database.Quarter++;
            Database.Day = 1;
        }
        else Database.Day++;

        Save();
        UpdateUI();
    }
    public bool CanGoToNextDay()
    {
        return Database.Tournaments.Values.Where(x => x.League.Season == Database.Season && x.Quarter == Database.Quarter && x.Day == Database.Day && !x.IsDone).Count() == 0;
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
        AddNewLeague("Grand League", Database.Season, 0, grandLeaguePlayers);
        AddNewLeague("Challenger League", Database.Season, 1, challengeLeaguePlayers);
        AddNewLeague("Open League", Database.Season, 2, openLeaguePlayers);



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
        Tournament newTournament = Tournament.CreateTournament(type, Database.Season, quarter, day, Database.Players.Values.Where(x => x.LeagueType == type).ToList(), Database.Leagues.Values.ToList());
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
        List<Player> grandLegueRanking = Database.CurrentGrandLeague.Ranking;
        for(int i = 19; i < grandLegueRanking.Count; i++) grandLegueRanking[i].SetLeague(LeagueType.ChallengeLeague);

        List<Player> challengeLegueRanking = Database.CurrentChallengeLeague.Ranking;
        for(int i = 0; i < 5; i++) challengeLegueRanking[i].SetLeague(LeagueType.GrandLeague);
        for (int i = 19; i < challengeLegueRanking.Count; i++) challengeLegueRanking[i].SetLeague(LeagueType.OpenLeague);

        List<Player> openLegueRanking = Database.CurrentOpenLeague.Ranking;
        for (int i = 0; i < 5; i++) openLegueRanking[i].SetLeague(LeagueType.ChallengeLeague);
        List<Player> eliminatedPlayers = new List<Player>();
        for (int i = openLegueRanking.Count - 5; i < openLegueRanking.Count; i++)
        {
            eliminatedPlayers.Add(openLegueRanking[i]);
            openLegueRanking[i].SetLeague(LeagueType.None);
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
        List<Player> candidates = Database.Players.Values.Where(x => x.LeagueType == LeagueType.None && !excludedPlayers.Contains(x)).ToList();
        Player playerToRevive = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        playerToRevive.SetLeague(LeagueType.OpenLeague);
        Debug.Log(playerToRevive.ToString() + " has been revived.");
    }


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
        data.Leagues = Database.Leagues.Select(x => x.Value.ToData()).ToList();
        data.Tournaments = Database.Tournaments.Select(x => x.Value.ToData()).ToList();
        data.Matches = Database.Matches.Select(x => x.Value.ToData()).ToList();
        return data;
    }

    #endregion
}
