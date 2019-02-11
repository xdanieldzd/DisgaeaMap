#version 330

in vec4 position;
in vec3 normal;
in vec4 color;
in vec2 texCoord;
out vec4 out_frag_color;

uniform sampler2D texture;

void main(void)
{
    vec4 texture = texture2D(texture, texCoord);
    if (texture.a < 0.9) discard;
    out_frag_color = color * texture;
}
