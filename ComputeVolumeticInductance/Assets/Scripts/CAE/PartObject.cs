using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartObject : MonoBehaviour
{
    public InputPart partAsset;

    public bool enableDrawLine = true;

    public bool enableDrawTetrahedron = false;

    private Material mat;

    private readonly int[] indices = new int[] { 0, 1, 2, 3, 0, 1 };

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

            GL.Vertex(partAsset.MovedPositions[edge[0]]);
            GL.Vertex(partAsset.MovedPositions[edge[1]]);
        }
        GL.End();

        GL.PopMatrix();
    }

    private void DrawTetrahedrons()
    {
        if (partAsset == null)
            return;

        if (partAsset.Elements == null)
            return;

        GL.PushMatrix();

        if (mat == null)
            mat = LineMaterialGenerator.GenerateMaterial();

        int[] indices = new int[3] { 0, 1, 2 };

        GL.MultMatrix(transform.localToWorldMatrix);

        GL.Begin(GL.TRIANGLE_STRIP);
        GL.Color(Color.gray);
        foreach (var element in partAsset.Elements)
        {
            GL.Vertex(partAsset.MovedPositions[element[0]]);
            GL.Vertex(partAsset.MovedPositions[element[1]]);
            GL.Vertex(partAsset.MovedPositions[element[2]]);
        }
        GL.End();
        GL.PopMatrix();
    }

    private void OnDrawGizmos()
    {
        if (enableDrawLine)
            DrawLines();

        if (enableDrawTetrahedron)
            DrawTetrahedrons();
    }
}
