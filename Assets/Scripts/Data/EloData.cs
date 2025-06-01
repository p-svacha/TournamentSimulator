using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EloData
{
    public string Discipline { get; set; }
    public int Elo { get; set; }

    public EloData(string discipline, int elo)
    {
        Discipline = discipline;
        Elo = elo;
    }
}