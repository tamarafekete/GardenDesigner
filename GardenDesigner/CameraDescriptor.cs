using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Szeminarium1_24_02_17_2
{
    internal class CameraDescriptor
    {
        public Vector3D<float> Position { get; private set; } = new(0, 1, 0);
        public float Pitch { get; private set; } = 0; // Up/down
        public float Yaw { get; private set; } = -90; // Left/right (starts facing -Z)

        private Vector3D<float> front = -Vector3D<float>.UnitZ;
        private Vector3D<float> up = Vector3D<float>.UnitY;
        private Vector3D<float> right = Vector3D<float>.UnitX;
        private const float Sensitivity = 0.5f;
        private const float MoveSpeed = 0.5f;
        private GardenerArrangementModel gardener;

        public CameraDescriptor(GardenerArrangementModel model)
        {
            this.gardener = model;
        }

        public Matrix4X4<float> GetViewMatrix()
        {
            return Matrix4X4.CreateLookAt(Position, Position + front, up);
        }

        public void MoveForward(bool[,] positionMatrix) {
            /*Vector3D<float> position = Position + front * MoveSpeed;
            int posX = (int)position.X+20;
            int posZ = (int)position.Z+20;
            if (positionMatrix[posX, posZ])
            {
                Position = position;
            }*/
            Position = new Vector3D<float>((float)gardener.positionX, 1, (float)gardener.positionZ);
            
        }
        public void MoveBackward(bool[,] positionMatrix) { 
            /*Vector3D<float> position = Position - front * MoveSpeed;
            int posX = (int)position.X + 20;
            int posZ = (int)position.Z + 20;
            if (positionMatrix[posX, posZ])
            {
                Position = position;
            }*/
            Position = new Vector3D<float>((float)gardener.positionX, 1, (float)gardener.positionZ);
        }
        public void MoveLeft(bool[,] positionMatrix) { 
           /* Vector3D<float> position = Position - right * MoveSpeed;
            int posX = (int)position.X + 20;
            int posZ = (int)position.Z + 20;
            if (positionMatrix[posX, posZ])
            {
                Position = position;
            }*/
            Position = new Vector3D<float>((float)gardener.positionX, 1, (float)gardener.positionZ);
        }
        public void MoveRight(bool[,] positionMatrix) { 
           /* Vector3D<float> position = Position + right * MoveSpeed;
            int posX = (int)position.X + 20;
            int posZ = (int)position.Z + 20;
            if (positionMatrix[posX, posZ])
            {
                Position = position;

            }*/
            Position = new Vector3D<float>((float)gardener.positionX, 1, (float)gardener.positionZ);
        }
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

