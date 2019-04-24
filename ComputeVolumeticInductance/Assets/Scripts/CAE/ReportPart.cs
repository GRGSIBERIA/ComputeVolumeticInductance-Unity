using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class TimeData
{
    public float Time;

    public Vector3[] Displacements;

    public TimeData(int numofDisplacements)
    {
        Displacements = new Vector3[numofDisplacements];
    }
}


public class ReportPart : ScriptableObject
{
    [SerializeField] public TimeData[] Data;
}
