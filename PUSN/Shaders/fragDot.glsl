#version 420 core
//out vec4 FragColor;
out float Height;

in FInput
{
    vec2 TexCoord;
    vec3 ModelCoord;
}frag_in;

uniform float Radius;
uniform mat4 transform;
uniform int Spherical;    //if tool shape is Sphere then Spherical =1 else Spherical =0 
uniform sampler2D heights;

vec2 GetTexPos(vec2 screen)
{
    return (screen + 1f)*0.5f;
}


void main()
{
    
    float check = pow(frag_in.TexCoord.x, 2) + pow(frag_in.TexCoord.y, 2);  // geometry shader draws square and then fragment shader selects pixels that makes perfect circle
    if (1 - check<0) { discard; }

    float deltaZ = sqrt(1 - check);
    float r = (vec4(0, 0, Radius, 0) * transform).z;

    float z = frag_in.ModelCoord.z - r;// *(deltaZ-1) * Spherical;

    //for now just to load Spherical uniform it must be used in logical sense (have impact on result)
    if (Spherical == 0)
    {
        discard;
    }

    //TODO 
    float currentH = texture2D(heights,GetTexPos(frag_in.ModelCoord.xy)).r;

    //float h = heightMap.Sample()
    if(z>currentH) discard;

    //FragColor = vec4(1.0f, 1.0f, 0.0f, 1.0f);
    Height = z;
}



