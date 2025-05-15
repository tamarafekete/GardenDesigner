using Silk.NET.Maths;

namespace Szeminarium1_24_02_17_2
{
    internal class CameraDescriptor
    {
        public Vector3D<float> Position { get; private set; } = new(0, 1, 3);
        public float Pitch { get; private set; } = 0; // Up/down
        public float Yaw { get; private set; } = -90; // Left/right (starts facing -Z)

        private Vector3D<float> front = -Vector3D<float>.UnitZ;
        private Vector3D<float> up = Vector3D<float>.UnitY;
        private Vector3D<float> right = Vector3D<float>.UnitX;
        private const float Sensitivity = 0.1f;
        private const float MoveSpeed = 0.1f;

        public Matrix4X4<float> GetViewMatrix()
        {
            return Matrix4X4.CreateLookAt(Position, Position + front, up);
        }

        public void MoveForward() => Position += front * MoveSpeed;
        public void MoveBackward() => Position -= front * MoveSpeed;
        public void MoveLeft() => Position -= right * MoveSpeed;
        public void MoveRight() => Position += right * MoveSpeed;
        public void MoveUp() => Position += up * MoveSpeed;
        public void MoveDown() => Position -= up * MoveSpeed;

        public void Rotate(float yawOffset, float pitchOffset)
        {
            Yaw += yawOffset * Sensitivity;
            Pitch += pitchOffset * Sensitivity;

            // Clamp pitch to avoid gimbal lock
            Pitch = Math.Clamp(Pitch, -89f, 89f);

            UpdateVectors();
        }

        private void UpdateVectors()
        {
            float yawRad = MathF.PI / 180 * Yaw;
            float pitchRad = MathF.PI / 180 * Pitch;

            front.X = MathF.Cos(yawRad) * MathF.Cos(pitchRad);
            front.Y = MathF.Sin(pitchRad);
            front.Z = MathF.Sin(yawRad) * MathF.Cos(pitchRad);
            front = Vector3D.Normalize(front);

            right = Vector3D.Normalize(Vector3D.Cross(front, Vector3D<float>.UnitY));
            up = Vector3D.Normalize(Vector3D.Cross(right, front));
        }
    }
}

