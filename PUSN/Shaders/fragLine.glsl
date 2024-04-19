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
    float deltaZ = sqrt(1 - pow(frag_in.TexCoord.y,2));
    float r = (vec4(0, 0, Radius, 0) * transform).z;

    float z = frag_in.ModelCoord.z + r * (1 - deltaZ) * Spherical;

    //for now just to load Spherical uniform it must be used in logical sense (have impact on result)
    if (Spherical == 0) discard;

    //if (deltaZ == 1) discard;
    //if (frag_in.TexCoord.y <= 0.1f && frag_in.TexCoord.y>=-0.1f) discard;

    //TODO 
    //float h = heightMap.Sample()
    //if(y>h) discard

    //TODO 
    float currentH = texture2D(heights,GetTexPos(frag_in.ModelCoord.xy)).r;

    //float h = heightMap.Sample()
    if(z>currentH) discard;

    Height = z;
}



