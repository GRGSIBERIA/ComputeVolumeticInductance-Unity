using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class ReportFileLoaderWindow : LoaderWindowBase
{
    public GameObject inputGameObjectWithPart;
    public Object reportFile;

    [MenuItem ("CAE Tools/Open Report File")]
    public static void Open()
    {
        ShowWizerd<ReportFileLoaderWindow>("Open .rpt file wizard", 160);
    }

    private ReportFileImporter ImportObject()
    {
        var path = AssetDatabase.GetAssetPath(reportFile);
        if (Path.GetExtension(path) != ".rpt")
        {
            EditorUtility.DisplayDialog("ERROR", "Please, connect .rpt file to ReportFile.", "OK");
            return null;
        }

        var part = inputGameObjectWithPart.GetComponent<PartObject>();
        if (part == null)
        {
            EditorUtility.DisplayDialog("ERROR", "Please, attached PartObject to InputGameObject.", "OK");
            return null;
        }

        if (part.partAsset == null)
        {
            EditorUtility.DisplayDialog("ERROR", "Please, attached PartAsset to InputGameObject.", "OK");
            return null;
        }

        return new ReportFileImporter(path, part.partAsset);
    }

    private void OnWizardCreate()
    {
        var report = ImportObject();
        if (report == null)
            return;

        var path = AssetDatabase.GetAssetPath(reportFile);
        var basepath = Path.GetFileNameWithoutExtension(path);
        var basedir = Path.GetDirectoryName(path);
        var filename = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(reportFile));

        CheckDirectory(basedir, basepath);

        var newpath = string.Format("{0}/{1}/{2}.asset", basedir, basepath, filename);
        AssetDatabase.CreateAsset(report.Report, newpath);
        AssetDatabase.ImportAsset(newpath);
        AssetDatabase.SaveAssets();
    }
}
