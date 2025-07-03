using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UI_Dashboard : MonoBehaviour
{
    /// <summary>
    /// Gets called once when starting the program.
    /// </summary>
    public abstract void Init();

    /// <summary>
    /// Gets called when the content of this dashboard should be loaded and displayed for the given season.
    /// </summary>
    public abstract void Refresh(DisciplineDef discipline, int season);

    /// <summary>
    /// Display label of the dashboard used in the dashboard dropdown.
    /// </summary>
    public abstract string Label { get; }
}
