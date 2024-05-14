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
    public class Dot
    {
        Vector3 center;
        float ToolRadius;
        Matrix4 ModelMatrix;
        public int VertexArrayObject { get; set; }
        public int VertexBufferObject { get; set; }
        public int ElementBufferObject { get; set; }

        float[] vertices; int[] ind;

        public Dot(Vector3 c, float r)
        {
            center=c; ToolRadius = r;
            vertices = new float[3];
            ind = new int[1];
            vertices[0] = c.X; vertices[1] = c.Y; vertices[2] = c.Z;
            ind[0] = 0;
            ModelMatrix = Matrix4.Identity;
            GenerateVAO();
        }


        public void Draw(ShaderGeometry shader)
        {
            //Draw Thick Line
            shader.Use();
            shader.SetFloat("Radius", ToolRadius);
            //shader.SetInt("Spherical", 1); //1 - spherical shaped tool end
            shader.SetMatrix4("transform", ModelMatrix);
            GL.BindVertexArray(VertexArrayObject);
            GL.DrawElements(PrimitiveType.Points, 1, DrawElementsType.UnsignedInt, 0);
        }

        public void Draw(ShaderGeometry shader, Matrix4 view, Matrix4 perspective)
        {
            //Draw Thick Line
            shader.Use();
            shader.SetFloat("Radius", ToolRadius);
            shader.SetMatrix4("transform", view*perspective);
            GL.BindVertexArray(VertexArrayObject);
            GL.DrawElements(PrimitiveType.Points, 1, DrawElementsType.UnsignedInt, 0);
        }

        public void Update(Vector3 c, float r)
        {
            ToolRadius = r;
            vertices[0] = c.X; vertices[1] = c.Y; vertices[2] = c.Z;
            center = c;
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
            ModelMatrix = ModelMatrix * Matrix4.CreateScale(1 / sX, 1 / sY, 1 / (2*sZ));    //2 * sZ bo czasami frez może być poniżej NDC a jego okolice wypłyną kostkę (ale trzeba pamiętac, że żeby uzyskać prawidłowe Z trzeba będzie mnożyć 2)
        }
    }
}