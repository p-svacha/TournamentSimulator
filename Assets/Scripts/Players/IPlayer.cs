using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayer
{
    /// <summary>
    /// Attribute that defines the limits the range of the gaussian normal distribution when calculating the score for a skill.
    /// <br/>All calculated skill scores will be within the inconsistency range of the skill base value.
    /// </summary>
    public float Inconsistency { get; }

    /// <summary>
    /// Attribute that is used to resolve ties in tournaments and leagues.
    /// <br/>Should never be changed because of backwards compatibility of resolved ties.
    /// </summary>
    public float TiebreakerScore { get; }

    /// <summary>
    /// Attribute that defines the chance that a skill score calculation returns 0.
    /// </summary>
    public float MistakeChance { get; }
}
