using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dModelViewer.Graphics
{
    public class Scene
    {
        public readonly List<LoadedModel> LoadedModels = new List<LoadedModel>();

        private int shaderProgramHandle;
        private Light light;
        private Camera camera;

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
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(shaderProgramHandle);
            //render objects
            RenderScene();
        }

        private void RenderScene()
        {
            foreach (LoadedModel model in LoadedModels)
                model.Draw(shaderProgramHandle);
        }

        public void Init(double width, double height)
        {
            string vertexShaderSource = @"
#version 330

in vec4 inPosition;
in vec3 inNormal;
in vec2 inUvCoord;
uniform mat4 vpMatrix;
uniform mat4 modelMatrix;
out vec2 passUvCoord;
out vec3 passNormal;
out vec3 passPosition;

void main(void){
    gl_Position = vpMatrix * modelMatrix * inPosition;
    passUvCoord = inUvCoord;
    passNormal = inNormal;
    passPosition = vec3(modelMatrix*inPosition);
}";
            string fragmentShaderSource = @"
#version 330

struct Material{
    vec4 diffuse;
    vec4 ambient;
    vec4 specular;
    float shininess;
};
struct Light{
    vec3 position;
    vec4 color;
    float attenuation;
};

in vec2 passUvCoord;
in vec3 passNormal;
in vec3 passPosition;
uniform mat3 normalMatrix;
uniform vec3 cameraPos;
uniform float hasTextureNormal;
uniform Material material;
uniform Light light;
uniform sampler2D textureDiffuse;
uniform sampler2D textureNormal;
uniform sampler2D textureSpecular;
out vec4 fragColor;

void main(void){
    vec4 surfaceColor = material.diffuse + texture(textureDiffuse, passUvCoord);
    vec3 normal = normalMatrix*passNormal;
    if(hasTextureNormal > 0.0)
        normal = normal * (2.0*texture(textureNormal, passUvCoord).rgb - 1.0);
    
    //ambient
    vec4 ambient = light.color*material.ambient;
    //diffuse
    vec3 norm = normalize(normal);
    vec3 lightDir = normalize(light.position - passPosition); 
    float diff = max(dot(norm, lightDir), 0.0);
    vec4 diffuse = diff*surfaceColor*light.color;
    // specular
    vec3 viewDir = normalize(cameraPos - passPosition);
    vec3 reflectDir = reflect(-lightDir, norm);  
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    vec4 specular = spec * (material.specular + texture(textureSpecular, passUvCoord)) * light.color;
    //attenuation
    float distanceToLight = length(light.position - passPosition);
    float attenuation = 1.0/(1.0 + light.attenuation*pow(distanceToLight, 2));
    //gamma correction
    vec3 linearColor = vec3(ambient + attenuation*(diffuse + specular));
    vec3 gamma = vec3(1.0/2.2);
    //fragColor = vec4(pow(linearColor, gamma), surfaceColor.a);
    fragColor = ambient + attenuation*(diffuse + specular);
    //fragColor = specular;
}";
            //create shaders
            int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShaderHandle, vertexShaderSource);
            GL.CompileShader(vertexShaderHandle);
            int fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShaderHandle, fragmentShaderSource);
            GL.CompileShader(fragmentShaderHandle);

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

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(0.7f, 0.7f, 0.9f, 1f);

            InitializeCameraAndLight();
            Resize(width, height);
        }

        private void InitializeCameraAndLight()
        {
            Camera = new Camera(new Vector3(0.5f, 2f, 5f), new Vector3(0f, 0f, 0f));
            Light = new Light
            {
                Position = new Vector3(2f, 1f, 3f),
                Color = new Vector4(1f, 1f, 1f, 1f),
                Attenuation = 0.0001f
            };
        }

        public void Resize(double width, double height)
        {
            GL.Viewport(0, 0, (int)width, (int)height);
            if(Camera != null)
                Camera.AspectRatio = (float)(width / height);
        }
    }
}
