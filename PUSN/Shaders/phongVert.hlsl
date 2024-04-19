#version 400 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 FragPos;
out vec3 Normal;

void main()
{
    FragPos = vec3(vec4(aPosition, 1.0)*model);
    Normal = aNormal * mat3(transpose(inverse(model)));
    gl_Position = vec4(aPosition, 1.0) * model*view*projection;
}