using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;

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

public class InputFileImporter
{
    public string Path { get; private set; }
    public Dictionary<string, InputPart> Parts { get; private set; }
    
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
        id = int.Parse(s[0]) - 1;
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
            {
                break;
            }

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
        Vector3[] retpos = new Vector3[size + 1];
        for (int i = 0; i < nodes.Count; ++i)
            retpos[ids[i]] = nodes[i];
        return retpos;
    }

    int[] MakeElement(string line)
    {
        int[] element = new int[5];
        var splists = line.Split(',');

        for (int i = 0; i < 5; ++i)
            element[i] = int.Parse(splists[i]) - 1;
        return element;
    }

    ElementList[] GetElements(string[] lines, ref int linenum, out int maxSize)
    {
        Dictionary<int, int[]> edict = new Dictionary<int, int[]>();
        int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(int));

        ++linenum;

        while (linenum < lines.Length)
        {
            // 要素定義が来たら抜ける
            if (lines[linenum].Contains("*"))
            {
                break;
            }

            // 一時的に辞書配列に入れていく
            var element = MakeElement(lines[linenum]);

            var dest = new int[4];
            Buffer.BlockCopy(element, size, dest, 0, size * 4);
            edict[element[0]] = dest;

            ++linenum;
        }

        // リストに辞書配列を追加する
        int[] keys = new int[edict.Count];
        edict.Keys.CopyTo(keys, 0);
        int max = edict.Keys.Max();
        int elsize = max + 1;

        // 配列の初期化, -1のときはnull扱い
        ElementList[] elements = new ElementList[elsize];

        // 各要素を代入する
        for (int cnt = 0; cnt < keys.Length; ++cnt)
        {
            var val = edict[keys[cnt]];
            elements[cnt] = new ElementList();
            try
            {
                elements[keys[cnt]].SetElement(val);
            }
            catch
            {
                Debug.Log("What happen?");
            }
        }

        maxSize = max;

        return elements;
    }

    void ReadNode(string[] lines, string currentPart, ref int i)
    {
        // 要素の定義
        var line = lines[i];
        if (line.Contains("*Node"))
        {
            var part = Parts[currentPart];
            int maxNodeId;
            part.Positions = GetNodes(lines, ref i, out maxNodeId);
            part.MaxNodeId = maxNodeId;
        }
    }

    void ReadElement(string[] lines, string currentPart, ref int i)
    {
        // 要素節点番号の定義
        var line = lines[i];
        if (line.Contains("*Element"))
        {
            var part = Parts[currentPart];
            int maxSize;
            part.Elements = GetElements(lines, ref i, out maxSize);
            part.MaxElementId = maxSize;
        }
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
                var partName = ContainsParameter(line, "name").Trim();
                var part = InputPart.CreateInstance<InputPart>();
                part.PartName = partName;
                Parts[partName] = part;
                currentPart = partName;
                ++i;

                EditorUtility.DisplayProgressBar("Open Input File", "Read Node", 0.2f);
                ReadNode(lines, currentPart, ref i);
                EditorUtility.DisplayProgressBar("Open Input File", "Read Element", 0.4f);
                ReadElement(lines, currentPart, ref i);
            }

            // インスタンス定義の行が来る
            else if (line.Contains("*Instance"))
            {
                var partName = ContainsParameter(line, "part").Trim();
                if (!Parts.ContainsKey(partName))
                    throw new System.Exception("Do not defined a part: " + partName);

                currentPart = partName;

                // 要素定義じゃなければ座標値のデータが来る
                ++i;
                if (!lines[i].Contains("*"))
                {
                    Parts[currentPart].SetTranslate(lines[i]);
                    ++i;
                }
                // その次は回転の回転値データが来る
                if (!lines[i].Contains("*"))
                {
                    Parts[currentPart].SetRotation(lines[i]);
                    ++i;
                }

                EditorUtility.DisplayProgressBar("Open Input File", "Read Node", 0.2f);
                ReadNode(lines, currentPart, ref i);
                EditorUtility.DisplayProgressBar("Open Input File", "Read Element", 0.4f);
                ReadElement(lines, currentPart, ref i);
            }
            ++i;
        }
    }

    public InputFileImporter(string path)
    {
        Path = path;
        Parts = new Dictionary<string, InputPart>();

        if (!File.Exists(path))
            throw new FileNotFoundException("Do not exists file: " + path);

        EditorUtility.DisplayProgressBar("Open Input File", "Read Lines", 0.0f);

        // 文字列をすべて行に変換する
        string text;
        using (StreamReader sr = new StreamReader(path))
        {
            text = sr.ReadToEnd();
        }
        string[] lines = text.Split('\n');

        ReadLines(lines);

        // エッジを生成する
        EditorUtility.DisplayProgressBar("Open Input File", "Constructing Edges", 0.6f);
        foreach (var part in Parts)
            part.Value.ConstructEdges();

        EditorUtility.DisplayProgressBar("Open Input File", "Constructing Moved Position", 0.8f);
        foreach (var part in Parts)
            part.Value.ConstructMovedPosition();

        EditorUtility.ClearProgressBar();
    }
}
