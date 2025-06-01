using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public abstract class Tournament
{
    public int Id { get; private set; }
    public string Name { get; protected set; }
    public DisciplineDef Discipline { get; private set; }
    public TournamentType Format { get; private set; }
    public League League { get; protected set; }
    public int Season { get; protected set; }
    public bool IsDone { get; protected set; }
    public List<Player> Players { get; protected set; }
    public List<TournamentGroup> Groups { get; protected set; }
    public List<Match> Matches { get; protected set; }

    // Attributes for team tournaments
    public int NumPlayersPerTeam { get; protected set; }
    public List<Team> Teams { get; protected set; }
    public bool IsTeamTournament => NumPlayersPerTeam > 0;

    // New tournament
    public Tournament(TournamentType format, int season, League league = null)
    {
        Id = Database.GetNewTournamentId();

        Format = format;
        Season = season;
        IsDone = false;
        League = league;

        if (League != null) League.Tournaments.Add(this);

        Players = new List<Player>();
        Teams = new List<Team>();
    }

    public abstract void Initialize();

    /// <summary>
    /// Returns the label of the n'th day that the tournament takes place.
    /// </summary>
    public abstract string GetMatchDayTitle(int index);

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

    /// <summary>
    /// Returns a list like { 4, 3, 2, 1 } based on the given amount (here: n = 4).
    /// </summary>
    public static List<int> GetBasicPointDistribution(int n)
    {
        List<int> playerPointDistribution = new List<int>();
        for (int i = 0; i < n; i++) playerPointDistribution.Add((n) - i);
        return playerPointDistribution;
    }

    /// <summary>
    /// Returns the (top) players for each rank.
    /// </summary>
    public abstract Dictionary<int, List<Player>> PlayerRanking { get; }

    /// <summary>
    /// Returns the (top) teams for each rank.
    /// </summary>
    public abstract Dictionary<int, List<Team>> TeamRanking { get; }

    /// <summary>
    /// Returns a list of all players that played for the given team in this tournament.
    /// </summary>
    public List<Player> GetTeamPlayers(Team team)
    {
        List<Player> players = new List<Player>();
        foreach(TeamMatch match in Matches.Select(x => (TeamMatch)x))
        {
            if(match.IncludesTeam(team))
            {
                foreach(MatchParticipant_Player player in match.PlayerParticipants.Where(x => x.Team == team))
                {
                    if (!players.Contains(player.Player)) players.Add(player.Player);
                }
            }
        }
        return players;
    }

    #region Display

    public abstract void DisplayTournament(UI_Base baseUI, GameObject container);

    /// <summary>
    /// Displays the tournament as layers whereas each phase is represented as one row with the final phase on top.
    /// </summary>
    protected void DisplayTournamentAsLayers(UI_Base baseUI, GameObject container, int[] playersPerPhase, int[] matchesPerPhase)
    {
        int numPhases = playersPerPhase.Length;
        int matchCounter = 0;

        HelperFunctions.SetRectTransformMargins(container.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f); // Reset container size to fit screen size

        float totalWidth = container.GetComponent<RectTransform>().rect.width;
        float totalHeight = container.GetComponent<RectTransform>().rect.height;
        Debug.Log("Tournament screen is " + totalWidth + "x" + totalHeight);
        float matchWidth = 300; // width of TMatch

        float[] matchHeights = new float[playersPerPhase.Length];
        for (int i = 0; i < playersPerPhase.Length; i++) matchHeights[i] = 35 + (playersPerPhase[i] * 35);

        float totalMatchHeight = matchHeights.Sum(x => x);
        float totalYSpacing = totalHeight - totalMatchHeight;
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
                UI_TMatch match = GameObject.Instantiate(matchPrefab, container.transform);
                RectTransform rect = match.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(groupMargin + (m * (matchWidth + groupMargin)), yPos);
                match.Init(baseUI, Matches[matchCounter++]);
            }
        }
    }

    /// <summary>
    /// Displays the tournament as a dynamic tableau meaning it will be scrollable in all sides and get bigger with more players and spacing between matches will always be the same.
    /// </summary>
    protected void DisplayAsDynamicTableau(UI_Base baseUI, GameObject container, int[] playersPerPhase, int[] matchesPerPhase)
    {
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

        container.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalWidth);
        container.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);

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

        DisplayTableau(baseUI, container, Matches, numColumns, numRows, matchWidth, matchHeight, columnSpacing, rowSpacing);
    }

    /// <summary>
    /// Displays the given matches as a fixed tableau meaning the spacing between matches will get dynamically smaller to fit the screen size.
    /// </summary>
    protected void DisplayAsFixedTableau(UI_Base baseUI, GameObject container, List<Match> matches, int numPlayersPerMatch, float marginBot = 0f)
    {
        // Prefab constants
        float matchWidth = 190;
        float matchHeight = 5 + 35 * numPlayersPerMatch;

        // Calculate display values
        float containerWidth = container.GetComponent<RectTransform>().rect.width;
        float containerHeight = container.GetComponent<RectTransform>().rect.height;

        float displayWidth = containerWidth;
        float displayHeight = containerHeight - marginBot;

        // Calculate column layout values
        int numPhases = (int)Mathf.Log(matches.Count, 2);
        int numColumns = (numPhases * 2) - 1;

        float totalMatchWidth = numColumns * matchWidth;
        float totalColSpacing = displayWidth - totalMatchWidth;
        float colSpacing = totalColSpacing / (numColumns + 1);

        // Calculate row layout values
        int[] numRows = new int[numColumns];
        float[] rowSpacing = new float[numColumns];
        for (int col = 0; col < numColumns; col++)
        {
            // Amount of rows in column
            int distanceFromFinal = Mathf.Abs((numColumns / 2) - col);
            if (distanceFromFinal == 0) numRows[col] = 2;
            else numRows[col] = (int)Mathf.Pow(2, distanceFromFinal - 1);

            // Vertical spacing between matches
            float totalMatchHeight = numRows[col] * matchHeight;
            float totalRowSpacing = displayHeight - totalMatchHeight;
            rowSpacing[col] = totalRowSpacing / (numRows[col] + 1);
        }

        // Display tableau
        DisplayTableau(baseUI, container, matches, numColumns, numRows, matchWidth, matchHeight, colSpacing, rowSpacing, marginBot);
    }

    /// <summary>
    /// Returns the position of a match in the tableau display as a Vector2Int(x = rowIndex, y = colIndex)
    /// </summary>
    private int GetTableauMatchIndexFor(int numMatches, int numColumns, int col, int row)
    {
        int reverseIndex = -1;
        int distanceFromFinal = Mathf.Abs((numColumns / 2) - col);
        if (distanceFromFinal == 0) reverseIndex = 1 - row;
        else
        {
            int numRows = (int)Mathf.Pow(2, distanceFromFinal - 1);
            int startIndexRight = (int)Mathf.Pow(2, distanceFromFinal);
            int startIndexLeft = (int)Mathf.Pow(2, distanceFromFinal) + numRows;
            if (col < (numColumns / 2f)) reverseIndex = startIndexLeft + row;
            if (col > (numColumns / 2f)) reverseIndex = startIndexRight + row;
        }

        return numMatches - reverseIndex - 1;
    }

    /// <summary>
    /// Displays the tournament as a tableau where the first phase is split on both edges left and right going inwards with the final phase is in the center.
    /// <br/>Layout values need to be given.
    /// </summary>
    private void DisplayTableau(UI_Base baseUI, GameObject container, List<Match> matches, int numColumns, int[] numRows, float matchWidth, float matchHeight, float colSpacing, float[] rowSpacing, float marginBot = 0)
    {
        float finalScale = 1.3f;
        float lineWidth = 1.5f;

        // Display matches column by column
        for (int col = 0; col < numColumns; col++)
        {
            for (int row = 0; row < numRows[col]; row++)
            {
                int distanceFromFinal = Mathf.Abs((numColumns / 2) - col);
                bool isOnLeftSide = col < numColumns / 2f;
                bool isCenter = col == numColumns / 2;

                // Get match index based on row and col
                int matchIndex = GetTableauMatchIndexFor(matches.Count, numColumns, col, row);
                // Debug.Log("Match index for " + col + "/" + row + " with " + numColumns + " columns and " + matches.Count + " matches is " + matchIndex);

                // Calculate final match position
                Vector2 matchPos = GetTableauMatchPosition(col, row, matchWidth, matchHeight, colSpacing, rowSpacing, marginBot);
                float matchCenterY = matchPos.y + matchHeight / 2;

                // Center final match
                bool isFinal = (isCenter && row == 1);
                if (isFinal)
                {
                    matchPos.x = (container.GetComponent<RectTransform>().rect.width / 2f) - (matchWidth * finalScale / 2);
                    matchPos.y = marginBot + ((container.GetComponent<RectTransform>().rect.height - marginBot) / 2f) - (matchHeight * finalScale / 2f);
                }

                UI_TMatch matchPrefab = ResourceManager.Singleton.TournamentMatchCompactPrefab;
                UI_TMatch match = GameObject.Instantiate(matchPrefab, container.transform);
                RectTransform rect = match.GetComponent<RectTransform>();
                rect.anchoredPosition = matchPos;
                if (isFinal) rect.localScale = new Vector3(finalScale, finalScale, 1f);
                match.Init(baseUI, matches[matchIndex]);

                // Draw line going to next match
                if (!isCenter)
                {
                    float toLineStartX = isOnLeftSide ? matchPos.x + matchWidth : matchPos.x;

                    float width = distanceFromFinal == 1 ? colSpacing : colSpacing / 2;
                    float toLineEndX = isOnLeftSide ? toLineStartX + width : toLineStartX - width;
                    HelperFunctions.DrawLine(container, new Vector2(toLineStartX, matchCenterY), new Vector2(toLineEndX, matchCenterY), Color.white, lineWidth);
                }

                // Draw lines coming from previous 2 matches
                if(col != 0 && col != numColumns - 1 && !isCenter)
                {
                    // Vertical line
                    float verticalX = isOnLeftSide ? matchPos.x - colSpacing / 2 : matchPos.x + matchWidth + colSpacing / 2;

                    Vector2Int prevMatch1GridPosition = isOnLeftSide ? new Vector2Int(col - 1, row * 2) : new Vector2Int(col + 1, row * 2);
                    Vector2 prevMatch1Pos = GetTableauMatchPosition(prevMatch1GridPosition.x, prevMatch1GridPosition.y, matchWidth, matchHeight, colSpacing, rowSpacing, marginBot);
                    float verticalYStart = prevMatch1Pos.y + matchHeight / 2;

                    Vector2Int prevMatch2GridPosition = isOnLeftSide ? new Vector2Int(col - 1, row * 2 + 1) : new Vector2Int(col + 1, row * 2 + 1);
                    Vector2 prevMatch2Pos = GetTableauMatchPosition(prevMatch2GridPosition.x, prevMatch2GridPosition.y, matchWidth, matchHeight, colSpacing, rowSpacing, marginBot);
                    float verticalYEnd = prevMatch2Pos.y + matchHeight / 2;

                    HelperFunctions.DrawLine(container, new Vector2(verticalX, verticalYStart), new Vector2(verticalX, verticalYEnd), Color.white, lineWidth);

                    // Horizontal line
                    float fromLineEndX = isOnLeftSide ? matchPos.x : matchPos.x + matchWidth;
                    HelperFunctions.DrawLine(container, new Vector2(verticalX, matchCenterY), new Vector2(fromLineEndX, matchCenterY), Color.white, lineWidth);
                }
            }
        }
    }

    private Vector2 GetTableauMatchPosition(int col, int row, float matchWidth, float matchHeight, float colSpacing, float[] rowSpacing, float marginBot)
    {
        // Calculate final match position
        float xPos = ((col + 1) * colSpacing) + (col * matchWidth);

        //float rowSpacing = (col == 0 || col == numColumns - 1) ? outerColumnRowSpacing : innerColumnRowSpacing;
        float yPos = (row * matchHeight) + ((row + 1) * rowSpacing[col]);
        yPos += marginBot;

        return new Vector2(xPos, yPos);
    }

    protected void DisplayAsGroupAndTableau(UI_Base baseUI, GameObject container)
    {
        HelperFunctions.SetRectTransformMargins(container.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f); // Reset container size to fit screen size
        float containerWidth = container.GetComponent<RectTransform>().rect.width;
        float containerHeight = container.GetComponent<RectTransform>().rect.height;

        // Groups
        int numGroups = Groups.Count;
        float groupWidth = 400; // width of TGroup
        float totalGroupWidth = numGroups * groupWidth;
        float totalGroupXSpacing = containerWidth - totalGroupWidth;
        float groupSpacingX = totalGroupXSpacing / (numGroups + 1);

        float groupStartY = 20;
        float groupHeight = 0;

        for(int i = 0; i < Groups.Count; i++)
        {
            float xPos = ((i + 1) * groupSpacingX) + (i * groupWidth);
            float yPos = groupStartY;
            UI_TGroup group = GameObject.Instantiate(ResourceManager.Singleton.TournamentGroupPrefab, container.transform);
            group.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, yPos);
            group.Init(baseUI, Groups[i]);
            groupHeight = group.GetComponent<RectTransform>().rect.height;
        }

        // KO-Tableau
        float tableauStartY = 500;
        DisplayAsFixedTableau(baseUI, container, Matches.Skip(24).ToList(), numPlayersPerMatch: 2, tableauStartY);
    }

    #endregion

    #region Save / Load

    public TournamentData ToData()
    {
        TournamentData data = new TournamentData();
        data.Id = Id;
        data.Name = Name;
        data.Discipline = Discipline.DefName;
        data.Format = (int)Format;
        data.LeagueId = League == null ? -1 : League.Id;
        data.Season = Season;
        data.IsDone = IsDone;
        data.Players = Players.Select(x => x.Id).ToList();
        data.Groups = Groups.Select(x => x.ToData()).ToList();
        data.Teams = Teams.Select(x => x.Id).ToList();
        data.NumPlayersPerTeam = NumPlayersPerTeam;
        return data;
    }

    public static Tournament LoadTournament(TournamentData data)
    {
        TournamentType format = (TournamentType)data.Format;
        if (format == TournamentType.GrandLeague) return new Format_GrandLeague(data);
        if (format == TournamentType.ChallengeLeague) return new Format_ChallengeLeague(data);
        if (format == TournamentType.OpenLeague) return new Format_OpenLeague(data);
        if (format == TournamentType.SeasonCup) return new Format_SeasonCup(data);
        if (format == TournamentType.WorldCup) return new Format_WorldCup(data);
        throw new System.Exception("Format not handled");
    }
    protected Tournament(TournamentData data)
    {
        Id = data.Id;
        Name = data.Name;
        Discipline = DefDatabase<DisciplineDef>.GetNamed(data.Discipline);
        Format = (TournamentType)data.Format;
        League = data.LeagueId == -1 ? null : Database.GetLeague(data.LeagueId);
        Season = data.Season;
        IsDone = data.IsDone;
        Players = data.Players.Select(id => Database.GetPlayer(id)).ToList();
        Groups = data.Groups.Select(x => new TournamentGroup(this, x)).ToList();

        Teams = data.Teams.Select(id => Database.GetTeam(id)).ToList();
        NumPlayersPerTeam = data.NumPlayersPerTeam;

        // References
        if(League != null) League.Tournaments.Add(this);

        Matches = new List<Match>();
    }

    #endregion
}
