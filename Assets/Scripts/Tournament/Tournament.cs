using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Tournament
{
    public int Id { get; private set; }
    public string Name { get; protected set; }
    public int LeagueId { get; protected set; }
    public League League { get; protected set; }
    public int Quarter { get; protected set; }
    public int Day { get; protected set; }
    public bool IsDone { get; protected set; }
    public List<Player> Players { get; protected set; }
    public List<Match> Matches { get; protected set; }

    protected int[] PlayersPerPhase;
    protected int[] MatchesPerPhase;

    // New tournament
    public Tournament(LeagueType format, int season, int quarter, int day, List<Player> players, List<League> allLeagues)
    {
        Id = Database.GetNewTournamentId();

        Quarter = quarter;
        Day = day;
        IsDone = false;

        Players = players;

        League = allLeagues.First(x => x.LeagueType == format && x.Season == season);
        LeagueId = League.Id;

        Initialize();
    }

    public abstract void Initialize();

    public virtual List<UI_Group> DisplayTournament(UI_Base baseUI, GameObject Container, UI_Group groupPrefab)
    {
        List<UI_Group> matches = new List<UI_Group>();
        int matchCounter = 0;

        float totalWidth = Container.GetComponent<RectTransform>().rect.width;
        float totalHeight = Container.GetComponent<RectTransform>().rect.height;
        float groupWidth = 300;

        float[] groupHeights = new float[PlayersPerPhase.Length];
        for (int i = 0; i < PlayersPerPhase.Length; i++) groupHeights[i] = 35 + (PlayersPerPhase[i] * 35);
        float totalGroupHeight = groupHeights.Sum(x => x);
        float totalYMargin = totalHeight - totalGroupHeight;
        float yMargin = totalYMargin / (MatchesPerPhase.Length + 1);
        float currentGroupHeight = yMargin + groupHeights[0];

        for (int m = 0; m < MatchesPerPhase.Length; m++)
        {
            int numMatches = MatchesPerPhase[m];

            float yPos = currentGroupHeight;
            if(m < MatchesPerPhase.Length - 1) currentGroupHeight += groupHeights[m + 1] + yMargin;

            float groupTotalMargin = totalWidth - (numMatches * groupWidth);
            float groupMargin = groupTotalMargin / (numMatches + 1);

            for (int i = 0; i < numMatches; i++)
            {
                UI_Group group = GameObject.Instantiate(groupPrefab, Container.transform);
                RectTransform rect = group.GetComponent<RectTransform>();
                rect.position = new Vector2(groupMargin + (i * (groupWidth + groupMargin)), yPos);
                group.Init(baseUI, Matches[matchCounter++]);
                matches.Add(group);
            }
        }
        return matches;
    }

    public static Tournament CreateTournament(LeagueType format, int season, int quarter, int day, List<Player> players, List<League> allLeagues)
    {
        if(format == LeagueType.GrandLeague) return new Format_GrandLeague(format, season, quarter, day, players, allLeagues);
        if(format == LeagueType.ChallengeLeague) return new Format_ChallengeLeague(format, season, quarter, day, players, allLeagues);
        if(format == LeagueType.OpenLeague) return new Format_OpenLeague(format, season, quarter, day, players, allLeagues);
        throw new System.Exception("Format not handled");
    }

    public void SetDone()
    {
        IsDone = true;
        DistributeLeaguePoints();
    }

    protected abstract void DistributeLeaguePoints();
    public override string ToString() => Name;

    #region Save / Load

    public TournamentData ToData()
    {
        TournamentData data = new TournamentData();
        data.Id = Id;
        data.Name = Name;
        data.LeagueId = League.Id;
        data.Quarter = Quarter;
        data.Day = Day;
        data.IsDone = IsDone;
        data.Players = Players.Select(x => x.Id).ToList();
        return data;
    }

    public static Tournament LoadTournament(TournamentData data)
    {
        LeagueType type = Database.Leagues[data.LeagueId].LeagueType;
        if (type == LeagueType.GrandLeague) return new Format_GrandLeague(data);
        if (type == LeagueType.ChallengeLeague) return new Format_ChallengeLeague(data);
        if (type == LeagueType.OpenLeague) return new Format_OpenLeague(data);
        throw new System.Exception("Format not handled");
    }
    public Tournament(TournamentData data)
    {
        Id = data.Id;
        Name = data.Name;
        League = Database.Leagues[data.LeagueId];
        Quarter = data.Quarter;
        Day = data.Day;
        IsDone = data.IsDone;
        Players = data.Players.Select(x => Database.Players[x]).ToList();

        Matches = new List<Match>();
    }

    #endregion
}
