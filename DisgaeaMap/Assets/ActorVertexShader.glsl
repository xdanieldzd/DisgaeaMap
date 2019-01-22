#version 330

layout(location = 0) in vec4 in_position;
layout(location = 1) in vec4 in_color;
layout(location = 2) in vec2 in_texCoord;
layout(location = 3) in vec3 in_offset;
out vec4 position;
out vec4 color;
out vec2 texCoord;
out vec3 offset;

uniform mat4 projection_matrix;
uniform mat4 modelview_matrix;
uniform vec2 sprite_size;

void main(void)
{
    position = in_position;
    color = in_color;
    texCoord = in_texCoord;
    offset = in_offset;
    
    mat4 model = mat4(1.0,      0.0,      0.0,      0.0,
                      0.0,      1.0,      0.0,      0.0,
                      0.0,      0.0,      1.0,      0.0,
                      offset.x, offset.y, offset.z, 1.0);
    
    mat4 mvmat = modelview_matrix * model;
    
    mvmat[0][0] = sprite_size.x;
    mvmat[1][1] = sprite_size.y;
    mvmat[2][2] = 1.0;
    mvmat[0][1] = mvmat[0][2] = mvmat[1][2] = 0.0;
    mvmat[1][0] = mvmat[2][0] = mvmat[2][1] = 0.0;
    
    gl_Position = projection_matrix * mvmat * vec4(position.xyz, 1.0);
}
