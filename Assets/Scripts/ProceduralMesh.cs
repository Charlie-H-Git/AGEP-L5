using System;
using System.Collections;
using System.Collections.Generic;
using ProceduralMeshes;
using ProceduralMeshes.Generators;
using ProceduralMeshes.Streams;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMesh : MonoBehaviour
{
    
    private Mesh mesh;
    static MeshJobScheduleDelegate[] jobs = {
        MeshJob<SquareGrid, SingleStream>.ScheduleParallel,
        MeshJob<SharedSquareGrid, SingleStream>.ScheduleParallel
    };
    
    [SerializeField]
    MeshType meshType;
    public enum MeshType {
        SquareGrid, SharedSquareGrid
    }
    
    private void Awake()
    {
        mesh = new Mesh {name = "Procedural Mesh" };
        // GenerateMesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }
    [SerializeField, Range(1, 100)]
    int resolution = 1;
    
    void OnValidate () => enabled = true;

    void Update () {
        GenerateMesh();
        enabled = false;
    }
    
    void GenerateMesh()
    {
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData meshData = meshDataArray[0];
        //MeshJob<SquareGrid, MultiStream>.ScheduleParallel(mesh,meshData,resolution,default).Complete();
        jobs[(int)meshType](mesh, meshData, resolution, default).Complete();
        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray,mesh);
    }
}
