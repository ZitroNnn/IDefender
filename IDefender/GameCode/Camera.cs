using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace IDefender
{
    class Camera
    {
        // Create camera
        public Camera(Vector2 _pos, float _zoom)
        {
            position = _pos;
            zoom = _zoom;
        }

        private Vector2 position; // Camera Position
        private float zoom;

        // Update camera
        public void UpdateMe(GameTime gt, Rectangle _mapSize, Rectangle screenSize, Rectangle _target, GamePadState pad, GameState gameState)
        {
            if (gameState == GameState.PlayState && (pad.ThumbSticks.Right.X != 0 || pad.ThumbSticks.Right.Y != 0))
            {
                //position -= new Vector2(_target.X + (_target.Width / 2) - (screenSize.Width / 2), _target.Y + (_target.Height / 2) - (screenSize.Height / 2));
                position.X += (int)(pad.ThumbSticks.Right.X * 10);
                position.Y -= (int)(pad.ThumbSticks.Right.Y * 10);
            }

            // Calculate camera position
            position.X -= (position.X - (_target.X + _target.Width / 2 - screenSize.Width / 2)) * (float)gt.ElapsedGameTime.TotalSeconds;
            position.Y -= (position.Y - (_target.Y + _target.Height / 2 - screenSize.Height / 2)) * (float)gt.ElapsedGameTime.TotalSeconds;

            position.X = MathHelper.Clamp(position.X, 0, _mapSize.Width / 2);
            position.Y = MathHelper.Clamp(position.Y, 0, _mapSize.Height / 3);
        }

        public Matrix GetCam()
        {
            Matrix temp;
            temp = Matrix.CreateTranslation(new Vector3((int)-position.X, (int)-position.Y, 0));
            temp *= Matrix.CreateScale(zoom);
            return temp;
        }

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
    }
}
