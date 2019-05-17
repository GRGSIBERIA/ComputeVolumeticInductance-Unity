using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class LoaderWindowBase : ScriptableWizard
{
    protected static T ShowWizerd<T>(string caption, int height = 100)
        where T : ScriptableWizard
    {
        var wiz = DisplayWizard<T>(caption);
        var pos = wiz.position;
        pos.height = height;
        pos.x = 100;
        pos.y = 100;
        wiz.position = pos;
        wiz.Show();
        return wiz;
    }

    protected void CheckDirectory(string basedir, string basepath)
    {
        if (!Directory.Exists(basedir + "/" + basepath))
            AssetDatabase.CreateFolder(basedir, basepath);
    }
}
