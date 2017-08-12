using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace _3dModelViewer.Graphics
{
    public class Floor
    {
        const float floorSize = 20;

        private int floorVaoHandler;
        private Vector4 colorDiffuse = new Vector4(1f, 0f, 0f, 1f);
        private Vector4 colorAmbient = new Vector4(0.2f, 0.2f, 0.2f, 1f);
        private Vector4 colorSpecular = new Vector4(0.2f, 0.2f, 0.2f, 1f);
        float shininess = 1;
        private double baseHeight = -1;
        private double userSetHeight = 0;
        private int indicesCount;

        public void Draw(int shaderProgram)
        {
            if (floorVaoHandler < 1)
                InitGeometry();
            //material attrs
            GL.Uniform4(GL.GetUniformLocation(shaderProgram, "material.diffuse"), ref colorDiffuse);
            GL.Uniform4(GL.GetUniformLocation(shaderProgram, "material.ambient"), ref colorAmbient);
            GL.Uniform4(GL.GetUniformLocation(shaderProgram, "material.specular"), ref colorSpecular);
            GL.Uniform1(GL.GetUniformLocation(shaderProgram, "material.shininess"), shininess);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Uniform1(GL.GetUniformLocation(shaderProgram, "hasTextureNormal"), 0f);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            //draw
            Matrix4 transform = Matrix4.CreateTranslation(0f, (float)(baseHeight + userSetHeight), 0f);
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "modelMatrix"), false, ref transform);
            Matrix3 normalMatrix = (new Matrix3(transform)).Inverted();
            GL.UniformMatrix3(GL.GetUniformLocation(shaderProgram, "normalMatrix"), true, ref normalMatrix);
            GL.BindVertexArray(floorVaoHandler);
            GL.DrawElements(PrimitiveType.Triangles, indicesCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.BindVertexArray(0);
        }

        public void SetHeight(double height)
        {
            userSetHeight = height;
        }

        public void SetColor(Color color)
        {
            colorDiffuse = new Vector4(color.ScR, color.ScG, color.ScB, color.ScA);
        }

        private void InitGeometry()
        {
            float hSize = floorSize / 2;
            Vector3[] positions =
            {
                //front
                new Vector3(-hSize, 1, hSize),
                new Vector3(-hSize, -1, hSize),
                new Vector3(hSize, -1, hSize),
                new Vector3(hSize, 1, hSize),
                //back
                new Vector3(hSize, 1, -hSize),
                new Vector3(hSize, -1, -hSize),
                new Vector3(-hSize, -1, -hSize),
                new Vector3(-hSize, 1, -hSize),
                //right
                new Vector3(hSize, 1, hSize),
                new Vector3(hSize, -1, hSize),
                new Vector3(hSize, -1, -hSize),
                new Vector3(hSize, 1, -hSize),
                //left
                new Vector3(-hSize, 1, -hSize),
                new Vector3(-hSize, -1, -hSize),
                new Vector3(-hSize, -1, hSize),
                new Vector3(-hSize, 1, hSize),
                //top
                new Vector3(-hSize, 1, -hSize),
                new Vector3(-hSize, 1, hSize),
                new Vector3(hSize, 1, hSize),
                new Vector3(hSize, 1, -hSize),
                //bottom
                new Vector3(-hSize, -1, hSize),
                new Vector3(-hSize, -1, -hSize),
                new Vector3(hSize, -1, -hSize),
                new Vector3(hSize, -1, hSize)
            };
            Vector3[] normals =
            {
                //front
                new Vector3(0, 0, 1),
                new Vector3(0, 0, 1),
                new Vector3(0, 0, 1),
                new Vector3(0, 0, 1),
                //back
                new Vector3(0, 0, -1),
                new Vector3(0, 0, -1),
                new Vector3(0, 0, -1),
                new Vector3(0, 0, -1),
                //right
                new Vector3(1, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(1, 0, 0),
                //left
                new Vector3(-1, 0, 0),
                new Vector3(-1, 0, 0),
                new Vector3(-1, 0, 0),
                new Vector3(-1, 0, 0),
                //top
                new Vector3(0, 1, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 1, 0),
                //bottom
                new Vector3(0, -1, 0),
                new Vector3(0, -1, 0),
                new Vector3(0, -1, 0),
                new Vector3(0, -1, 0)
            };
            int[] indices =
            {
                //front
                0, 1, 2, 0, 2, 3,
                //back
                4, 5, 6, 4, 6, 7,
                //right
                8, 9, 10, 8, 10, 11,
                //left
                12, 13, 14, 12, 14, 15,
                //top
                16, 17, 18, 16, 18, 19,
                //bottom
                20, 21, 22, 20, 22, 23
            };
            indicesCount = indices.Length;

            int posVbo, normVbo, ebo;
            GL.CreateBuffers(1, out posVbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, posVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, Vector3.SizeInBytes * positions.Length, positions, BufferUsageHint.StaticDraw);
            GL.CreateBuffers(1, out normVbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, normVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, Vector3.SizeInBytes * normals.Length, normals, BufferUsageHint.StaticDraw);
            GL.CreateBuffers(1, out ebo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(int) * indices.Length, indices, BufferUsageHint.StaticDraw);

            //create floor vao
            GL.CreateVertexArrays(1, out floorVaoHandler);
            GL.BindVertexArray(floorVaoHandler);

            GL.BindBuffer(BufferTarget.ArrayBuffer, posVbo);
            GL.EnableVertexAttribArray((int)AttributeIndex.PositionAttrIndex);
            GL.VertexAttribPointer((int)AttributeIndex.PositionAttrIndex, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, normVbo);
            GL.EnableVertexAttribArray((int)AttributeIndex.NormalAttrIndex);
            GL.VertexAttribPointer((int)AttributeIndex.NormalAttrIndex, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);

            GL.DisableVertexAttribArray((int)AttributeIndex.UvAttrIndex);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
    }
}
