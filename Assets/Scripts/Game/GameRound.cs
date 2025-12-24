using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class GameRound
{
    protected Game game;
    public SkillDef Skill { get; private set; }
    public List<PlayerGameRound> PlayerResults { get; private set; }

    public GameRound (Game game, SkillDef skill, List<PlayerGameRound> playerResults)
    {
        this.game = game;
        Skill = skill;
        PlayerResults = playerResults;
    }

    public int NumParticipants => PlayerResults.Count;
    public bool HasParticipated(Player p) => PlayerResults.FirstOrDefault(r => r.Player == p) != null;
    public PlayerGameRound GetPlayerResult(Player p) => PlayerResults.FirstOrDefault(r => r.Player == p);
    public int GetPointsGained(Player p) => HasParticipated(p) ? GetPlayerResult(p).PointsGained : 0;
    public int GetScore(Player p) => HasParticipated(p) ? GetPlayerResult(p).Score : 0;
    public List<Player> PlayerRanking => PlayerResults.OrderByDescending(x => x.Score).Select(x => x.Player).ToList();

    #region Save / Load

    public GameRoundData ToData()
    {
        GameRoundData data = new GameRoundData();
        data.Skill = Skill.DefName;
        data.PlayerResults = PlayerResults.Select(x => x.ToData()).ToList();
        return data;
    }

    public static GameRound LoadGameRound(Game game, GameRoundData data)
    {
        if (game.IsTeamGame) return new TeamGameRound((TeamGame)game, data);
        else return new SoloGameRound((SoloGame)game, data);
    }

    protected GameRound(Game game, GameRoundData data)
    {
        this.game = game;
        Skill = DefDatabase<SkillDef>.GetNamed(data.Skill);
        PlayerResults = data.PlayerResults.Select(x => new PlayerGameRound(x)).ToList();
    }

    #endregion
}
