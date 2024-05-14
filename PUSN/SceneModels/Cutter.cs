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
    public class Cutter : ISceneModel       //almost the same thing as cylinder but vertices are connected in triangles and information about normal is also kept inside
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

        float[] vertices; int[] indices; //float[] normals; normals are kept inside vertices array

        public float Radius { get; set; }
        public float Height { get; set; }

        public int ResX { get; set; }
        //public int ResY { get; set; }

        Sphere sphere;
        public Cutter()
        {
            sphere = new Sphere();
            sphere.Rot = new Vector3(180, 0, 0);
            sphere.Translation = new Vector3(0,0,0.02f);
            sphere.UpdateModelMatrix();
            ObjectID = 0;
            ObjectName = $"Cutter {ObjectID}";
            Radius = 3.0f;
            Height = 10.0f;
            ResX = 50;
            //ResY = 36;
            Rot = new Vector3(90, 0, 0);
            Translation = new Vector3(0, -5, 0);
            Scale = Vector3.One;
            IsSelected = false;
            vertices = new float[0];
            indices = new int[0];
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
            //List<float> norms = new List<float>();

            float dalfa = (float)(2 * Math.PI / ResX);

            var BasicCircleVerts = new List<float>();
            for (int i = 0; i <= ResX; ++i)
            {
                var angle = i * dalfa;
                BasicCircleVerts.Add((float)Math.Cos(angle));
                BasicCircleVerts.Add((float)Math.Sin(angle));
                BasicCircleVerts.Add(0.0f);
            }

            //add lower circle and upper circle
            for (int i = 0; i < 2; ++i)
            {
                var h = i * Height;

                for (int j = 0; j <= ResX; j++)
                {
                    var k = j * 3;
                    float x = BasicCircleVerts[k];
                    float y = BasicCircleVerts[k + 1];
                    float z = BasicCircleVerts[k + 2];

                    verts.Add(x * Radius);
                    verts.Add(h);
                    verts.Add(y * Radius);

                    //norms.Add(x);
                    //norms.Add(z);
                    //norms.Add(y);

                    verts.Add(x);
                    verts.Add(z);
                    verts.Add(y);
                }
            }

            //verts.Count()/3 is the first next free index 
            int baseCenter = verts.Count() / 6; //in next lines of code there is gonna be added new vert in the center of bottom circle
            int topCenter = baseCenter + ResX + 2; //after base point there is gonna be added again base circle of ResX points with new normals


            for (int i = 0; i < 2; ++i)
            {
                var h = i * Height;
                float z = -1 + i * 2;

                //add base center and in next step top center point
                verts.Add(0.0f);
                verts.Add(h);
                verts.Add(0.0f);

                //norms.Add(0.0f);
                //norms.Add(z);
                //norms.Add(0.0f);

                verts.Add(0.0f);
                verts.Add(z);
                verts.Add(0.0f);


                for (int j = 0; j <= ResX; j++)
                {
                    var k = j * 3;
                    float x = BasicCircleVerts[k];
                    float y = BasicCircleVerts[k + 1];

                    verts.Add(x * Radius);
                    verts.Add(h);
                    verts.Add(y * Radius);

                    //norms.Add(x);
                    //norms.Add(z);
                    //norms.Add(y);

                    verts.Add(x);
                    verts.Add(z);
                    verts.Add(y);
                }
            }

            int bottom_ind = 0;
            int top_ind = ResX + 1;

            //making side surface triangles
            for (int i = 0; i < ResX; i++)
            {
                //first triangle
                ind.Add(bottom_ind + 1);
                ind.Add(bottom_ind);
                ind.Add(top_ind);



                //second triangle
                ind.Add(bottom_ind + 1);
                ind.Add(top_ind);
                ind.Add(top_ind + 1);



                bottom_ind++; top_ind++;
            }

            int actual_bottom_ind = baseCenter + 1; //after adding center point in bottom circle there was added ResX of duplicate bottom circle points with diffrent normals (dirceted down)
            //making bottom surface
            for (int i = 0; i < ResX; i++)
            {
                if (i == ResX - 1)  //dopięcie
                {

                    ind.Add(baseCenter);
                    ind.Add(actual_bottom_ind);
                    ind.Add(baseCenter);
                }
                else
                {
                    ind.Add(actual_bottom_ind + 1);
                    ind.Add(bottom_ind);
                    ind.Add(actual_bottom_ind);


                }
                actual_bottom_ind++;
            }


            int actual_upper_ind = topCenter + 1;
            //making upper surface
            for (int i = 0; i < ResX; i++)
            {

                if (i == ResX - 1)  //dopięcie
                {
                    ind.Add(topCenter);
                    ind.Add(topCenter + 1);
                    ind.Add(actual_upper_ind);
                }
                else
                {
                    ind.Add(topCenter);
                    ind.Add(actual_upper_ind + 1);
                    ind.Add(actual_upper_ind);

                }
                actual_upper_ind++;
            }

            vertices = verts.ToArray();
            indices = ind.ToArray();
            //normals = norms.ToArray();
        }

        public void UpdateModelMatrix()
        {
            ModelMatrix = Matrix4.Identity;
            ModelMatrix = ModelMatrix * Matrix4.CreateScale(Scale);
            ModelMatrix = ModelMatrix * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rot[0])) * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rot[1])) * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rot[2]));
            ModelMatrix = ModelMatrix * Matrix4.CreateTranslation(Translation);
        }

        public void Render(Shader shader, Matrix4 View, Matrix4 Perspective,Vector3 cameraPos, Vector3 ObjectColor)
        {
            shader.Use();
            shader.SetMatrix4("model", ModelMatrix);
            shader.SetMatrix4("view", View);
            shader.SetMatrix4("projection", Perspective);
            shader.SetVec3("objectColor", ObjectColor);  //uniform vec3 objectColor;
            shader.SetVec3("viewPos", cameraPos);
            GL.BindVertexArray(VertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, IndicesCount, DrawElementsType.UnsignedInt, 0);    //PrimitiveType.Triangles
            //GL.BindVertexArray(0);

            // if spherical draw half of sphere 
            sphere.Render(shader,View,Perspective,cameraPos,ObjectColor);
        }
    }
}



//public void GenerateMesh()
//{
//    List<float> verts = new List<float>();
//    List<int> ind = new List<int>();
//    //List<float> norms = new List<float>();

//    float dalfa = (float)(2*Math.PI/ResX);

//    var BasicCircleVerts = new List<float>();
//    for(int i=0; i<=ResX;++i)
//    {
//        var angle = i*dalfa;
//        BasicCircleVerts.Add((float)Math.Cos(angle));
//        BasicCircleVerts.Add((float)Math.Sin(angle));
//        BasicCircleVerts.Add(0.0f);
//    }

//    //add lower circle and upper circle
//    for(int i=0; i<2;++i)
//    {
//        var h = i * Height;

//        for(int j=0; j<=ResX;j++)
//        {
//            var k = j * 3;
//            float x = BasicCircleVerts[k];
//            float y = BasicCircleVerts[k+1];
//            float z = BasicCircleVerts[k+2];

//            verts.Add(x*Radius);
//            verts.Add(h);
//            verts.Add(y*Radius);

//            //norms.Add(x);
//            //norms.Add(z);
//            //norms.Add(y);

//            verts.Add(x);
//            verts.Add(z);
//            verts.Add(y);
//        }
//    }

//    //verts.Count()/3 is the first next free index 
//    int baseCenter = verts.Count() / 6; //in next lines of code there is gonna be added new vert in the center of bottom circle
//    int topCenter = baseCenter + ResX+2; //after base point there is gonna be added again base circle of ResX points with new normals


//    for(int i=0; i<2;++i)
//    {
//        var h = i * Height;
//        float z = -1 + i * 2;

//        //add base center and in next step top center point
//        verts.Add(0.0f);
//        verts.Add(h);
//        verts.Add(0.0f);

//        //norms.Add(0.0f);
//        //norms.Add(z);
//        //norms.Add(0.0f);

//        verts.Add(0.0f);
//        verts.Add(z);
//        verts.Add(0.0f);


//        for (int j = 0; j <= ResX; j++)
//        {
//            var k = j * 3;
//            float x = BasicCircleVerts[k];
//            float y = BasicCircleVerts[k + 1];

//            verts.Add(x * Radius);
//            verts.Add(h);
//            verts.Add(y * Radius);

//            //norms.Add(x);
//            //norms.Add(z);
//            //norms.Add(y);

//            verts.Add(x);
//            verts.Add(z);
//            verts.Add(y);
//        }
//    }

//    int bottom_ind = 0;
//    int top_ind = ResX+1;

//    //making side surface triangles
//    for(int i=0; i<ResX; i++)
//    {
//        //first triangle

//        ind.Add(top_ind);
//        ind.Add(bottom_ind + 1);
//        ind.Add(bottom_ind);

//        //second triangle
//        ind.Add(top_ind);
//        ind.Add(top_ind + 1);
//        ind.Add(bottom_ind + 1);


//        bottom_ind++;top_ind++;
//    }

//    int actual_bottom_ind = baseCenter + 1; //after adding center point in bottom circle there was added ResX of duplicate bottom circle points with diffrent normals (dirceted down)
//    //making bottom surface
//    for (int i = 0; i < ResX; i++)
//    {
//        if (i == ResX - 1)  //dopięcie
//        {
//            ind.Add(actual_bottom_ind);
//            ind.Add(baseCenter + 1);
//            ind.Add(baseCenter);
//        }
//        else
//        {
//            ind.Add(actual_bottom_ind + 1);
//            ind.Add(bottom_ind);
//            ind.Add(actual_bottom_ind);

//        }
//        actual_bottom_ind++;
//    }


//    int actual_upper_ind = topCenter + 1;
//    //making upper surface
//    for (int i = 0; i < ResX; i++)
//    {

//        if (i == ResX - 1)  //dopięcie
//        {
//            ind.Add(topCenter);
//            ind.Add(topCenter + 1);
//            ind.Add(actual_upper_ind);
//        }
//        else
//        {
//            ind.Add(topCenter);
//            ind.Add(actual_upper_ind + 1);
//            ind.Add(actual_upper_ind);

//        }
//        actual_upper_ind++;
//    }

//    vertices = verts.ToArray();
//    indices = ind.ToArray();
//    //normals = norms.ToArray();
//}