#version 400 core
out vec4 FragColor;

in vec3 Normal;
in vec3 FragPos;
in vec2 Texture;

uniform vec3 lightPos;
uniform vec3 viewPos;
uniform vec3 lightColor;
uniform vec3 objectColor;

uniform sampler2D heights;

//in case of this project axis z is for height

vec2 GetTexPos(vec2 screen)
{
    return (screen + 1f)*0.5f;
}



void main()
{
    //ambient
    float ambientStrength = 0.2;
    vec3 ambient = ambientStrength * lightColor;

    //if (Texture.x == 0.5f) discard;

    //diffuse
    //vec3 norm = normalize(Normal);    //now it's better to calculate normal straight from heightmap 
    vec2 t = Texture + vec2(0, 0.001);
    vec2 b = Texture + vec2(0, -0.001);
    vec2 l = Texture + vec2(-0.001,0);
    vec2 r = Texture + vec2(0.001,0);
    
    float top = texture2D(heights, t).r;
    float bottom = texture2D(heights,b).r;
    float left = texture2D(heights, l).r;
    float right = texture2D(heights, r).r;

    
    //normal can be calculated with the gradient of surface function. In that case
    //it can be done by making derivative on horizontal and vertical axis (example: value on the right minus value on left diveded by the differrence in x space)
    //if(t.y>1) vec3 norm = normalize(vec3((right-left)/0.02,-(bottom-top)/0.02,1));
    
    vec3 norm = normalize(vec3((right-left)/0.02,-(bottom-top)/0.02,1));

    //vec3 norm = normalize(vec3(0, 0, 1));
    
    vec3 lightDir = normalize(lightPos - FragPos);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor;

    //specular 
    float specularStrength = 0.5;
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 16); //32 = shiness of material
    vec3 specular = specularStrength * spec * lightColor;

    //ambient test
    //vec3 res = (ambient + diffuse + specular) * objectColor;
    vec3 res = (ambient + diffuse + specular) * objectColor;
    //vec3 res = (ambient) * objectColor;
    FragColor = vec4(res, 1.0f);

    //just return the color of the light (let's see if shaders work at all)
    //FragColor = vec4(objectColor, 1.0f);
}