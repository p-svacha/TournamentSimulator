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
    private static int NextGameId;
    private static int NextTournamentId;
    private static int NextLeagueId;

    private static Dictionary<int, Country> Countries;
    private static Dictionary<int, Player> Players;
    private static Dictionary<int, Team> Teams;

    private static Dictionary<int, League> Leagues;
    private static Dictionary<int, Tournament> Tournaments;
    private static Dictionary<int, Match> Matches;
    private static Dictionary<int, Game> Games;

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
        Games = data.Games.Select(x => Game.LoadGame(x)).ToDictionary(x => x.Id, x => x);

        NextPlayerId = Players.Count == 0 ? 1 :         Players.Values.Max(x => x.Id) + 1;
        NextTeamId = Teams.Count == 0 ? 1 :             Teams.Values.Max(x => x.Id) + 1;
        NextLeagueId = Leagues.Count == 0 ? 1 :         Leagues.Values.Max(x => x.Id) + 1;
        NextTournamentId = Tournaments.Count == 0 ? 1 : Tournaments.Values.Max(x => x.Id) + 1;
        NextMatchId = Matches.Count == 0 ? 1 :          Matches.Values.Max(x => x.Id) + 1;
        NextGameId = Games.Count == 0 ? 1 :             Games.Values.Max(x => x.Id) + 1;

        Debug.Log("Loaded simulation state at " + GetQuarterName(Quarter) + " " + Day + ", Season " + Season);
    }

    #endregion

    #region Save

    public static void AddCountry(Country c) => Countries.Add(c.Id, c);
    public static void AddPlayer(Player p) => Players.Add(p.Id, p);
    public static void AddTeam(Team t) => Teams.Add(t.Id, t);
    public static void AddLeague(League l) => Leagues.Add(l.Id, l);
    public static void AddTournament(Tournament t) => Tournaments.Add(t.Id, t);
    public static void AddMatch(Match m) => Matches.Add(m.Id, m);
    public static void AddGame(Game g) => Games.Add(g.Id, g);

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
    public static int GetNewGameId() => NextGameId++;
    public static int GetNewTournamentId() => NextTournamentId++;
    public static int GetNewLeagueId() => NextLeagueId++;

    #endregion

    #region Getters / Stats

    // Read full tables
    public static List<Country> AllCountries => Countries.Values.ToList();
    public static List<Player> AllPlayers => Players.Values.ToList();
    public static List<Team> AllTeams => Teams.Values.ToList();
    public static List<League> AllLeagues => Leagues.Values.ToList();
    public static List<Tournament> AllTournaments => Tournaments.Values.ToList();
    public static List<Match> AllMatches => Matches.Values.ToList();
    public static List<Game> AllGames => Games.Values.ToList();

    // Read single entries by id
    public static Country GetCountry(int id) => Countries[id];
    public static Player GetPlayer(int id) => Players[id];
    public static Team GetTeam(int id) => Teams[id];
    public static League GetLeague(int id) => Leagues[id];
    public static Tournament GetTournament(int id) => Tournaments[id];
    public static Match GetMatch(int id) => Matches[id];
    public static Game GetGame(int id) => Games[id];

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

    public static List<Player> GetPlayerEloRanking(DisciplineDef discipline)
    {
        return Players.Values.OrderByDescending(x => x.Elo[discipline]).ToList();
    }
    public static List<Team> GetTeamEloRanking(DisciplineDef discipline)
    {
        return Teams.Values.Where(x => x.GetMatches().Count > 0).OrderByDescending(x => x.Elo[discipline]).ThenByDescending(x => x.GetAveragePlayerElo(discipline)).ToList();
    }

    /// <summary>
    /// Returns an ordered dictionary representing the country leaderboard of a given country based on elo rating.
    /// </summary>
    public static Dictionary<Player, int> GetCountryRanking(DisciplineDef discipline, string country)
    {
        return Database.Players.Values.Where(x => x.Country.Name == country).OrderByDescending(x => x.Elo[discipline]).ToDictionary(x => x, x => x.Elo[discipline]);
    }
    /// <summary>
    /// Returns an ordered dictionary representing the region leaderboard of a given region based on elo rating.
    /// </summary>
    public static Dictionary<Player, int> GetRegionPlayerRanking(DisciplineDef discipline, string region)
    {
        return Database.Players.Values.Where(x => x.Country.Region == region).OrderByDescending(x => x.Elo[discipline]).ToDictionary(x => x, x => x.Elo[discipline]);
    }
    /// <summary>
    /// Returns an ordered dictionary representing the region leaderboard of a given region based on elo rating.
    /// </summary>
    public static Dictionary<Player, int> GetContinentPlayerRanking(DisciplineDef discipline, string continent)
    {
        return Database.Players.Values.Where(x => x.Country.Continent == continent).OrderByDescending(x => x.Elo[discipline]).ToDictionary(x => x, x => x.Elo[discipline]);
    }

    /// <summary>
    /// Returns an ordered dictionary representing the region leaderboard of a given region based on elo rating for teams.
    /// </summary>
    public static Dictionary<Team, int> GetRegionTeamRanking(DisciplineDef discipline, string region)
    {
        return Teams.Values.Where(x => x.GetMatches().Count > 0 && x.Country.Region == region).OrderByDescending(x => x.Elo[discipline]).ToDictionary(x => x, x => x.Elo[discipline]);
    }
    /// <summary>
    /// Returns an ordered dictionary representing the region leaderboard of a given region based on elo rating for teams.
    /// </summary>
    public static Dictionary<Team, int> GetContinentTeamRanking(DisciplineDef discipline, string continent)
    {
        return Teams.Values.Where(x => x.GetMatches().Count > 0 && x.Country.Continent == continent).OrderByDescending(x => x.Elo[discipline]).ToDictionary(x => x, x => x.Elo[discipline]);
    }

    public static Vector3Int GetTeamMedals(Team team)
    {
        Vector3Int medals = Vector3Int.zero;

        foreach(Tournament tournament in AllTournaments.Where(t => t.IsTeamTournament && t.IsDone))
        {
            Dictionary<int, List<Team>> teamRanking = tournament.TeamRanking;
            
            if(teamRanking[0].Contains(team)) medals += new Vector3Int(1, 0, 0);
            if(teamRanking[1].Contains(team)) medals += new Vector3Int(0, 1, 0);
            if(teamRanking[2].Contains(team)) medals += new Vector3Int(0, 0, 1);
        }

        return medals;
    }

    public static void GetAddMedals<T>(Dictionary<int, List<T>> ranking, Dictionary<T, Vector3Int> medals)
    {
        foreach (T goldWinner in ranking[0])
        {
            if (!medals.ContainsKey(goldWinner)) medals.Add(goldWinner, new Vector3Int(1, 0, 0));
            else medals[goldWinner] += new Vector3Int(1, 0, 0);
        }

        foreach (T silverWinner in ranking[1])
        {
            if (!medals.ContainsKey(silverWinner)) medals.Add(silverWinner, new Vector3Int(0, 1, 0));
            else medals[silverWinner] += new Vector3Int(0, 1, 0);
        }

        foreach (T bronzeWinner in ranking[2])
        {
            if (!medals.ContainsKey(bronzeWinner)) medals.Add(bronzeWinner, new Vector3Int(0, 0, 1));
            else medals[bronzeWinner] += new Vector3Int(0, 0, 1);
        }
    }

    public static Dictionary<Country, List<Player>> GetPlayersByCountry() => Players.Values.GroupBy(x => x.Country).OrderByDescending(x => x.Count()).ToDictionary(x => x.Key, x => x.ToList());

    public static Dictionary<Player, int> GetTeamAppearances(Team team)
    {
        Dictionary<Player, int> appearances = new Dictionary<Player, int>();

        foreach(TeamMatch m in team.GetMatches().Where(x => x.IsDone))
        {
            List<Player> players = m.GetTeamMembersOf(team);
            foreach (Player p in players) appearances.Increment(p);
        }

        return appearances;
    }

    /// <summary>
    /// Returns the x most played teams of a specific team, and returns the results as a Vector3 (x = wins, y = draws, z = losses)
    /// </summary>
    public static Dictionary<Team, Vector3Int> GetTeamRivals(Team team, int maxAmount = 5)
    {
        Dictionary<Team, Vector3Int> rivals = new Dictionary<Team, Vector3Int>();

        // Get all rivals
        foreach(TeamMatch m in team.GetMatches().Where(x => x.IsDone))
        {
            if (m.TeamParticipants.Count != 2) throw new Exception("Statistic currently only works for 1v1 matches.");

            Team opponent = m.TeamParticipants.First(p => p.Team != team).Team;
            if (!rivals.ContainsKey(opponent)) rivals.Add(opponent, Vector3Int.zero);

            if (m.IsWinner(team)) rivals[opponent] += new Vector3Int(1, 0, 0);
            else if (m.IsDraw()) rivals[opponent] += new Vector3Int(0, 1, 0);
            else rivals[opponent] += new Vector3Int(0, 0, 1);
        }

        // Sort by amount of matches
        rivals = rivals.OrderByDescending(x => x.Value.x + x.Value.y + x.Value.z).ToDictionary(x => x.Key, x => x.Value);

        // Limit by maxAmount
        rivals = rivals.Take(maxAmount).ToDictionary(x => x.Key, x => x.Value);

        return rivals;
    }

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
