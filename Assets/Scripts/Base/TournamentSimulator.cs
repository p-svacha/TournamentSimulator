using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

public class TournamentSimulator : MonoBehaviour
{
    public static TournamentSimulator Instance;

    public const int DEFAULT_RATING = 5000;

    private const int NUM_GRAND_CHALLENGE_SWAPS = 5;
    private const int NUM_CHALLENGE_OPEN_SWAPS = 5;
    private const int NUM_OPEN_LEAGUE_RELEGATIONS = 10;

    private const int NUM_RANDOM_REVIVES = 2;
    private const int NUM_NEW_GENERATIONS = 7;

    [HideInInspector]
    public UI_Base UI;

    #region Init

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        DefDatabaseRegistry.AddAllDefs();
        DefDatabaseRegistry.ResolveAllReferences();
        DefDatabaseRegistry.OnLoadingDone();

        Database.LoadData();
        PlayerGenerator.InitGenerator(Database.AllCountries);

        Database.ListCountriesByPlayerAmount();

        AddMissingPlayerSkills();
        AddMissingCountryTeams();
        // FlagImageScraper.DownloadAllFlagImages();

        UI = GetComponent<UI_Base>();
        UI.Init(this);
        UpdateUI();

        // ### TESTS
        // StartTestMatch();
        // StartBigTestMatch();
        // StartKnockoutTestMatch();

        /*
        int n = 16;
        for (int i = 0; i < n; i++)
        {
            Debug.Log($"first match id for seed {i} = {SingleElimination.GetFirstMatchId(i, n)}");
        }

        int numQualified = 4;
        int groupSize = 4;
        int numPlayers = numQualified * groupSize;
        for (int seed = 0; seed < numPlayers; seed++)
        {
            Debug.Log($"Seed {seed} is assigned to group {SingleElimination.GetGroupForSeed(seed, numPlayers, numQualified)} with group seed {SingleElimination.GetSeedWithinGroup(seed, numPlayers, numQualified)}.");
        }
        */
    }

    private void AddMissingCountryTeams()
    {
        foreach(Country c in Database.AllCountries)
        {
            if (Database.AllTeams.Where(x => x.Country == c).Count() > 0) continue; // Country team already exists
            if (Database.AllPlayers.Where(x => x.Country == c).Count() == 0) continue; // No player with that country exists yet

            CreateNationalTeam(c);
        }
    }

    /// <summary>
    /// If a new SkillDef was added, randomized values will be added to all players that don't yet have it.
    /// </summary>
    private void AddMissingPlayerSkills()
    {
        foreach (SkillDef skillDef in DefDatabase<SkillDef>.AllDefs)
        {
            foreach (Player player in Database.AllPlayers)
            {
                if (!player.Skills.ContainsKey(skillDef))
                {
                    Skill skill = PlayerGenerator.GenerateNewRandomizedSkill(skillDef);
                    player.Skills.Add(skillDef, skill);
                }
            }
        }
    }

    private void CreateNationalTeam(Country c)
    {
        Team newCountryTeam = new Team(c);
        Database.AddTeam(newCountryTeam);
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
        return Database.AllMatches.Where(x => x.Tournament.Season == Database.Season && x.Quarter == Database.Quarter && x.Day == Database.Day && !x.IsDone).Count() == 0;
    }

    /// <summary>
    /// Gets executed whenever a new day has started.
    /// </summary>
    private void OnDayStart()
    {
        foreach (Tournament t in Database.AllTournaments.Where(t => t.StartsToday())) t.OnTournamentStart();
        foreach (Match m in Database.AllMatches.Where(x => x.IsToday)) m.OnDayStart();
    }

    #region Season Transition

    private void StartSeason()
    {
        // Create seasonal leagues
        List<Player> grandLeaguePlayers = new List<Player>();
        List<Player> challengeLeaguePlayers = new List<Player>();
        List<Player> openLeaguePlayers = new List<Player>();
        foreach (Player p in Database.AllPlayers)
        {
            if (p.LeagueType == TournamentType.GrandLeague) grandLeaguePlayers.Add(p);
            else if (p.LeagueType == TournamentType.ChallengeLeague) challengeLeaguePlayers.Add(p);
            else if (p.LeagueType == TournamentType.OpenLeague) openLeaguePlayers.Add(p);
        }
        AddNewLeague(DisciplineDefOf.Football, "Grand League", Database.Season, 0, grandLeaguePlayers, numPromotions: 0, numRelegations: NUM_GRAND_CHALLENGE_SWAPS);
        AddNewLeague(DisciplineDefOf.Football, "Challenger League", Database.Season, 1, challengeLeaguePlayers, numPromotions: NUM_GRAND_CHALLENGE_SWAPS, numRelegations: NUM_CHALLENGE_OPEN_SWAPS);
        AddNewLeague(DisciplineDefOf.Football, "Open League", Database.Season, 2, openLeaguePlayers, numPromotions: NUM_CHALLENGE_OPEN_SWAPS, numRelegations: NUM_OPEN_LEAGUE_RELEGATIONS);

        // Create league tournaments
        ScheduleTournament(DisciplineDefOf.Football, TournamentType.GrandLeague, 1, 10);
        ScheduleTournament(DisciplineDefOf.Football, TournamentType.GrandLeague, 2, 10);
        ScheduleTournament(DisciplineDefOf.Football, TournamentType.GrandLeague, 3, 10);
        ScheduleTournament(DisciplineDefOf.Football, TournamentType.GrandLeague, 3, 14);
        ScheduleTournament(DisciplineDefOf.Football, TournamentType.GrandLeague, 4, 4);

        ScheduleTournament(DisciplineDefOf.Football, TournamentType.ChallengeLeague, 1, 6);
        ScheduleTournament(DisciplineDefOf.Football, TournamentType.ChallengeLeague, 2, 6);
        ScheduleTournament(DisciplineDefOf.Football, TournamentType.ChallengeLeague, 2, 14);
        ScheduleTournament(DisciplineDefOf.Football, TournamentType.ChallengeLeague, 3, 6);
        ScheduleTournament(DisciplineDefOf.Football, TournamentType.ChallengeLeague, 4, 3);

        ScheduleTournament(DisciplineDefOf.Football, TournamentType.OpenLeague, 1, 2);
        ScheduleTournament(DisciplineDefOf.Football, TournamentType.OpenLeague, 1, 14);
        ScheduleTournament(DisciplineDefOf.Football, TournamentType.OpenLeague, 2, 2);
        ScheduleTournament(DisciplineDefOf.Football, TournamentType.OpenLeague, 3, 2);
        ScheduleTournament(DisciplineDefOf.Football, TournamentType.OpenLeague, 4, 2);

        // Season Cup
        ScheduleTournament(DisciplineDefOf.Football, TournamentType.SeasonCup);

        // World Cup
        ScheduleTournament(DisciplineDefOf.Football, TournamentType.WorldCup);

        // BIG Cup
        ScheduleTournament(DisciplineDefOf.Football, TournamentType.BIGCup, quarter: 3, day: 1);

        // Switch season view
        UI.DashboardScreen.RefreshDropdownOptions();
        UI.DashboardScreen.SelectSeason(Database.Season);

        Save();
    }

    private void AddNewLeague(DisciplineDef discipline, string name, int season, int formatId, List<Player> players, int numPromotions, int numRelegations)
    {
        League newLeague = new League(discipline, name, season, formatId, players, numPromotions, numRelegations);
        Database.AddLeague(newLeague);
    }

    public void ScheduleTournament(DisciplineDef disciplineDef, TournamentType type, int quarter = 0, int day = 0)
    {
        int season = Database.Season;

        Tournament newTournament;
        if (type == TournamentType.GrandLeague) newTournament = new Format_GrandLeague(disciplineDef, season, quarter, day, Database.CurrentGrandLeague);
        else if (type == TournamentType.ChallengeLeague) newTournament = new Format_ChallengeLeague(disciplineDef, season, quarter, day, Database.CurrentChallengeLeague);
        else if (type == TournamentType.OpenLeague) newTournament = new Format_OpenLeague(disciplineDef, season, quarter, day, Database.CurrentOpenLeague);
        else if (type == TournamentType.SeasonCup) newTournament = new Format_SeasonCup(disciplineDef, season);
        else if (type == TournamentType.WorldCup) newTournament = new Format_WorldCup(disciplineDef, season);
        else if (type == TournamentType.BIGCup) newTournament = new Format_BigCup(disciplineDef, season, quarter, day);
        else throw new System.Exception("TournamentType " + type.ToString() + " not handled.");

        Database.AddTournament(newTournament);
        foreach (Match m in newTournament.Matches)
        {
            Database.AddMatch(m);
        }
    }

    public void EndSeason()
    {
        // Increment age of all players
        foreach (Player p in Database.AllPlayers) p.Age++;

        // Skill and attribute shuffle
        Debug.Log("Shuffling skills and attributes of all players.");
        AdjustAllSkillsOfAllPlayers_EndOfSeason();

        // Relegations
        Debug.Log("Performing relegations.");
        League greandLeague = Database.GetLeague(TournamentType.GrandLeague, Database.Season - 1);
        for(int i = greandLeague.Ranking.Count - greandLeague.NumRelegations; i < greandLeague.Ranking.Count; i++) greandLeague.Ranking[i].SetLeague(TournamentType.ChallengeLeague);

        League challengeLeague = Database.GetLeague(TournamentType.ChallengeLeague, Database.Season - 1);
        for(int i = 0; i < challengeLeague.NumPromotions; i++) challengeLeague.Ranking[i].SetLeague(TournamentType.GrandLeague);
        for (int i = challengeLeague.Ranking.Count - challengeLeague.NumRelegations; i < challengeLeague.Ranking.Count; i++) challengeLeague.Ranking[i].SetLeague(TournamentType.OpenLeague);

        League openLeague = Database.GetLeague(TournamentType.OpenLeague, Database.Season - 1);
        for (int i = 0; i < openLeague.NumPromotions; i++) openLeague.Ranking[i].SetLeague(TournamentType.ChallengeLeague);
        List<Player> eliminatedPlayers = new List<Player>();
        for (int i = openLeague.Ranking.Count - openLeague.NumRelegations; i < openLeague.Ranking.Count; i++)
        {
            eliminatedPlayers.Add(openLeague.Ranking[i]);
            openLeague.Ranking[i].SetLeague(TournamentType.None);
        }

        // Revive the inactive player with the most elo
        Player toRevive = Database.AllPlayers.Where(x => x.LeagueType == TournamentType.None && !eliminatedPlayers.Contains(x)).OrderByDescending(x => x.Elo[openLeague.Discipline]).First();
        Debug.Log($"Reviving best inactive player: {toRevive.FirstName} {toRevive.LastName}");
        toRevive.SetLeague(TournamentType.OpenLeague);

        // Revive 1 inactive players and put them into open league (only ones that haven't just been eliminated)
        Debug.Log("Reviving random inactive players.");
        for (int i = 0; i < 1; i++) ReviveRandomPlayer(eliminatedPlayers);

        // Add 8 completely new players to the open league
        Debug.Log("Generating new players for open league.");
        for (int i = 0; i < 8; i++) AddRandomPlayer(league: TournamentType.OpenLeague);

        /*
        // Just once: Add 4 additional players so player count is always divisible by 4
        Debug.Log("Generating new players for divisibility.");
        for (int i = 0; i < 4; i++) AddRandomPlayer(league: TournamentType.None);
        */

        // Save
        Save();
    }

    public void AdjustAllSkillsOfAllPlayers_EndOfSeason()
    {
        foreach(Player p in Database.AllPlayers) p.AdjustEndOfSeasonSkills();
    }

    public void ReviveRandomPlayer(List<Player> excludedPlayers)
    {
        List<Player> candidates = Database.AllPlayers.Where(x => x.LeagueType == TournamentType.None && !excludedPlayers.Contains(x)).ToList();
        Player playerToRevive = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        playerToRevive.SetLeague(TournamentType.OpenLeague);
        Debug.Log(playerToRevive.ToString() + " has been revived.");
    }

    /// <summary>
    /// Generates a new random player with default rating and adds them to the lowest league with space.
    /// </summary>
    public void AddRandomPlayer(string region = "", string continent = "", TournamentType league = TournamentType.OpenLeague)
    {
        Player newPlayer = PlayerGenerator.GenerateRandomPlayer(region, continent);

        /* Old logic when there were few players
        int league = (Database.AllPlayers.Count / 24);
        if (league > 2) league = 2;
        newPlayer.SetLeague((TournamentType)league);
        */

        newPlayer.SetLeague(league);

        Database.AddPlayer(newPlayer);

        // Add to country team
        Team nationalTeam = Database.GetNationalTeam(newPlayer.Country);
        if (nationalTeam == null) CreateNationalTeam(newPlayer.Country);
        else nationalTeam.AddPlayer(newPlayer);

        Debug.Log($"{newPlayer} from {newPlayer.Country.Name} has been generated.");
    }

    #endregion

    #region Test

    private void StartTestMatch()
    {
        SoloMatch testMatch = new SoloMatch("Test", Database.AllTournaments.Last(), Database.Quarter, Database.Day, MatchFormatDefOf.SingleGame, maxPlayers: 3, new List<int>() { 5, 3, 1 });
        testMatch.AddPlayerToMatch(Database.GetPlayer(0), 0);
        testMatch.AddPlayerToMatch(Database.GetPlayer(1), 0);
        testMatch.AddPlayerToMatch(Database.GetPlayer(2), 0);
        testMatch.SimulateNextGame(1.5f);
    }

    private void StartBigTestMatch()
    {
        SoloMatch testMatch = new SoloMatch("Test", Database.AllTournaments.Last(), Database.Quarter, Database.Day, MatchFormatDefOf.SingleGame, maxPlayers: 20, new List<int>() { 20,19,18,17,16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1 });
        testMatch.AddPlayerToMatch(Database.GetPlayer(0), 0);
        testMatch.AddPlayerToMatch(Database.GetPlayer(1), 0);
        testMatch.AddPlayerToMatch(Database.GetPlayer(2), 0);
        testMatch.AddPlayerToMatch(Database.GetPlayer(3), 0);
        testMatch.AddPlayerToMatch(Database.GetPlayer(4), 0);
        testMatch.AddPlayerToMatch(Database.GetPlayer(5), 0);
        testMatch.AddPlayerToMatch(Database.GetPlayer(6), 0);
        testMatch.AddPlayerToMatch(Database.GetPlayer(7), 0);
        testMatch.AddPlayerToMatch(Database.GetPlayer(8), 0);
        testMatch.AddPlayerToMatch(Database.GetPlayer(9), 0);
        testMatch.AddPlayerToMatch(Database.GetPlayer(10), 0);
        testMatch.AddPlayerToMatch(Database.GetPlayer(11), 0);
        testMatch.AddPlayerToMatch(Database.GetPlayer(12), 0);
        testMatch.AddPlayerToMatch(Database.GetPlayer(13), 0);
        testMatch.AddPlayerToMatch(Database.GetPlayer(14), 0);
        testMatch.AddPlayerToMatch(Database.GetPlayer(15), 0);
        testMatch.AddPlayerToMatch(Database.GetPlayer(16), 0);
        testMatch.AddPlayerToMatch(Database.GetPlayer(17), 0);
        testMatch.AddPlayerToMatch(Database.GetPlayer(18), 0);
        testMatch.AddPlayerToMatch(Database.GetPlayer(19), 0);
        testMatch.SimulateNextGame(1.5f);
    }

    private void StartKnockoutTestMatch()
    {
        SoloMatch testMatch = new SoloMatch("Test", Database.AllTournaments.Last(), Database.Quarter, Database.Day, MatchFormatDefOf.SingleGame, maxPlayers: 8, isKnockout: true, knockoutStartingLives: 3, koLiveGainers: 1, koLiveLosers: 2);
        testMatch.AddPlayerToMatch(Database.GetPlayer(0));
        testMatch.AddPlayerToMatch(Database.GetPlayer(1));
        testMatch.AddPlayerToMatch(Database.GetPlayer(2));
        testMatch.AddPlayerToMatch(Database.GetPlayer(3));
        testMatch.AddPlayerToMatch(Database.GetPlayer(4));
        testMatch.AddPlayerToMatch(Database.GetPlayer(5));
        testMatch.AddPlayerToMatch(Database.GetPlayer(6));
        testMatch.AddPlayerToMatch(Database.GetPlayer(7));
        testMatch.SimulateNextGame(1.5f);
    }

    private void StartTestTeamMatch()
    {
        TeamMatch testMatch = new TeamMatch("Test Match", Database.AllTournaments.Last(), Database.Quarter, Database.Day, MatchFormatDefOf.SingleGame, numTeams: 2, numPlayersPerTeam: 2, new List<int>() { 1, 0 }, new List<int>() { 4, 3, 2, 1 });
        testMatch.AddTeamToMatch(Database.GetNationalTeam(Database.GetCountry("Pakistan")), 0);
        testMatch.AddTeamToMatch(Database.GetNationalTeam(Database.GetCountry("Guatemala")), 0);
        //Database.Matches.Add(testMatch.Id, testMatch); // only uncomment if you want to save the match
        testMatch.SimulateNextGame(1.5f);
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
        data.Players = Database.AllPlayers.Select(x => x.ToData()).ToList();
        data.Teams = Database.AllTeams.Select(x => x.ToData()).ToList();
        data.Leagues = Database.AllLeagues.Select(x => x.ToData()).ToList();
        data.Tournaments = Database.AllTournaments.Select(x => x.ToData()).ToList();
        data.Matches = Database.AllMatches.Select(x => x.ToData()).ToList();
        data.Games = Database.AllGames.Select(x => x.ToData()).ToList();
        return data;
    }

    #endregion
}
