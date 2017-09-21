using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace _3dModelViewer.Graphics
{
    public class Scene
    {
        const int depthTextureSize = 4096;

        public readonly List<LoadedModel> LoadedModels = new List<LoadedModel>();
        public readonly Floor Floor = new Floor();

        private int shaderProgramHandle;
        private Light light;
        private Camera camera;
        private int depthProgramHandle;
        private int depthFBO;
        private int depthTexture;
        private int viewPortWidth;
        private int viewPortHeight;
        private Matrix4 depthVP;

        public bool DrawFloorEnabled { get; set; }
        public Light Light
        {
            get => light;
            set
            {
                if (value != null)
                {
                    if (light != null)
                        light.Active = false;
                    light = value;
                    //apply light
                    if (shaderProgramHandle > 0)
                    {
                        Vector3 lightPos = Light.Position;
                        Vector4 lightColor = Light.Color;
                        GL.Uniform3(GL.GetUniformLocation(shaderProgramHandle, "light.position"), ref lightPos);
                        GL.Uniform4(GL.GetUniformLocation(shaderProgramHandle, "light.color"), ref lightColor);
                        GL.Uniform1(GL.GetUniformLocation(shaderProgramHandle, "light.attenuation"), Light.Attenuation);
                        light.PropertyChanged += (s, e) =>
                        {
                            switch (e.PropertyName)
                            {
                                case "Position":
                                    Vector3 lp = light.Position;
                                    GL.Uniform3(GL.GetUniformLocation(shaderProgramHandle, "light.position"), ref lp);
                                    break;
                                case "Color":
                                    Vector4 lc = light.Color;
                                    GL.Uniform4(GL.GetUniformLocation(shaderProgramHandle, "light.color"), ref lc);
                                    break;
                                case "Attenuation":
                                    GL.Uniform1(GL.GetUniformLocation(shaderProgramHandle, "light.attenuation"), light.Attenuation);
                                    break;
                                default: break;
                            }
                        };
                    }
                }
            }
        }
        public Camera Camera
        {
            get => camera;
            set
            {
                if (value != null)
                {
                    if (camera != null)
                    {
                        value.AspectRatio = camera.AspectRatio;
                        camera.Active = false;
                    }
                    camera = value;
                    if (shaderProgramHandle > 0)
                    {
                        Matrix4 vpMatrix = Camera.ViewProjectionMatrix;
                        Vector3 cameraPosition = Camera.Position;
                        GL.Uniform3(GL.GetUniformLocation(shaderProgramHandle, "cameraPos"), ref cameraPosition);
                        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgramHandle, "vpMatrix"), false, ref vpMatrix);
                        camera.PropertyChanged += (s, e) =>
                        {
                            Matrix4 vp = Camera.ViewProjectionMatrix;
                            Vector3 cp = Camera.Position;
                            GL.Uniform3(GL.GetUniformLocation(shaderProgramHandle, "cameraPos"), ref cp);
                            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgramHandle, "vpMatrix"), false, ref vp);
                        };
                    }
                }
            }
        }

        public void Draw()
        {
            //render shadow map
            if (depthProgramHandle > 0)
                RenderShadowMap();


            if(shaderProgramHandle > 0)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.Viewport(0, 0, viewPortWidth, viewPortHeight);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.UseProgram(shaderProgramHandle);

                GL.UniformMatrix4(GL.GetUniformLocation(shaderProgramHandle, "depthVP"), false, ref depthVP);
                GL.ActiveTexture(TextureUnit.Texture3);
                GL.BindTexture(TextureTarget.Texture2D, depthTexture);
                //render objects
                RenderScene(shaderProgramHandle);
            }
        }

        public void Init(double width, double height)
        {
            ///////////////////
            /// DEPTH SHADER///
            ///////////////////
            string vertexShaderSource = ReadShaderString("DepthShader.vert");
            string fragmentShaderSource = ReadShaderString("DepthShader.frag");
            //create shaders
            int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShaderHandle, vertexShaderSource);
            GL.CompileShader(vertexShaderHandle);
            int fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShaderHandle, fragmentShaderSource);
            GL.CompileShader(fragmentShaderHandle);

            //create shader program
            depthProgramHandle = GL.CreateProgram();
            //attach shaders
            GL.AttachShader(depthProgramHandle, vertexShaderHandle);
            GL.AttachShader(depthProgramHandle, fragmentShaderHandle);
            //bind attr locations
            GL.BindAttribLocation(depthProgramHandle, (int)AttributeIndex.PositionAttrIndex, "inPosition");
            //link
            GL.LinkProgram(depthProgramHandle);

            ///////////////////
            /// MAIN SHADER ///
            ///////////////////
            vertexShaderSource = ReadShaderString("MainShader.vert");
            fragmentShaderSource = ReadShaderString("MainShader.frag");
            //create shaders
            vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShaderHandle, vertexShaderSource);
            GL.CompileShader(vertexShaderHandle);
            fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShaderHandle, fragmentShaderSource);
            GL.CompileShader(fragmentShaderHandle);

            //debugging
            string vinfo = GL.GetShaderInfoLog(vertexShaderHandle);
            string finfo = GL.GetShaderInfoLog(fragmentShaderHandle);

            //create shader program
            shaderProgramHandle = GL.CreateProgram();
            //attach shaders
            GL.AttachShader(shaderProgramHandle, vertexShaderHandle);
            GL.AttachShader(shaderProgramHandle, fragmentShaderHandle);
            //bind attr locations
            GL.BindAttribLocation(shaderProgramHandle, (int)AttributeIndex.PositionAttrIndex, "inPosition");
            GL.BindAttribLocation(shaderProgramHandle, (int)AttributeIndex.NormalAttrIndex, "inNormal");
            GL.BindAttribLocation(shaderProgramHandle, (int)AttributeIndex.UvAttrIndex, "inUvCoord");
            //link use
            GL.LinkProgram(shaderProgramHandle);
            GL.UseProgram(shaderProgramHandle);
            //bind texture locations
            GL.Uniform1(GL.GetUniformLocation(shaderProgramHandle, "textureDiffuse"), 0);
            GL.Uniform1(GL.GetUniformLocation(shaderProgramHandle, "textureNormal"), 1);
            GL.Uniform1(GL.GetUniformLocation(shaderProgramHandle, "textureSpecular"), 2);
            GL.Uniform1(GL.GetUniformLocation(shaderProgramHandle, "shadowMap"), 3);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.ClearColor(0.7f, 0.7f, 0.9f, 1f);

            InitializeCameraAndLight();
            Resize(width, height);
        }

        private void RenderShadowMap()
        {
            GL.UseProgram(depthProgramHandle);

            if (depthFBO < 1)
            {
                //create fbo and texture
                GL.GenFramebuffers(1, out depthFBO);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthFBO);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.GenTextures(1, out depthTexture);
                GL.BindTexture(TextureTarget.Texture2D, depthTexture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, depthTextureSize, depthTextureSize, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, depthTexture, 0);
                GL.DrawBuffer(DrawBufferMode.None);
                GL.ReadBuffer(ReadBufferMode.None);
                if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                    return;
            }

            GL.CullFace(CullFaceMode.Front);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 1);
            GL.Viewport(0, 0, depthTextureSize, depthTextureSize);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Vector3 lightLookAt = new Vector3(0, 0, 0);
            Matrix4 depthP = Light.IsDirectional ? Matrix4.CreateOrthographic(20f, 20f, 0.1f, 100f) : Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, 1f, 0.1f, 100f);
            Matrix4 depthV = Matrix4.LookAt(Light.Position, lightLookAt, Vector3.UnitY);
            depthVP = depthV * depthP;
            GL.UniformMatrix4(GL.GetUniformLocation(depthProgramHandle, "depthVP"), false, ref depthVP);

            RenderScene(depthProgramHandle);
            GL.CullFace(CullFaceMode.Back);
        }

        public void Resize(double width, double height)
        {
            viewPortHeight = (int)height;
            viewPortWidth = (int)width;
            if (Camera != null)
                Camera.AspectRatio = (float)(width / height);
        }

        private void RenderScene(int shaderProgram)
        {
            foreach (LoadedModel model in LoadedModels)
                model.Draw(shaderProgram);
            if (DrawFloorEnabled)
                Floor.Draw(shaderProgram);
        }

        private void InitializeCameraAndLight()
        {
            Camera = new Camera(new Vector3(0f, 1f, 5f), new Vector3(0f, 0f, 0f));
            Light = new Light
            {
                Position = new Vector3(20f, 20f, 20f),
                Color = new Vector4(1f, 1f, 1f, 1f),
                Attenuation = 0.0001f
            };
        }

        private string ReadShaderString(string fileName)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("_3dModelViewer.Graphics.Shaders." + fileName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
