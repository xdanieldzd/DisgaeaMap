#version 330

in vec4 position;
in vec4 color;
in vec2 texCoord;
out vec4 out_frag_color;

uniform sampler2D texture;

void main(void)
{
    vec4 result = color * texture2D(texture, texCoord);
    out_frag_color = result;
}
