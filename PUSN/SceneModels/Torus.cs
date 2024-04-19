using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using PUSN.SceneModels;

namespace PUSN.Shapes
{
    public class Torus : ISceneModel
    {
       
        public Vector3 Rot;
        public Vector3 Translation;
        public Vector3 GroupTranslation;
        public Vector3 Scale;
        //public float Scale { get; set; }
        public Matrix4 ModelMatrix { get; set; }
        public int IndicesCount { get; set; }
        public int VertexArrayObject { get; set; }
        public int VertexBufferObject { get; set; }
        public int ElementBufferObject { get; set; }
        public string ObjectName { get; set; }
        public int ObjectID { get; set; }
        public bool IsSelected { get; set; }

        public Torus(int N_sampling, int n_sampling, float Radius, float rad)
        {
            ObjectID = -1;
            Rot = new Vector3(0, 0, 0);
            Translation = new Vector3(0, 0, 0);
            GroupTranslation = new Vector3(0, 0, 0);
            //Scale = 1;
            Scale = new Vector3(1, 1, 1);
            ModelMatrix = Matrix4.Identity;
            nR = N_sampling;
            nr = n_sampling;
            R = Radius;
            r = rad;
            IndicesCount = 0;
            IsSelected = true;
            ObjectName = "OriginalTorus";
            GenerateVAO();
        }
       
        public (float x, float y, float z) GetGroupTranslation()
        {
            return (GroupTranslation.X, GroupTranslation.Y, GroupTranslation.Z);
        }
        public void GenerateVAO() //generates Vertex Array Object used for drawing it on screen
        {
            var (torus_verts, torus_ind) = MakeTorusMesh();
            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);
            VertexBufferObject = GL.GenBuffer(); //tworzy bufor na karcie graficznej i daje uchwyt dla VertexBufferObject
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject); //przypisujemy bufor do danego typu
            GL.BufferData(BufferTarget.ArrayBuffer, torus_verts.Length * sizeof(float), torus_verts, BufferUsageHint.DynamicDraw); //copies the previously defined vertex data into the buffer's memory
            ElementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, torus_ind.Length * sizeof(uint), torus_ind, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            IndicesCount = torus_ind.Length;
        }

        public void UpdateVAO()
        {
            GL.BindVertexArray(VertexArrayObject);
            var (torus_verts, torus_ind) = MakeTorusMesh();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, torus_verts.Length * sizeof(float), torus_verts, BufferUsageHint.DynamicDraw); //copies the previously defined vertex data into the buffer's memory
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, torus_ind.Length * sizeof(uint), torus_ind, BufferUsageHint.DynamicDraw);
            IndicesCount = torus_ind.Length;
        }

        public int nR { get; set; } //big R sampling number
        public int nr { get; set; } //small r sampling number
        public float R { get; set; } //big radius
        public float r { get; set; } //small radius
        Vector3 ISceneModel.Rot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        Vector3 ISceneModel.Translation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        Vector3 ISceneModel.Scale { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        
        public void UpdateModelMatrix()
        {
            ModelMatrix = Matrix4.Identity;
            ModelMatrix = ModelMatrix * Matrix4.CreateScale(Scale[0], Scale[1], Scale[2]);
            ModelMatrix = ModelMatrix * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rot[0])) * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rot[1])) * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rot[2]));
            ModelMatrix = ModelMatrix * Matrix4.CreateTranslation(new Vector3(Translation[0] + GroupTranslation.X, Translation[1] + GroupTranslation.Y, Translation[2] + GroupTranslation.Z));
        }

        public void ChangeRotation(float val, int ind)
        {
            if (ind > 2 || ind < 0) return;
            Rot[ind] = val;
            UpdateModelMatrix();
        }

        public void ChangeTranslation(float val, int ind)
        {
            if (ind > 2 || ind < 0) return;
            Translation[ind] = val;
            UpdateModelMatrix();
        }
        public void ChangeGroupTranslation(float val, int ind)
        {
            if (ind > 2 || ind < 0) return;
            GroupTranslation[ind] = val;
            UpdateModelMatrix();
        }

        public void FinishGroupTranslation()
        {
            Translation.X += GroupTranslation.X; Translation.Y += GroupTranslation.Y; Translation.Z += GroupTranslation.Z;
            GroupTranslation.X = 0; GroupTranslation.Y = 0; GroupTranslation.Z = 0;
        }
        public void ChangeScale(float val, int ind)
        {
            if (ind > 2 || ind < 0) return;
            Scale[ind] = val;
            UpdateModelMatrix();
        }
        public (float[] vertices, int[] indices) MakeTorusMesh()
        {
            List<float> verts = new List<float>();
            List<int> indices = new List<int>();
            float d_alfa = (float)(2 * Math.PI / nR);
            float d_beta = (float)(2 * Math.PI / nr);

            for (int i = 0; i < nR; i++)
            {
                float alfa = (i % nR) * d_alfa;

                for (int j = 0; j < nr; j++)
                {
                    float beta = (j % nr) * d_beta;
                    float a = alfa;
                    float x = (float)((R + r * Math.Cos(beta)) * Math.Cos(a));
                    float y = (float)((R + r * Math.Cos(beta)) * Math.Sin(a));
                    float z = (float)(r * Math.Sin(beta));

                    verts.Add(x); verts.Add(y); verts.Add(z);

                    //vertcies making little circle 
                    if (j > 0)
                    {
                        indices.Add(j - 1 + i * nr);
                        indices.Add(j + i * nr);
                    }
                    if (j == nr - 1)
                    {
                        indices.Add(j + i * nr);
                        indices.Add(i * nr);
                    }
                    //vertcies making connection between little circles
                    if (i > 0)
                    {
                        indices.Add(i * nr + j);    //point from next little circle
                        indices.Add((i - 1) * nr + j);    //point from previous little circle
                    }
                }
            }
            //vertices connecting last circle with first circle
            var first_last_point = (nR - 1) * nr;
            for (int k = 0; k < nr; k++)
            {
                indices.Add(first_last_point);
                indices.Add(k);
                first_last_point += 1;
            }
            return (verts.ToArray(), indices.ToArray());
        }

        public (float x, float y, float z) GetTrans()
        {
            return (Translation[0], Translation[1], Translation[2]);
        }

        public (float x, float y, float z) GetRot()
        {
            return (Rot[0], Rot[1], Rot[2]);
        }

        public (float x, float y, float z) GetScale()
        {
            return (Scale[0], Scale[1], Scale[2]);
        }

    }
}
