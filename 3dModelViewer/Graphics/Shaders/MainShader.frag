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
in vec4 shadowCoord;
uniform mat3 normalMatrix;
uniform vec3 cameraPos;
uniform float hasTextureNormal;
uniform Material material;
uniform Light light;
uniform sampler2D textureDiffuse;
uniform sampler2D textureNormal;
uniform sampler2D textureSpecular;
uniform sampler2D shadowMap;
out vec4 fragColor;

//local variables
vec3 norm;
vec4 surfaceColor;
vec3 lightDir;
vec3 viewDir;

void ComputeLocals(){
    vec3 normal = passNormal;
    if(hasTextureNormal > 0.0)
        normal = normal * (2.0*texture(textureNormal, passUvCoord).rgb - 1.0);
    normal = normalMatrix*normal;
    norm = normalize(normal);
    surfaceColor = material.diffuse + texture(textureDiffuse, passUvCoord);
    lightDir = normalize(light.position - passPosition); 
    viewDir = normalize(cameraPos - passPosition);
}

vec4 GetAmbient(){
	return light.color*material.ambient;
}

vec4 GetDiffuse(){
    float diff = max(dot(norm, lightDir), 0.0);
    return diff*surfaceColor*light.color;
}

vec4 GetSpecular(){
    vec3 reflectDir = reflect(-lightDir, norm);  
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    return spec * (material.specular + texture(textureSpecular, passUvCoord)) * light.color;
}

float GetAttenuation(){
    float distanceToLight = length(light.position - passPosition);
    return 1.0/(1.0 + light.attenuation*pow(distanceToLight, 2));
}

float GetVisibility(){
	vec3 shadowUv = shadowCoord.xyz / shadowCoord.w;
	shadowUv = (shadowUv + 1.0)/2.0;
	float bias = max(0.00001 * (1.0 - dot(norm, lightDir)), 0.000001); 
	float visibility = 1.0;
	if(texture(shadowMap, shadowUv.xy).r < shadowUv.z - bias)
		visibility = 0.5;
	return visibility;
}

void main(void){
	ComputeLocals();

    vec4 ambient = GetAmbient();
    vec4 diffuse = GetDiffuse();
	vec4 specular = GetSpecular();
    float attenuation = GetAttenuation();
	float visibility = GetVisibility();

    ////gamma correction
    //vec3 linearColor = vec3(ambient + attenuation*(diffuse + specular));
    //vec3 gamma = vec3(1.0/2.2);
	
    fragColor = visibility*(ambient + attenuation*(diffuse + specular));
	//fragColor = vec4(shadowUv.x, shadowUv.y, 0.0, 1.0);
	//fragColor = visibility*surfaceColor;
	//fragColor = texture(shadowMap, passUvCoord);
    //fragColor = vec4(pow(linearColor, gamma), surfaceColor.a);
    //fragColor = vec4(1.0,0.0,1.0,1.0);
}