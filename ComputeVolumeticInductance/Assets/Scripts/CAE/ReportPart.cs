using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class DisplacementData
{
    /// <summary>
    /// 変位
    /// </summary>
    public Vector3[] Displacements;

    public DisplacementData(int numofDisplacements)
    {
        Displacements = new Vector3[numofDisplacements];
    }
}


public class ReportPart : ScriptableObject
{
    /// <summary>
    /// 変位のデータ
    /// </summary>
    [SerializeField] public DisplacementData[] Data;

    /// <summary>
    /// 時間の配列
    /// </summary>
    [SerializeField] public float[] Times;
}
