using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using PUSN.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUSN.SceneModels
{
    public class Path
    {
        public int VertexArrayObject { get; set; }
        public int VertexBufferObject { get; set; }
        public int ElementBufferObject { get; set; }
        public int IndicesCount { get; set; }

        public float[] vertices;
        public int[] indices;

        public List<Vector3> points;

        public Path()
        {
            points = new List<Vector3>();
            IndicesCount = 0;
            vertices = new float[4];
            indices = new int[4];
            GenerateVAO();
        }

        public Path(List<Vector3> point)
        {
            points = point;
            IndicesCount = 0;
            vertices = new float[4];
            indices = new int[4];
            GenerateVAO();
        }

        public void AddPoints(List<Vector3> points)
        {
            this.points = points;
            UpdateVAO();
        }

        private void GenVertsInd()
        {
            List<float> verts = new List<float>();
            List<int> ind = new List<int>();
            for (int i = 0; i < points.Count; i++)
            {
                verts.Add(points[i].X);
                verts.Add(points[i].Y);
                verts.Add(points[i].Z);

                if (i != points.Count - 1) { ind.Add(i); ind.Add(i + 1); }
            }
            vertices = verts.ToArray();
            indices = ind.ToArray();
        }

        public void GenerateVAO()
        {
            GenVertsInd();
            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);
            VertexBufferObject = GL.GenBuffer(); //tworzy bufor na karcie graficznej i daje uchwyt dla VertexBufferObject
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject); //przypisujemy bufor do danego typu
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw); //copies the previously defined vertex data into the buffer's memory
            ElementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            IndicesCount = indices.Length;
        }

        public void UpdateVAO()
        {
            GenVertsInd();
            GL.BindVertexArray(VertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw); //copies the previously defined vertex data into the buffer's memory
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.DynamicDraw);
            IndicesCount = indices.Length;
        }

        public void Draw(Shader shader, Matrix4 View, Matrix4 Perspective)
        {
            if (points.Count > 0)
            {
                shader.Use();
                shader.SetMatrix4("transform", View * Perspective);
                GL.BindVertexArray(VertexArrayObject);
                GL.LineWidth(5);
                GL.DrawElements(PrimitiveType.Lines, IndicesCount, DrawElementsType.UnsignedInt, 0);
                GL.LineWidth(1);
            }
        }
    }
}
