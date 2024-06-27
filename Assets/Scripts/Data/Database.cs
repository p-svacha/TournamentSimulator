using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Singleton class that acts as a central database that can be accessed from everywhere.
/// <br/>Provides logic to get readable data but does not change the data by itself in any way.
/// </summary>
public static class Database
{
    private static string BasePath = "Assets/Resources/Database/";
    private static string CountryDbPath = BasePath + "Countries.txt";
    private static string ScheduleDbPath = BasePath + "Schedule.txt";

    private static int NextPlayerId;
    private static int NextMatchId;
    private static int NextTournamentId;
    private static int NextLeagueId;

    public static Dictionary<int, Country> Countries;
    public static Dictionary<int, Player> Players;

    public static Dictionary<int, League> Leagues;
    public static Dictionary<int, Tournament> Tournaments;
    public static Dictionary<int, Match> Matches;

    public static int Season;
    public static int Quarter;
    public static int Day;

    #region Init

    public static void LoadData()
    {
        Countries = ReadCountries();

        SimulationData data = JsonUtilities.LoadData<SimulationData>("simulation_data");

        Season = data.CurrentSeason;
        Quarter = data.CurrentQuarter;
        Day = data.CurrentDay;
        Players = data.Players.Select(x => new Player(x)).ToDictionary(x => x.Id, x => x);
        Leagues = data.Leagues.Select(x => new League(x)).ToDictionary(x => x.Id, x => x);
        Tournaments = data.Tournaments.Select(x => Tournament.LoadTournament(x)).ToDictionary(x => x.Id, x => x);
        Matches = data.Matches.Select(x => new Match(x)).ToDictionary(x => x.Id, x => x);

        NextPlayerId = data.Players.Max(x => x.Id) + 1;
        NextLeagueId = data.Leagues.Max(x => x.Id) + 1;
        NextTournamentId = data.Tournaments.Max(x => x.Id) + 1;
        NextMatchId = data.Matches.Max(x => x.Id) + 1;

        Debug.Log("Loaded simulation state at " + GetQuarterName(Quarter) + " " + Day + ", Season " + Season);
    }

    #endregion

    #region Read

    public static Dictionary<int, Country> ReadCountries()
    {
        Dictionary<int, Country> countries = new Dictionary<int, Country>();
        int counter = 0;
        string line;

        System.IO.StreamReader file = new System.IO.StreamReader(CountryDbPath);
        while ((line = file.ReadLine()) != null)
        {
            if(counter > 0)
            {
                string[] fields = line.Split(';');

                int id = int.Parse(fields[0]);
                string name = fields[1];
                int population = int.Parse(fields[2]);
                string region = fields[3];
                string continent = fields[4];

                countries.Add(id, new Country(id, name, population, region, continent));

                if (!File.Exists("Assets/Resources/Icons/Flags/" + name.Replace(' ','-') + ".png")) Debug.LogWarning("No flag icon found for " + name);
            }
            counter++;
        }

        file.Close();
        Debug.Log("Read " + countries.Count + " countries from database");
        return countries;
    }

    public static List<Tuple<int, int, int>> ReadSchedule()
    {
        List<Tuple<int, int, int>> schedule = new List<Tuple<int, int, int>>();
        int counter = 0;
        string line;

        System.IO.StreamReader file = new System.IO.StreamReader(ScheduleDbPath);
        while ((line = file.ReadLine()) != null)
        {
            if (counter > 0)
            {
                string[] fields = line.Split(';');
                int fieldIndex = 0;
                int quarter = int.Parse(fields[fieldIndex++]);
                int day = int.Parse(fields[fieldIndex++]);
                int format = int.Parse(fields[fieldIndex++]);
                schedule.Add(new Tuple<int, int, int>(quarter, day, format));
            }
            counter++;
        }
        return schedule;
    }


    #endregion

    #region Id

    public static int GetNewPlayerId()
    {
        return NextPlayerId++;
    }
    public static int GetNewMatchId()
    {
        return NextMatchId++;
    }
    public static int GetNewTournamentId()
    {
        return NextTournamentId++;
    }
    public static int GetNewLeagueId()
    {
        return NextLeagueId++;
    }

    #endregion

    #region Getters / Stats

    public static int LatestSeason => Leagues.Values.Max(x => x.Season);

    public static string GetQuarterName(int quarter)
    {
        if (quarter == 1) return "Spring";
        if (quarter == 2) return "Summer";
        if (quarter == 3) return "Autumn";
        if (quarter == 4) return "Winter";
        return "???";
    }

    public static League GetLeague(LeagueType type, int season) => Leagues.Values.FirstOrDefault(x => x.LeagueType == type && x.Season == season);
    public static League GetCurrentLeague(LeagueType type) => GetLeague(type, Season);
    public static League CurrentGrandLeague => GetLeague(LeagueType.GrandLeague, Season);
    public static League CurrentChallengeLeague => GetLeague(LeagueType.ChallengeLeague, Season);
    public static League CurrentOpenLeague => GetLeague(LeagueType.OpenLeague, Season);

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

    public static List<Tuple<Player, int, int, int>> GetHistoricGrandLeagueMedals()
    {
        List<Tuple<Player, int, int, int>> medals = new List<Tuple<Player, int, int, int>>();
        Dictionary<Player, int> goldMedals = new Dictionary<Player, int>();
        Dictionary<Player, int> silverMedals = new Dictionary<Player, int>();
        Dictionary<Player, int> bronzeMedals = new Dictionary<Player, int>();
        Dictionary<Player, int> medalScore = new Dictionary<Player, int>();

        foreach (Player p in Players.Values)
        {
            goldMedals.Add(p, 0);
            silverMedals.Add(p, 0);
            bronzeMedals.Add(p, 0);
            medalScore.Add(p, 0);
        }

        foreach (League l in Leagues.Values.Where(x => x.Season < Season && x.LeagueType == LeagueType.GrandLeague))
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

    #endregion
}
