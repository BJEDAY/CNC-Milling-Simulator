using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Compute.OpenCL;
using System.Drawing;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using OpenTK.Mathematics;

namespace PUSN
{
    public class Texture : IDisposable
    {
        public int Handle;
        public TextureUnit Unit;

        public int sampler { get; set; }

        // default constructor to generate 2D texture with random bytes
        public Texture(int size, int unit)
        {
            Handle = GL.GenTexture();
            sampler = unit;
            Unit = TextureUnit.Texture0 + unit;
            Use();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, size, size, 0, PixelFormat.Rgba, PixelType.UnsignedByte, GenerateRandom2DBytes(size));
        }

        public Texture(int sizeX,int sizeY, int unit)
        {
            Handle = GL.GenTexture();
            sampler = unit;
            //sampler = GL.GenSampler();
            Unit = TextureUnit.Texture0 + unit;
            Use();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, sizeX, sizeY, 0, PixelFormat.Rgba, PixelType.UnsignedByte, GenerateRandom2DBytes(sizeX,sizeY));

            //GL.SamplerParameter(sampler,SamplerParameterName.TextureMinFilter,(int)TextureMinFilter.Nearest);
            //GL.SamplerParameter(sampler,SamplerParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            //GL.SamplerParameter(sampler,SamplerParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            //GL.SamplerParameter(sampler,SamplerParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        }

        public Texture(float[,] data, int unit)
        {
            Handle = GL.GenTexture();
            sampler = unit;
            Unit = TextureUnit.Texture0 + unit;
            Use();

            GL.TexImage2D(TextureTarget.Texture2D,
                0, PixelInternalFormat.R32f,
                data.GetLength(0)-1, data.GetLength(1)-1,
                0, PixelFormat.Red, PixelType.Float, data);

            
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
            //GL.SamplerParameter(sampler, SamplerParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        
            //GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        public void UpdateTexture(float[,] data)
        {
            Use();
            GL.TexImage2D(TextureTarget.Texture2D,
                0, PixelInternalFormat.R32f,
                data.GetLength(0) - 1, data.GetLength(1) - 1,
                0, PixelFormat.Red, PixelType.Float, data);
        }

        public Texture(Vector3[,] data, int unit)
        {
            Handle = GL.GenTexture();
            sampler = unit;
            Unit = TextureUnit.Texture0 + unit;
            Use();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, data.GetLength(0), data.GetLength(1), 0, PixelFormat.Rgb, PixelType.Float, data);
        }

        public void Use()
        {
            GL.ActiveTexture(Unit);
            //GL.BindSampler((int)(Unit), sampler);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }

        public void ChangeUnit(TextureUnit unit)
        {
            Unit = unit;
        }

        private byte[] GenerateRandom2DBytes(int size)
        {
            var rand = new Random();
            var res = new byte[4 * (int)Math.Pow(size, 2)];
            rand.NextBytes(res);
            return res;
        }

        private byte[] GenerateRandom2DBytes(int sizeX, int sizeY)
        {
            var rand = new Random();
            var res = new byte[4 * (int)(sizeX*sizeY)];
            rand.NextBytes(res);
            return res;
        }

        public void UpdateTexture(Vector3[,] data)
        {
            GL.ActiveTexture(Unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, data.GetLength(0), data.GetLength(1), 0, PixelFormat.Rgb, PixelType.Float, data);
        }

        public void Dispose()
        {
            GL.DeleteTexture(Handle);
            GL.DeleteSampler(sampler);
        }
    }
}


//public int ID { get; set; }
//public int sampler { get; set; }

//public Texture(float[,] data, int x, int y)
//{
//    ID = GL.GenTexture();
//    sampler = GL.GenSampler();
//    //use(0);
//    int ResX = x + 1;
//    int ResY = y + 1;

//    GL.TexImage2D(TextureTarget.Texture2D,
//        0,PixelInternalFormat.R32f,
//        ResX, ResY,
//        0, PixelFormat.Red, PixelType.Float,data);

//    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
//    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);

//    //GL.SamplerParameter(sampler, SamplerParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);


//    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
//    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

//    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

//}

//public Texture(float[] heights, int x, int y) //using 1D array of heights to create whole height texture (used inside terrain shader)
//{
//    ID = GL.GenTexture();
//    sampler = GL.GenSampler();
//    use(0);

//    //converting float array to bytes
//    var byteArray = new byte[heights.Length * 4];
//    //System.Buffer.BlockCopy(heights, 0, byteArray, 0, byteArray.Length);

//    for (int i = 0; i < byteArray.Length; i++)
//    {
//        byteArray[i] = (byte)255;
//    }

//    //var wynik = BitConverter.ToString(byteArray);
//    //var testing = byteArray[0..4];
//    //var wynik2 = BitConverter.ToSingle(byteArray[0..4],0);

//    ////string res = BitConverter.ToString(byteArray);
//    //int val = 125;
//    //var byte_val = BitConverter.GetBytes(val);
//    //string res2 = BitConverter.ToString(byte_val);

//    //float val2 = 125;
//    //var byte_val2 = BitConverter.GetBytes(val2);
//    //string res3 = BitConverter.ToString(byte_val2);

//    //double val2_again = BitConverter.ToDouble(byte_val2);
//    //byte testowanko = Convert.ToByte(val2);
//    //var testowanko2 = Convert.ToString(testowanko);
//    //var byte_test = (byte)val2;

//    ////new method of converting array of floats to bytes
//    //List<Byte> bit_heights = new List<Byte>();
//    //foreach(var elem in heights)
//    //{
//    //    var res = BitConverter.GetBytes(elem);
//    //    var test = BitConverter.ToString(res);
//    //    foreach(var b in res)
//    //    {
//    //        bit_heights.Add(b);
//    //    }
//    //}

//    //byteArray = bit_heights.ToArray();

//    //genereting new texture
//    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, x, y, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, byteArray);

//    GL.SamplerParameter(sampler, SamplerParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
//    GL.SamplerParameter(sampler,SamplerParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
//    GL.SamplerParameter(sampler, SamplerParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
//    GL.SamplerParameter(sampler,SamplerParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
//    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
//}

//public void use(int unit)
//{
//    GL.ActiveTexture(TextureUnit.Texture0+unit);
//    GL.BindTexture(TextureTarget.Texture2D, ID);
//    GL.BindSampler(unit, sampler);
//}

//public void Dispose()
//{
//    GL.DeleteTexture(ID);
//    GL.DeleteSampler(sampler);
//}

//public void update(float[] heights, int w, int h)
//{
//    GL.BindTexture(TextureTarget.Texture2D, ID);

//    //converting float array to bytes
//    var byteArray = new byte[heights.Length * 4];
//    System.Buffer.BlockCopy(heights, 0, byteArray, 0, byteArray.Length);

//    //updating sub region of texture starting at (0,0) with specific width and height
//    GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, w, h, OpenTK.Graphics.OpenGL4.PixelFormat.Red, PixelType.Float, byteArray);
//    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
//    GL.BindTexture(TextureTarget.Texture2D, 0); //current texture after updating is unbinded
//}

//public void update(float[] heights, int w, int h, int x, int y)
//{
//    GL.BindTexture(TextureTarget.Texture2D, ID);

//    //converting float array to bytes
//    var byteArray = new byte[heights.Length * 4];
//    System.Buffer.BlockCopy(heights, 0, byteArray, 0, byteArray.Length);

//    //updating sub region of texture starting at (0,0) with specific width and height
//    GL.TexSubImage2D(TextureTarget.Texture2D, 0, x, y, w, h, OpenTK.Graphics.OpenGL4.PixelFormat.Red, PixelType.Float, byteArray);
//    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
//    GL.BindTexture(TextureTarget.Texture2D, 0); //current texture after updating is unbinded
//}