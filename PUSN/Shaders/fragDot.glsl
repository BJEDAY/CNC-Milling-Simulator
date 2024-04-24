﻿#version 400 core
//out vec4 FragColor;
out float Height;

in FInput
{
    vec2 TexCoord;
    vec2 ModelXY;
    float GlobalZ;
}frag_in;

uniform float Radius;
uniform mat4 transform;
uniform int Spherical;    //if tool shape is Sphere then Spherical =1 else Spherical =0 
uniform sampler2D heights;

vec2 GetTexPos(vec2 screen)
{
    return (screen* vec2(1,-1) + 1f)*0.5f;
}


void main()
{
    
    float check = pow(frag_in.TexCoord.x, 2) + pow(frag_in.TexCoord.y, 2);  // geometry shader draws square and then fragment shader selects pixels that makes perfect circle

    float currentH = texture2D(heights,GetTexPos(frag_in.ModelXY)).r;
    //float currentH = texture2D(heights,vec2(0.1f,0.1f)).x;
    //1 - check<=0
    //Spherical == 1
    if (1 - check<0) 
    { // If it's not affected by current position of tool leave current h
        //discard;
        //Height = 30;
        Height = currentH;
        //Height = frag_in.GlobalZ;
        //Height = 5;
    }
    else
    {
        float deltaZ = sqrt(1 - check);

        //float r = (vec4(0, 0, Radius, 0) * transform).z;
        float r = Radius/6;
        //-frag_in.ModelCoord.z
        float z =  frag_in.GlobalZ-r *(deltaZ-1)* Spherical;// - r ;
           
        // Discard prawdopodobnie zwraca h=0, a to źle
        //if(currentH<-100000)
        if(z-currentH>0 || z>50)
        {
            //discard;
            Height = currentH;
        }
        else
        {
            //FragColor = vec4(1.0f, 1.0f, 0.0f, 1.0f);
            Height = z;
        } 
    }
}



