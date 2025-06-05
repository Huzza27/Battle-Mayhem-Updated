using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MatchStat")]
public class MatchStatTitle : ScriptableObject
{
    public string registeredName;
    public string title;
    public string description;
    [NonSerialized] public Func<PlayerStats, float> statEvaluator;
}
