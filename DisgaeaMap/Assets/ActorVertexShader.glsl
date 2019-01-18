#version 330

layout(location = 0) in vec4 in_position;
layout(location = 1) in vec4 in_color;
layout(location = 2) in vec2 in_texCoord;
out vec4 position;
out vec4 color;
out vec2 texCoord;

uniform mat4 projection_matrix;
uniform mat4 modelview_matrix;

void main(void)
{
    position = in_position;
    color = in_color;
    texCoord = in_texCoord;
    
    gl_Position = projection_matrix * modelview_matrix * position;
}
