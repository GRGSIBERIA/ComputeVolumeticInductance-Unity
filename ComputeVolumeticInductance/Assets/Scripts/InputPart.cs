using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[System.Serializable]
public class EList
{
    public int[] Element = new int[4] { -1, -1, -1, -1 };

    public int this[int i]
    {
        get
        {
            return Element[i];
        }
        set
        {
            Element[i] = value;
        }
    }

    public void SetElement(int[] e)
    {
        int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(int));
        Buffer.BlockCopy(e, 0, Element, 0, size * 4);
    }
}

public class InputPart : ScriptableObject
{
    /// <summary>
    /// パート名
    /// </summary>
    [SerializeField] public string PartName;

    /// <summary>
    /// 最も大きいノード番号
    /// </summary>
    [SerializeField] public int MaxNodeId;

    /// <summary>
    /// 節点の座標値
    /// </summary>
    [SerializeField] public Vector3[] Positions;

    /// <summary>
    /// 最も大きい要素節点番号
    /// </summary>
    [SerializeField] public int MaxElementId;

    /// <summary>
    /// 要素節点番号, 何もないときは-1で埋めている
    /// 4*size な2次元を1次元配列にまとめたもの
    /// </summary>
    [SerializeField] public EList[] Elements;

    /// <summary>
    /// 移動値
    /// </summary>
    [SerializeField] public Vector3 Translate = Vector3.zero;

    /// <summary>
    /// 回転値
    /// </summary>
    [SerializeField] public Quaternion Rotation = Quaternion.identity;

    // 文字列から移動値を設定する
    public void SetTranslate(string line)
    {
        var splits = line.Split(',');
        Vector3 t;
        t.x = float.Parse(splits[0]);
        t.y = float.Parse(splits[1]);
        t.z = float.Parse(splits[2]);
        Translate = t;
    }

    public void SetRotation(string line)
    {
        var splits = line.Split(',');
        Vector3 a, b;
        a.x = float.Parse(splits[0]);
        a.y = float.Parse(splits[1]);
        a.z = float.Parse(splits[2]);
        b.x = float.Parse(splits[3]);
        b.y = float.Parse(splits[4]);
        b.z = float.Parse(splits[5]);
        float angle = float.Parse(splits[6]);
        Rotation = Quaternion.AngleAxis(angle, b - a);
    }
}