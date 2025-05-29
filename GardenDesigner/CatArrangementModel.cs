namespace Szeminarium1_24_02_17_2
{
    internal class CatArrangementModel
    {
        /// <summary>
        /// Gets or sets wheather the animation should run or it should be frozen.
        /// </summary>
        public bool AnimationEnabeld { get; set; } = true;

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

            MoveSpeed = deltaTime*5;
            MoveForward(positionMatrix, dist);
        }
    }
}
