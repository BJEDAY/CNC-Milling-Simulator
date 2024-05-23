using OpenTK.Mathematics;
using PUSN.SceneModels;
using PUSN.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace PUSN
{
    public class SimulationController
    {
        public bool run, pause, stop;
        public int CurrentFrameIndex;
        public int PosNum;
        List<Vector3> positions;
        //MillingTool tool;     //that's already inside Terrain
        Terrain terrain; // reference to terreain object inside MainWindow
        Cutter cutter; // reference to cutter object inside MainWindow (it's a visualization of MillingTool)
        public bool instant;
        public Texture valTex;
        public Label ErrorLabel;

        // Make sure another thread is made later that's gonna call Run() function (so when there is selected 1 second time beetween next two milling points all of the other program like camera or UI is not going to lag)
        ShaderGeometry line, dot;

        // TODO: Change inside Update inside Dot and Line so that the radius and spherical are setup once and then only positions are changed (need extra constructor)
        float CurrentRadius;
        float TotalMS;
        bool Spherical;
        public int Wait;
        public SimulationController(ref Terrain ter, ref Cutter cut, ShaderGeometry l, ShaderGeometry d)
        {
            positions = new List<Vector3>();
            PosNum = 0;
            TotalMS = 0;
            terrain = ter;
            cutter = cut;
            line = l;
            dot = d;
            Wait =  100;
            run = false;
            pause = false;
            stop = false;
            instant = false;
        }

        public void SetData(List<Vector3> positions, float r, bool s) 
        {
            this.positions = positions;
            PosNum = this.positions.Count;
            CurrentRadius = r;
            Spherical = s;
            cutter.spherical = s;
            cutter.SetRadius(r);
            cutter.SetPosition(positions[0] + new Vector3(0, 0, CurrentRadius));
        }

        public void Start()
        {
            if(!pause) CurrentFrameIndex = 1;   //pos 0 is start
            run = true;
            pause = false;
            stop = false;
            cutter.SetPosition(positions[0]+new Vector3(0,0,CurrentRadius));
            cutter.SetRadius(CurrentRadius);
        }

        public void Stop()
        {
            run = false;
            pause = false;
            stop = true;
            CurrentFrameIndex = 0;
        }

        public void TestMill()
        {
            terrain.RenderToHeight(new Vector3(-120, -120, 34), new Vector3(-120, 120, 34), 16, dot, line, true);
        }

        public void Run2(ref Terrain ter2, ref ShaderGeometry dotSh, ref ShaderGeometry thickShader)
        {
            terrain.RenderToHeight(new Vector3(-120, -120, 34), new Vector3(-120, 120, 34), 16, dot, thickShader, true);
        }

        public void Run(float currentMS)
        {
            if(run)
            {
                if(instant)
                {
                    while(instant)
                    {
                        var s = positions[CurrentFrameIndex - 1];
                        var e = positions[CurrentFrameIndex];
                        terrain.RenderToHeight(s, e, CurrentRadius, dot, line, Spherical);

                        if (cutter.spherical) cutter.SetPosition(e + new Vector3(0, 0, CurrentRadius));
                        else cutter.SetPosition(e);

                        CurrentFrameIndex++;

                        if(CurrentFrameIndex >= (PosNum)) instant = false;
                    }
                }
                else
                {
                    TotalMS += currentMS;
                    if (TotalMS >= Wait)
                    {
                        var s = positions[CurrentFrameIndex - 1];
                        var e = positions[CurrentFrameIndex];
                        terrain.RenderToHeight(s, e, CurrentRadius, dot, line, Spherical);

                        if (cutter.spherical) cutter.SetPosition(e + new Vector3(0, 0, CurrentRadius));
                        else cutter.SetPosition(e);

                        CurrentFrameIndex++;
                        TotalMS = 0;    //TotalMS zbiera czas trwania kolejnych klatek od czasu ostatniego ruchu, gdy ten czas przekroczy czas oczekiwania frezarka robi kolejny ruch i proces się powtarza od poczatku

                        GL.BindTexture(TextureTarget.Texture2D, valTex.Handle);
                        Vector4h[] pixels = new Vector4h[1 * 1];
                        GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.HalfFloat, pixels);
                        if (pixels[0] != new Vector4h(0, 0, 0, 0)) run = false;
                    }
                }
                    
                // jak tak się stanie to dotarliśmy do końca listy pozycji i się kończy działanie funkcji
                if (CurrentFrameIndex >= (PosNum) || run == false)
                {
                    run = false;
                    CurrentFrameIndex = 1;

                    GL.BindTexture(TextureTarget.Texture2D, valTex.Handle);
                    Vector4h[] pixels = new Vector4h[1*1];
                    GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.HalfFloat, pixels);
                    if (pixels[0].X > 0.0f)
                    {
                        ErrorLabel.Foreground = new SolidColorBrush(Colors.Red);
                        ErrorLabel.Content = "Error: Flat Vertical"; 
                    }

                    if (pixels[0].Y > 0.0f)
                    {
                        ErrorLabel.Foreground = new SolidColorBrush(Colors.Red);
                        ErrorLabel.Content = "Error: Non-Cutting Part";
                    }
                    if (pixels[0].Z > 0.0f)
                    {
                        ErrorLabel.Foreground = new SolidColorBrush(Colors.Red);
                        ErrorLabel.Content = "Error: Minimum Height";
                    }

                    //if (pixels[0].Y > 0.0f) MessageBox.Show("Error 2");
                    //if (pixels[0].Z > 0.0f) MessageBox.Show("Error 3");
                }
            }
        }
    }
}
