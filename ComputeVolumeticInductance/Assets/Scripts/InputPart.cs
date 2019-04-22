using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ElementList
{
    public int[] Element = new int[4] { -1, -1, -1, -1 };

    private static readonly int[] indices = { 0, 1, 2, 3, 0, 1 };

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

    /// <summary>
    /// 四面体からエッジを取得
    /// </summary>
    /// <returns>エッジの配列</returns>
    public EdgeList[] GetEdges()
    {
        var edges = new EdgeList[6];

        edges[0] = new EdgeList(Element[0], Element[1]);
        edges[1] = new EdgeList(Element[0], Element[2]);
        edges[2] = new EdgeList(Element[0], Element[3]);

        edges[3] = new EdgeList(Element[1], Element[2]);
        edges[4] = new EdgeList(Element[1], Element[3]);
        edges[5] = new EdgeList(Element[2], Element[3]);

        return edges;
    }

    /// <summary>
    /// 四面体のフェースを取得
    /// </summary>
    /// <returns>フェースの配列</returns>
    public FaceList[] GetFaces()
    {
        var faces = new FaceList[4];
        for (int i = 0; i < 4; ++i)
        {
            faces[i] = new FaceList(Element[indices[i]], Element[indices[i+1]], Element[indices[i+2]]);
        }
        return faces;
    }
}

[System.Serializable]
public class EdgeList : IEqualityComparer
{
    public int[] Edge = new int[2] { -1, -1 };

    public EdgeList(int a, int b)
    {
        Edge[0] = a;
        Edge[1] = b;
        Sort();
    }

    public int this[int i]
    {
        get
        {
            return Edge[i];
        }
        set
        {
            Edge[i] = value;
        }
    }

    public new bool Equals(object xx, object yy)
    {
        var x = (EdgeList)xx;
        var y = (EdgeList)yy;
        return x[0] == y[0] && x[1] == y[1];
    }

    public int GetHashCode(object ob)
    {
        var obj = (EdgeList)ob;
        return obj.Edge[0] ^ obj.Edge[1];
    }

    public void Sort()
    {
        if (Edge[0] > Edge[1])
        {
            Edge[0] ^= Edge[1];
            Edge[1] ^= Edge[0];
            Edge[0] ^= Edge[1];
        }
    }
}

[System.Serializable]
public class FaceList : IEqualityComparer
{
    public int[] Face = new int[3] { -1, -1, -1 };

    public FaceList(int a, int b, int c)
    {
        Face[0] = a;
        Face[1] = b;
        Face[2] = c;
    }

    public new bool Equals(object x, object y)
    {
        throw new NotImplementedException();
    }

    public int GetHashCode(object obj)
    {
        throw new NotImplementedException();
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
    /// 座標変換後の座標値
    /// </summary>
    [SerializeField] public Vector3[] MovedPositions;

    /// <summary>
    /// 最も大きい要素節点番号
    /// </summary>
    [SerializeField] public int MaxElementId;

    /// <summary>
    /// 要素節点番号, 何もないときは-1で埋めている
    /// </summary>
    [SerializeField] public ElementList[] Elements;

    /// <summary>
    /// 要素節点，エッジ
    /// </summary>
    [SerializeField] public EdgeList[] Edges;

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

    public void ConstructEdges()
    {
        List<EdgeList> edges = new List<EdgeList>(Positions.Length);

        if (Elements == null)
            return;

        foreach (var elem in Elements)
        {
            var elementEdge = elem.GetEdges();
            foreach (var e in elementEdge)
            {
                if (!edges.Contains(e))
                    edges.Add(e);
            }
        }

        Edges = edges.ToArray();
    }

    public void ConstructMovedPosition()
    {
        MovedPositions = new Vector3[Positions.Length];

        for (int i = 0; i < Positions.Length; ++i)
        {
            MovedPositions[i] = Rotation * (Positions[i] + Translate);
        }
    }
}