using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The value for a specific skill of a specific player.
/// </summary>
public class Skill
{
    /// <summary>
    /// The definition of the skill the values are for.
    /// </summary>
    public SkillDef Def { get; private set; }

    /// <summary>
    /// The base skill value, acting as the average and median.
    /// </summary>
    public float BaseValue { get; private set; }

    /// <summary>
    /// The inconsistency range of this skill. The resolved skill value in a match may differ from the BaseValue up to this value, in both positive and negative direction.
    /// <br/>Inconsistency is applied in a normal distribution, meaning lower discrepancies from the BaseValue are way more likely.
    /// </summary>
    public float Inconsistency { get; private set; }

    /// <summary>
    /// The chance in % that the player makes a mistake for this skill and therefore gets a resolved value of 0.
    /// </summary>
    public float MistakeChance { get; private set; }

    public Skill(SkillDef def, float baseValue, float inconsistency, float mistakeChance)
    {
        Def = def;
        BaseValue = baseValue;
        Inconsistency = inconsistency;
        MistakeChance = mistakeChance;
    }

    /// <summary>
    /// Returns a MatchRoundResult for this skill during match simulation, taking into account the base value, inconsistency and mistake chance.
    /// </summary>
    /// <returns></returns>
    public PlayerGameRound GetGameRoundResult(Player player)
    {
        List<string> modifiers = new List<string>();

        // Score
        int score;
        if (Random.value < MistakeChance) // Mistake
        {
            score = 0;
            modifiers.Add(Player.MISTAKE_MODIFIER);
        }
        else // Inconsistency
        {
            float minValue = BaseValue - Inconsistency;
            float maxValue = BaseValue + Inconsistency;
            score = Mathf.RoundToInt(HelperFunctions.RandomGaussian(minValue, maxValue));
            if (score < 0) score = 0;
        }
        return new PlayerGameRound(player, score, modifiers);
    }

    public void AdjustBaseValue(float adjustmentValue)
    {
        BaseValue += adjustmentValue;
        if (BaseValue < 0) BaseValue = 0;
    }
    public void AdjustInconsistency(float adjustmentValue)
    {
        Inconsistency += adjustmentValue;
        if (Inconsistency < 0) Inconsistency = 0;
    }
    public void AdjustMistakeChance(float adjustmentValue)
    {
        MistakeChance += adjustmentValue;
        MistakeChance = Mathf.Clamp01(MistakeChance);
    }

    #region Load / Save

    public PlayerSkillData ToData()
    {
        PlayerSkillData data = new PlayerSkillData();

        data.Skill = Def.DefName;
        data.BaseValue = BaseValue;
        data.Inconsistency = Inconsistency;
        data.MistakeChance = MistakeChance;

        return data;
    }

    public Skill(PlayerSkillData data)
    {
        Def = DefDatabase<SkillDef>.GetNamed(data.Skill);
        BaseValue = data.BaseValue;
        Inconsistency = data.Inconsistency;
        MistakeChance = data.MistakeChance;
    }

    #endregion
}
