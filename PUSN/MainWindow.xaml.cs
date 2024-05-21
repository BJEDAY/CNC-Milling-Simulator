using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Wpf;
using PUSN.SceneModels;
using PUSN.Shaders;
using PUSN.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PUSN
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    struct ViewPerspectiveSettings 
    {
        public float fov, f, n;
        public ViewPerspectiveSettings(float Fov, float F, float N) { fov = Fov; f = F; n = N; }
    }

    public partial class MainWindow : Window
    {
        OrbitCamera camera;
        Shader shader, phongShader, terrainShader, texShader;
        ShaderGeometry thickLineShader, dotShader;
        Vector3 block_size, lightPos, lightColor, terrainColor, cutterColor;
        ViewPerspectiveSettings perspectiveSettings;
        System.Windows.Point prev_mouse;
        MillingTool tool;
        SimulationController simulationController;
        Terrain terrain;
        Cutter cutter;
        Sphere sphere;
        float currentH = 0;
        Vector2i currentRes;
        bool flat = false;
        bool instant = false;

        public const string ShaderVertLoc = "../../../Shaders/vert.glsl";
        //public const string ShaderFragLoc = "Shaders/frag.hlsl";  //Properties -> Copy if newer
        public const string ShaderFragLoc = "../../../Shaders/frag.glsl";  //Albo może lepiej podać od razu ściężkę bezpośrednio do źródełka??? :)

        //thick line geometry shader source
        public const string ShaderVertLocLine = "../../../Shaders/vertLine.glsl";
        public const string ShaderGeoLocLine = "../../../Shaders/geometryLine.glsl";
        public const string ShaderFragLocLine = "../../../Shaders/fragLine.glsl";

        //dot geometry shader source
        public const string ShaderVertLocDot = "../../../Shaders/vertDot.glsl";
        public const string ShaderGeoLocDot = "../../../Shaders/geometryDot.glsl";
        public const string ShaderFragLocDot = "../../../Shaders/fragDot.glsl";

        //phong basic shader
        public const string ShaderVertLocPhong = "../../../Shaders/phongVert.glsl";
        public const string ShaderFragLocPhong = "../../../Shaders/phongFrag.glsl";

        //terrain heightmap sampling shader
        public const string ShaderVertLocTerrain = "../../../Shaders/terrainVert.glsl";
        public const string ShaderFragLocTerrain = "../../../Shaders/terrainFrag.glsl";

        //texture viewer shader (gonna be used to fast render all data from tempMap to heightMap)
        public const string ShaderVertLocViewerTex = "../../../Shaders/TexViewerVert.glsl";
        public const string ShaderFragLocViewerTex = "../../../Shaders/TexViewerFrag.glsl";

        public MainWindow()
        {
            InitializeComponent();

            var settings = new GLWpfControlSettings
            {
                MajorVersion = 4,
                MinorVersion = 2
            };
            OpenTkControl.Start(settings);

            
            SetupOpenGL();
            SetupObjects();
            SetupCamera();
            SetupShaders();
            SetupSim();
        }
        //

        private void SetupOpenGL()
        {
            //GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
        }

        private void SetupObjects()
        {
            var cylinderek = new Cylinder();
            cylinderek.UpdateModelMatrix();

            block_size = new Vector3(150, 150, 50);   //x,y,z

            tool = new MillingTool(new Vector3(-125f, -50f, 0), new Vector3(-30f, 125f, 0), 25f, block_size);

            terrain = new Terrain(new Vector2(150, 150), new Vector2i(1500, 1500));     //dlatego, że size tutaj jest 5 razy mniejszy niż naprawdę (bo tam jest 300) to Radius jest dzielony przez 6 w shaderze
            terrain.CurrentWindowHeight = (int)OpenTkControl.ActualHeight;
            terrain.CurrentWindowWidth = (int)OpenTkControl.ActualWidth;
            currentRes = terrain.Res;


            cutter = new Cutter();
            cutter.SetPosition(new Vector3(-150, -150, 70));
            //cutter.UpdateModelMatrix();
            cutter.SetHeightRadius(7, 80);
            //cutter.SetRadius(7);

            //sphere = new Sphere();
            //sphere.Translation = new Vector3(15f, 0f, 5f);
            //sphere.UpdateModelMatrix();
            
        }

        private void SetupSim()
        {
            simulationController = new SimulationController(ref terrain, ref cutter, thickLineShader, dotShader);
            terrain.ResetTexture();
        }
        private void StartSimulationButton_Click(object sender, RoutedEventArgs e)
        {

            if (simulationController.PosNum > 1)
            {
                simulationController.Wait = (int)(SimSpeed.Value * 1000);
                simulationController.Start();
            }
            //simulationController.TestMill();
            //terrain.RenderToHeight(new Vector3(-120, -120, 34), new Vector3(-120, 120, 34), 16, dotShader, thickLineShader, true); 
            //var p = new SimulationController(ref terrain, ref cutter,  ref thickLineShader, dotShader);
            //p.TestMill();
            //p.Run2(ref terrain, ref dotShader, ref thickLineShader);

        }
        private void MillButton_Click(object sender, RoutedEventArgs e)
        {
            float sX = 0, sY = 0, sZ = 0;
            if (!float.TryParse(StartX.Text, out sX)) { Console.WriteLine("Wrong start X!"); }
            if (!float.TryParse(StartY.Text, out sY)) { Console.WriteLine("Wrong start Y!"); }
            if (!float.TryParse(StartZ.Text, out sZ)) { Console.WriteLine("Wrong start Z!"); }
            Vector3 start = new Vector3(sX, sY, sZ);

            float eX = 0, eY = 0, eZ = 0;
            if (!float.TryParse(EndX.Text, out eX)) { Console.WriteLine("Wrong end X!"); }
            if (!float.TryParse(EndY.Text, out eY)) { Console.WriteLine("Wrong end Y!"); }
            if (!float.TryParse(EndZ.Text, out eZ)) { Console.WriteLine("Wrong end Z!"); }
            Vector3 end = new Vector3(eX, eY, eZ);

            float r = 10f;
            if (!float.TryParse(Radius.Text, out r)) { Console.WriteLine("Wrong radius!"); }
            RenderToTexture(start, end, r);
            //RenderToTexture(new Vector3(0f, -170f, 0f), new Vector3(0f, 160f, 15f), 18f);
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            (var pos, int type, float r) = FileManager.Load();
            bool Spherical;
            if (type == 0) Spherical = true;
            else Spherical = false;
            simulationController.SetData(pos, r, Spherical);
            FlatCheck.IsChecked = !Spherical;
            ToolSize.Value = r;
        }

        private void FlatCheck_Click(object sender, RoutedEventArgs e)
        {
            flat = (bool)FlatCheck.IsChecked;
            cutter.spherical = !flat;
        }

        private void SetMaterial_Click(object sender, RoutedEventArgs e)
        {
            Vector2i Res = new Vector2i(1000,1000);
            Vector3 Size = new Vector3(150, 150, 50);

            Res.X = (int)ResX.Value;
            Res.Y = (int)ResY.Value;
            Size.X = (float)SizeX.Value;
            Size.Y = (float)SizeY.Value;
            Size.Z = (float)SizeZ.Value;

            terrain.SetNewData(Size, Res);
            currentRes = Res;
            terrainShader.Use();
            terrainShader.SetInt("currentResX", currentRes.X);
            terrainShader.SetInt("currentResY", currentRes.Y);
        }

        private void SetTool_Click(object sender, RoutedEventArgs e)
        {
            cutter.SetHeightRadius((float)ToolSize.Value, (float)ToolHeight.Value);
        }

        private void InstantSimulationButton_Click(object sender, RoutedEventArgs e)
        {
            simulationController.instant = true;
        }

        private void ResetMap_Click(object sender, RoutedEventArgs e)
        {
            terrain.ResetTexture();
        }

        private void SetupCamera()
        {
            //Initialize camera object
            camera = new OrbitCamera();
            perspectiveSettings = new ViewPerspectiveSettings(45.0f, 80.0f, 0.5f);
            //camera.UpdateProjectionMatrix((float)OpenTkControl.ActualWidth, (float)OpenTkControl.ActualHeight, perspectiveSettings.fov, perspectiveSettings.n, perspectiveSettings.f);
            camera.UpdateProj((float)OpenTkControl.ActualWidth / (float)OpenTkControl.ActualHeight);
        }

        private void SetupShaders()
        {
            //Initialize light parameters
            lightColor = new Vector3(1f, 1f, 1f);
            lightPos = new Vector3(-150, -200, 170);
            //terrainColor = new Vector3(0.52f, 0.33f, 0.02f);
            terrainColor = new Vector3(0.32f, 0.55f, 0.52f);
            cutterColor = new Vector3(0.2f, 0.9f, 0.7f);

            // instead of using path "Shaders/ShaderVerts.glsl and using option "copy to output directory" the path is given directly to the source of shaders (every change gonna be instant)
            // e.g. shader = new Shader("../../../Shaders/ShaderVert.glsl", "../../../Shaders/ShaderFrag.glsl");

            //Initialize shader
            shader = new Shader(ShaderVertLoc, ShaderFragLoc);
            thickLineShader = new ShaderGeometry(ShaderVertLocLine, ShaderFragLocLine, ShaderGeoLocLine);
            dotShader = new ShaderGeometry(ShaderVertLocDot, ShaderFragLocDot, ShaderGeoLocDot);

            terrain.line = thickLineShader;
            terrain.dot = dotShader;

            phongShader = new Shader(ShaderVertLocPhong, ShaderFragLocPhong);
            terrainShader = new Shader(ShaderVertLocTerrain, ShaderFragLocTerrain);
            texShader = new Shader(ShaderVertLocViewerTex, ShaderFragLocViewerTex);
            terrain.TexShader =texShader;

            //phong shader light position and color are setup once and used globally for diffrent objects.
            //Althought viewPos is changed for all objects in every frame and objectColor every frame for every object using phong.
            phongShader.Use();
            phongShader.SetVec3("lightPos", lightPos);            //uniform vec3 lightPos;
            phongShader.SetVec3("lightColor", lightColor);        //uniform vec3 lightColor;

            terrainShader.Use();
            terrainShader.SetVec3("lightPos", lightPos);
            terrainShader.SetVec3("lightColor", lightColor);
            terrainShader.SetInt("currentResX", currentRes.X);
            terrainShader.SetInt("currentResY", currentRes.Y);

            dotShader.SetInt("heights", terrain.heightMap.sampler);
            thickLineShader.SetInt("heights",terrain.heightMap.sampler);
        }
        private void OpenTkControl_OnRender(TimeSpan obj)
        {
            var currentTime = obj.TotalMilliseconds;
            GL.ClearColor(Color4.Blue);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //GL.DepthFunc(DepthFunction.Always);
            //sphere.Render(phongShader, camera.viewMatrix, camera.projectionMatrix, camera.pos, cutterColor);
            //sphere.RenderLines(shader,camera.viewMatrix,camera.projectionMatrix);
            cutter.Render(phongShader, camera.viewMatrix, camera.projectionMatrix, camera.pos, cutterColor);
            terrain.Render(terrainShader, camera.viewMatrix, camera.projectionMatrix, camera.pos, terrainColor);

            GL.Viewport(0, 0, (int)OpenTkControl.ActualWidth / 4, (int)OpenTkControl.ActualHeight / 4);
            terrain.DrawTextureViewer(texShader);
            GL.Viewport(0, 0, (int)OpenTkControl.ActualWidth, (int)OpenTkControl.ActualHeight);
            //tool.Draw(dotShader, thickLineShader);
            //tool.Update(tool.start + new Vector3(0.01f, 0, 0), tool.end + new Vector3(0.01f, 0, 0), tool.Radius + 0.01f);
            simulationController.Run((float)currentTime);
        }

        private void RenderToTexture(Vector3 start,  Vector3 end, float r)
        {
            terrain.RenderToHeight(start, end, r, dotShader, thickLineShader, !flat);
            //GL.Viewport(0, 0, (int)OpenTkControl.ActualWidth, (int)OpenTkControl.ActualHeight);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //camera.UpdateProjectionMatrix((float)OpenTkControl.ActualWidth, (float)OpenTkControl.ActualHeight, perspectiveSettings.fov, perspectiveSettings.n, perspectiveSettings.f);
            camera.UpdateProj((float)OpenTkControl.ActualWidth / (float)OpenTkControl.ActualHeight);
            terrain.CurrentWindowHeight = (int)OpenTkControl.ActualHeight;
            terrain.CurrentWindowWidth = (int)OpenTkControl.ActualWidth;
        }

        private void OpenTkControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                double speed = 0.002;
                var pos = e.GetPosition(OpenTkControl);
                double deltaY = pos.Y - prev_mouse.Y;
                double deltaX = pos.X - prev_mouse.X;
                //camera.UpdateRotation((float)(deltaY * speed), (float)(deltaX * speed));
                camera.RotateX((float)(deltaY * speed));
                camera.RotateY((float)(deltaX * speed));
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                double speed = 0.001;
                var pos = e.GetPosition(OpenTkControl);
                double deltaY = pos.Y - prev_mouse.Y;
                //camera.ChangeDistance((float)(deltaY * speed));
                camera.ChangeDist((float)(deltaY * speed));
            }

            prev_mouse = e.GetPosition(OpenTkControl);
        }

        private void OpenTkControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W)
            {
                //camera.MoveCamera('W');
            }
            if (e.Key == Key.S)
            {
                //camera.MoveCamera('S');
            }
            if (e.Key == Key.A)
            {
                //camera.MoveCamera('A');
            }
            if (e.Key == Key.D)
            {
                //camera.MoveCamera('D');
            }
            //if(e.Key == Key.E)
            //{
            //    tool.Update(tool.start + new Vector3(20f, 0, 0), tool.end + new Vector3(20f, 0, 0), tool.Radius + 0.01f);
            //}
        }
    }
}


//foreach(var model in models)
//{
//    if(model is Cylinder)
//    {
//        //((Cylinder)model).Render(shader,camera.viewMatrix,camera.projectionMatrix);
//    }
//}
//test_line.Draw(thickLineShader);
//test_dot.Draw(dotShader); 

//shader.SetVec3("viewPos", camera.);   //uniform vec3 viewPos;


//test_line = new SceneModels.Line(new Vector3(-10f, -10f, 0), new Vector3(10f, 10f, 0), 2f);
//test_line.UpdateModelMatrix(block_size.X / 2.0f, block_size.Y / 2.0f, block_size.Z);

//test_dot = new SceneModels.Dot(new Vector3(0f, 0f, 0f), 2f);
//test_dot.UpdateModelMatrix(block_size.X / 2.0f, block_size.Y / 2.0f, block_size.Z);