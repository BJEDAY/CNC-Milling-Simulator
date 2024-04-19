using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUSN.SceneModels
{
    public interface ISceneModel
    {
        public void GenerateVAO();
        public void UpdateVAO();
        string ObjectName { get; set; }
        int VertexArrayObject { get; }
        int IndicesCount { get; }
        bool IsSelected { get; set; }
        Matrix4 ModelMatrix { get; set; }
        int ObjectID { get; set; }
        public Vector3 Rot { get; set; }
        public Vector3 Translation { get; set; }
        public Vector3 Scale { get; set; }
    }
}
