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

    private static int NextPlayerId;
    private static int NextTeamId;
    private static int NextMatchId;
    private static int NextTournamentId;
    private static int NextLeagueId;

    public static Dictionary<int, Country> Countries;
    public static Dictionary<int, Player> Players;
    public static Dictionary<int, Team> Teams;

    public static Dictionary<int, League> Leagues;
    public static Dictionary<int, Tournament> Tournaments;
    public static Dictionary<int, Match> Matches;

    public const int DAYS_PER_QUARTER = 15;
    public const int QUARTERS_PER_SEASON = 4;

    public static int Season;
    public static int Quarter;
    public static int Day;

    public static int ToAbsoluteDay(int season, int quarter, int day) => ((season - 1) * QUARTERS_PER_SEASON * DAYS_PER_QUARTER) + ((quarter - 1) * DAYS_PER_QUARTER) + (day - 1);
    public static int CurrentDayAbsolute => ToAbsoluteDay(Season, Quarter, Day);
    public static int ToRelativeSeason(int absoluteDay) => (absoluteDay / (QUARTERS_PER_SEASON * DAYS_PER_QUARTER)) + 1;
    public static int ToRelativeQuarter(int absoluteDay) => ((absoluteDay % (QUARTERS_PER_SEASON * DAYS_PER_QUARTER)) / DAYS_PER_QUARTER) + 1;
    public static int ToRelativeDay(int absoluteDay) => (absoluteDay % (DAYS_PER_QUARTER)) + 1;
    public static string GetDateString(int season, int quarter, int day) => GetQuarterName(quarter) + " " + day + ", Season " + season;

    #region Init

    public static void LoadData()
    {
        Countries = ReadCountries();

        SimulationData data = JsonUtilities.LoadData<SimulationData>("simulation_data");

        Season = data.CurrentSeason;
        Quarter = data.CurrentQuarter;
        Day = data.CurrentDay;
        Players = data.Players.Select(x => new Player(x)).ToDictionary(x => x.Id, x => x);
        Teams = data.Teams == null ? new Dictionary<int, Team>() : data.Teams.Select(x => new Team(x)).ToDictionary(x => x.Id, x => x);
        Leagues = data.Leagues.Select(x => new League(x)).ToDictionary(x => x.Id, x => x);
        Tournaments = data.Tournaments.Select(x => Tournament.LoadTournament(x)).ToDictionary(x => x.Id, x => x);
        Matches = data.Matches.Select(x => Match.LoadMatch(x)).ToDictionary(x => x.Id, x => x);

        NextPlayerId = Players.Count == 0 ? 1 :         Players.Values.Max(x => x.Id) + 1;
        NextTeamId = Teams.Count == 0 ? 1 :             Teams.Values.Max(x => x.Id) + 1;
        NextLeagueId = Leagues.Count == 0 ? 1 :         Leagues.Values.Max(x => x.Id) + 1;
        NextTournamentId = Tournaments.Count == 0 ? 1 : Tournaments.Values.Max(x => x.Id) + 1;
        NextMatchId = Matches.Count == 0 ? 1 :          Matches.Values.Max(x => x.Id) + 1;

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
                string fifaCode = fields[5];

                countries.Add(id, new Country(id, name, population, region, continent, fifaCode));

                if (!File.Exists("Assets/Resources/Icons/Flags/180x120/" + fifaCode + ".png")) Debug.LogWarning("No big (180x120) flag icon found for " + name + " (" + fifaCode + ")");
                if (!File.Exists("Assets/Resources/Icons/Flags/48x32/" + fifaCode + ".png")) Debug.LogWarning("No small (48x32) flag icon found for " + name + " (" + fifaCode + ")");
            }
            counter++;
        }

        file.Close();
        Debug.Log("Read " + countries.Count + " countries from database");
        return countries;
    }

    #endregion

    #region Id

    public static int GetNewPlayerId() => NextPlayerId++;
    public static int GetNewTeamId() => NextTeamId++;
    public static int GetNewMatchId() => NextMatchId++;
    public static int GetNewTournamentId() => NextTournamentId++;
    public static int GetNewLeagueId() => NextLeagueId++;

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

    public static Country GetCountry(string name) => Database.Countries.Values.First(x => x.Name == name);

    public static League GetLeague(TournamentType type, int season) => Leagues.Values.FirstOrDefault(x => x.LeagueType == type && x.Season == season);
    public static League GetCurrentLeague(TournamentType type) => GetLeague(type, Season);
    public static League CurrentGrandLeague => GetLeague(TournamentType.GrandLeague, Season);
    public static League CurrentChallengeLeague => GetLeague(TournamentType.ChallengeLeague, Season);
    public static League CurrentOpenLeague => GetLeague(TournamentType.OpenLeague, Season);

    public static List<Tournament> GetTournaments(int season) => Tournaments.Values.Where(x => x.Season == season).ToList();

    public static Team GetNationalTeam(Country c) => Teams.Values.FirstOrDefault(x => x.Country == c);
    public static List<Team> GetNationalTeams(int minPlayers = 9999999) => Teams.Values.Where(x => x.IsCountryTeam && x.Players.Count >= minPlayers).ToList();

    public static List<Player> WorldRanking => Players.Values.OrderByDescending(x => x.Elo).ThenByDescending(x => x.TiebreakerScore).ToList();
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

        foreach (League l in Leagues.Values.Where(x => x.Season < Season && x.LeagueType == TournamentType.GrandLeague))
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

    public static Dictionary<Country, List<Player>> GetPlayersByCountry() => Players.Values.GroupBy(x => x.Country).OrderByDescending(x => x.Count()).ToDictionary(x => x.Key, x => x.ToList());

    #endregion

    #region Debug / Log

    public static void ListCountriesByPlayerAmount()
    {
        string s = "Countries by amount of players:";
        int rank = 1;
        foreach(var x in GetPlayersByCountry())
        {
            s += "\n" + rank + ". " +  x.Key.Name + ": " + x.Value.Count;
            rank++;
        }
        Debug.Log(s);
    }

    #endregion
}
