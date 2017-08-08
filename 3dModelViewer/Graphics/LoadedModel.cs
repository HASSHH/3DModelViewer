using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace _3dModelViewer.Graphics
{
    public class LoadedModel
    {
        public readonly List<LoadedMesh> Meshes = new List<LoadedMesh>();
        public readonly List<LoadedMaterial> Materials = new List<LoadedMaterial>();
        public Vector3 MinimumPosition;
        public Vector3 MaximumPosition;

        private LoadedNode rootNode;

        public LoadedModel(Assimp.Scene source, string dirName)
        {
            MinimumPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            MaximumPosition = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            UserTransform = new ModelTransform();

            for (int i = 0; i < source.MeshCount; ++i)
                Meshes.Add(new LoadedMesh(source.Meshes[i]));
            for (int i = 0; i < source.MaterialCount; ++i)
                Materials.Add(new LoadedMaterial(source.Materials[i], dirName));
            ProcessTree(source.RootNode, null, out rootNode);
        }

        public LoadedNode RootNode { get => rootNode; }
        public ModelTransform UserTransform { get; private set; }

        public void Draw(int shaderProgram)
        {
            DrawNode(rootNode, shaderProgram);
        }

        private void DrawNode(LoadedNode node, int shaderProgram)
        {
            LoadedNode tempNode = node;
            Matrix4 transform = Matrix4.Identity;
            do
            {
                transform = tempNode.Transform * transform;
                tempNode = tempNode.Parent;
            } while (tempNode != null);
            Matrix4 userTransform = UserTransform.ScaleMatrix * UserTransform.TranslateBeforeMatrix * UserTransform.RotationMatrix * UserTransform.TranslateAfterMatrix;
            userTransform.Transpose();
            transform = userTransform * transform;

            foreach (VaoDescription desc in node.Meshes)
            {
                LoadedMesh mesh = Meshes[desc.CorrespondingMeshIndex];
                if (mesh.MaterialIndex >= 0 && mesh.MaterialIndex < Materials.Count)
                    Materials[mesh.MaterialIndex].Apply(shaderProgram);
                GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "modelMatrix"), true, ref transform);
                Matrix3 normalMatrix = (new Matrix3(transform)).Inverted();
                GL.UniformMatrix3(GL.GetUniformLocation(shaderProgram, "normalMatrix"), false, ref normalMatrix);
                GL.BindVertexArray(desc.VaoHandler);
                GL.DrawElements(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, mesh.IndicesCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
                GL.BindVertexArray(0);
            }
            foreach (LoadedNode child in node.Children)
                DrawNode(child, shaderProgram);
        }

        private void ProcessTree(Node node, LoadedNode parent, out LoadedNode result)
        {
            result = new LoadedNode();
            result.Transform = AssimpToOpenTKConverter.Matrix4x4ToMatrix4(node.Transform);
            result.Parent = parent;
            if (node.HasMeshes)
            {
                foreach (int meshIndex in node.MeshIndices)
                {
                    int vaoHandler;
                    LoadedMesh mesh = Meshes[meshIndex];
                    CreateVao(mesh, out vaoHandler);
                    result.Meshes.Add(new VaoDescription
                    {
                        VaoHandler = vaoHandler,
                        CorrespondingMeshIndex = meshIndex
                    });
                }
                //complete transform matrix used for finding min max pos
                LoadedNode tempNode = result;
                Matrix4 transform = Matrix4.Identity;
                do
                {
                    transform = tempNode.Transform * transform;
                    tempNode = tempNode.Parent;
                } while (tempNode != null);
                foreach(var mvao in result.Meshes)
                {
                    LoadedMesh mesh = Meshes[mvao.CorrespondingMeshIndex];
                    Vector3 meshMin = new Vector3(new Vector4(mesh.MinimumPosition, 1f) * transform);
                    Vector3 meshMax = new Vector3(new Vector4(mesh.MaximumPosition, 1f) * transform);

                    if (meshMin.X < MinimumPosition.X)
                        MinimumPosition.X = meshMin.X;
                    if (meshMin.Y < MinimumPosition.Y)
                        MinimumPosition.Y = meshMin.Y;
                    if (meshMin.Z < MinimumPosition.Z)
                        MinimumPosition.Z = meshMin.Z;
                    if (meshMin.X > MaximumPosition.X)
                        MaximumPosition.X = meshMin.X;
                    if (meshMin.Y > MaximumPosition.Y)
                        MaximumPosition.Y = meshMin.Y;
                    if (meshMin.Z > MaximumPosition.Z)
                        MaximumPosition.Z = meshMin.Z;
                }
            }
            if (node.HasChildren)
                foreach (Node child in node.Children)
                {
                    LoadedNode kid;
                    ProcessTree(child, result, out kid);
                    result.Children.Add(kid);
                }
        }

        private void CreateVao(LoadedMesh mesh, out int vaoHandler)
        {
            GL.CreateVertexArrays(1, out vaoHandler);
            GL.BindVertexArray(vaoHandler);

            if (mesh.HasPositions)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, mesh.PositionVboHandler);
                GL.EnableVertexAttribArray((int)AttributeIndex.PositionAttrIndex);
                GL.VertexAttribPointer((int)AttributeIndex.PositionAttrIndex, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);
            }
            else
                GL.DisableVertexAttribArray((int)AttributeIndex.PositionAttrIndex);

            if (mesh.HasNormals)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, mesh.NormalVboHandler);
                GL.EnableVertexAttribArray((int)AttributeIndex.NormalAttrIndex);
                GL.VertexAttribPointer((int)AttributeIndex.NormalAttrIndex, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);
            }
            else
            {
                GL.DisableVertexAttribArray((int)AttributeIndex.NormalAttrIndex);
            }

            if (mesh.HasUvCoords)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, mesh.UvVboHandler);
                GL.EnableVertexAttribArray((int)AttributeIndex.UvAttrIndex);
                GL.VertexAttribPointer((int)AttributeIndex.UvAttrIndex, 2, VertexAttribPointerType.Float, true, Vector2.SizeInBytes, 0);
            }
            else
                GL.DisableVertexAttribArray((int)AttributeIndex.UvAttrIndex);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.EboHandler);
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public class ModelTransform
        {
            private Matrix4 translateAfterMatrix = Matrix4.Identity;
            private Matrix4 translateBeforeMatrix = Matrix4.Identity;
            private Matrix4 scaleMatrix = Matrix4.Identity;
            private Matrix4 onTheFlyRotation = Matrix4.Identity;

            public readonly Stack<Matrix4> RotationMatrixList = new Stack<Matrix4>();

            public Matrix4 RotationMatrix
            {
                get
                {
                    Matrix4 rotationMatrix = OnTheFlyRotation;
                    foreach (Matrix4 rm in RotationMatrixList)
                        rotationMatrix = rm * rotationMatrix;
                    return rotationMatrix;
                }
            }
            public Matrix4 TranslateAfterMatrix { get => translateAfterMatrix; set => translateAfterMatrix = value; }
            public Matrix4 TranslateBeforeMatrix { get => translateBeforeMatrix; set => translateBeforeMatrix = value; }
            public Matrix4 ScaleMatrix { get => scaleMatrix; set => scaleMatrix = value; }
            public Matrix4 OnTheFlyRotation { get => onTheFlyRotation; set => onTheFlyRotation = value; }
        }
    }
}
