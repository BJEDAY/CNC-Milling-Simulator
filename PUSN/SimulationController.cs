using OpenTK.Mathematics;
using PUSN.SceneModels;
using PUSN.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            Wait =  1000;
            run = false;
            pause = false;
            stop = false;
        }

        public void SetData(List<Vector3> positions, float r, bool s) 
        {
            this.positions = positions;
            PosNum = this.positions.Count;
            CurrentRadius = r;
            Spherical = s;
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
                TotalMS += currentMS;
                if(TotalMS >= Wait)
                {
                    var s = positions[CurrentFrameIndex - 1];
                    var e = positions[CurrentFrameIndex];
                    terrain.RenderToHeight(s, e, CurrentRadius, dot, line, Spherical);

                    cutter.SetPosition(e + new Vector3(0, 0, CurrentRadius));

                    CurrentFrameIndex++;
                    TotalMS = 0;    //TotalMS zbiera czas trwania kolejnych klatek od czasu ostatniego ruchu, gdy ten czas przekroczy czas oczekiwania frezarka robi kolejny ruch i proces się powtarza od poczatku
                }
                
                // jak tak się stanie to dotarliśmy do końca listy pozycji i się kończy działanie funkcji
                if (CurrentFrameIndex >= (PosNum))
                {
                    run = false;
                    CurrentFrameIndex = 1;
                }
            }
        }
    }
}
