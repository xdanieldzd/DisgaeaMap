#version 330

layout(location = 0) in vec4 in_position;
layout(location = 1) in vec2 in_texCoord;
out vec4 position;
out vec2 texCoord;

uniform mat4 projection_matrix;
uniform mat4 modelview_matrix;
uniform mat4 sprite_matrix;

uniform vec4 sprite_rect;
uniform vec3 grid_position;

void main(void)
{
    position = in_position;
    texCoord = in_texCoord;
    
    mat4 model = mat4(1.0,             0.0,             0.0,             0.0,
                      0.0,             1.0,             0.0,             0.0,
                      0.0,             0.0,             1.0,             0.0,
                      grid_position.x, grid_position.y, grid_position.z, 1.0);
    
    mat4 mvmat = modelview_matrix * model;
    
    mvmat[0][0] = sprite_rect.z / 32.0;
    mvmat[1][1] = sprite_rect.w / 32.0;
    mvmat[2][2] = 1.0;
    mvmat[0][1] = mvmat[0][2] = mvmat[1][2] = 0.0;
    mvmat[1][0] = mvmat[2][0] = mvmat[2][1] = 0.0;
    
    gl_Position = projection_matrix * mvmat * sprite_matrix * vec4(position.x, position.y, position.z, 1.0);
}
