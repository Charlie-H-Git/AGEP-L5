using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor;

[Serializable]
public struct Pixel
{
    public string Type;
    public Vector2 Position;
}
public class PixelFinder : MonoBehaviour
{
    private GameObject Terrain;

    public Color32 ControlColourWater;

    public Color32 ControlColourSand;

    public Color32 ControlColourGrass;

    public Color32 ControlColourMountains;
    
    private Texture terrainTexture;

    private Texture2D terrainTexture2D;

    public List<Pixel> LandType = new List<Pixel>();
    
    public bool Scanned;

    public bool Generated;

    public int PixelCount;
    public List<Color32> Colour;
    
    void Start()
    {
        GetTexture();
        
    }

    void GetTexture()
    {
        Terrain = GameObject.FindGameObjectWithTag("Terrain"); 
        var terrainMaterial = Terrain.GetComponent<MeshRenderer>().material;
        terrainTexture = terrainMaterial.GetTexture("_MainTex");

        terrainTexture2D = (Texture2D) terrainTexture;
    }
    void ScanTexture()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            for (int x = 0; x < 512; x++)
            {
                for (int y = 0; y < 512; y++)
                {
                    Color32 colour = terrainTexture2D.GetPixel(x,y);
                    Colour.Add(colour);
                    Scanned = true;
                    if (colour.Equals(ControlColourWater))
                    {
                        
                        var l = new Pixel()
                        {
                            Type = "sea",
                            Position = new Vector2(x,y)
                        };
                        LandType.Add(l);
                        Scanned = true;
                    }
                    if (colour.Equals(ControlColourSand))
                    {
                        
                        var s = new Pixel()
                        {
                            Type = "beach",
                            Position = new Vector2(x,y)
                        };
                        LandType.Add(s);
                        Scanned = true;
                    }
                    if (colour.Equals(ControlColourGrass))
                    {
                        
                        var g = new Pixel()
                        {
                            Type = "grass",
                            Position = new Vector2(x,y)
                        };
                        LandType.Add(g);
                        Scanned = true;
                    }
                    if (colour.Equals(ControlColourMountains))
                    {
                        
                        var m = new Pixel()
                        {
                            Type = "mountains",
                            Position = new Vector2(x,y)
                        };
                        LandType.Add(m);
                        Scanned = true;
                    }
                }
            }
        } 
    }
    
    void Update()
    {
        ScanTexture();

        if (Scanned && Input.GetKeyDown(KeyCode.Insert))
        {
            var newTexture = new Texture2D(512, 512,TextureFormat.RGBA32,true);
            foreach (var pos in LandType)
            {
                var x = Mathf.RoundToInt(pos.Position.x);
                var y = Mathf.RoundToInt(pos.Position.y);
                if (pos.Type == "sea")
                {
                    newTexture.SetPixel(x, y, Color.black); 
                }

                if (pos.Type == "beach")
                {
                    newTexture.SetPixel(x,y,Color.yellow);
                }

                if (pos.Type == "grass")
                {
                    newTexture.SetPixel(x,y,Color.green);
                }

                if (pos.Type == "mountains")
                {
                    newTexture.SetPixel(x,y,Color.grey);
                }
                
                Generated = true;
            }
            
            byte[] bytes = newTexture.EncodeToJPG();
            File.WriteAllBytes(Application.dataPath + "/Image.png", bytes);
            print("Saved at" + Application.dataPath);
        
        }
    }
}
