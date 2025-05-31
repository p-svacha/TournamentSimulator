using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Discipline
{
    public DisciplineId Id;
    public string Label;
    public string LabelShort;

    public Discipline(DisciplineId id, string label, string labelShort)
    {
        Id = id;
        Label = label;
        LabelShort = labelShort;
    }
}
