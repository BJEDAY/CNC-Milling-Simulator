#version 400 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec2 texCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform mat4 display;
uniform sampler2D heights;

out vec3 FragPos;
out vec3 Normal;
out vec2 Texture;

//normals will be calculated using heigtmap texture, but for now let's leave it as it is

void main()
{
    FragPos = vec3(vec4(aPosition, 1.0) * model);
    Normal = aNormal * mat3(transpose(inverse(model)));
    Texture = texCoord;

    float h = texture2D(heights, texCoord).r;
    //if(texCoord.x == 0 || texCoord.x == 1 || texCoord.y ==0 || texCoord.y == 1) h =(vec4(0,0,0.0f,1.0) * display).z;
    //else 
    h = (vec4(0,0,h,1.0) * display).z;
    //if (h < 1) aPosition = vec3(0, 0, 0);
    //if (h = 0) FragPos.Z = 0.0f;
    //FragPos.z = h;


    //gl_Position = vec4(aPosition.x, aPosition.y, aPosition.z+h, 1.0) * model * view * projection;
    gl_Position = vec4(aPosition.x, aPosition.y, h, 1.0) * model * view * projection;
}