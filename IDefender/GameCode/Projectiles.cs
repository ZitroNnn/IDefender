using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IDefender
{
    class Trail : StaticGraphic
    {
        private bool isAlive = true;
        private float rotation;
        private float rotationSpeed;
        private float rotationDir = 1;
        private float fade = 1f;
        private float fadeOutSpeed;
        private float scale = 1f;
        private float scalingOverTime;
        private Color tint;

        public Trail(Texture2D _txr, Rectangle _rect, float _rotation, float _rotationSpeed, Color _tint, float _fadeOutSpeed = 8f, float _scalingOverTime = 10f) : base(_rect, _txr)
        {
            rotation = _rotation;
            rotationSpeed = _rotationSpeed;
            rotationDir = Game1.RNG.Next(0, 2) == 1 ? 1 : -1;
            tint = _tint;
            fadeOutSpeed = _fadeOutSpeed;
            scalingOverTime = _scalingOverTime;
        }

        public void UpdateMe(GameTime gt)
        {
            if (fade <= 0f) isAlive = false;
            fade -= fadeOutSpeed / 10 * (float)gt.ElapsedGameTime.TotalSeconds; // Make trail less visible over time
            scale += (scalingOverTime * (float)gt.ElapsedGameTime.TotalSeconds); // Changes size over time
            rotation += rotationDir * rotationSpeed * (float)gt.ElapsedGameTime.TotalSeconds; // Rotates trail
        }

        public void drawme(SpriteBatch sb)
        {
            sb.Draw(m_txr, new Vector2(m_rect.X, m_rect.Y), null, tint * fade, rotation, new Vector2(m_txr.Width / 2, m_txr.Height / 2), scale, SpriteEffects.None, 0);
        }

        public bool IsAlive
        {
            get { return isAlive; }
        }

    }

    class Projectile : Animated2D
    {
        // Projectile object
        private float speed = 12000f;
        private Vector2 direction;
        private float rotation;
        private Vector2 dir;
        private int collisionSize;
        private Color tint;

        private bool isPlayer = false;
        private bool isAlive = true;
        private float lifeTimer, lifeTime;
        private int damage;

        private bool hasTrail;
        private Color trailTint;
        private float trailRotationSpeed;
        private float trailFadeOutSpeed;
        private float trailScalingOverTime;

        private bool hasGravity;
        private float gravityStrength;

        public Projectile(Texture2D _txr, int _fps, Rectangle _rect, int _collisionSize, float _speed, float _rotation, Vector2 _dir, Color _tint, SpriteEffects fliped, bool _isPlayer, float _lifeTime, int _damage, Color _trailTint, bool _hasTrail = false, float _rotationSpeed = 1f, float _trailFadeOutSpeed = 8f, float _trailScalingOverTime = 10f, bool _hasGravity = false, float _gravityStrength = 10f) : base(_txr, _fps, _rect)
        {
            // Create and setup projectile object
            collisionSize = _collisionSize;
            speed = _speed;
            rotation = _rotation;
            dir = _dir;
            tint = _tint;
            if (dir != Vector2.Zero)
            {
                direction = dir;
            }
            else
            {
                // Calculate direction based on a rotation
                direction.X = (float)Math.Cos(rotation);
                direction.Y = (float)Math.Sin(rotation);
                if (fliped == SpriteEffects.None)
                {
                    direction.X *= -1;
                    direction.Y *= -1;
                    m_position.X -= 18f;
                }
            }

            isPlayer = _isPlayer;
            lifeTime = _lifeTime;
            damage = _damage;

            hasTrail = _hasTrail;
            trailTint = _trailTint;
            trailRotationSpeed = _rotationSpeed;
            trailFadeOutSpeed = _trailFadeOutSpeed;
            trailScalingOverTime = _trailScalingOverTime;

            hasGravity = _hasGravity;
            gravityStrength = _gravityStrength;
        }

        public override void UpdateMe(GameTime gt)
        {

            // Apply gravity movement
            if (hasGravity) direction.Y += gravityStrength * (float)gt.ElapsedGameTime.TotalSeconds;
            m_position += direction * speed * (float)gt.ElapsedGameTime.TotalSeconds;

            // Destroy object after some time
            lifeTimer += (float)gt.ElapsedGameTime.TotalSeconds;
            if (lifeTimer >= lifeTime)
            {
                isAlive = false;
            }

            // Update collision sphere for a projectile
            m_collisionSphere.Radius = collisionSize;
            m_collisionSphere.Center = new Vector3(m_position.X + m_txr.Width / 2 - collisionSize / 2, m_position.Y + m_txr.Width / 2 - collisionSize / 2, 0);
            
            base.UpdateMe(gt);

        }

        public void drawme(SpriteBatch sb)
        {
            // Draw projectile
            m_rect.X = (int)m_position.X;
            m_rect.Y = (int)m_position.Y;

            sb.Draw(m_txr, new Rectangle(Rect.X + m_txr.Width / 2, Rect.Y + m_txr.Height / 2, Rect.Width, Rect.Height), null, tint, rotation, new Vector2(m_txr.Width / 2, m_txr.Height / 2), SpriteEffects.None, 0);
        }

        // Access to some variables
        public bool IsPlayer
        {
            get { return isPlayer; }
        }
        public bool IsAlive
        {
            get { return isAlive; }
        }
        public float Rotation
        {
            get { return rotation; }
        }
        public bool HasTrail
        {
            get { return hasTrail; }
        }
        public Color TrailTint
        {
            get { return trailTint; }
        }
        public float TrailFadeOutSpeed
        {
            get { return trailFadeOutSpeed; }
        }
        public float TrailScalingOverTime
        {
            get { return trailScalingOverTime; }
        }

    }
    
}
