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

    public Color32[] ControlColours = new Color32[4];

    private Texture terrainTexture;

    private Texture2D terrainTexture2D;

    [SerializeField]private List<Pixel> LandType = new List<Pixel>();

    public bool Scanned;

    public bool Generated;

    public int PixelCount;

    [SerializeField] private List<Color32> Colour;

    void Start()
    {
        GetTexture();
    }
   
    void Update()
    {
        ScanTexture();
        GenerateLayerMap();
        GetColour();
    }

    void GetTexture()
    {
        Terrain = GameObject.FindGameObjectWithTag("Terrain");
        var terrainMaterial = Terrain.GetComponent<MeshRenderer>().material;
        terrainTexture = terrainMaterial.GetTexture("_MainTex");

        terrainTexture2D = (Texture2D) terrainTexture;

        print(terrainTexture.name);
    }

    void GetColour()
    {
        for (int x = 0; x < 512; x++)
        {
            for (int y = 0; y < 512; y++)
            {
                Color32 colour = terrainTexture2D.GetPixel(x, y);
                if (colour.b > 150)
                {
                    //water
                    ControlColours[0] = colour;
                }

                if (!colour.Equals(ControlColours[0]) && colour.g > 100 && colour.r < 100)
                {
                    //grass
                    ControlColours[1] = colour;
                }

                if (!colour.Equals(ControlColours[1]) && !colour.Equals(ControlColours[0]) && colour.g > 175 &&
                    colour.b < 65)
                {
                    //sand
                    ControlColours[2] = colour;
                }

                if (!colour.Equals(ControlColours[0]) && !colour.Equals(ControlColours[1]) &&
                    !colour.Equals(ControlColours[2]))
                {
                    //mountains
                    if (colour.r == colour.g)
                    {
                        ControlColours[3] = colour;
                    }
                }
            }
        }
    }

    void ScanTexture()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            for (int x = 0; x < 512; x++)
            {
                for (int y = 0; y < 512; y++)
                {
                    Color32 colour = terrainTexture2D.GetPixel(x, y);
                    Colour.Add(colour);
                    Scanned = true;
                    if (colour.Equals(ControlColours[0]))
                    {
                        PixelCount++;
                        var l = new Pixel()
                        {
                            Type = "sea",
                            Position = new Vector2(x, y)
                        };
                        LandType.Add(l);
                        Scanned = true;
                    }
                    if (colour.Equals(ControlColours[2]))
                    {
                        PixelCount++;
                        var s = new Pixel()
                        {
                            Type = "beach",
                            Position = new Vector2(x, y)
                        };
                        LandType.Add(s);
                        Scanned = true;
                    }

                    if (colour.Equals(ControlColours[1]))
                    {
                        PixelCount++;
                        var g = new Pixel()
                        {
                            Type = "grass",
                            Position = new Vector2(x, y)
                        };
                        LandType.Add(g);
                        Scanned = true;
                    }

                    if (colour.Equals(ControlColours[3]))
                    {

                        PixelCount++;

                        var m = new Pixel()
                        {
                            Type = "mountains",
                            Position = new Vector2(x, y)
                        };
                        LandType.Add(m);
                        Scanned = true;
                    }

                    PixelCount = Colour.Count;
                }
            }
        }
    }


    public float scale;

    Color CalculateColorSea(int x, int y)
    {
        float xCoord = (float) x / 512 * scale;
        float yCoord = (float) y / 512 * scale;
        float Sample = Mathf.PerlinNoise(xCoord, yCoord);
        return new Color(Sample * 0.3f, Sample * 0.3f, Sample * 0.3f);
    }

    Color CalculateColorBeach(int x, int y)
    {
        float xCoord = (float) x / 512 * 10 + 50;
        float yCoord = (float) y / 512 * 10 + 50;
        float Sample = Mathf.PerlinNoise(xCoord, yCoord);
        return new Color(Sample / 1.5f, Sample / 1.5f, Sample / 1.5f);
    }
    
    private int x;
    private int y;
    void GenerateLayerMap()
    {
        if (Scanned && Input.GetKeyDown(KeyCode.RightBracket))
        {
            var newTexture = new Texture2D(512, 512, TextureFormat.RGBA32, true);
            foreach (var pos in LandType)
            {
                Color color;
                x = Mathf.RoundToInt(pos.Position.x);
                y = Mathf.RoundToInt(pos.Position.y);
                if (pos.Type == "sea")
                {
                    // color = CalculateColorSea(x, y);
                    // newTexture.SetPixel(x, y, color);
                    newTexture.SetPixel(x,y,Color.black);
                }
                if (pos.Type == "beach")
                {
                    // color = CalculateColorBeach(x, y);
                    // newTexture.SetPixel(x, y, color);
                    newTexture.SetPixel(x,y,Color.yellow);
                }
                if (pos.Type == "grass")
                {
                    newTexture.SetPixel(x, y, Color.green);
                }
                if (pos.Type == "mountains")
                {
                    newTexture.SetPixel(x, y, Color.grey);
                }
                Generated = true;
            }

            byte[] bytes = newTexture.EncodeToJPG();
            File.WriteAllBytes(Application.dataPath + "/Image.png", bytes);
            
            print("Saved at" + Application.dataPath);
        }
    }


  
}
