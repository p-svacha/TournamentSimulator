using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoloGameRound : GameRound
{
    public SoloGame Game => (SoloGame)game;
    public SoloGameRound(SoloGame game, SkillDef skill, List<PlayerGameRound> playerResults) : base(game, skill, playerResults) { }
    public SoloGameRound(SoloGame game, GameRoundData data) : base(game, data) { }
}
