using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using PUSN.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace PUSN.SceneModels
{
    public class Sphere
    {
        public string ObjectName { get; set; }
        public int VertexArrayObject { get; set; }
        public int VertexBufferObject { get; set; }
        public int ElementBufferObject { get; set; }
        public Matrix4 ModelMatrix { get; set; }
        public Vector3 Rot { get; set; }
        public Vector3 Translation { get; set; }
        public Vector3 Scale { get; set; }

        bool HalfSphere;

        float[] vertices; int[] indices; //float[] normals; normals are kept inside vertices array

        int[] lineInd;
        public float Radius { get; set; }
        public float Height { get; set; }

        public int ResX { get; set; }
        //public int ResY { get; set; }
        int IndicesCount, SectorCount,  StackCount;

        public Sphere()
        {
            HalfSphere = true;
            SectorCount = 120;
            StackCount = 120;
            Radius = 3.0f;
            Height = 10.0f;
            ResX = 50;
            //ResY = 36;
            Rot = new Vector3(0, 0, 0);
            Translation = new Vector3(0, 0, 0f);
            Scale = Vector3.One;
            vertices = new float[] {-1, -1, 0,  0,0,-1,
                                    -1,  1, 0,  0,0,-1,
                                     1,  1, 0,  0,0,-1,
                                     1, -1, 0,  0,0,-1,
                                    };
            indices = new int[] { 1,3 ,0,
                                  1,2,3                       
                                };

            //normals = new float[0];
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
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicCopy);    //Wgrywanie zawartości.

            ElementBufferObject = GL.GenBuffer();   //Uchwyt do bufora zawierającego informacje o kolejności łączenia wierzchołków z VBO.
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(float), indices, BufferUsageHint.DynamicCopy);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);   //określamy w jakiej formie podane są kolejne wierzchołki, w tym przypadku zaczynamy od indeksu 0 odczyt i przez 3 kolejne pozycje wyłuskujemy pozycje x y i z 
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

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

            float X, Y, Z, RCos;
            float Nx, Ny, Nz, lenInv = 1.0f / Radius;
            //float s,t  // if wanna add tex coord and sample earth tex or whatever that's gonna be useful

            float ThetaStep = (float)(2 * Math.PI / SectorCount);
            float PhiStep = (float)(Math.PI / StackCount);
            float Phi, Theta;

            int Max = StackCount;
            if (HalfSphere) { Max /=  2; }

            for(int i=0; i<= Max; ++i)
            {
                Phi = (float)(Math.PI / 2 - i * PhiStep);
                RCos = (float)(Radius * Math.Cos(Phi));
                Z = (float)(Radius * Math.Sin(Phi));

                // first and last vertices will have the same position but diffrent tex coord
                for(int j=0; j<=SectorCount; ++j)
                {
                    Theta = j* ThetaStep;

                    // vertices - they gonna hold info about positions, then normals, and in future if needed also texture
                    X = (float)(RCos * Math.Cos(Theta));
                    Y = (float)(RCos * Math.Sin(Theta));
                    verts.Add(X);
                    verts.Add(Y);
                    verts.Add(Z);

                    // normals
                    Nx = X * lenInv;
                    Ny = Y * lenInv;
                    Nz = Z * lenInv;
                    verts.Add(Nx);
                    verts.Add(Ny);
                    verts.Add(Nz);

                    // tex coord
                    // s = (float) j/SectorCount;
                    // t = (float) i/StackCount;
                }
            }

            List<int> lineIndices = new List<int>();
            List<int> JustHorizon = new List<int>();

            int k1, k2; //k1 is point on upper horizontal line, k2 is on one line beneath it, when you add +1 you have got point on right
            for(int i=0; i< Max; ++i) // that go almost to end (so it don't overwrite the same triangles again)
            {
                k1 = i * (SectorCount + 1); // start of current stack
                k2 = k1 + SectorCount + 1;  // start of next stack

                for(int j=0; j<SectorCount; ++j, ++k1, ++k2) 
                {
                    // there are two triangles per stack except first and last one 

                    // first stack holds triangle (K1+1) -> K2 -> (K2+1)

                    // last stack holds triangle K1 -> K2 -> (K1+1)

                    if (i != 0) // wszystkie stacki poza pierwszym mają tego trójkąta
                    {
                        ind.Add(k1);
                        ind.Add(k2);
                        ind.Add(k1+1);  
                    }

                    if(i != (StackCount-1))
                    {
                        ind.Add(k1 + 1);
                        ind.Add(k2);
                        ind.Add(k2 + 1);
                    }

                    // vertical line
                    lineIndices.Add(k1); lineIndices.Add(k2);

                    // horizontal line
                    if(i!=0)
                    {
                        lineIndices.Add(k1);
                        lineIndices.Add(k1+1);

                        JustHorizon.Add(k1);
                        JustHorizon.Add(k1 + 1);
                    }
                }
            }
            vertices = verts.ToArray();
            indices = ind.ToArray();
            lineInd = lineIndices.ToArray();
        }

        public void UpdateModelMatrix()
        {
            ModelMatrix = Matrix4.Identity;
            ModelMatrix = ModelMatrix * Matrix4.CreateScale(Scale);
            ModelMatrix = ModelMatrix * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rot[0])) * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rot[1])) * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rot[2]));
            ModelMatrix = ModelMatrix * Matrix4.CreateTranslation(Translation);
        }

        public void Render(Shader shader, Matrix4 View, Matrix4 Perspective, Vector3 cameraPos, Vector3 ObjectColor)
        {
            shader.Use();
            shader.SetMatrix4("model", ModelMatrix);
            shader.SetMatrix4("view", View);
            shader.SetMatrix4("projection", Perspective);
            shader.SetVec3("objectColor", ObjectColor);  //uniform vec3 objectColor;
            shader.SetVec3("viewPos", cameraPos);
            GL.BindVertexArray(VertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, IndicesCount, DrawElementsType.UnsignedInt, 0);    //PrimitiveType.Triangles
            GL.BindVertexArray(0);
        }

        public void RenderLines(Shader shader, Matrix4 View, Matrix4 Perspective)
        {
            shader.Use();
            shader.SetMatrix4("transform", ModelMatrix*View*Perspective);
            GL.BindVertexArray(VertexArrayObject);
            GL.DrawElements(PrimitiveType.Lines, IndicesCount, DrawElementsType.UnsignedInt, 0);    //PrimitiveType.Triangles
        }
    }
}
