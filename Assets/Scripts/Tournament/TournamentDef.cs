using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TournamentDef : Def
{
    /// <summary>
    /// The discipline the tournament played on.
    /// </summary>
    public DisciplineDef Discipline { get; init; }

    /// <summary>
    /// The number of players/teams participating.
    /// </summary>
    public int NumParticipants { get; init; }

    /// <summary>
    /// The format that this tournament is played in.
    /// </summary>
    public TournamentFormatDef Format { get; init; }

    /// <summary>
    /// The set of maps that can be played during this tournament.
    /// </summary>
    public List<GameModifierDef> MapPool { get; init; }

    /// <summary>
    /// How maps are chosen in games.
    /// </summary>
    public MapSelectionType MapSelectionType { get; init; }

    /// <summary>
    /// The breakdown of how participants qualify for this tournament.
    /// </summary>
    public List<TournamentQualificationBatch> Qualification { get; init; }
}
