using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

/**
 * 入力ファイルの書式
 * *Part, name={part name}
 * *Node
 * n, x, y, z
 * *Element
 * n, a, b, c, d
 * *Instance, name={part name}
 * tx, ty, tz
 * *Node
 * n, x, y, z
 */

public class Part
{
    /// <summary>
    /// パート名
    /// </summary>
    public string PartName { get; private set; }

    /// <summary>
    /// 最も大きいノード番号
    /// </summary>
    public int MaxNodeId { get; set; }

    /// <summary>
    /// 節点の座標値
    /// </summary>
    public Vector3[] Positions { get; set; }

    /// <summary>
    /// 最も大きい要素節点番号
    /// </summary>
    public int MaxElementId { get; set; }

    /// <summary>
    /// 要素節点番号, 何もないときは-1で埋めている
    /// </summary>
    public int[,] Elements { get; set; }

    /// <summary>
    /// 移動値
    /// </summary>
    public Vector3 Translate { get; private set; }

    public Part(string partName)
    {
        PartName = partName;
    }

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
}

public class InputFileImporter
{
    public string Path { get; private set; }
    public Dictionary<string, Part> Parts { get; private set; }
    
    string ContainsParameter(string line, string parameter)
    {
        var splits = line.Split(',');
        foreach (var s in splits)
        {
            if (s.Contains(parameter))
            {
                return s.Split('=')[1];
            }
        }
        throw new System.Exception("Do not have parameter(" + parameter + ")");
    }

    Vector3 MakePosition(string line, out int id)
    {
        var s = line.Split(',');
        Vector3 v;
        v.x = float.Parse(s[1]);
        v.y = float.Parse(s[2]);
        v.z = float.Parse(s[3]);
        id = int.Parse(s[0]);
        return v;
    }

    Vector3[] GetNodes(string[] lines, ref int linenum, out int maxNodeId)
    {
        List<Vector3> nodes = new List<Vector3>(16384);
        List<int> ids = new List<int>(16384);
        ++linenum;    // 1行進める

        int id;

        while (linenum < lines.Length)
        {
            // 要素定義が来るまではNodeのデータが挿入されている
            if (lines[linenum].Contains("*"))
                break;

            nodes.Add(MakePosition(lines[linenum], out id));
            ids.Add(id);

            ++linenum;
        }

        // 要素の最大値が同じであれば内容が詰まっているはずなのでそれを返す
        int size = ids.Max();
        maxNodeId = size;
        if (size == nodes.Count)
            return nodes.ToArray();

        // 戻り値を代入する
        Vector3[] retpos = new Vector3[size];
        for (int i = 0; i < nodes.Count; ++i)
            retpos[ids[i]] = nodes[i];
        return retpos;
    }

    int[] MakeElement(string line)
    {
        int[] element = new int[4];
        var splists = line.Split(',');

        for (int i = 0; i < 4; ++i)
            element[i] = int.Parse(splists[i]);
        return element;
    }

    int[,] GetElements(string[] lines, ref int linenum, out int maxSize)
    {
        Dictionary<int, int[]> edict = new Dictionary<int, int[]>();
        ++linenum;
        int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(int));
        var dest = new int[4];

        while (linenum < lines.Length)
        {
            // 要素定義が来たら抜ける
            if (lines[linenum].Contains("*"))
                break;

            // 一時的に辞書配列に入れていく
            var element = MakeElement(lines[linenum]);
            Buffer.BlockCopy(element, size, dest, 0, 4 * size);
            edict[element[0]] = dest;

            ++linenum;
        }

        // リストに辞書配列を追加する
        int[] keys = new int[edict.Count];
        edict.Keys.CopyTo(keys, 0);
        int max = edict.Keys.Max();
        int[,] elements = new int[max, 4];

        // 配列の初期化, -1のときはnull扱い
        for (int cnt = 0; cnt < max; ++cnt)
        {
            for (int loop = 0; loop < 4; ++loop)
                elements[cnt, loop] = -1;
        }

        // 各要素を代入する
        for (int cnt = 0; cnt < keys.Length; ++cnt)
        {
            var val = edict[keys[cnt]];
            for (int loop = 0; loop < 4; ++loop)
                elements[keys[cnt], loop] = val[loop];
        }

        maxSize = max;

        return elements;
    }

    void ReadLines(string[] lines)
    {
        int i = 0;
        string currentPart = "";

        while (i < lines.Length)
        {
            var line = lines[i];

            // パート定義の行が来る
            if (line.Contains("*Part"))
            {
                var partName = ContainsParameter(line, "name");
                Parts[partName] = new Part(partName);
                currentPart = partName;
            }

            // インスタンス定義の行が来る
            else if (line.Contains("*Instance"))
            {
                var partName = ContainsParameter(line, "part");
                if (!Parts.ContainsKey(partName))
                    throw new System.Exception("Do not defined a part: " + partName);

                currentPart = partName;

                // 要素定義じゃなければ座標値のデータが来る
                if (!lines[i + 1].Contains("*"))
                {
                    // ローカル座標値のデータが来る
                    Parts[currentPart].SetTranslate(lines[i + 1]);
                }
            }

            else if (line.Contains("*Node"))
            {
                // 要素の定義
                var part = Parts[currentPart];
                int maxNodeId;
                part.Positions = GetNodes(lines, ref i, out maxNodeId);
                part.MaxNodeId = maxNodeId;
            }

            else if (line.Contains("*Element"))
            {
                // 要素節点番号の定義
                var part = Parts[currentPart];
                int maxSize;
                part.Elements = GetElements(lines, ref i, out maxSize);
                part.MaxElementId = maxSize;
            }

            ++i;
        }
    }

    public InputFileImporter(string path)
    {
        Path = path;

        if (!File.Exists(path))
            throw new FileNotFoundException("Do not exists file: " + path);

        // 文字列をすべて行に変換する
        string text;
        using (StreamReader sr = new StreamReader(path))
        {
            text = sr.ReadToEnd();
        }
        string[] lines = text.Split('\n');

        ReadLines(lines);
    }
}
