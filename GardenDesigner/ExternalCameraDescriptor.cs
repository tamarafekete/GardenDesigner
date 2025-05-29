using Silk.NET.Input;
using Silk.NET.Maths;
using System;

namespace Szeminarium1_24_02_17_2
{
    internal class ExternalCameraDescriptor
    {
        public Vector3D<float> Target { get; set; } = new(0.6f, 1, 0);
        public float Distance { get; set; } = 4f;

        public float Pitch { get; private set; } = 0; // up/down
        public float Yaw { get; private set; } = 0f;    // left/right

        private Vector3D<float> position = new(0, 0, 0);
        private Vector3D<float> up = Vector3D<float>.UnitY;

        private const float Sensitivity = 0.5f;
        private const float MinDistance = 1f;
        private const float MaxDistance = 20f;
        private const float MoveSpeed = 0.5f;
        private Vector3D<float> front = -Vector3D<float>.UnitZ;
        private Vector3D<float> right = Vector3D<float>.UnitX;

        private GardenerArrangementModel gardener;

        public ExternalCameraDescriptor(GardenerArrangementModel model)
        {
            this.gardener = model;
        }
        public Matrix4X4<float> GetViewMatrix()
        {
            UpdatePosition();
            return Matrix4X4.CreateLookAt(position, Target, up);
        }

        public void MoveForward()
        {
            Target = new Vector3D<float> ( (float)gardener.positionX, 1f, (float)gardener.positionZ );


        }
        public void MoveBackward()
        {
            Target = new Vector3D<float>((float)gardener.positionX, 1f, (float)gardener.positionZ);
        }
        public void MoveLeft()
        {
            Target = new Vector3D<float>((float)gardener.positionX, 1f, (float)gardener.positionZ);
        }
        public void MoveRight()
        {
            Target = new Vector3D<float>((float)gardener.positionX, 1f, (float)gardener.positionZ);
        }

        private void UpdatePosition()
        {
            float pitchRad = MathF.PI / 180 * Pitch;
            float yawRad = MathF.PI / 180 * Yaw;

            position.X = Target.X + Distance * MathF.Cos(pitchRad) * MathF.Sin(yawRad);
            position.Y = Target.Y + Distance * MathF.Sin(pitchRad);
            position.Z = Target.Z + Distance * MathF.Cos(pitchRad) * MathF.Cos(yawRad);

            front = Vector3D.Normalize(Target - position); 
            right = Vector3D.Normalize(Vector3D.Cross(front, Vector3D<float>.UnitY)); 
            up = Vector3D.Normalize(Vector3D.Cross(right, front)); 
        }


        public void Rotate(float yawOffset, float pitchOffset)
        {
            Yaw += yawOffset * Sensitivity;
            Pitch += pitchOffset * Sensitivity;

            Pitch = Math.Clamp(Pitch, -89f, 89f);
        }

        public void Zoom(float zoomOffset)
        {
            Distance -= zoomOffset;
            Distance = Math.Clamp(Distance, MinDistance, MaxDistance);
        }
    }
}

