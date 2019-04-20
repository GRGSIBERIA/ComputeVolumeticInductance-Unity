using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class InputFileLoaderWindow : ScriptableWizard
{
    public Object inpFile;

    [MenuItem ("CAE Tools/Open Input File")]
    public static void Open()
    {
        var wiz = DisplayWizard<InputFileLoaderWindow>("Loading .inp file wizerd");
        var pos = wiz.position;
        pos.height = 128;
        wiz.position = pos;
    }
    
    private void OnWizardCreate()
    {
        var path = AssetDatabase.GetAssetPath(inpFile);
        var inp = new InputFileImporter(path);
    }
}
