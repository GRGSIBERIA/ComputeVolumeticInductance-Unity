using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class InputFileLoaderWindow : LoaderWindowBase
{
    public Object inpFile;

    [MenuItem ("CAE Tools/Open Input File")]
    public static void Open()
    {
        ShowWizerd<InputFileLoaderWindow>("Open .inp file wizerd");
    }
    
    private void OnWizardCreate()
    {
        var path = AssetDatabase.GetAssetPath(inpFile);
        if (Path.GetExtension(path) != ".inp")
        {
            EditorUtility.DisplayDialog("ERROR", "Please, connect .inp file to InpFile", "OK");
            return;
        }

        var inp = new InputFileImporter(path);

        var basepath = Path.GetFileNameWithoutExtension(path);
        var basedir = Path.GetDirectoryName(path);

        // ディレクトリが存在しなければ作っておく
        CheckDirectory(basedir, basepath);

        foreach (var kv in inp.Parts)
        {
            var newpath = string.Format("{0}/{1}/{2}.asset", basedir, basepath, kv.Key);
            AssetDatabase.CreateAsset(kv.Value, newpath);
            AssetDatabase.ImportAsset(newpath);
        }
        AssetDatabase.SaveAssets();
    }
}
