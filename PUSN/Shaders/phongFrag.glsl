#version 400 core
out vec4 FragColor;

in vec3 Normal;
in vec3 FragPos;

uniform vec3 lightPos;
uniform vec3 lightPos2;
uniform vec3 viewPos;
uniform vec3 lightColor;
uniform vec3 objectColor;

void main()
{
    //ambient
    float ambientStrength = 0.2;
    vec3 ambient = ambientStrength * lightColor;

    //diffuse
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(lightPos - FragPos);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor;

    // second light
    vec3 lightDir2 = normalize(lightPos2-FragPos);
    float diff2 = max(dot(norm,lightDir2),0.0);
    diffuse += 0.2*(diff2*lightColor);

    //specular 
    float specularStrength = 0.5;
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 16); //32 = shiness of material
    vec3 specular = specularStrength * spec * lightColor;

    // second light
    vec3 reflectDir2 = reflect(-lightDir2,norm);
    spec = pow(max(dot(viewDir, reflectDir2), 0.0), 16);
    specular += 0.2*(specularStrength * spec * lightColor);

    //ambient test
    vec3 res = (ambient + diffuse + specular) *objectColor;
    FragColor = vec4(res, 1.0f);

    //just return the color of the light (let's see if shaders work at all)
    //FragColor = vec4(diffuse, 1.0f);
}