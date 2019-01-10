#version 330

in vec4 position;
in vec4 color;
in vec2 texCoord0;
in vec2 texCoord1;
out vec4 out_frag_color;

uniform int wireframe;
uniform sampler2D texture0;
uniform sampler2D texture1;
uniform int blendMode0;
uniform int blendMode1;

// TODO: verify blendmodes

void main(void)
{
    vec4 result = color;
    
    vec4 tex0 = texture2D(texture0, texCoord0);
    vec4 tex1 = texture2D(texture1, texCoord1);
    
    if (blendMode0 == 0)
    {
        // Normal
        result = mix(result, tex0, tex0.a);
    }
    else if (blendMode0 == 1)
    {
        // Translucent
        result.rgb = mix(result.rgb, tex0.rgb, tex0.a / 2.0);
    }
    else if (blendMode0 == 2)
    {
        // Addition
         result.rgb = result.rgb + tex0.rgb;
    }
    else if (blendMode0 == 3)
    {
        // Subtraction
        result.rgb = result.rgb - tex0.rgb;
    }
        
    if (blendMode1 == 0)
    {
        // Normal
        result.rgb = mix(result.rgb, tex1.rgb, tex1.a);
    }
    else if (blendMode1 == 1)
    {
        // Translucent
        result.rgb = mix(result.rgb, tex1.rgb, tex1.a / 2.0);
    }
    else if (blendMode1 == 2)
    {
        // Addition
        result.rgb = result.rgb + tex1.rgb;
    }
    else if (blendMode1 == 3)
    {
        // Subtraction
        result.rgb = result.rgb - tex1.rgb;
    }
    
    if (wireframe == 1)
    {
        out_frag_color = vec4(1.0 - result.r, 1.0 - result.g, 1.0 - result.b, 0.5);
    }
    else
    {
        if(result.a < 0.15) discard;
        out_frag_color = result;
    }
}
