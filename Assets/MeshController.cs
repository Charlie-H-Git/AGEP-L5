using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(ProceduralMesh))]
public class MeshController : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ProceduralMesh _proceduralMesh = (ProceduralMesh)target;
        int r = _proceduralMesh.resolution;
        
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Shared Grid"))
        {
            //_proceduralMesh.meshType = ProceduralMesh.MeshType.SharedSquareGrid;
            //TODO Use get set to change the enum type from custom inspector and hide the values on the main script.
        }

        if (GUILayout.Button("Square Grid"))
        {
            //_proceduralMesh.meshType = ProceduralMesh.MeshType.SquareGrid;
        }
        GUILayout.EndHorizontal();
        
        GUILayout.HorizontalSlider(r, 0, 100);
        

        if (GUILayout.Button("Generate Mesh"))
        {
            _proceduralMesh.GenerateMesh();
        }

    }
}
