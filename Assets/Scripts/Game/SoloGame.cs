using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoloGame : Game
{
    public new SoloMatch Match => (SoloMatch)base.Match;
    public override bool IsTeamGame => false;
    public new List<SoloGameRound> Rounds => base.Rounds.Select(r => (SoloGameRound)r).ToList();

    public SoloGame(Match match, int gameIndex, List<GameModifierDef> gameModifierDefs) : base(match, gameIndex, gameModifierDefs) { }
    public SoloGame(Match match, GameData data) : base(match, data) { }

    public SoloGameRound CreateGameRound(SkillDef skill)
    {
        Dictionary<Player, PlayerGameRound> roundResults = CalculateRoundResult(skill);
        return new SoloGameRound(this, skill, roundResults.Values.ToList());
    }
}
