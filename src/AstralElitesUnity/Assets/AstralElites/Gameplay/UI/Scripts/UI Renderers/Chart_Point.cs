using System;
using UnityEngine;

[Serializable]
public struct Chart_Point
{
    [Range(0.0f, 1.0f)]
    public float Time;

    [Range(0.0f, 1.0f)]
    public float Value;

    public Chart_Point(float time, float value)
    {
        Time = time;
        Value = value;
    }
}
