#version 420 core
//out vec4 FragColor;
out float Height;
//out vec4 res;

in FInput
{
    vec2 TexCoord;
    vec3 ModelCoord;
    float GlobalZ;
}frag_in;

uniform float Radius;
uniform mat4 transform;
uniform int Spherical;    //if tool shape is Sphere then Spherical =1 else Spherical =0 
uniform sampler2D heights;

uniform sampler2D valTex;
uniform float MinHeight;
uniform float ToolCuttingHeight;
uniform int moveDown;

layout(rgba32f,binding=2) uniform image2D valImage;

vec2 GetTexPos(vec2 screen)
{
    return (screen * vec2(1,-1) + 1f)*0.5f;
}

void main()
{    
    // czyli jak był błąd to niech nie frezuje dalej
    vec4 valCheck = texture2D(valTex,vec2(0,0));
    if(valCheck != vec4(0,0,0,0))
    {
        //imageStore(valImage,ivec2(0,0),vec4(0.0f,0.0f,1.0f,0.0f));
        discard;
    }

    float deltaZ = sqrt(1.0 - pow(frag_in.TexCoord.y,2));
    float r = (vec4(0, 0, Radius, 0) * transform).z * 2;
    float z  = frag_in.ModelCoord.z*2 - r *(deltaZ-1) * Spherical;
    float currentH = texture2D(heights,GetTexPos(frag_in.ModelCoord.xy)).r;

    gl_FragDepth = z; 

    Height = z;

    //Validation check
    // potem wartośc currentH będzie tożsama z wysokością z poprzedniej klatki i będzie użyta do walidacji czy np płaski frez nie schodzi pionowo w dół
    float decrease = currentH-z;

    if(Spherical==0 && decrease>0 && moveDown==1)
    {
        imageStore(valImage,ivec2(0,0),vec4(1.0f,0.0f,0.0f,0.0f));
    }
    if(decrease>ToolCuttingHeight)
    {
        imageStore(valImage,ivec2(0,0),vec4(0.0f,1.0f,0.0f,0.0f));
    }
    if(z<MinHeight)
    {
        imageStore(valImage,ivec2(0,0),vec4(0.0f,0.0f,1.0f,0.0f));
    }
    
}



// OLD:
//    float deltaZ = sqrt(1.0 - pow(frag_in.TexCoord.y,2));
//    float r = (vec4(0, 0, Radius, 0) * transform).z * 2;
//    //float r = Radius/6;
//    //float z  = frag_in.GlobalZ - r *(deltaZ-1) * Spherical;// - r;
//    float z  = frag_in.ModelCoord.z*2 - r *(deltaZ-1) * Spherical;
//
//    float currentH = texture2D(heights,GetTexPos(frag_in.ModelCoord.xy)).r;
//
//    //float h = heightMap.Sample()
//    // if(currentH == 10000000f)
//    if(z-currentH>0)
//    {
//        //discard;
//        gl_FragDepth = 1-currentH;
//        //res = vec4(currentH,0,currentH,1);
//        Height = currentH;
//    }
//    else
//    {
//        //Height = z;
//        gl_FragDepth = 1-z;
//        //res = vec4(z,0,z,1);
//        Height = z;
//    }