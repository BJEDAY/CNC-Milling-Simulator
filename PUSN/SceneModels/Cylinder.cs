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
    public class Cylinder : ISceneModel
    {
        public string ObjectName { get; set; }

        public int VertexArrayObject { get; set; }
        public int VertexBufferObject { get; set; }
        public int ElementBufferObject { get; set; }

        public int IndicesCount { get; set; }

        public bool IsSelected { get; set; }
        public Matrix4 ModelMatrix { get; set; }
        public int ObjectID { get; set; }

        public Vector3 Rot { get; set; }
        public Vector3 Translation { get; set; }
        public Vector3 Scale { get; set; }

        float[] vertices; int[] indices;

        public float Radius { get; set; }
        public float Height { get; set; }

        public int ResX { get; set; } 
        public int ResY { get; set; } 

        public Cylinder()
        {
            ObjectID = 0;
            ObjectName = $"Cylinder {ObjectID}";
            Radius = 3.0f;
            Height = 10.0f;
            ResX = 36;
            ResY = 36;
            Rot = Vector3.Zero;
            Translation = Vector3.Zero;
            Scale = Vector3.One;
            IsSelected = false;
            vertices = new float[0];
            indices = new int[0];
            GenerateVAO();
            UpdateModelMatrix();
        }

        public void GenerateVAO()
        {
            GenerateMesh();
            
            VertexArrayObject = GL.GenVertexArray(); //VAO - to tak jakby etykieta naszego modelu.
            GL.BindVertexArray(VertexArrayObject);  //Bindujemy czyli mówimy, że teraz wszystkie kolejne działania będą podpięte pod to VAO właśnie.
            
            VertexBufferObject = GL.GenBuffer();    //Tworzymy uchwyt do bufora obiektu, czyli tutaj włożymy wierzchołki. Jako, że wcześniej podpiety został VAO to zawartość VBO jest skojarzona z tym VAO. VBO to trochę wnętrze VAO.
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);    //Bindujemy VBO po to, zeby teraz wgrać do niego zawartość. Operacja BufferData wykona się na aktualnie podpiętmy elemencie (czyli VBO).
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length*sizeof(float),vertices,BufferUsageHint.DynamicCopy);    //Wgrywanie zawartości.

            ElementBufferObject = GL.GenBuffer();   //Uchwyt do bufora zawierającego informacje o kolejności łączenia wierzchołków z VBO.
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length*sizeof(float),indices,BufferUsageHint.DynamicCopy);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);   //określamy w jakiej formie podane są kolejne wierzchołki, w tym przypadku zaczynamy od indeksu 0 odczyt i przez 3 kolejne pozycje wyłuskujemy pozycje x y i z 
            GL.EnableVertexAttribArray(0);
        
            IndicesCount = indices.Length;
        }

        public void UpdateVAO()
        {
            GenerateMesh();

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);    //nie musimy podpinać znów VAO, bo to VBO jest skojarzone z odpowiednim VAO (chyba)
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(float), indices, BufferUsageHint.DynamicDraw);

            IndicesCount = indices.Length;
        }

        public void GenerateMesh()
        {
            List<float> verts = new List<float>();
            List<int> ind = new List<int>();

            float d_alfa = (float)(2 * Math.PI / ResX);
            float d_up = (float)(Height / ResY);

            for (int h = 0; h < ResY; h++)
            {
                for (int i = 0; i < ResX; i++)
                {
                    //calculate current alfa and height
                    float alfa = d_alfa * i;
                    float height = h * d_up;

                    //calculate points of circle
                    float x = Radius * (float)Math.Cos(alfa);
                    float y = Radius * (float)Math.Sin(alfa);
                    float z = height;

                    //add vertices
                    verts.Add(x); verts.Add(y); verts.Add(z);

                    //add indices
                    int current_ind = h * ResX + i;
                    if (i > 0)  //conecting new elem of circle with previous
                    {
                        ind.Add(current_ind - 1);
                        ind.Add(current_ind);
                    }
                    if (i == ResX - 1) //connecting last eleme of circle with first
                    {
                        ind.Add(current_ind);
                        ind.Add(current_ind - i);
                    }
                    if (h > 0) //connecting points from circle above to circle below
                    {
                        ind.Add(current_ind);
                        ind.Add(current_ind - ResX);
                    }
                }
            }

            vertices = verts.ToArray();
            indices = ind.ToArray();
        }

        public void UpdateModelMatrix()
        {
            ModelMatrix = Matrix4.Identity;
            ModelMatrix = ModelMatrix * Matrix4.CreateScale(Scale);
            ModelMatrix = ModelMatrix * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rot[0])) * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rot[1])) * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rot[2]));
            ModelMatrix = ModelMatrix * Matrix4.CreateTranslation(Translation);
        }

        public void Render(Shader shader, Matrix4 View, Matrix4 Perspective)
        {
            shader.Use();
            var mvp = ModelMatrix * View * Perspective;
            shader.SetMatrix4("transform", mvp);
            GL.BindVertexArray(VertexArrayObject);
            GL.DrawElements(PrimitiveType.Lines, IndicesCount, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }
    }
}
