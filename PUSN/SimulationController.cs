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
        int PosNum;
        List<Vector3> positions;
        //MillingTool tool;     //that's already inside Terrain
        Terrain terrain; // reference to terreain object inside MainWindow
        Cutter cutter; // reference to cutter object inside MainWindow (it's a visualization of MillingTool)

        // Make sure another thread is made later that's gonna call Run() function (so when there is selected 1 second time beetween next two milling points all of the other program like camera or UI is not going to lag)
        ShaderGeometry line, dot;

        // TODO: Change inside Update inside Dot and Line so that the radius and spherical are setup once and then only positions are changed (need extra constructor)
        float CurrentRadius;
        bool Spherical;
        public int Wait;
        public SimulationController(ref Terrain ter, ref Cutter cut, ref ShaderGeometry line, ref ShaderGeometry dot)
        {
            positions = new List<Vector3>();
            PosNum = 0;
            terrain = ter;
            cutter = cut;
            this.line = line;
            this.dot = dot;
            Wait = 0;
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
        }

        public void Stop()
        {
            run = false;
            pause = false;
            stop = true;
            CurrentFrameIndex = 0;
        }

        public void Run()
        {
            if(run)
            {
                var s = positions[CurrentFrameIndex-1];
                var e = positions[CurrentFrameIndex];
                terrain.RenderToHeight(s,e,CurrentRadius,dot,line,Spherical);

                // TODO: Animate cutter
                //cutter.Translation = e;
                //cutter.UpdateVAO();
                Thread.Sleep(Wait);

                CurrentFrameIndex++;
                // jak tak się stanie to dotarliśmy do końca listy pozycji i się kończy działanie funkcji
                if (CurrentFrameIndex >= (PosNum - 1))
                {
                    run = false;
                    CurrentFrameIndex = 1;
                }
            }
        }
    }
}
