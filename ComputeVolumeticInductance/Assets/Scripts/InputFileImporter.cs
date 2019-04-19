using System.Collections;
using System.Collections.Generic;
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
    public string PartName { get; private set; }

    public Vector3[] Positions { get; set; }
    public int[][] Elements { get; set; }

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

    Vector3 MakePosition(string line)
    {
        var s = line.Split(',');
        Vector3 v;
        v.x = float.Parse(s[0]);
        v.y = float.Parse(s[1]);
        v.z = float.Parse(s[2]);
        return v;
    }

    Vector3[] GetNodes(string[] lines, ref int i)
    {
        List<Vector3> nodes = new List<Vector3>();
        ++i;    // 1行進める

        while (i < lines.Length)
        {
            // 要素定義が来るまではNodeのデータが挿入されている
            if (lines[i].Contains("*"))
                break;

            nodes.Add(MakePosition(lines[i]));

            ++i;
        }

        return nodes.ToArray();
    }

    int[] MakeElement(string line)
    {
        int[] element = new int[4];
        var splists = line.Split(',');

        // 0番の要素は要素番号なので, 1番以降を代入しないといけない
        for (int i = 0; i < 4; ++i)
            element[i] = int.Parse(splists[i + 1]);
        return element;
    }

    int[][] GetElements(string[] lines, ref int i)
    {
        List<int[]> elements = new List<int[]>();
        ++i;

        while (i < lines.Length)
        {
            // 要素定義が来たら抜ける
            if (lines[i].Contains("*"))
                break;

            elements.Add(MakeElement(lines[i]));

            ++i;
        }

        return elements.ToArray();
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
                Parts[currentPart].Positions = GetNodes(lines, ref i);
            }

            else if (line.Contains("*Element"))
            {
                Parts[currentPart].Elements = GetElements(lines, ref i);
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
