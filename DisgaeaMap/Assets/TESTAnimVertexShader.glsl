#version 330

layout(location = 0) in vec4 in_position;
layout(location = 1) in vec2 in_texCoord;
out vec4 position;
out vec2 texCoord;

uniform mat4 projection_matrix;
uniform mat4 modelview_matrix;

uniform vec4 sprite_rect;

void main(void)
{
    position = in_position;
    texCoord = in_texCoord;
    
    gl_Position = projection_matrix * modelview_matrix * vec4(position.x * sprite_rect.z, position.y * sprite_rect.w, position.zw);
}
