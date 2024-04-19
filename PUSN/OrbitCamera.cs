using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUSN
{
    public class OrbitCamera
    {
        public Matrix4 viewMatrix;
        public Matrix4 projectionMatrix;
        Vector3 lookAt;
        public Vector3 pos;
        float aspect, dist, angleX, angleY;
        public OrbitCamera()
        {
            dist = 3;
            aspect = 1;
            UpdateView();
            UpdateProj(1);
        }

        public void RotateX(float val)
        {
            angleX = (float)Math.Clamp(angleX + val, -Math.PI / 2 + 0.001f, Math.PI - 0.001f);
            UpdateView();
        }

        public void RotateY(float val)
        {
            angleY += val;
            UpdateView();
        }

        public void ChangeDist(float val)
        {
            Dist(dist * (1 + val));
        }

        public void Dist(float val)
        {
            dist = Math.Clamp(val, 0.05f, 100000.0f);
            UpdateView();
        }
        public void UpdateView()
        {
            var Eye = new Vector4(0, dist, 0, 1) * Matrix4.CreateRotationX(angleX) * Matrix4.CreateRotationZ(-angleY); //*Matrix4.CreateTranslation(lookAt);
            var LookAt = new Vector4(lookAt, 1);
            var Up = new Vector4(0, 0, 1, 0);
            pos = Eye.Xyz;
            viewMatrix = Matrix4.LookAt(Eye.Xyz, LookAt.Xyz, Up.Xyz);
        }

        public void UpdateProj(float aspect)
        {
            this.aspect = aspect;
            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60), aspect, 0.01f, 200.0f);
        }
    }
}
