using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Tournament
{
    public int Id { get; private set; }
    public string Name { get; protected set; }
    public TournamentType Format { get; private set; }
    public League League { get; protected set; }
    public int Season { get; protected set; }
    public bool IsDone { get; protected set; }
    public List<Player> Players { get; protected set; }
    public List<Match> Matches { get; protected set; }

    protected int[] PlayersPerPhase;
    protected int[] MatchesPerPhase;

    // New tournament
    public Tournament(TournamentType format, int season, League league = null)
    {
        Id = Database.GetNewTournamentId();

        Format = format;
        Season = season;
        IsDone = false;
        League = league;
    }

    public abstract void Initialize();
    public abstract string GetMatchDayTitle(int index);

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

    public static Tournament CreateTournament(TournamentType format, int season, int quarter, int day)
    {
        if (format == TournamentType.GrandLeague) return new Format_GrandLeague(season, quarter, day, Database.CurrentGrandLeague);
        if (format == TournamentType.ChallengeLeague) return new Format_ChallengeLeague(season, quarter, day, Database.CurrentChallengeLeague);
        if (format == TournamentType.OpenLeague) return new Format_OpenLeague(season, quarter, day, Database.CurrentOpenLeague);
        if (format == TournamentType.SeasonCup) return new Format_SeasonCup(season);
        throw new System.Exception("Format not handled");
    }

    public void SetDone()
    {
        IsDone = true;
        OnTournamentDone();
    }

    protected virtual void OnTournamentDone() { }

    public override string ToString() => Name;
    public List<Match> GetMatchesOf(int season, int quarter, int day) => Matches.Where(x => Season == season && x.Quarter == quarter && x.Day == day).ToList();
    public List<Match> GetTodaysMatches() => GetMatchesOf(Database.Season, Database.Quarter, Database.Day);
    public bool HasOpenMatchesToday() => GetTodaysMatches().Any(x => !x.IsDone);

    /// <summary>
    /// Returns a list of all days (as absolute days) that have at least 1 match of this tournament.
    /// </summary>
    public List<int> GetMatchDays() => Matches.Select(x => x.AbsoluteDay).Distinct().ToList();

    #region Save / Load

    public TournamentData ToData()
    {
        TournamentData data = new TournamentData();
        data.Id = Id;
        data.Name = Name;
        data.Format = (int)Format;
        data.LeagueId = League.Id;
        data.Season = Season;
        data.IsDone = IsDone;
        data.Players = Players.Select(x => x.Id).ToList();
        return data;
    }

    public static Tournament LoadTournament(TournamentData data)
    {
        TournamentType format = (TournamentType)data.Format;
        if (format == TournamentType.GrandLeague) return new Format_GrandLeague(data);
        if (format == TournamentType.ChallengeLeague) return new Format_ChallengeLeague(data);
        if (format == TournamentType.OpenLeague) return new Format_OpenLeague(data);
        if (format == TournamentType.SeasonCup) return new Format_SeasonCup(data);
        throw new System.Exception("Format not handled");
    }
    public Tournament(TournamentData data)
    {
        Id = data.Id;
        Name = data.Name;
        Format = (TournamentType)data.Format;
        League = Database.Leagues[data.LeagueId];
        Season = data.Season;
        IsDone = data.IsDone;
        Players = data.Players.Select(x => Database.Players[x]).ToList();

        Matches = new List<Match>();
    }

    #endregion
}
