using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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

    public static Tournament CreateTournament(TournamentType format, int season, int quarter = 0, int day = 0)
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

    #region Display

    public abstract List<UI_TMatch> DisplayTournament(UI_Base baseUI, GameObject Container);

    /// <summary>
    /// Displays the tournament as layers whereas each phase is represented as one row with the final phase on top.
    /// </summary>
    protected List<UI_TMatch> DisplayTournamentAsLayers(UI_Base baseUI, GameObject Container, int[] playersPerPhase, int[] matchesPerPhase)
    {
        List<UI_TMatch> matches = new List<UI_TMatch>();
        int numPhases = playersPerPhase.Length;

        int matchCounter = 0;

        HelperFunctions.SetRectTransformMargins(Container.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f); // Reset container size

        float totalWidth = Container.GetComponent<RectTransform>().rect.width;
        float totalHeight = Container.GetComponent<RectTransform>().rect.height;
        Debug.Log("Tournament screen is " + totalWidth + "x" + totalHeight);
        float matchWidth = 300; // width of TMatch

        float[] matchHeights = new float[playersPerPhase.Length];
        for (int i = 0; i < playersPerPhase.Length; i++) matchHeights[i] = 35 + (playersPerPhase[i] * 35);

        float totalGroupHeight = matchHeights.Sum(x => x);
        float totalYSpacing = totalHeight - totalGroupHeight;
        float ySpacingStep = totalYSpacing / (matchesPerPhase.Length + 1);
        float currentMatchY = ySpacingStep;

        for (int i = 0; i < numPhases; i++)
        {
            int numMatches = matchesPerPhase[i];

            float yPos = currentMatchY;
            currentMatchY += matchHeights[i] + ySpacingStep;

            float groupTotalMargin = totalWidth - (numMatches * matchWidth);
            float groupMargin = groupTotalMargin / (numMatches + 1);

            for (int m = 0; m < numMatches; m++)
            {
                UI_TMatch matchPrefab = ResourceManager.Singleton.TournamentMatchPrefab;
                UI_TMatch group = GameObject.Instantiate(matchPrefab, Container.transform);
                RectTransform rect = group.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(groupMargin + (m * (matchWidth + groupMargin)), yPos);
                group.Init(baseUI, Matches[matchCounter++]);
                matches.Add(group);
            }
        }
        return matches;
    }

    /// <summary>
    /// Displays the tournament as a tableau where the first phase is split on both edges left and right going inwards with the final phase is in the center.
    /// </summary>
    protected List<UI_TMatch> DisplayTournamentAsTableau(UI_Base baseUI, GameObject Container, int[] playersPerPhase, int[] matchesPerPhase)
    {
        List<UI_TMatch> matches = new List<UI_TMatch>();

        // Calculate amount of columns
        int numPhases = playersPerPhase.Length;
        int numColumns = (numPhases * 2) - 1;

        // Calculate amount of rows on the outside columns
        int maxNumRows = matchesPerPhase.Max() / 2;

        // Calculate total display size of tournament
        float outerColumnRowSpacing = 10; // vertical space between matches on the outest-most rows (1st round)
        //float innerColumnRowSpacing = 20; // vertical space between matches on the inner rows (2nd+ rounds)
        float columnSpacing = 50;
        float matchWidth = 190; // width of compact TMatch + 10 padding
        float matchHeight = 5 + 35 * playersPerPhase.Max();

        float totalMatchWidth = numColumns * matchWidth;
        float totalWidth = totalMatchWidth + (numColumns + 1) * columnSpacing;

        float totalHeight = (maxNumRows * matchHeight) + ((maxNumRows + 1) * outerColumnRowSpacing);

        Container.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalWidth);
        Container.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);

        // Calculate amount of rows and vertical spacing in each column
        int[] numRows = new int[numColumns];
        float[] rowSpacing = new float[numColumns];
        for (int col = 0; col < numColumns; col++)
        {
            // Amount of rows in column
            int distanceFromFinal = Mathf.Abs((numColumns / 2) - col);
            numRows[col] = matchesPerPhase[(numPhases - 1) - distanceFromFinal] / 2; // all phases are split in 2 on left and right side
            if (distanceFromFinal == 0) numRows[col] = matchesPerPhase[numPhases - 1]; // final phase in center

            // Vertical spacing between matches
            float totalMatchHeight = numRows[col] * matchHeight;
            float totalSpacingHeight = totalHeight - totalMatchHeight;
            rowSpacing[col] = totalSpacingHeight / (numRows[col] + 1);
            //Debug.Log("row spacing for column " + col + " is " + firstRowYPosition[col]);
        }

        // Calculate the exact row and column index for each match
        int[][] matchMap = new int[numColumns][];
        for (int col = 0; col < numColumns; col++) matchMap[col] = new int[numRows[col]];
        
        for(int i = 0; i < Matches.Count; i++)
        {
            int matchIndex = Matches.Count - i - 1; // we start in final and go backwards from there
            if(i < 2) // Finals
            {
                int col = numColumns / 2;
                int row = Mathf.Abs(i - 1);
                matchMap[col][row] = matchIndex;
            }
            else if(i < 4) // Semifinals
            {
                int col = i < 3 ? (numColumns / 2) + 1 : (numColumns / 2) - 1;
                int row = 0;
                matchMap[col][row] = matchIndex;
            }
            else if (i < 8) // Quarters
            {
                int col = i < 6 ? (numColumns / 2) + 2 : (numColumns / 2) - 2;
                int row = i % 2;
                matchMap[col][row] = matchIndex;
            }
            else if (i < 16) // Ro16
            {
                int col = i < 12 ? (numColumns / 2) + 3 : (numColumns / 2) - 3;
                int row = i % 4;
                matchMap[col][row] = matchIndex;
            }
            else if (i < 32) // Ro32
            {
                int col = i < 24 ? (numColumns / 2) + 4 : (numColumns / 2) - 4;
                int row = i % 8;
                matchMap[col][row] = matchIndex;
            }
            else if (i < 64) // Ro64
            {
                int col = i < 48 ? (numColumns / 2) + 5 : (numColumns / 2) - 5;
                int row = i % 16;
                matchMap[col][row] = matchIndex;
            }
            else if (i < 128) // Ro128
            {
                int col = i < 96 ? (numColumns / 2) + 6 : (numColumns / 2) - 6;
                int row = i % 32;
                matchMap[col][row] = matchIndex;
            }
            else if (i < 256) // Ro256
            {
                int col = i < 192 ? (numColumns / 2) + 7 : (numColumns / 2) - 7;
                int row = i % 64;
                matchMap[col][row] = matchIndex;
            }
        }

        // Display matches column by column
        for (int col = 0; col < numColumns; col++)
        {
            for (int row = 0; row < numRows[col]; row++)
            {
                // Get match index based on row and col
                int matchIndex = matchMap[col][row];

                // Calculate final match position
                float xPos = ((col + 1) * columnSpacing) + (col * matchWidth);

                //float rowSpacing = (col == 0 || col == numColumns - 1) ? outerColumnRowSpacing : innerColumnRowSpacing;
                float yPos = (row * matchHeight) + ((row + 1) * rowSpacing[col]);

                UI_TMatch matchPrefab = ResourceManager.Singleton.TournamentMatchCompactPrefab;
                UI_TMatch group = GameObject.Instantiate(matchPrefab, Container.transform);
                RectTransform rect = group.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(xPos, yPos);
                group.Init(baseUI, Matches[matchIndex]);

                matches.Add(group);
            }
        }
        return matches;
    }

    #endregion

    #region Save / Load

    public TournamentData ToData()
    {
        TournamentData data = new TournamentData();
        data.Id = Id;
        data.Name = Name;
        data.Format = (int)Format;
        data.LeagueId = League == null ? -1 : League.Id;
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
    protected Tournament(TournamentData data)
    {
        Id = data.Id;
        Name = data.Name;
        Format = (TournamentType)data.Format;
        League = data.LeagueId == -1 ? null : Database.Leagues[data.LeagueId];
        Season = data.Season;
        IsDone = data.IsDone;
        Players = data.Players.Select(x => Database.Players[x]).ToList();

        Matches = new List<Match>();
    }

    #endregion
}
