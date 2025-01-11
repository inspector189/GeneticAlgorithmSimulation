using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Gene
{
    public string Name;
    public int Min;
    public int Max;
    public int CurrentValue;

    public Gene(string name, int min, int max, int currentValue)
    {
        Name = name;
        Min = min;
        Max = max;
        CurrentValue = currentValue;
    }
    public Gene(Gene gene)
    {
        Name = gene.Name;
        Min = gene.Min;
        Max = gene.Max;
        CurrentValue = gene.CurrentValue;
    }
    public int getMin()
    { return Min; }
    public int getMax()
    { return Max; }
}