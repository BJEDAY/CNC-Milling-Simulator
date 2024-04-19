#version 420 core
out vec4 FragColor;

in FInput
{
    vec2 TexCoord;
    vec3 ModelCoord;
}frag_in;

uniform float Radius;
uniform mat4 transform;
uniform int Spherical;    //if tool shape is Sphere then Spherical =1 else Spherical =0 

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

    FragColor = vec4(1.0f, 1.0f, 0.0f, 1.0f);
}



