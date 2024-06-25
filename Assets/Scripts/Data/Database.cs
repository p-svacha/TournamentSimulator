using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Singleton class that acts as a central database that can be accessed from everywhere.
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

    #region Init

    public static SimulationData LoadData(TournamentSimulator sim)
    {
        Countries = ReadCountries();

        SimulationData data = JsonUtilities.LoadData<SimulationData>("simulation_data");
        NextPlayerId = data.Players.Max(x => x.Id) + 1;
        NextLeagueId = data.Leagues.Max(x => x.Id) + 1;
        NextTournamentId = data.Tournaments.Max(x => x.Id) + 1;
        NextMatchId = data.Matches.Max(x => x.Id) + 1;
        
        return data;
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
}
