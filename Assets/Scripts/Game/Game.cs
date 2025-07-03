using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A game represents a subcomponent of a match. Each match is a series of 1 - n games.
/// <br/>Games are made up of game rounds, where in each round the values of a specific skill are pitted against each other.
/// <br/>The skills in the game are defined by the map the game is played on.
/// </summary>
public abstract class Game
{
    public int Id { get; private set; }

    /// <summary>
    /// The match is game is a part of.
    /// </summary>
    public Match Match { get; private set; }
    public abstract bool IsTeamGame { get; }
    public Discipline Discipline => Match.Discipline;

    /// <summary>
    /// The index defining the how many'th game this is within a match. (1. match in a bo5 has GameIndex = 0, 5. match is GameIndex = 4)
    /// </summary>
    public int GameIndex { get; private set; }

    /// <summary>
    /// The complete set of skills that are competed on in this game.
    /// </summary>
    public List<SkillDef> Skills { get; private set; }

    /// <summary>
    /// The set of game modifiers that are active in this game.
    /// </summary>
    public List<GameModifier> GameModifiers { get; private set; }

    /// <summary>
    /// The list of each round in this game.
    /// </summary>
    public List<GameRound> Rounds { get; private set; }

    /// <summary>
    /// Flag if this game has already been played.
    /// </summary>
    public bool IsDone { get; private set; }

    /// <summary>
    /// Flag if this game is currently running.
    /// </summary>
    public bool IsRunning { get; private set; }

    public string Label => $"{Match.Name} - Game {GameIndex + 1}";

    public Game(Match match, int gameIndex, List<GameModifierDef> gameModifierDefs)
    {
        Id = Database.GetNewMatchId();
        Match = match;
        GameIndex = gameIndex;

        // Generate set of skills
        Skills = new List<SkillDef>();
        Skills.AddRange(Discipline.Skills);
        GameModifiers = gameModifierDefs.Select(def => new GameModifier(def)).ToList();
        foreach (GameModifier modifier in GameModifiers) Skills.AddRange(modifier.Skills);
    }

    public bool CanStartGame()
    {
        if (Match.IsDone) return false;
        if (!Match.IsRunning) return false;
        if (!IsDone) return false;
        if (IsRunning) return false;
        return true;
    }

    public void StartGame()
    {
        if (!CanStartGame()) throw new System.Exception("Can't start game.");

        IsRunning = true;
        OnStartGame();
    }
    protected virtual void OnStartGame() { }


    /// <summary>
    /// Ends the game, adding it's result to the match score of all participants.
    /// </summary>
    public void SetDone()
    {
        IsDone = true;
        IsRunning = false;
        Match.OnGameDone(this);
    }

    /// <summary>
    /// Executes all the logic behind a round in the game for a certain skill and returns the complete results, sorted by player.
    /// </summary>
    protected Dictionary<Player, PlayerGameRound> CalculateRoundResult(SkillDef skill)
    {
        // Calculate skill score for each participant
        Dictionary<Player, PlayerGameRound> roundResults = new Dictionary<Player, PlayerGameRound>();

        foreach (MatchParticipant_Player p in Match.PlayerParticipants)
        {
            PlayerGameRound playerResult = p.Player.GetMatchRoundResult(skill);
            roundResults.Add(p.Player, playerResult);
        }

        // Save all values for the round
        List<Player> attributeRanking = roundResults.OrderBy(x => x.Value.Score).Select(x => x.Key).ToList();
        int lastScore = -1;
        int lastPoints = -1;
        for (int rank = 0; rank < attributeRanking.Count; rank++)
        {
            Player player = attributeRanking[rank];
            int score = roundResults[player].Score;
            int points = Match.PointDistribution[Match.PointDistribution.Count - rank - 1];
            if (score == 0) points = 0;
            else if (score == lastScore) points = lastPoints;
            roundResults[player].SetPointsGained(points);
            lastScore = score;
            lastPoints = points;
        }

        return roundResults;
    }

    /// <summary>
    /// Distributes the player points according to the round and saves the round as part of this game.
    /// </summary>
    public void ApplyGameRound(GameRound round)
    {
        // Distribute Points
        foreach (PlayerGameRound playerResult in round.PlayerResults)
        {
            MatchParticipant_Player participant = Match.GetParticipant(playerResult.Player);
            int pointsGained = round.GetPlayerResult(playerResult.Player).PointsGained;
        }

        // Save
        Rounds.Add(round);

        // Hook
        OnApplyGameRound(round);
    }
    protected virtual void OnApplyGameRound(GameRound round) { }

    /// <summary>
    /// The primary rating of the game leaderboard are the points. Points are awarded based on the ranking of the score that each player makes for each skill.
    /// </summary>
    public int GetPlayerPoints(MatchParticipant_Player player) => Rounds.Sum(x => x.GetPlayerResult(player.Player).PointsGained);

    /// <summary>
    /// Returns the accumulated amount of SCORE a player has gathered throughout the game.
    /// </summary>
    public int GetTotalPlayerScore(MatchParticipant_Player player) => Rounds.Sum(x => x.GetPlayerResult(player.Player).Score);

    /// <summary>
    /// Returns the player ranking as a dictionary ordered by total amount of points, and then total amount of score.
    /// </summary>
    public List<MatchParticipant_Player> GetPlayerRanking()
    {
        if (IsDone || IsRunning)
        {
            return Match.PlayerParticipants.OrderByDescending(x => GetPlayerPoints(x)).ThenByDescending(p => GetTotalPlayerScore(p)).ToList();
        }
        return new List<MatchParticipant_Player>();
    }

    #region Save / Load

    public virtual GameData ToData()
    {
        GameData data = new GameData();

        data.Id = Id;
        data.MatchId = Match.Id;
        data.Index = GameIndex;
        data.Skills = Skills.Select(s => s.DefName).ToList();
        data.GameModifiers = GameModifiers.Select(m => m.ToData()).ToList();
        data.Rounds = Rounds.Select(x => x.ToData()).ToList();

        return data;
    }

    public static Game LoadGame(GameData data)
    {
        Match match = Database.GetMatch(data.MatchId);
        if (match.IsTeamMatch) return new TeamGame(match, data);
        else return new SoloGame(match, data);
    }
    protected Game(Match match, GameData data)
    {
        Id = data.Id;
        this.Match = match;
        GameIndex = data.Index;
        Skills = data.Skills.Select(s => DefDatabase<SkillDef>.GetNamed(s)).ToList();
        GameModifiers = data.GameModifiers.Select(m => new GameModifier(m)).ToList();
        Rounds = data.Rounds.Select(x => GameRound.LoadGameRound(this, x)).ToList();
        IsDone = true;

        // Parent ref
        match.Games.Add(this);
    }

    #endregion
}