#version 330

in vec4 position;
in vec2 texCoord;
out vec4 out_frag_color;

uniform sampler2D texture;

uniform vec4 sprite_rect;
uniform vec2 sheet_size;

void main(void)
{
    vec2 sheet_coord = vec2((sprite_rect.x + (sprite_rect.z * texCoord.x)) / sheet_size.x,
                            (sprite_rect.y + (sprite_rect.w * texCoord.y)) / sheet_size.y);
    vec4 result = texture2D(texture, sheet_coord);
    if (result.a < 0.15) discard;
    out_frag_color = result;
}
