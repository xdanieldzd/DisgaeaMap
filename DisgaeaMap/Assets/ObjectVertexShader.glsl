#version 330

layout(location = 0) in vec4 in_position;
layout(location = 1) in vec3 in_normal;
layout(location = 2) in vec4 in_color;
layout(location = 3) in vec2 in_texCoord;
out vec4 position;
out vec3 normal;
out vec4 color;
out vec2 texCoord;

uniform mat4 projection_matrix;
uniform mat4 modelview_matrix;
uniform mat4 local_matrix;

void main(void)
{
    position = in_position;
    normal = (modelview_matrix * vec4(in_normal, 0)).xyz;
    color = in_color;
    texCoord = in_texCoord;
    
    gl_Position = projection_matrix * modelview_matrix * local_matrix * in_position;
}
