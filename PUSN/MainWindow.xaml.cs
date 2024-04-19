﻿using OpenTK.Graphics.OpenGL4;
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
        List<ISceneModel> models;
        Camera camera;
        Shader shader, phongShader, terrainShader;
        ShaderGeometry thickLineShader, dotShader;
        Vector3 block_size, lightPos, lightColor, terrainColor, cutterColor;
        ViewPerspectiveSettings perspectiveSettings;
        System.Windows.Point prev_mouse;

        MillingTool tool;
        SceneModels.Line test_line;
        SceneModels.Dot test_dot;

        Terrain terrain;
        Cutter cutter;


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

        public MainWindow()
        {
            InitializeComponent();

            var settings = new GLWpfControlSettings
            {
                MajorVersion = 4,
                MinorVersion = 2
            };
            OpenTkControl.Start(settings);


            //Initialize light parameters
            lightColor = new Vector3(1f, 1f, 1f);
            lightPos = new Vector3(-10, -20, 20);
            //terrainColor = new Vector3(0.52f, 0.33f, 0.02f);
            terrainColor = new Vector3(0.32f, 0.55f, 0.52f);
            cutterColor = new Vector3(0.2f, 0.9f, 0.7f);

            SetupOpenGL();
            SetupShaders();
            //Initialize camera object
            camera = new Camera();
            perspectiveSettings = new ViewPerspectiveSettings(45.0f, 80.0f, 0.5f);
            camera.UpdateProjectionMatrix((float)OpenTkControl.ActualWidth, (float)OpenTkControl.ActualHeight, perspectiveSettings.fov, perspectiveSettings.n, perspectiveSettings.f);

            //Initialize model objects list
            models = new List<ISceneModel>();

            var cylinderek = new Cylinder();
            //cylinderek.Translation = new Vector3(0, -5, 0);
            cylinderek.UpdateModelMatrix();
            models.Add(cylinderek);

            models.Add(new Torus(10, 10, 7, 1));

            //Initialize block size
            block_size = new Vector3(300, 300, 50);   //x,y,z



            test_line = new SceneModels.Line(new Vector3(-10f, -10f, 0),new Vector3(10f, 10f,0),2f);
            test_line.UpdateModelMatrix(block_size.X/2.0f, block_size.Y / 2.0f, block_size.Z);

            test_dot = new SceneModels.Dot(new Vector3(0f, 0f, 0f), 2f);
            test_dot.UpdateModelMatrix(block_size.X / 2.0f, block_size.Y / 2.0f, block_size.Z);

            tool = new MillingTool(new Vector3(-125f, -50f, 0), new Vector3(-30f, 125f, 0), 25f, block_size);

            terrain = new Terrain(new Vector2(60, 60), new Vector2i(300, 300));

            cutter = new Cutter();
            cutter.Translation = new Vector3(0f, 0f,-1f);
            cutter.UpdateModelMatrix(); 
        }
        //

        private void SetupOpenGL()
        {
            GL.ClearDepth(0.0f);
            //GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Gequal);
            GL.Enable(EnableCap.Texture2D);
        }

        private void SetupShaders()
        {
            // instead of using path "Shaders/ShaderVerts.glsl and using option "copy to output directory" the path is given directly to the source of shaders (every change gonna be instant)
            // e.g. shader = new Shader("../../../Shaders/ShaderVert.glsl", "../../../Shaders/ShaderFrag.glsl");

            //Initialize shader
            shader = new Shader(ShaderVertLoc, ShaderFragLoc);
            thickLineShader = new ShaderGeometry(ShaderVertLocLine, ShaderFragLocLine, ShaderGeoLocLine);
            dotShader = new ShaderGeometry(ShaderVertLocDot, ShaderFragLocDot, ShaderGeoLocDot);
            phongShader = new Shader(ShaderVertLocPhong, ShaderFragLocPhong);
            terrainShader = new Shader(ShaderVertLocTerrain, ShaderFragLocTerrain);

            //phong shader light position and color are setup once and used globally for diffrent objects.
            //Althought viewPos is changed for all objects in every frame and objectColor every frame for every object using phong.
            phongShader.Use();
            phongShader.SetVec3("lightPos", lightPos);            //uniform vec3 lightPos;
            phongShader.SetVec3("lightColor", lightColor);        //uniform vec3 lightColor;

            terrainShader.Use();
            terrainShader.SetVec3("lightPos", lightPos);
            terrainShader.SetVec3("lightColor", lightColor);
        }
        private void OpenTkControl_OnRender(TimeSpan obj)
        {
                GL.ClearColor(Color4.Blue);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                foreach(var model in models)
                {
                    if(model is Cylinder)
                    {
                        //((Cylinder)model).Render(shader,camera.viewMatrix,camera.projectionMatrix);
                    }
                }
            //test_line.Draw(thickLineShader);
            //test_dot.Draw(dotShader); 
            //tool.Draw(dotShader, thickLineShader);
            //tool.Update(tool.start + new Vector3(0.01f, 0, 0), tool.end + new Vector3(0.01f, 0, 0), tool.Radius + 0.01f);
            //shader.SetVec3("viewPos", camera.);   //uniform vec3 viewPos;
            cutter.Render(phongShader, camera.viewMatrix, camera.projectionMatrix, camera.cameraPosition, cutterColor);
            terrain.Render(terrainShader, camera.viewMatrix, camera.projectionMatrix,camera.cameraPosition,terrainColor);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            camera.UpdateProjectionMatrix((float)OpenTkControl.ActualWidth, (float)OpenTkControl.ActualHeight, perspectiveSettings.fov, perspectiveSettings.n, perspectiveSettings.f);
        }

        private void OpenTkControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                double speed = 0.2;
                var pos = e.GetPosition(OpenTkControl);
                double deltaY = pos.Y - prev_mouse.Y;
                double deltaX = pos.X - prev_mouse.X;
                camera.UpdateRotation((float)(deltaY * speed), (float)(deltaX * speed));
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                double speed = 0.05;
                var pos = e.GetPosition(OpenTkControl);
                double deltaY = pos.Y - prev_mouse.Y;
                camera.ChangeDistance((float)(deltaY * speed));
            }

            prev_mouse = e.GetPosition(OpenTkControl);
        }

        private void OpenTkControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W)
            {
                camera.MoveCamera('W');
            }
            if (e.Key == Key.S)
            {
                camera.MoveCamera('S');
            }
            if (e.Key == Key.A)
            {
                camera.MoveCamera('A');
            }
            if (e.Key == Key.D)
            {
                camera.MoveCamera('D');
            }
            //if(e.Key == Key.E)
            //{
            //    tool.Update(tool.start + new Vector3(20f, 0, 0), tool.end + new Vector3(20f, 0, 0), tool.Radius + 0.01f);
            //}
        }
    }
}
