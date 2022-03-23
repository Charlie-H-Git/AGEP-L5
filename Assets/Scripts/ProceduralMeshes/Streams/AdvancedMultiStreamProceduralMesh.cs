using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using float2 = Unity.Mathematics.float2;
using float4 = Unity.Mathematics.float4;

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class AdvancedMultiStreamProceduralMesh : MonoBehaviour
{
    private void OnEnable()
    {
        int vertexAttributeCount = 4;
        int vertexCount = 4;
        int triangleIndexCount = 6;
        
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData meshData = meshDataArray[0];
        
        
        var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
            vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory
        );
        
        //Vertex Attributes
        vertexAttributes[0] = new VertexAttributeDescriptor(dimension: 3);
        vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal,dimension: 3, stream: 1);
        vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent,dimension: 4, stream: 2);
        vertexAttributes[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0,dimension: 2, stream: 3);
        
        meshData.SetVertexBufferParams(vertexCount,vertexAttributes);
        vertexAttributes.Dispose();
        
        //Positions
        NativeArray<float3> positions = meshData.GetVertexData<float3>();
        positions[0] = 0f;
        positions[1] = right();
        positions[2] = up();
        positions[3] = float3(1f, 1f, 0f);
        
        //Normals
        NativeArray<float3> normals = meshData.GetVertexData<float3>(1);
        normals[0] = normals[1] = normals[2] = normals[3] = back();
        
        //Tangents
        NativeArray<float4> tangents = meshData.GetVertexData<float4>(2);
        tangents[0] = tangents[1] = tangents[2] = tangents[3] = float4(1f, 0f, 0f, -1f);

        //texCoords
        NativeArray<float2> texCoords = meshData.GetVertexData<float2>(3);
        texCoords[0] = 0f;
        texCoords[1] = float2(1f, 0f);
        texCoords[2] = float2(0f, 1f);
        texCoords[3] = 1f;

        //Triangle Index
        meshData.SetIndexBufferParams(triangleIndexCount, IndexFormat.UInt16);
        NativeArray<ushort> triangleIndices = meshData.GetIndexData<ushort>();
        triangleIndices[0] = 0;
        triangleIndices[1] = 2;
        triangleIndices[2] = 1;
        triangleIndices[3] = 1;
        triangleIndices[4] = 2;
        triangleIndices[5] = 3;
        
        //Mesh and Submesh Bounds
        var bounds = new Bounds(new Vector3(0.5f, 0.5f), new Vector3(1f, 1f));
        
        meshData.subMeshCount = 1;
        meshData.SetSubMesh(0,new SubMeshDescriptor(0,triangleIndexCount)
        {
            bounds = bounds,
            vertexCount = vertexCount
        }, MeshUpdateFlags.DontRecalculateBounds);
        
        //Mesh Name Assignment
        var mesh = new Mesh
        {
            bounds = bounds,
            name = "Procedural Mesh"
        };
        
        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray,mesh);
        
        GetComponent<MeshFilter>().mesh = mesh;
        
    }
    
    
}
