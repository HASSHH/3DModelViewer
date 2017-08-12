#version 330

in vec4 inPosition;
in vec3 inNormal;
in vec2 inUvCoord;
uniform mat4 vpMatrix;
uniform mat4 modelMatrix;
uniform mat4 depthVP;
out vec2 passUvCoord;
out vec3 passNormal;
out vec3 passPosition;
out vec4 shadowCoord;

void main(void){
	shadowCoord = depthVP* modelMatrix * inPosition;
    gl_Position = vpMatrix * modelMatrix * inPosition;
    passUvCoord = inUvCoord;
    passNormal = inNormal;
    passPosition = vec3(modelMatrix*inPosition);
}