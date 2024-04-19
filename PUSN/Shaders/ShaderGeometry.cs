using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.IO;

namespace PUSN.Shaders
{
    public class ShaderGeometry            
    {
        int Handle;
        private readonly Dictionary<string, int> UniformLoc;
        public ShaderGeometry(string vertexPath, string fragmentPath, string geometryPath)
        {
            //uchwyty na shadery
            int VertexShader = 0, FragmentShader = 0, GeometryShader = 0;
            //zapisanie shadera do stringa
            string VertexShaderSource = File.ReadAllText(vertexPath);
            string FragmentShaderSource = File.ReadAllText(fragmentPath);
            string GeometryShaderSource = File.ReadAllText(geometryPath);
            //stworzenie shaderów i przypisanie do uchwytów + uzupełnienie zawartością
            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);
            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);
            GeometryShader = GL.CreateShader(ShaderType.GeometryShader);
            GL.ShaderSource(GeometryShader, GeometryShaderSource);

            //kompilacja shaderów i test czy działa
            GL.CompileShader(VertexShader);
            int success = 0;
            GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(VertexShader);
                Console.WriteLine(infoLog);
            }

            GL.CompileShader(FragmentShader);

            GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(FragmentShader);
                Console.WriteLine(infoLog);
            }

            GL.CompileShader(GeometryShader);

            GL.GetShader(GeometryShader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(GeometryShader);
                Console.WriteLine(infoLog);
            }

            //stworzenie programu
            Handle = GL.CreateProgram();


            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, GeometryShader);
            GL.AttachShader(Handle, FragmentShader);

            // Set the input type of the primitives we are going to feed the geometry shader, this should be the same as
            // the primitive type given to GL.Begin. If the types do not match a GL error will occur (todo: verify GL_INVALID_ENUM, on glBegin)
            GL.Ext.ProgramParameter(Handle, AssemblyProgramParameterArb.GeometryInputType, (int)BeginMode.Points);
            // Set the output type of the geometry shader. Becasue we input Lines we will output LineStrip(s).
            GL.Ext.ProgramParameter(Handle, AssemblyProgramParameterArb.GeometryOutputType, (int)BeginMode.LineStrip);

            // We must tell the shader program how much vertices the geometry shader will output (at most).
            // One simple way is to query the maximum and use that.
            // NOTE: Make sure that the number of vertices * sum(components of active varyings) does not
            // exceed MaxGeometryTotalOutputComponents.
            GL.Ext.ProgramParameter(Handle, AssemblyProgramParameterArb.GeometryVerticesOut, 50);

            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(Handle);
                Console.WriteLine(infoLog);
            }

            //he individual vertex and fragment shaders are useless now that they've been linked; 
            //the compiled data is copied to the shader program when you link it. 
            //You also don't need to have those individual shaders attached to the program; 
            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);
            GL.DetachShader(Handle, GeometryShader);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(GeometryShader);
            GL.DeleteShader(VertexShader);

            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            UniformLoc = new Dictionary<string, int>();

            for (var i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(Handle, i, out _, out _);
                var location = GL.GetUniformLocation(Handle, key);
                UniformLoc.Add(key, location);
            }
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }


        public void SetMatrix4(string name, Matrix4 matrix)
        {
            int location = GL.GetUniformLocation(Handle, name);

            GL.UniformMatrix4(location, true, ref matrix);
        }

        public void SetVerticesCount(int num)
        {
            GL.UseProgram(Handle);
            int location = GL.GetUniformLocation(Handle, "VertsCount");
            GL.Uniform1(location, num);
        }

        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(UniformLoc[name], data);
        }

        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(UniformLoc[name], data);
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(Handle);

                disposedValue = true;
            }
        }

        ~ShaderGeometry()       
        {
            GL.DeleteProgram(Handle);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); //Garbage Collector
        }
    }
}
