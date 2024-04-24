#version 400 core


layout(points) in;
layout(triangle_strip, max_vertices = 6) out;


out FInput
{
    vec2 TexCoord;
    vec2 ModelXY;
    float GlobalZ;
}frag;

uniform float Radius;
uniform mat4 transform;

void main() {
    vec4 global = gl_in[0].gl_Position;
    vec4 center = gl_in[0].gl_Position * transform;
    vec4 size = vec4(Radius, Radius, 0, 0) * transform;
    
    //first triangle

    gl_Position = center + vec4(size.x, -size.y, 0, 0); //bottom right
    frag.TexCoord = vec2(1, -1);
    frag.ModelXY = gl_Position.xy;
    frag.GlobalZ = global.z;
    EmitVertex();

    gl_Position = center + vec4(size.x,size.y,0,0); //top right
    frag.TexCoord = vec2(1, 1);
    frag.ModelXY = gl_Position.xy;
    frag.GlobalZ = global.z;
    EmitVertex();

    gl_Position = center + vec4(-size.x,size.y,0,0); //top left
    frag.TexCoord = vec2(-1, 1);
    frag.ModelXY = gl_Position.xy;
    frag.GlobalZ = global.z;
    EmitVertex();

    EndPrimitive();

    //second triangle
    gl_Position = center + vec4(-size.x, -size.y, 0, 0); //bottom left
    frag.TexCoord = vec2(-1, -1);
    frag.ModelXY = gl_Position.xy;
    frag.GlobalZ = global.z;
    EmitVertex();

    gl_Position = center + vec4(size.x, -size.y, 0, 0); //bottom right
    frag.TexCoord = vec2(1, -1);
    frag.ModelXY = gl_Position.xy;
    frag.GlobalZ = global.z;
    EmitVertex();

    gl_Position = center + vec4(-size.x, size.y, 0, 0); //top left
    frag.TexCoord = vec2(-1, 1);
    frag.ModelXY = gl_Position.xy;
    frag.GlobalZ = global.z;
    EmitVertex();

    EndPrimitive();
}








