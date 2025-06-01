using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TournamentDefs
{
    public static List<TournamentDef> Defs => new List<TournamentDef>()
    {
        new TournamentDef()
        {
            DefName = "WarlordsQualifier",
            Label = "Warlords Qualifier",
            Discipline = DisciplineDefOf.AgeOfEmpires,
            NumParticipants = 64,
            Format = TournamentFormatDefOf.SingleElimQualifier64To4Bo5,
            Qualification = new List<TournamentQualificationBatch>()
            {
                new TournamentQualificationBatch()
                {
                    IsInvitation = true,
                    NumParticipants = 64,
                }
            },
            MapPool = new List<MapDef>()
            {

            },
            MapSelectionType = MapSelectionType.Random,
        },

        new TournamentDef()
        {
            DefName = "Warlords",
            Label = "Warlords",
            Discipline = DisciplineDefOf.AgeOfEmpires,
            NumParticipants = 16,
            Format = TournamentFormatDefOf.Groups4x4Into12Playoffs,
            Qualification = new List<TournamentQualificationBatch>()
            {
                new TournamentQualificationBatch()
                {
                    IsInvitation = true,
                    NumParticipants = 4,
                },
                new TournamentQualificationBatch()
                {
                    Tournament = TournamentDefOf.WarlordsQualifier,
                    NumParticipants = 4,
                },
                new TournamentQualificationBatch()
                {
                    Tournament = TournamentDefOf.WarlordsQualifier,
                    NumParticipants = 4,
                },
                new TournamentQualificationBatch()
                {
                    Tournament = TournamentDefOf.WarlordsQualifier,
                    NumParticipants = 4,
                },
            },
            MapPool = new List<MapDef>()
            {

            },
            MapSelectionType = MapSelectionType.Random,
        },
    };
}
