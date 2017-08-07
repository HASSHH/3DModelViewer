using Assimp;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace _3dModelViewer.Graphics
{
    public class LoadedMaterial
    {
        private int textureDiffuseHandler;
        private int textureNormalHandler;
        private int textureSpecularHandler;
        private Vector4 colorDiffuse;
        private Vector4 colorAmbient;
        private Vector4 colorSpecular;
        private float shininess;

        public LoadedMaterial(Material source, string dirName)
        {
            shininess = source.Shininess * source.ShininessStrength;
            colorAmbient = source.HasColorAmbient ? AssimpToOpenTKConverter.Color4DToVector4(source.ColorAmbient) : new Vector4(0.2f, 0.2f, 0.2f, 1f);
            //diffuse
            if (source.HasTextureDiffuse)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                LoadTexture(source.TextureDiffuse, dirName, out textureDiffuseHandler);
                colorDiffuse = new Vector4(0f, 0f, 0f, 0f);
                HasTextureDiffuse = true;
            }
            else
            {
                colorDiffuse = source.HasColorDiffuse ? AssimpToOpenTKConverter.Color4DToVector4(source.ColorDiffuse) : new Vector4(0.8f, 0.8f, 0.8f, 1f);
                HasTextureDiffuse = false;
            }
            //normal/height
            if (source.HasTextureNormal)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                LoadTexture(source.TextureNormal, dirName, out textureNormalHandler);
                HasTextureNormal = true;
            }
            else if (source.HasTextureHeight)
            {
                //in texture normal...
                GL.ActiveTexture(TextureUnit.Texture1);
                LoadTexture(source.TextureHeight, dirName, out textureNormalHandler);
                HasTextureNormal = true;
            }
            else
                HasTextureNormal = false;
            //specular
            if (source.HasTextureSpecular)
            {
                GL.ActiveTexture(TextureUnit.Texture2);
                LoadTexture(source.TextureSpecular, dirName, out textureSpecularHandler);
                colorSpecular = new Vector4(0f, 0f, 0f, 0f);
                HasTextureSpecular = true;
            }
            else
            {
                colorSpecular = source.HasColorSpecular ? AssimpToOpenTKConverter.Color4DToVector4(source.ColorSpecular) : new Vector4(0f, 0f, 0f, 1f);
                HasTextureSpecular = false;
            }
        }

        public int TextureDiffuseHandler { get => textureDiffuseHandler; }
        public int TextureNormalHandler { get => textureNormalHandler; }
        public int TextureSpecularHandler { get => textureSpecularHandler; }
        public Vector4 ColorDiffuse { get => colorDiffuse; }
        public Vector4 ColorAmbient { get => colorAmbient; }
        public Vector4 ColorSpecular { get => colorSpecular; }
        public bool HasTextureDiffuse { get; private set; }
        public bool HasTextureNormal { get; private set; }
        public bool HasTextureSpecular { get; private set; }
        public float Shininess { get => shininess; }

        public void Apply(int shaderProgram)
        {
            GL.Uniform4(GL.GetUniformLocation(shaderProgram, "material.diffuse"), ref colorDiffuse);
            GL.Uniform4(GL.GetUniformLocation(shaderProgram, "material.ambient"), ref colorAmbient);
            GL.Uniform4(GL.GetUniformLocation(shaderProgram, "material.specular"), ref colorSpecular);
            GL.Uniform1(GL.GetUniformLocation(shaderProgram, "material.shininess"), shininess);

            GL.ActiveTexture(TextureUnit.Texture0);
            if (HasTextureDiffuse)
                GL.BindTexture(TextureTarget.Texture2D, textureDiffuseHandler);
            else
                GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.ActiveTexture(TextureUnit.Texture1);
            if (HasTextureNormal)
                GL.BindTexture(TextureTarget.Texture2D, textureNormalHandler);
            else
                GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Uniform1(GL.GetUniformLocation(shaderProgram, "hasTextureNormal"), HasTextureNormal ? 1f : 0f);

            GL.ActiveTexture(TextureUnit.Texture2);
            if (HasTextureSpecular)
                GL.BindTexture(TextureTarget.Texture2D, textureSpecularHandler);
            else
                GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        private void LoadTexture(TextureSlot texture, string dirName, out int textureId)
        {
            string fileName = Path.Combine(dirName, texture.FilePath);
            if (!File.Exists(fileName))
            {
                textureId = 0;
                return;
            }
            try
            {
                Bitmap textureBitmap = new Bitmap(fileName);
                BitmapData TextureData =
                        textureBitmap.LockBits(
                        new System.Drawing.Rectangle(0, 0, textureBitmap.Width, textureBitmap.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format24bppRgb
                    );
                GL.GenTextures(1, out textureId);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, textureId);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, textureBitmap.Width, textureBitmap.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, TextureData.Scan0);
                textureBitmap.UnlockBits(TextureData);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
            catch
            {
                textureId = 0;
            }
        }
    }
}
