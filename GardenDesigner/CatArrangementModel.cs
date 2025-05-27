namespace Szeminarium1_24_02_17_2
{
    internal class CatArrangementModel
    {
        /// <summary>
        /// Gets or sets wheather the animation should run or it should be frozen.
        /// </summary>
        public bool AnimationEnabeld { get; set; } = true;
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

        private bool TryMove(double newX, double newZ, bool[,] positionMatrix, float dist)
        {
            int xi = (int)Math.Round(newX + 20);
            int zi = (int)Math.Round(newZ + 20);

            if (xi >= dist && xi < positionMatrix.GetLength(0)-dist &&
                zi >= dist && zi < positionMatrix.GetLength(1)-dist &&
                positionMatrix[xi, zi])
            {
                positionX = newX;
                positionZ = newZ;
                return true;
            }
            return false;
        }


        internal void MoveForward(bool[,] positionMatrix, float dist)
        {
            double dx = MoveSpeed * Math.Sin(AngleY);
            double dz = MoveSpeed * Math.Cos(AngleY);
            if(!TryMove(positionX + dx, positionZ + dz, positionMatrix, dist))
            {
                RotateRight(5);
            }
        }
        internal void RotateRight(float yaw)
        {
            AngleY -= Sensitivity * (yaw / 90);
        }


        internal void AdvanceTime(double deltaTime, bool[,] positionMatrix, float dist)
        {
            // we do not advance the simulation when animation is stopped
            if (!AnimationEnabeld)
                return;

            // set a simulation time
            Time += deltaTime;
            MoveSpeed = deltaTime*5;
            MoveForward(positionMatrix, dist);

            // lets produce an oscillating scale in time
            CenterCubeScale = 1 + 0.2 * Math.Sin(1.5 * Time);

            DiamondCubeAngleOwnRevolution = Time * 10;

            DiamondCubeAngleRevolutionOnGlobalY = -Time;
        }
    }
}
