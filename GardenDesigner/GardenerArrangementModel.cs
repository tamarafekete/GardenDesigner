namespace Szeminarium1_24_02_17_2
{
    internal class GardenerArrangementModel
    {
        /// <summary>
        /// Gets or sets wheather the animation should run or it should be frozen.
        /// </summary>
        public bool AnimationEnabeld { get; set; } = false;
        public bool MoveForwardEnabled {  get; set; } = false;
        public bool MoveBackwardEnabled { get; set; } = false;
        public bool MoveRightEnabled { get; set; } = false;
        public bool MoveLeftEnabled { get; set; } = false;

        /// <summary>
        /// The time of the simulation. It helps to calculate time dependent values.
        /// </summary>
        private double Time { get; set; } = 0;

        /// <summary>
        /// The value by which the center cube is scaled. It varies between 0.8 and 1.2 with respect to the original size.
        /// </summary>
        public double CenterCubeScale { get; private set; } = 1;

        /// <summary>
        /// The angle with which the diamond cube is rotated around the diagonal from bottom right front to top left back.
        /// </summary>
        public double DiamondCubeAngleOwnRevolution { get; private set; } = 0;

        /// <summary>
        /// The angle with which the diamond cube is rotated around the diagonal from bottom right front to top left back.
        /// </summary>
        public double DiamondCubeAngleRevolutionOnGlobalY { get; private set; } = 0;

        public double positionX { get; set; } = 0;
        public double positionZ { get; set; } = 0;
        public double MoveSpeed = 0.5f;
        private const float Sensitivity = 0.75f;
        public double AngleY { get; set; } = Math.PI;

        private void TryMove(double newX, double newZ, bool[,] positionMatrix)
        {
            int xi = (int)Math.Round(newX + 20);
            int zi = (int)Math.Round(newZ + 20);

            if (xi >= 0 && xi < positionMatrix.GetLength(0) &&
                zi >= 0 && zi < positionMatrix.GetLength(1) &&
                positionMatrix[xi, zi])
            {
                positionX = newX;
                positionZ = newZ;
            }
        }


        internal void MoveForward(bool[,] positionMatrix)
        {
            double dx = MoveSpeed * Math.Sin(AngleY);
            double dz = MoveSpeed * Math.Cos(AngleY);
            TryMove(positionX + dx, positionZ + dz, positionMatrix);
        }

        internal void MoveBackward(bool[,] positionMatrix)
        {
            double dx = -MoveSpeed * Math.Sin(AngleY);
            double dz = -MoveSpeed * Math.Cos(AngleY);
            TryMove(positionX + dx, positionZ + dz, positionMatrix);
        }

        internal void MoveLeft(bool[,] positionMatrix)
        {
            double dx = MoveSpeed * Math.Sin(AngleY + Math.PI / 2);
            double dz = MoveSpeed * Math.Cos(AngleY + Math.PI / 2);
            TryMove(positionX + dx, positionZ + dz, positionMatrix);
        }

        internal void MoveRight(bool[,] positionMatrix)
        {
            double dx = MoveSpeed * Math.Sin(AngleY - Math.PI / 2);
            double dz = MoveSpeed * Math.Cos(AngleY - Math.PI / 2);
            TryMove(positionX + dx, positionZ + dz, positionMatrix);
        }


        internal void RotateLeft(float yaw)
        {
            AngleY += Sensitivity * (yaw / 90);
        }

        internal void RotateRight(float yaw)
        {
            AngleY -= Sensitivity * (yaw / 90);
        }

    }
}
