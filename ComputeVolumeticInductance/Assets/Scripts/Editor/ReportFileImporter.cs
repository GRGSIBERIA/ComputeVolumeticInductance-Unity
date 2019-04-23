using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class ReportFileImporter
{
    public string FilePath { get; private set; }

    public ReportFileImporter(string path)
    {
        FilePath = path;

        string text;
        using (var sr = new StreamReader(path))
        {
            text = sr.ReadToEnd();
        }
        var lines = text.Split('\n');

        ReadLines(lines);
    }

    private void ReadLines(string[] lines)
    {
        int num = 0;
        while (num < lines.Length)
        {
            Debug.Log(lines[num]);
        }
    }
}
