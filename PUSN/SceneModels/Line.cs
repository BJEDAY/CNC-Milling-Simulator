using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using PUSN.Shaders;

namespace PUSN.SceneModels
{
    public class Line
    {
        Vector3 f;
        Vector3 t;
        float ToolRadius;
        Matrix4 ModelMatrix;
        public int VertexArrayObject { get; set; }
        public int VertexBufferObject { get; set; }
        public int ElementBufferObject { get; set; }

        float[] vertices; int[] ind;

        public Line(Vector3 from, Vector3 to, float r) 
        {
            f = from; t = to; ToolRadius = r;
            vertices = new float[6];
            ind = new int[2];
            vertices[0] = from.X; vertices[1] = from.Y; vertices[2] = from.Z;
            vertices[3] = to.X; vertices[4] = to.Y; vertices[5] = to.Z;
            ind[0] = 0; ind[1] = 1;
            ModelMatrix = Matrix4.Identity;
            GenerateVAO();
        }


        public void Draw(ShaderGeometry shader)
        {
            //Draw Thick Line
            shader.Use();
            shader.SetFloat("Radius", ToolRadius);
            shader.SetMatrix4("transform", ModelMatrix);
            //shader.SetInt("Spherical", 1); //1 - spherical shaped tool end
            GL.BindVertexArray(VertexArrayObject);
            GL.DrawElements(PrimitiveType.Lines, 2, DrawElementsType.UnsignedInt, 0);
        }
        
        public void Update(Vector3 from, Vector3 to, float r)
        {
            ToolRadius = r;
            vertices[0] = from.X; vertices[1] = from.Y; vertices[2] = from.Z;
            vertices[3] = to.X; vertices[4] = to.Y; vertices[5] = to.Z;
            f = from;
            t = to;
            UpdateVAO();
        }

        public void GenerateVAO() //generates Vertex Array Object used for drawing it on screen
        {
            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);
            VertexBufferObject = GL.GenBuffer(); //tworzy bufor na karcie graficznej i daje uchwyt dla VertexBufferObject
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject); //przypisujemy bufor do danego typu
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw); //copies the previously defined vertex data into the buffer's memory
            ElementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, ind.Length * sizeof(uint), ind, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
        }

        public void UpdateVAO()
        {
            GL.BindVertexArray(VertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw); //copies the previously defined vertex data into the buffer's memory
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, ind.Length * sizeof(uint), ind, BufferUsageHint.DynamicDraw);
        }

        public void UpdateModelMatrix(float sX, float sY, float sZ)
        {
            ModelMatrix = Matrix4.Identity;
            ModelMatrix = ModelMatrix * Matrix4.CreateScale(1/sX, 1/sY, 1/(2*sZ));
        }
    }
}
