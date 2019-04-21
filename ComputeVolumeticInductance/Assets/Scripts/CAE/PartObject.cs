using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartObject : MonoBehaviour
{
    [SerializeField]
    private InputPart partAsset;

    private Material mat;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void DrawLines()
    {
        if (partAsset == null)
            return;

        if (partAsset.Elements == null || partAsset.Edges == null)
            return;

        GL.PushMatrix();

        if (mat == null)
            mat = LineMaterialGenerator.GenerateMaterial();
        mat.SetPass(0);

        GL.MultMatrix(transform.localToWorldMatrix);

        GL.Begin(GL.LINES);
        GL.Color(Color.yellow);
        foreach (var edge in partAsset.Edges)
        {
            if (edge.Edge == null)
                continue;

            GL.Vertex(partAsset.Positions[edge[0]]);
            GL.Vertex(partAsset.Positions[edge[1]]);
        }
        GL.End();

        GL.PopMatrix();
    }

    private void OnDrawGizmos()
    {
        DrawLines();
    }
}
