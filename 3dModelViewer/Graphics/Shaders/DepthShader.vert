#version 330

in vec4 inPosition;
uniform mat4 depthVP;
uniform mat4 modelMatrix;

void main(void){
    gl_Position = depthVP * modelMatrix * inPosition;
}