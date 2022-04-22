using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ProceduralMeshes;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Jobs;
using static Unity.Mathematics.math;
using UnityEngine.UIElements;

namespace ProceduralMeshes
{
    public struct Vertex
    {
        public float3 position, normal;
        public float4 tangent;
        public float2 texCoord0;
    }
    
    public delegate JobHandle MeshJobScheduleDelegate (Mesh mesh, Mesh.MeshData meshData, int resolution, JobHandle dependency);
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct MeshJob<G, S> : IJobFor
        where G : struct, IMeshGenerator
        where S : struct, IMeshStreams
    {
        G generator;

        [WriteOnly]
        S streams;

        public void Execute (int i) => generator.Execute(i, streams);
        
        public static JobHandle ScheduleParallel (Mesh mesh,Mesh.MeshData meshData,int resolution, JobHandle dependency) 
        {
            var job = new MeshJob<G, S>();
            job.generator.Resolution = resolution;
            job.streams.Setup(meshData, job.generator.Bounds,job.generator.VertexCount, job.generator.IndexCount);
            return job.ScheduleParallel(job.generator.JobLength, 1, dependency);
        }
    }
    
    
    
    public interface IMeshGenerator
    {
        int Resolution { get; set; }
        Bounds Bounds { get; }
        void Execute<S> (int i, S streams) where S : struct, IMeshStreams;
        int VertexCount { get; }
        int IndexCount { get; }
        int JobLength { get; }

    }
    public interface IMeshStreams
    {
        void Setup(Mesh.MeshData meshData,Bounds Bounds, int vertexCount, int indexCount);

        void SetVertex(int index, Vertex data);

        void SetTriangle(int index, int3 triangle);
    }
    
}

namespace ProceduralMeshes.Streams
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TriangleUInt16
    {
        public ushort a, b, c;

        public static implicit operator TriangleUInt16(int3 t) => new TriangleUInt16()
        {
            a = (ushort)t.x,
            b = (ushort)t.y,
            c = (ushort)t.z
        };
    }
    public struct SingleStream : IMeshStreams
    {
        [StructLayout(LayoutKind.Sequential)]
        struct Stream0
        {
            public float3 position, normal;
            public float4 tangent;
            public float2 texCoord0;
        }

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<Stream0> stream0;
        [NativeDisableContainerSafetyRestriction]
        private NativeArray<TriangleUInt16> triangles;
        
        public void Setup(Mesh.MeshData meshData,Bounds bounds, int vertexCount, int indexCount)
        {
            var descriptor =
                new NativeArray<VertexAttributeDescriptor>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory)
                {
                    [0] = new VertexAttributeDescriptor(dimension: 3),
                    [1] = new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3),
                    [2] = new VertexAttributeDescriptor(VertexAttribute.Tangent, dimension: 4),
                    [3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2)
                };
            meshData.SetVertexBufferParams(vertexCount , descriptor);
            descriptor.Dispose();
            
            meshData.SetIndexBufferParams(indexCount, IndexFormat.UInt16);

            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, indexCount){
                bounds = bounds,
                vertexCount = vertexCount
            }, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);

            stream0 = meshData.GetVertexData<Stream0>();
            triangles = meshData.GetIndexData<ushort>().Reinterpret<TriangleUInt16>(2);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertex(int index, Vertex vertex) => stream0[index] = new Stream0
        {
            position = vertex.position,
            normal = vertex.normal,
            tangent = vertex.tangent,
            texCoord0 = vertex.texCoord0
        };

        public void SetTriangle(int index, int3 triangle) => triangles[index] = triangle;
        
    }
    
    public struct MultiStream : IMeshStreams
    {
       
        [NativeDisableContainerSafetyRestriction]
        private NativeArray<TriangleUInt16> triangles;
        
        [NativeDisableContainerSafetyRestriction]
        NativeArray<float3> stream0, stream1;

        [NativeDisableContainerSafetyRestriction]
        NativeArray<float4> stream2;

        [NativeDisableContainerSafetyRestriction]
        NativeArray<float2> stream3;
        
        public void Setup(Mesh.MeshData meshData, Bounds bounds,int vertexCount, int indexCount)
        {
            var descriptor =
                new NativeArray<VertexAttributeDescriptor>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory)
                {
                    [0] = new VertexAttributeDescriptor(dimension: 3),
                    [1] = new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3, stream: 1),
                    [2] = new VertexAttributeDescriptor(VertexAttribute.Tangent, dimension: 4, stream: 2),
                    [3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2, stream: 3)
                };
            meshData.SetVertexBufferParams(vertexCount , descriptor);
            descriptor.Dispose();
            
            meshData.SetIndexBufferParams(indexCount, IndexFormat.UInt16);

            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, indexCount){
                bounds = bounds,
                vertexCount = vertexCount
            } ,MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);

            stream0 = meshData.GetVertexData<float3>();
            stream1 = meshData.GetVertexData<float3>(1);
            stream2 = meshData.GetVertexData<float4>(2);
            stream3 = meshData.GetVertexData<float2>(3);
            triangles = meshData.GetIndexData<ushort>().Reinterpret<TriangleUInt16>(2);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertex(int index, Vertex vertex) 
        {
            stream0[index] = vertex.position;
            stream1[index] = vertex.normal;
            stream2[index] = vertex.tangent;
            stream3[index] = vertex.texCoord0;
        }

        public void SetTriangle(int index, int3 triangle) => triangles[index] = triangle;
        
    }
}

namespace ProceduralMeshes.Generators
{
    public struct SharedSquareGrid : IMeshGenerator
    {
        public Bounds Bounds => new Bounds(new Vector3(0.5f, 0,0.5f), new Vector3(1f, 0f, 1f));
        public int Resolution { get; set; }
        public int VertexCount => (Resolution + 1) * (Resolution + 1);
        public int IndexCount => 6 * Resolution * Resolution; 
        public int JobLength => Resolution + 1;

        public void Execute<S>(int z, S streams) where S : struct, IMeshStreams
        {
            int vi = (Resolution + 1) * z, ti = 2 * Resolution * (z - 1);     
            
            var vertex = new Vertex();
            vertex.normal.y = 1f;
            vertex.tangent.xw = float2(1f, -1f);
            
            vertex.position.x = -0.5f;
            vertex.position.z = (float)z / Resolution;
            vertex.texCoord0.y = (float)z / Resolution;
            streams.SetVertex(vi, vertex);
            
            vi += 1;

            for (int x = 1; x <= Resolution; x++, vi++, ti += 2) {
                vertex.position.x = (float)x / Resolution - 0.5f;
                vertex.texCoord0.x = (float)x / Resolution;
                streams.SetVertex(vi, vertex);

                if (z > 0)
                {
                    streams.SetTriangle(ti + 0, vi + int3(-Resolution - 2, -1, -Resolution - 1));
                    streams.SetTriangle(ti + 1, vi + int3(-Resolution - 1, -1, 0));
                }

            }
        }
    }
    public struct SquareGrid : IMeshGenerator
    {
        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(1f, 0f, 1f));
        public int Resolution { get; set; }
        public int VertexCount => 4 * Resolution * Resolution;
        public int IndexCount => 6 * Resolution * Resolution; 
        public int JobLength => Resolution;

        public void Execute<S>(int z, S streams) where S : struct, IMeshStreams
        {
            int vi = 4 * Resolution * z, ti = 2 * Resolution * z;     
            //Determines the position offset of the bottom left corner of the Quad
            //int z = i / resolution;
            //int x = i - resolution * z;
            
            for (int x = 0; x < Resolution; x++, vi += 4, ti += 2)
            {
                //var coordinates = float4(x, x + 1f, z, z + 1f) / resolution - 0.5f;

                //Defines the Vertex  Position of each quad
                var vertex = new Vertex();
                vertex.normal.y = 1f;
                vertex.tangent.xw = float2(1f, -1f);

                //var coordinates = float4(x, x + 1f, z, z + 1f) / Resolution - 0.5f;
                var xCoordinates = float2(x, x + 1f) / Resolution - 0.5f;
                var zCoordinates = float2(z, z + 1f) / Resolution - 0.5f;
			
                //vertex.position.xz = coordinates.xz;
                vertex.position.x = xCoordinates.x;
                vertex.position.z = zCoordinates.x;
                streams.SetVertex(vi + 0, vertex);

                //vertex.position.xz = coordinates.yz;
                vertex.position.x = xCoordinates.y;
                vertex.texCoord0 = float2(1f, 0f);
                streams.SetVertex(vi + 1, vertex);

                //vertex.position.xz = coordinates.xw;
                vertex.position.x = xCoordinates.x;
                vertex.position.z = zCoordinates.y;
                vertex.texCoord0 = float2(0f, 1f);
                streams.SetVertex(vi + 2, vertex);

                //vertex.position.xz = coordinates.yw;
                vertex.position.x = xCoordinates.y;
                vertex.texCoord0 = 1f;
                streams.SetVertex(vi + 3, vertex);

                streams.SetTriangle(ti + 0, vi + int3(0, 2, 1));
                streams.SetTriangle(ti + 1, vi + int3(1, 2, 3));
            }
        }
    }
    
}

