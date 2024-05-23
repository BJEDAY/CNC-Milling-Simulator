using OpenTK.Mathematics;
using PUSN.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUSN.SceneModels
{
    public class MillingTool
    {
        SceneModels.Dot s;
        Dot e;
        Line line;
        public float Radius;
        Vector3 blockSize;
        public Vector3 start;
        public Vector3 end;
        public bool Spherical;
        public int Sampler;
        public int ValTexSampler;
        public float MinHeight, CuttingPartHeight;
        public MillingTool(Vector3 start, Vector3 end, float r, Vector3 blockSize)
        {
            s = new SceneModels.Dot(start, r);
            e = new SceneModels.Dot(end, r);
            line = new SceneModels.Line(start, end, r);
            Radius = r;
            this.blockSize = blockSize; 
            this.start = start;
            this.end = end;
            MinHeight = 10;
            CuttingPartHeight = 40f;
            ValTexSampler = 2;
            s.UpdateModelMatrix(blockSize.X/2f, blockSize.Y/2f, blockSize.Z);       // left is x=-150 right is x=150 bottom is z=0 and top is z=50 so SizeX is 300 and SizeZ is 50. To get normalized coordinates the value in X (or Y) need to be dived by only half of size (so if X=150 we gonna get 150/(300/2)=1) and Z value diveded by all height value (so for 25 we gonna get 0.5)
            e.UpdateModelMatrix(blockSize.X/2f, blockSize.Y/2f, blockSize.Z);
            line.UpdateModelMatrix(blockSize.X/2f,blockSize.Y/2f, blockSize.Z);
            Spherical = true;
        }

        public void UpdateBlockSize(Vector3 block)
        {
            blockSize = block;
            s.UpdateModelMatrix(blockSize.X / 2f, blockSize.Y / 2f, blockSize.Z);       // left is x=-150 right is x=150 bottom is z=0 and top is z=50 so SizeX is 300 and SizeZ is 50. To get normalized coordinates the value in X (or Y) need to be dived by only half of size (so if X=150 we gonna get 150/(300/2)=1) and Z value diveded by all height value (so for 25 we gonna get 0.5)
            e.UpdateModelMatrix(blockSize.X / 2f, blockSize.Y / 2f, blockSize.Z);
            line.UpdateModelMatrix(blockSize.X / 2f, blockSize.Y / 2f, blockSize.Z);
        }

        public void Draw(ShaderGeometry dotShader, ShaderGeometry lineShader)
        {
            lineShader.Use();
            lineShader.SetInt("heights", Sampler);
            if(Spherical) lineShader.SetInt("Spherical", 1);
            else lineShader.SetInt("Spherical", 0);
            lineShader.SetInt("valTex", ValTexSampler);
            lineShader.SetFloat("MinHeight", MinHeight / blockSize.Z);
            lineShader.SetFloat("ToolCuttingHeight", CuttingPartHeight / blockSize.Z);
            line.Draw(lineShader);

            dotShader.Use();
            dotShader.SetInt("heights", Sampler);
            if (Spherical) dotShader.SetInt("Spherical", 1);
            else dotShader.SetInt("Spherical", 0);
            dotShader.SetInt("valTex", ValTexSampler);
            dotShader.SetFloat("MinHeight", MinHeight/blockSize.Z);
            dotShader.SetFloat("ToolCuttingHeight", CuttingPartHeight/blockSize.Z);
            //dotShader.SetInt("TestingInt", ValTexSampler);
            s.Draw(dotShader);
            e.Draw(dotShader);
        }

        public void Update(Vector3 start, Vector3 end, float r)
        {
            s.Update(start, r);
            e.Update(end, r);
            line.Update(start, end, r);
            this.start = start;
            this.end = end;
            this.Radius = r;
        }
    }
}
