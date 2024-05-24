using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using PUSN.Shaders;
using System.Transactions;
using System.Windows.Media.Media3D;
using System.Collections;
using System.Windows.Shapes;

namespace PUSN.SceneModels
{
    public class Terrain
    {
        Vector2 Size { get; set; }
        public Vector2i Res { get; set; }

        Matrix4 ModelMatrix;
        Matrix4 DisplayMatrix;
        public int VertexArrayObject { get; set; }
        public int VertexBufferObject { get; set; }
        public int ElementBufferObject { get; set; }
        public int IndicesCount { get; set; }
        public Vector3 Rot { get; set; }
        public Vector3 Translation { get; set; }
        public Vector3 Scale { get; set; }

        public Vector3 BlockSize { get; set; }

        float[] vertices; int[] indices; float[] normals;

        public float CurrentWindowWidth, CurrentWindowHeight;

        public Texture heightMap;
        private Texture tempMap;    
        private Texture depthMap;
        private Texture woodTex;
        public Texture valTex;
        public TextureViewer TexViewer;
        public ShaderGeometry dot, line;
        public bool showMaterial;

        // Trzeba to zassać z głownego okna
        public Shader TexShader;

        // TODO: create depth and temp texure
        // Framebuffer gonnna use depth to determinate if current pixel z is okey (so there is no need inside shaders)
        // All mill data are gonna be rendered to temp texture so for validation could be used info from heigtmap tex
        // Now when rendering at the same time to heightmap and reading values from it it can create some problems
        // It could be from that the heightmap is updated every frame, so when in one frame two operation on heightmap are performed heightmap don't have info about last line changes or dot

        public MillingTool tool;

        int FrameBufferHandle, FrameBufferHandle2;  // jedna robi render do Temp z uwzględnieniem Depth, a druga robi render do Height 

        float[] heights;
        float[,] intHeights;
        public Terrain(Vector2 s, Vector2i r)
        {
            //r = new Vector2i(r.X + 4, r.Y + 4);
            Size = s; Res = r; ModelMatrix = Matrix4.Identity; Rot = Vector3.Zero;  Scale = Vector3.One;
            //Translation = new Vector3(-s.X / 2, -s.Y / 2, 0);
            Translation = Vector3.Zero;
            vertices = new float[1];
            indices = new int[1];
            normals = new float[1];
            heights = new float[(Res.X+1)*(Res.Y+1)];
            intHeights = new float[Res.X+1,Res.Y+1];

            BlockSize = new Vector3(s.X, s.Y, 50);   //x,y,z
            //Size = new Vector2(300, 300);
            Size = s;

            tool = new MillingTool(new Vector3(-125f, -50f, 0), new Vector3(-30f, 125f, 0), 25f, BlockSize);
            DisplayMatrix = Matrix4.CreateScale(s.X, s.Y, 50);

            for (int i = 0; i <= Res.X; i++)
            {
                for (int j = 0; j <= Res.Y; j++)
                {
                    setHeight(i, j, 1.0f);
                }
            }

            for (int i = 0; i <= Res.X; i++)
            {
                for (int j = 0; j <= Res.Y; j++)
                {
                    intHeights[i, j] = 1f;
                }
            }

            heightMap = new Texture(heights, 0, Res.X, Res.Y);
            tempMap = new Texture(heights, 0, Res.X, Res.Y);
            depthMap = new Texture(Res,0);

            //woodTex = new Texture(1, "../../../Textures/wood3.jpeg");
            woodTex = new Texture(1, "../../../Textures/wood1.jpg");

            //valTex = new Texture(2, true);
            valTex = new Texture(2, false);

            tool.Sampler = heightMap.sampler;
            TexViewer = new TextureViewer();

            showMaterial = true;
            //heightMap = new Texture(Res.X+1,Res.Y+1,4);   
            GenerateFramebuffer();
            GenerateVAO();
            UpdateModelMatrix();
            SetupValidationTextureImage();
        }

        public void SetNewData(Vector3 Size,Vector2i Res)
        {
            //Res = new Vector2i(Res.X+4, Res.Y+4);   
            this.Size = Size.Xy; this.Res = Res; ModelMatrix = Matrix4.Identity; Rot = Vector3.Zero; Scale = Vector3.One;
            Translation = Vector3.Zero;
            vertices = new float[1];
            indices = new int[1];
            normals = new float[1];
            heights = new float[((int)Res.X + 1) * (Res.Y + 1)];
            BlockSize = new Vector3(Size);
            tool.UpdateBlockSize(BlockSize);
            DisplayMatrix = Matrix4.CreateScale(Size);

            for (int i = 0; i <= Res.X; i++)
            {
                for (int j = 0; j <= Res.Y; j++)
                {
                    setHeight(i, j, 1.0f);
                }
            }

            // jednak chyba nie da rady tak elegancko :( Jak sie zmiejszy rozdzialke na mniejsza to opengl szaleje

            heightMap.UpdateTextureData(heights, Res.X, Res.Y);
            tempMap.UpdateTextureData(heights,Res.X,Res.Y);
            depthMap.UpdateDepthData(Res.X, Res.Y);

            //heightMap = new Texture(heights, 0, Res.X, Res.Y);
            //tempMap = new Texture(heights, 0, Res.X, Res.Y);
            //depthMap = new Texture(Res, 0);

            GenerateFramebuffer();
            GenerateVAO();
            UpdateModelMatrix();
            ResetTexture();
        }

        public void ResetTexture()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferHandle);
            GL.ClearColor(1, 0, 0, 0);
            //GL.DepthMask(true);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferHandle2);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            RenderResetHeights();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
        private void GenerateFramebuffer()
        {
            // To jest frameBuffer który przeniesie milling product z GPU do tekstury temp (uwzględniając Depth)
            // TODO: Wyjeb liczenie 'z' w shaderze i zrób tutaj Depthbuffer i ustaw odpowiednio DepthFunc(DepthFunction.Less);
            FrameBufferHandle = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, FrameBufferHandle);
            //GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt,FramebufferAttachment.ColorAttachment0Ext,TextureTarget.Texture2D,heightMap.Handle,0);
            GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt,FramebufferAttachment.ColorAttachment0Ext,TextureTarget.Texture2D,tempMap.Handle,0);
            GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachmentExt, TextureTarget.Texture2D, depthMap.Handle, 0);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            // error check
            FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.FramebufferExt);
            if(status != FramebufferErrorCode.FramebufferComplete &&
                status != FramebufferErrorCode.FramebufferCompleteExt)
            {
                Console.WriteLine("Error creating framebuffer: {0}", status);
            }

            FrameBufferHandle2 = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, FrameBufferHandle2);
            GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, heightMap.Handle, 0);

            // error check
            status = GL.CheckFramebufferStatus(FramebufferTarget.FramebufferExt);
            if (status != FramebufferErrorCode.FramebufferComplete &&
                status != FramebufferErrorCode.FramebufferCompleteExt)
            {
                Console.WriteLine("Error creating framebuffer: {0}", status);
            }
        }

        public void RenderToHeight(Vector3 start, Vector3 end, float r, ShaderGeometry geo, ShaderGeometry line, bool Spherical)
        {
            // Step 1: Mill on GPU and render it to Temp texture
            tool.Update(start,end,r);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferHandle);
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, Res.X, Res.Y);
            //GL.BindTexture(TextureTarget.Texture2D, depthMap.Handle);
            //GL.Enable(EnableCap.DepthTest);
            //GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.DepthFunc(DepthFunction.Less);
            //GL.DepthFunc(DepthFunction.Always);
            tool.Spherical = Spherical;
            //SetupValidationTextureImage();
            tool.Draw(geo, line);
            GL.DepthFunc(DepthFunction.Less);
            // Step 2: Render Temp texture to Height texture
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferHandle2);
            TexViewer.Draw(TexShader, tempMap);     //FrameBufferHandle2 jest ustawiony na render do HeightMapy, ale używa TempMapy do wzięcia danych


            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, (int)CurrentWindowWidth, (int)CurrentWindowHeight);
        }

        public void RenderResetHeights()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferHandle);
            GL.Viewport(0,0, Res.X, Res.Y);
            GL.DepthFunc(DepthFunction.Less);
            TexViewer.DrawZeroEdges(TexShader, heightMap, Res.X, Res.Y);
            
            // ------ JUST TESTING -------
            //tool.Update(new Vector3(0,0,0), new Vector3(100,0,0), 50f);
            //tool.Spherical = true;
            //tool.Draw(dot, line);
            
            
            // przełożenie danych z Temp do głównej tekstury
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferHandle2);
            TexViewer.Draw(TexShader, tempMap);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, (int)CurrentWindowWidth, (int)CurrentWindowHeight);
        }

        public void SetupValidationTextureImage()
        {
            // 1 - make texture object
            // 2 - BindImageTexture to that object
            // 3 - inside shader get it from - layout(pixel type, binding = num) uniform image1D validationTexImage;
            // 4 - store data inside it with - imageStore(validationTexImage, texCoord, value);

            //valTex made in constructor of terrain
            GL.BindImageTexture(2, valTex.Handle, 0, true, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
            //valTex.Use1D();
            valTex.Use();
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
            //GL.MemoryBarrier(MemoryBarrierFlags.TextureUpdateBarrierBit);
        }

        public void UpdateParameters(Vector2 s, Vector2i r)
        {
            Size = s; Res = r;
            //GenerateVAO();
            //UpdateModelMatrix();
        }
        
        public void GenerateVerticesNormals()   //for now normals are kept inside vertices and used for phong shading
        {
            List<float> vertices = new List<float>();
            //List<float> normals = new List<float>();
            List<int> indices = new List<int>();

            List<float> testList = new List<float>();

            float dx = Size.X / Res.X;
            float dy = Size.Y / Res.Y;

            Size = Size + new Vector2(4*dx, 4*dy);
            dx = Size.X / Res.X;
            dy = Size.Y / Res.Y;

            float startX = -Size.X / 2;
            float startY = -Size.Y / 2;

            for (int j=0; j<=Res.Y; j++)  //we fill vertices with new rows from bottom to top
            {
                for(int i=0; i<=Res.X; i++)
                {
                    float x = startX + dx * i;
                    float y = startY + dy * j;
                    float z = 0.0f;

                    //just temporary for phong testing
                    //if(i==(int)Res.X/2) z= 1.0f;

                    // jak to jest dolna-górna lub prawa-lewa krawędź (czyli dla i=0, i=ResX, j=0 i j =ResY) to trzeba te wierzchołki wsunać "pod dywan"
                    if (i == 0) x += 2*dx;
                    if (i == Res.X) x -= 2*dx;
                    if (j == 0) y += 2*dy;
                    if (j == Res.Y) y -= 2*dy;

                    if (i == 1) x += dx;
                    if (i == Res.X-1) x -= dx;
                    if (j == 1) y += dy;
                    if (j == Res.Y-1) y -= dy;


                    vertices.Add(x); vertices.Add(y);vertices.Add(z);

                    
                    //normals.Add(0.0f); normals.Add(0.0f); normals.Add(1.0f);
                    vertices.Add(0.0f); vertices.Add(0.0f); vertices.Add(1.0f); //temporary normals 

                    vertices.Add((float)i / Res.X); vertices.Add((float)j / Res.Y);    //texture coord

                    // Testing purpose only
                    //testList.Add((float)i/Res.X); testList.Add((float)j / Res.Y);

                    //vertices.Add(1.0f); vertices.Add(1.0f);    //texture coord just to test if it reads anything
                }
            }
            //
            for(int j = 0; j<Res.Y;j++)
            {
                for(int i=0; i<Res.X;i++)
                {
                    //if(i==Res.X) { continue; }//last element of row
                    int actual_ind = i + j * (Res.X+1);
                    int ind_up = actual_ind + (Res.X+1);
                    int ind_r = actual_ind + 1;
                    int ind_up_r = ind_up + 1;

                    indices.Add(actual_ind);  indices.Add(ind_r); indices.Add(ind_up);//first triangle clockwise
                    indices.Add(ind_up);  indices.Add(ind_r); indices.Add(ind_up_r);//second triangle clockwise
                }
            }

            // Testing purpose only
            //int x12 = 5;

            this.vertices = vertices.ToArray();
            this.indices = indices.ToArray();
        }
        
        public void GenerateVertices()  //if normals are hold inside texture there is no need to put them inside vertices
        {

        }
        
        public void GenerateVAO()
        {
            GenerateVerticesNormals();

            VertexArrayObject = GL.GenVertexArray(); //VAO - to tak jakby etykieta naszego modelu.
            GL.BindVertexArray(VertexArrayObject);  //Bindujemy czyli mówimy, że teraz wszystkie kolejne działania będą podpięte pod to VAO właśnie.

            VertexBufferObject = GL.GenBuffer();    //Tworzymy uchwyt do bufora obiektu, czyli tutaj włożymy wierzchołki. Jako, że wcześniej podpiety został VAO to zawartość VBO jest skojarzona z tym VAO. VBO to trochę wnętrze VAO.
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);    //Bindujemy VBO po to, zeby teraz wgrać do niego zawartość. Operacja BufferData wykona się na aktualnie podpiętmy elemencie (czyli VBO).
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicCopy);    //Wgrywanie zawartości.

            ElementBufferObject = GL.GenBuffer();   //Uchwyt do bufora zawierającego informacje o kolejności łączenia wierzchołków z VBO.
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(float), indices, BufferUsageHint.DynamicCopy);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);   //określamy w jakiej formie podane są kolejne wierzchołki, w tym przypadku zaczynamy od indeksu 0 odczyt i przez 3 kolejne pozycje wyłuskujemy pozycje x y i z 
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3*sizeof(float));   //określamy w jakiej formie podane są kolejne wierzchołki, w tym przypadku zaczynamy od indeksu 0 odczyt i przez 3 kolejne pozycje wyłuskujemy pozycje x y i z 
            GL.EnableVertexAttribArray(1);

            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 *sizeof(float),6*sizeof(float));   //points to the tex coords
            GL.EnableVertexAttribArray(2);  

            IndicesCount = indices.Length;
        }

        public void DrawTextureViewer(Shader shader)
        {
            //TexViewer.Draw(shader, tempMap);
            //TexViewer.Draw(shader, heightMap);
            TexViewer.Draw(shader, depthMap);
        }

        public void DrawWood(Shader shader)
        {
            TexViewer.DrawEdgesNum(shader, woodTex,2);
        }
        public void UpdateVAO()
        {
            GenerateVerticesNormals();

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);    //nie musimy podpinać znów VAO, bo to VBO jest skojarzone z odpowiednim VAO (chyba)
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(float), indices, BufferUsageHint.DynamicDraw);

            IndicesCount = indices.Length;
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
            if(showMaterial)
            {
                shader.Use();
                //int unit = 0;
                heightMap.Use();
                woodTex.Use();
                shader.SetMatrix4("model", ModelMatrix);
                shader.SetMatrix4("display", DisplayMatrix);
                shader.SetMatrix4("view", View);
                shader.SetMatrix4("projection", Perspective);
                shader.SetVec3("objectColor", ObjectColor);  //uniform vec3 objectColor;
                shader.SetVec3("viewPos", cameraPos);
                shader.SetInt("heights", heightMap.sampler);
                shader.SetInt("colorTex", woodTex.sampler);
                GL.BindVertexArray(VertexArrayObject);
                GL.DrawElements(PrimitiveType.Triangles, IndicesCount, DrawElementsType.UnsignedInt, 0);    //PrimitiveType.Triangles
                GL.BindVertexArray(0);
            }
        }

        public void setHeight(int i, int j, float h)    //set height in i column and j row
        {
            int width = Res.X + 1;
            heights[i+j*width] = h;
        }

        public float getHeight(int i, int j)
        {
            int width = Res.X + 1;
            return heights[i + j * width];
        }
    }
}
