using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayer
{
    /// <summary>
    /// Attribute that defines the standard deviation when calculating the score for a skill.
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
