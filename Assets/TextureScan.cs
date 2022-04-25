using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PixelFinder))]
public class TextureScan : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PixelFinder pixelFinder = (PixelFinder)target;
        
        if (GUILayout.Button("Set Texture"))
        {
            pixelFinder.SetTexture();
        }
        GUILayout.Space(15);
        if (GUILayout.Button("Get Colour"))
        {
            pixelFinder.GetColour();
        }
        GUILayout.Space(15);
        if (GUILayout.Button("Scan Texture"))
        {
            Debug.Log("Scanned Image");
            pixelFinder.Start = true;
            pixelFinder.ScanTexture();
        }
        GUILayout.Space(15);
        if (GUILayout.Button("Generate Layer Mask"))
        {
            Debug.Log("Generating Layers");
            pixelFinder.Start = true;
            pixelFinder.GenerateLayerMap();
        }
        GUILayout.Space(15);
        if (GUILayout.Button("Purge Lists"))
        {
            pixelFinder.PurgeLists();
        }
    }
    
}
