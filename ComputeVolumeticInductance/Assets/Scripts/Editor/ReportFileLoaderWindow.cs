using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class ReportFileLoaderWindow : ScriptableWizard
{
    public GameObject inputGameObject;
    public Object reportFile;

    [MenuItem ("CAE Tools/Open Report File")]
    public static void Open()
    {
        var wiz = DisplayWizard<ReportFileLoaderWindow>("Open .rpt file wizard");
        var pos = wiz.position;
        pos.height = 168;
        pos.x = 100;
        pos.y = 100;
        wiz.position = pos;
        wiz.Show();
    }

    private void OnWizardCreate()
    {
        var path = AssetDatabase.GetAssetPath(reportFile);
        if (Path.GetExtension(path) != ".rpt")
        {
            EditorUtility.DisplayDialog("ERROR", "Please, connect .rpt file to ReportFile.", "OK");
            return;
        }

        var part = inputGameObject.GetComponent<PartObject>();
        if (part == null)
        {
            EditorUtility.DisplayDialog("ERROR", "Please, attached PartObject to InputGameObject.", "OK");
            return;
        }

        if (part.partAsset == null)
        {
            EditorUtility.DisplayDialog("ERROR", "Please, attached PartAsset to InputGameObject.", "OK");
            return;
        }

        var report = new ReportFileImporter(path, part.partAsset);
    }
}
