using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineMaterialGenerator
{
    static public Material GenerateMaterial()
    {
        Material mat = new Material(Shader.Find("Hidden/Internal-Colored"));
        mat.hideFlags = HideFlags.HideAndDontSave;
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        mat.SetInt("_ZWrite", 0);
        return mat;
    }
}
