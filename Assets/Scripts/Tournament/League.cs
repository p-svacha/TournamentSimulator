using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class League
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public int Season { get; private set; }
    public TournamentType LeagueType { get; private set; }
    public Dictionary<Player, int> Standings { get; private set; }
    public int NumPromotions { get; private set; }
    public int NumRelegations { get; private set; }
    public List<Tournament> Tournaments { get; private set; }
    

    public Sprite Icon => ColorManager.Singleton.LeagueIcons[(int)LeagueType];
    public Color Color => ColorManager.Singleton.LeagueColors[(int)LeagueType];
    public List<Player> Players => Standings.Keys.ToList();
    public bool IsDone => Tournaments.All(x => x.IsDone);

    public League(string name, int season, int formatId, List<Player> players, int numPromotions, int numRelegations)
    {
        Id = Database.GetNewLeagueId();
        Name = name;
        Season = season;
        LeagueType = (TournamentType)formatId;
        NumPromotions = numPromotions;
        NumRelegations = numRelegations;

        Standings = new Dictionary<Player, int>();
        foreach (Player p in players) Standings.Add(p, 0);

        Tournaments = new List<Tournament>();
    }

    public List<Player> Ranking => Standings.OrderByDescending(x => x.Value).ThenByDescending(x => x.Key.TiebreakerScore).Select(x => x.Key).ToList();
    public int GetRankOf(Player p) => Ranking.IndexOf(p) + 1;

    #region Save / Load

    public LeagueData ToData()
    {
        LeagueData data = new LeagueData();
        data.Id = Id;
        data.Name = Name;
        data.Season = Season;
        data.LeagueType = (int)LeagueType;
        data.Participants = Standings.Select(x => new LeagueParticipantData() { PlayerId = x.Key.Id, LeaguePoints = x.Value }).ToList();
        data.NumPromotions = NumPromotions;
        data.NumRelegations = NumRelegations;
        return data;
    }

    public League(LeagueData data)
    {
        Id = data.Id;
        Name = data.Name;
        Season = data.Season;
        LeagueType = (TournamentType)data.LeagueType;
        Standings = data.Participants.ToDictionary(x => Database.Players[x.PlayerId], x => x.LeaguePoints);
        NumPromotions = data.NumPromotions;
        NumRelegations = data.NumRelegations;

        Tournaments = new List<Tournament>();
    }

    #endregion
}
