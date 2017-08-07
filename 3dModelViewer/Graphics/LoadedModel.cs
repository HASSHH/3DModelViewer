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

        private LoadedNode rootNode;

        public LoadedModel(Assimp.Scene source, string dirName)
        {
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

        public void ApplyTransform(Matrix4 transformMatrix)
        {
            rootNode.Transform = transformMatrix * rootNode.Transform;
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
            Matrix4 userTransform = UserTransform.ScaleMatrix * UserTransform.RotationMatrix * UserTransform.TranslateMatrix;
            userTransform.Transpose();
            transform = userTransform * transform;
            transform.Transpose();

            foreach (VaoDescription desc in node.Meshes)
            {
                LoadedMesh mesh = Meshes[desc.CorrespondingMeshIndex];
                if (mesh.MaterialIndex >= 0 && mesh.MaterialIndex < Materials.Count)
                    Materials[mesh.MaterialIndex].Apply(shaderProgram);
                GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "modelMatrix"), false, ref transform);
                Matrix3 normalMatrix = (new Matrix3(transform)).Inverted();
                normalMatrix.Transpose();
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
                foreach (int meshIndex in node.MeshIndices)
                {
                    int vaoHandler;
                    CreateVao(Meshes[meshIndex], out vaoHandler);
                    result.Meshes.Add(new VaoDescription
                    {
                        VaoHandler = vaoHandler,
                        CorrespondingMeshIndex = meshIndex
                    });
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
            private Matrix4 translateMatrix = Matrix4.Identity;
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
            public Matrix4 TranslateMatrix { get => translateMatrix; set => translateMatrix = value; }
            public Matrix4 ScaleMatrix { get => scaleMatrix; set => scaleMatrix = value; }
            public Matrix4 OnTheFlyRotation { get => onTheFlyRotation; set => onTheFlyRotation = value; }
        }
    }
}
