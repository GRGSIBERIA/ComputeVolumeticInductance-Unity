﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public class ReportFileImporter
{
    public string FilePath { get; private set; }

    public ReportPart Report { get; private set; }

    public ReportFileImporter(string path, InputPart part)
    {
        FilePath = path;

        // 行の読み込み
        List<string> lines = new List<string>(131072);
        using (var sr = new StreamReader(path))
        {
            long last = sr.BaseStream.Seek(0, SeekOrigin.End);
            sr.BaseStream.Seek(0, SeekOrigin.Begin);

            long per = last / 100;
            long count = 0;

            while (!sr.EndOfStream)
            {
                lines.Add(sr.ReadLine().Trim());

                if (count > 80000)
                {
                    long position = sr.BaseStream.Seek(0, SeekOrigin.Current);
                    long rate = position / per;
                    string summary = $"{position:#,0}/{last:#,0} bytes";
                    EditorUtility.DisplayProgressBar("Reading Report File", summary, 0.01f * rate);
                    count = 0;
                }

                ++count;
            }
        }
        EditorUtility.ClearProgressBar();

        // 読み込んだ行を処理する
        ReadLines(lines, part);
    }

    private string ReadHeader(List<string> lines, int count)
    {
        // Xが含まれる行の文字位置を取得
        int xpos = lines[count].IndexOf("  X  ");
        string header = lines[count].Substring(xpos + 5).Trim();
        --count;

        while (lines[count].Length > 0)
        {
            header += lines[count].Trim();
            --count;
        }
        return header;
    }

    private float[] ReadTimes(List<string> lines)
    {
        List<float> times = new List<float>(5000);

        int linenum = 0;
        while (linenum < lines.Count)
        {
            if (lines[linenum].Length == 0)
            {
                ++linenum;
                continue;
            }

            // ヘッダが来たら2行飛ばしてからパーズする
            if (lines[linenum].Contains("  X  "))
            {
                linenum += 2;
                
                while (lines[linenum].Length != 0)
                {
                    string time = lines[linenum].Trim().Split(' ')[0];
                    times.Add(float.Parse(time));
                    ++linenum;
                }

                break;
            }
        }

        return times.ToArray();
    }

    private float ExtractDisplacement(string line)
    {
        // 行はトリミング済みなので，0_______1.0のような空白で分割する
        return float.Parse(Regex.Split(line, "\\s+")[1]);
    }

    private Vector3[,] ReadDisplacement(List<string> lines, int timeLength, int positionLength, int maximumNodeId)
    {
        // 変位を初期化
        Vector3[,] move = new Vector3[timeLength,positionLength];
        for (int i = 0; i < timeLength; ++i)
            for (int j = 0; j < positionLength; ++j)
                move[i, j] = Vector3.zero;

        int linenum = 0;
        while (linenum < lines.Count)
        {
            // 文字列長が空のときは無視する
            if (lines[linenum].Length == 0)
            {
                ++linenum;
                continue;
            }

            if (lines[linenum].Contains("  X  "))
            {
                // ヘッダから情報を抜き出す
                string header = ReadHeader(lines, linenum);
                int pos = header.LastIndexOf("N: ");
                int num = int.Parse(header.Substring(pos, 3)) - 1;  // インデックスは1を引く

                // 二次要素など要素数をオーバーする要因があれば排除
                if (maximumNodeId < num)
                {
                    linenum += timeLength + 1;
                    continue;
                }

                int axis = int.Parse(header.Substring(3, 1)) - 1;
                for (int timeCount = 0; timeCount < timeLength; ++timeCount, ++linenum)
                {
                    float val = ExtractDisplacement(lines[linenum]);
                    move[timeCount, num][axis] = val;
                }
            }
            ++linenum;
        }

        return move;
    }

    private void ReadLines(List<string> lines, InputPart part)
    {
        Report = new ReportPart();

        // 時間だけ先に読み込む
        float[] times = ReadTimes(lines);

        // 変位の読み込み
        Vector3[,] move = ReadDisplacement(lines, times.Length, part.Positions.Length, part.MaxNodeId);

        TimeData[] data = new TimeData[times.Length];

        for (int timeid = 0; timeid < times.Length; ++timeid)
        {
            data[timeid] = new TimeData(part.Positions.Length);
            data[timeid].Time = times[timeid];

            for (int did = 0; did < part.Positions.Length; ++did)
                data[timeid].Displacements[did] = move[timeid, did];
        }

        Report.Data = data;
    }
}
