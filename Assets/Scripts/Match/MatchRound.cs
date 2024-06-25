using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchRound : MonoBehaviour
{
    public SkillId SkillId { get; private set; }
    public List<PlayerMatchRound> PlayerResults { get; private set; }

    public MatchRound (SkillId skillId, List<PlayerMatchRound> playerResults)
    {
        SkillId = skillId;
        PlayerResults = playerResults;
    }

    #region Save / Load

    public MatchRoundData ToData()
    {
        MatchRoundData data = new MatchRoundData();
        data.SkillId = (int)SkillId;
        data.PlayerResults = PlayerResults.Select(x => x.ToData()).ToList();
        return data;
    }

    public MatchRound(MatchRoundData data)
    {
        SkillId = (SkillId)data.SkillId;
        PlayerResults = data.PlayerResults.Select(x => new PlayerMatchRound(x)).ToList();
    }

    #endregion
}
