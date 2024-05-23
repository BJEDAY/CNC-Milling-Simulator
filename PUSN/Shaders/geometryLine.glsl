#version 420 core


layout(lines) in;
layout(triangle_strip, max_vertices = 6) out;

out FInput
{
    vec2 TexCoord;
    vec3 ModelCoord;
    float GlobalZ;
}frag;

uniform float Radius;
uniform mat4 transform;

void main() {
    vec4 globalStart = gl_in[0].gl_Position;
    vec4 globalEnd = gl_in[1].gl_Position;
    vec4 start = gl_in[0].gl_Position* transform;
    vec4 end = gl_in[1].gl_Position* transform;
    vec4 dir = end - start;

    vec4 ort = vec4(cross(vec3(dir.xy,0),vec3(0,0,1)),0);
    ort = normalize(ort);

    vec4 size = vec4(Radius, Radius, 0, 0) * transform;
    ort = ort * size;

    //first triangle
    gl_Position = start - ort + dir;    //bottom right
    frag.TexCoord = vec2(1, -1);
    frag.ModelCoord = gl_Position.xyz;
    frag.GlobalZ = globalEnd.z;
    EmitVertex();

    gl_Position = start + ort;          //top left
    frag.TexCoord = vec2(-1, 1);
    frag.ModelCoord = gl_Position.xyz;
    frag.GlobalZ = globalStart.z;
    EmitVertex();

    gl_Position = start + ort + dir;    //top right
    frag.TexCoord = vec2(1, 1);
    frag.ModelCoord = gl_Position.xyz;
    frag.GlobalZ = globalEnd.z;
    EmitVertex();

    EndPrimitive();

    //second triangle
    gl_Position = start - ort;  //bottom left
    frag.TexCoord = vec2(-1, -1);
    frag.ModelCoord = gl_Position.xyz;
    frag.GlobalZ = globalStart.z;
    EmitVertex();

    gl_Position = start - ort + dir;    //bottom right
    frag.TexCoord = vec2(1, -1);
    frag.ModelCoord = gl_Position.xyz;
    frag.GlobalZ = globalEnd.z;
    EmitVertex();


    gl_Position = start + ort;      //top left
    frag.TexCoord = vec2(-1, 1);
    frag.ModelCoord = gl_Position.xyz;
    frag.GlobalZ = globalStart.z;
    EmitVertex();

    EndPrimitive();
}








