#version 330

layout(location = 0) in vec4 in_position;
layout(location = 1) in vec4 in_color;
layout(location = 2) in vec2 in_texCoord0;
layout(location = 3) in vec2 in_texCoord1;
out vec4 position;
out vec4 color;
out vec2 texCoord0;
out vec2 texCoord1;

uniform mat4 projection_matrix;
uniform mat4 modelview_matrix;

void main(void)
{
    position = in_position;
    color = in_color;
    texCoord0 = in_texCoord0;
    texCoord1 = in_texCoord1;
    
    gl_Position = projection_matrix * modelview_matrix * in_position;
}
