using Assimp;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dModelViewer.Graphics
{
    public class LoadedMesh : IDisposable
    {
        public Vector3 MinimumPosition;
        public Vector3 MaximumPosition;

        private int eboHandler;
        private int positionVboHandler;
        private int uvVboHandler;
        private int normalVboHandler;
        private int indicesCount;
        private int materialIndex;

        public LoadedMesh(Mesh source)
        {
            Vector3[] positions;
            Vector2[] uvCoords;
            Vector3[] normals;
            int[] indices;

            materialIndex = source.MaterialIndex;

            //positions
            MinimumPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            MaximumPosition = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            positions = new Vector3[source.VertexCount];
            for (int i = 0; i < source.VertexCount; ++i)
            {
                positions[i] = AssimpToOpenTKConverter.Vector3DToVector3(source.Vertices[i]);

                if (positions[i].X < MinimumPosition.X)
                    MinimumPosition.X = positions[i].X;
                if (positions[i].Y < MinimumPosition.Y)
                    MinimumPosition.Y = positions[i].Y;
                if (positions[i].Z < MinimumPosition.Z)
                    MinimumPosition.Z = positions[i].Z;

                if (positions[i].X > MaximumPosition.X)
                    MaximumPosition.X = positions[i].X;
                if (positions[i].Y > MaximumPosition.Y)
                    MaximumPosition.Y = positions[i].Y;
                if (positions[i].Z > MaximumPosition.Z)
                    MaximumPosition.Z = positions[i].Z;

            }
            HasPositions = true;

            //normals
            if (source.HasNormals)
            {
                normals = new Vector3[source.VertexCount];
                for (int i = 0; i < source.VertexCount; ++i)
                    normals[i] = AssimpToOpenTKConverter.Vector3DToVector3(source.Normals[i]);
                HasNormals = true;
            }
            else
            {
                normals = new Vector3[0];
                HasNormals = false;
            }

            //TO DO multiple textures max 8
            //uv
            if (source.HasTextureCoords(0))
            {
                uvCoords = new Vector2[source.VertexCount];
                for (int i = 0; i < source.VertexCount; ++i)
                {
                    Vector3 uvw = AssimpToOpenTKConverter.Vector3DToVector3(source.TextureCoordinateChannels[0][i]);
                    uvCoords[i] = new Vector2(uvw.X, 1-uvw.Y);
                }
                HasUvCoords = true;
            }
            else
            {
                uvCoords = new Vector2[0];
                HasUvCoords = false;
            }

            //collecting indices from all faces - we assume they are all triangles
            List<int> indexColection = new List<int>();
            for (int i = 0; i < source.FaceCount; ++i)
                if (source.Faces[i].IndexCount == 3)
                    indexColection.AddRange(source.Faces[i].Indices);
            indices = indexColection.ToArray();
            indicesCount = indices.Length;

            //create buffers
            if(HasPositions)
                CreateVbo(positions, Vector3.SizeInBytes * positions.Length, out positionVboHandler);
            if (HasNormals)
                CreateVbo(normals, Vector3.SizeInBytes * normals.Length, out normalVboHandler);
            if (HasUvCoords)
                CreateVbo(uvCoords, Vector2.SizeInBytes * uvCoords.Length, out uvVboHandler);
            CreateEbo(indices, out eboHandler);
        }

        public int EboHandler { get => eboHandler; }
        public int PositionVboHandler { get => positionVboHandler; }
        public int UvVboHandler { get => uvVboHandler; }
        public int NormalVboHandler { get => normalVboHandler; }
        public int IndicesCount { get => indicesCount; }
        public int MaterialIndex { get => materialIndex; }
        public bool HasPositions { get; private set; }
        public bool HasNormals { get; private set; }
        public bool HasUvCoords { get; private set; }

        private void CreateVbo(object data, int size, out int vboHandler)
        {
            GL.CreateBuffers(1, out vboHandler);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandler);
            if (data is Vector2[])
                GL.BufferData(BufferTarget.ArrayBuffer, size, (Vector2[])data, BufferUsageHint.StaticDraw);
            else if (data is Vector3[])
                GL.BufferData(BufferTarget.ArrayBuffer, size, (Vector3[])data, BufferUsageHint.StaticDraw);
            else if (data is Vector4[])
                GL.BufferData(BufferTarget.ArrayBuffer, size, (Vector4[])data, BufferUsageHint.StaticDraw);
            else
                throw new Exception("Type of data not accepted.");
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        private void CreateEbo(int[] indices, out int eboHandler)
        {
            GL.CreateBuffers(1, out eboHandler);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboHandler);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(int) * indices.Length, indices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(positionVboHandler);
            GL.DeleteBuffer(uvVboHandler);
            GL.DeleteBuffer(normalVboHandler);
            GL.DeleteBuffer(eboHandler);
        }
    }
}
