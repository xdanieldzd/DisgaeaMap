#version 330

in vec4 position;
in vec2 texCoord;
out vec4 out_frag_color;

uniform sampler2D texture;

uniform vec4 sprite_rect;
uniform vec2 sheet_size;

uniform int wireframe;
uniform int flip_x;
uniform int flip_y;

void main(void)
{
    if (wireframe == 1)
    {
        out_frag_color = vec4(1.0, 1.0, 1.0, 1.0);
    }
    else
    {
        vec2 texture_coord = texCoord;
        
        if (flip_x != 0) texture_coord.x = 1.0 - texture_coord.x;
        if (flip_y != 0) texture_coord.y = 1.0 - texture_coord.y;
        
        vec2 sheet_coord = vec2((sprite_rect.x + (sprite_rect.z * texture_coord.x)) / sheet_size.x,
                                (sprite_rect.y + (sprite_rect.w * texture_coord.y)) / sheet_size.y);
        vec4 result = texture2D(texture, sheet_coord);
        
		if (result.a < 0.15) discard;
        out_frag_color = result;
    }
}
