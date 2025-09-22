using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

namespace IDefender
{
    class Player : Animated2D
    {
        // Player object
        private float rotation;
        private Vector2 centreOfRotation;
        private SpriteEffects spriteEffect;

        private Texture2D offScreenIndiTxr;
        private Rectangle offScreenIndiRect;

        private Camera cam;

        private int healthCurrent, healthMax;
        private float speedCurrent, speedMax;
        private float shootTimer, shootRate;
        private int bulletCount, bulletLimit;
        private float reloadTimer, reloadSpeed;
        private bool canReload = true;
        private Texture2D projectileTxr;

        // Create player
        public Player(Rectangle _rect, Texture2D _txr, Texture2D _offScreenIndiTxr, Texture2D _projectileTxr) : base(_txr, 5, _rect)
        {
            centreOfRotation = new Vector2((_txr.Width / 3) / 2, _txr.Height / 2);
            spriteEffect = SpriteEffects.FlipHorizontally;
            offScreenIndiTxr = _offScreenIndiTxr;
            projectileTxr = _projectileTxr;
            m_velocity = new Vector2(10f, 0f);
        }

        // Update player
        public void UpdateMe(GameTime gt, GamePadState pad, Camera _cam)
        {

            cam = _cam;

            // Calculate valocity
            if (m_velocity.X < -50f || m_velocity.X > 50f || m_velocity.Y < -50f || m_velocity.Y > 50f) m_velocity.X *= 0.95f;
            m_velocity.Y *= 0.95f;

            m_velocity.X += speedCurrent * (pad.Triggers.Left + 1) * pad.ThumbSticks.Left.X;
            m_velocity.Y -= speedCurrent * pad.ThumbSticks.Left.Y;

            // Apply rotation
            rotation = m_velocity.Y / 6f * (float)gt.ElapsedGameTime.TotalSeconds;

            // Flip Texture
            if (m_velocity.X > 0)
            {
                // Looking right
                spriteEffect = SpriteEffects.FlipHorizontally;
            }
            else if (m_velocity.X < 0)
            {
                // Looking left
                spriteEffect = SpriteEffects.None;
                rotation *= -1;
            }

            // Apply movement
            m_position += m_velocity * (float)gt.ElapsedGameTime.TotalSeconds;

            if (pad.ThumbSticks.Left.X != 0 || pad.ThumbSticks.Left.Y != 0)
            {
                playAnim = true;
                if (m_srcRect.X == 0) m_srcRect.X += m_srcRect.Width;
            }
            else
            {
                playAnim = false;
                m_srcRect.X = 0;
            }

            #region [Shooting]
            // Manage shooting mechanic
            shootTimer += (float)gt.ElapsedGameTime.TotalSeconds;
            if (pad.Triggers.Right > 0f && bulletCount > 0)
            {
                canReload = false;
                if (shootTimer >= shootRate)
                {
                    // Spawn bullet
                    shootTimer = 0f;
                    bulletCount--;
                    Vector2 dir = m_velocity;
                    dir.Normalize();
                    Rectangle projectileRect = new Rectangle((int)(m_rect.X + m_rect.Width / 2 + dir.X * 20), (int)(m_rect.Y + m_rect.Height / 2 + dir.Y * 20), 10, 2);
                    Game1.instance.SpawnProjectile(projectileTxr, 1, projectileRect,
                                                   _collisionSize: 10, _speed: 2500f, _rotation: rotation, _dir: Vector2.Zero, _tint: Color.Purple, _fliped: spriteEffect,
                                                   _isPlayer: true, _lifeTime: 10f, _damage: 1,
                                                   Color.MediumPurple,
                                                   _hasTrail: true, _rotationSpeed: 10f, _trailFadeOutSpeed: 32, _trailScalingOverTime: 4,
                                                   _hasGravity: false, _gravityStrength: 1f);
                }
            }
            else canReload = true;

            if (bulletCount < bulletLimit && canReload)
            {
                // Reload players weapon
                reloadTimer += (float)gt.ElapsedGameTime.TotalSeconds;
                if (reloadTimer >= reloadSpeed)
                {
                    reloadTimer = 0f;
                    bulletCount = bulletLimit;
                }
            }
            else reloadTimer = 0f;

            #endregion

            base.UpdateMe(gt);
            // Calculate collision sphere
            int collisionSize = 30;
            m_collisionSphere.Radius = collisionSize;
            m_collisionSphere.Center.X -= -collisionSize / 2 + m_txr.Width / 3;
            m_collisionSphere.Center.Y -= -collisionSize / 2;
            // Clamp stat values
            bulletLimit = MathHelper.Clamp(bulletLimit, 1, 79);
            bulletCount = MathHelper.Clamp(bulletCount, 0, bulletLimit);
            reloadSpeed = MathHelper.Clamp(reloadSpeed, 0.2f, 10f);
            healthMax = MathHelper.Clamp(healthMax, 0, 20);
            healthCurrent = MathHelper.Clamp(healthCurrent, 0, healthMax);
            speedCurrent = MathHelper.Clamp(speedCurrent, 1f, speedMax);

        }

        public void ResetSettings()
        {
            // Reset player settings
            healthMax = 5;
            healthCurrent = healthMax;
            speedMax = 30f;
            speedCurrent = 20f;
            shootRate = 0.2f;
            bulletLimit = 4;
            bulletCount = bulletLimit;
            reloadSpeed = 2;
        }

        public void Drawme(SpriteBatch sb, Rectangle screenSize, Rectangle _topBarSize)
        {

            // Draw off screen indicator
            #region Offscreen Indicator
            if (cam != null)
            {
                offScreenIndiRect = new Rectangle((int)MathHelper.Clamp(m_rect.X,       cam.Position.X + (offScreenIndiTxr.Width / 2),           cam.Position.X + screenSize.Width - (offScreenIndiTxr.Width / 2)),
                                                  (int)MathHelper.Clamp(m_rect.Y,       cam.Position.Y + _topBarSize.Height + (offScreenIndiTxr.Height / 2),          cam.Position.Y + screenSize.Height - (offScreenIndiTxr.Height / 2)),
                                                  offScreenIndiTxr.Width, offScreenIndiTxr.Height);

                if (Rect.X < cam.Position.X || Rect.X > cam.Position.X + screenSize.Width || Rect.Y < cam.Position.Y + _topBarSize.Height || Rect.Y > cam.Position.Y + screenSize.Height)
                    sb.Draw(offScreenIndiTxr, offScreenIndiRect, null, Color.White, rotation, new Vector2(offScreenIndiTxr.Width / 2, offScreenIndiTxr.Height / 2), spriteEffect, 0);
            }
            #endregion
            // Apply rotation
            m_rotation = rotation;

            base.drawme(sb, centreOfRotation, spriteEffect, 0);
        }

        public void SetPosition(Vector2 pos)
        {
            // Function for quick position set
            m_position = pos;
        }

        public float Rotation
        {
            get { return rotation; }
        }
        public SpriteEffects SE
        {
            get { return spriteEffect; }
        }
        public int MaxHealth
        {
            get { return healthMax; }
            set { healthMax = value; }
        }
        public int CurrentHealth
        {
            get { return healthCurrent; }
            set { healthCurrent = value; }
        }
        public float MaxSpeed
        {
            get { return speedMax; }
            set { speedMax = value; }
        }
        public float CurrentSpeed
        {
            get { return speedCurrent; }
            set { speedCurrent = value; }
        }
        public int BulletCount
        {
            get { return bulletCount; }
            set { bulletCount = value; }
        }
        public int BulletLimit
        {
            get { return bulletLimit; }
            set { bulletLimit = value; }
        }
        public float ReloadSpeed
        {
            get { return reloadSpeed; }
            set { reloadSpeed = value; }
        }
        public float ReloadTimer
        {
            get { return reloadTimer; }
        }

    }

    // Possible behaviour states for an alien
    enum AlienState
    {
        Descending,
        Waiting,
        Escaping,
        Escaped,
        Attacking
    }

    abstract class Alien : Animated2D
    {
        // Base alien class
        protected AlienState state;

        private int health;
        protected float moveSpeed;
        protected int score;
        private Camera cam;

        private Texture2D offScreenIndiTxr;
        private Rectangle offScreenIndiRect;
        protected Color tint = Color.White;
        protected Rectangle mapSize;

        protected abstract void MovementPattern(GameTime gt);

        // Create alien object
        public Alien(Rectangle _rect, Texture2D _txr, Texture2D _offScreenIndiTxr, int _fps, int _health, float _moveSpeed, Rectangle _mapSize) : base(_txr, _fps, _rect)
        {
            health = _health;
            moveSpeed = _moveSpeed;
            offScreenIndiTxr = _offScreenIndiTxr;
            mapSize = _mapSize;
        }

        public virtual void updateme(GameTime gt, Camera _cam, Rectangle _player)
        {
            cam = _cam;

            base.UpdateMe(gt);
        }

        public virtual void drawme(SpriteBatch sb, Rectangle screenSize, Rectangle _topBarSize)
        {
            // Draw alien object
            m_rect.X = (int)m_position.X;
            m_rect.Y = (int)m_position.Y;

            // Draw off screen indicator
            if (cam != null)
            {
                offScreenIndiRect = new Rectangle((int)MathHelper.Clamp(m_rect.X, cam.Position.X + (offScreenIndiTxr.Width / 2), cam.Position.X + screenSize.Width - (offScreenIndiTxr.Width / 2)),
                                              (int)MathHelper.Clamp(m_rect.Y, cam.Position.Y + _topBarSize.Y + (offScreenIndiTxr.Height / 2), cam.Position.Y + screenSize.Height - (offScreenIndiTxr.Height / 2)),
                                              offScreenIndiTxr.Width, offScreenIndiTxr.Height);

                if (Rect.X + Rect.Width < cam.Position.X || Rect.X > cam.Position.X + screenSize.Width || Rect.Y + Rect.Height < cam.Position.Y + _topBarSize.Y || Rect.Y > cam.Position.Y + screenSize.Height)
                    sb.Draw(offScreenIndiTxr, offScreenIndiRect, null, tint, 0, new Vector2(offScreenIndiTxr.Width / 2, offScreenIndiTxr.Height / 2), SpriteEffects.None, 0);
            }

            base.drawme(sb, Vector2.Zero, SpriteEffects.None, 0);
        }

        public int Health
        {
            get { return health; }
            set { health = value; }
        }
        public int Score
        {
            get { return score; }
        }
        public AlienState State
        {
            get { return state; }
            set { state = value; }
        }
        public Color Color
        {
            get { return tint; }
        }

    }

    // Invader alien object
    class Invader : Alien
    {
        // Create invader alien
        public Invader(Rectangle _rect, Texture2D _txr, Texture2D _offScreenIndiTxr, int _fps, int _health, float _moveSpeed, Rectangle _mapSize) : base(_rect, _txr, _offScreenIndiTxr, _fps, _health, _moveSpeed, _mapSize)
        {
            state = AlienState.Descending;
            score = 100;
        }

        // Update invader
        public override void updateme(GameTime gt, Camera _cam, Rectangle _player)
        {
            if (state == AlienState.Waiting)
            {
                tint = Color.Orange;
            }
            else
            {
                // Call update function
                MovementPattern(gt);
            }

            base.updateme(gt, _cam, _player);
        }

        protected override void MovementPattern(GameTime gt)
        {
            // Apply movement
            if (state == AlienState.Descending)
            {
                // Descending movement
                if (m_position.Y < mapSize.Height - 100)
                {
                    tint = Color.Green;

                    m_velocity.X = MathF.Sin(m_position.Y / 10) * 100;
                    m_position.X += m_velocity.X * (float)gt.ElapsedGameTime.TotalSeconds;


                    m_velocity.Y = moveSpeed;
                    m_position.Y += m_velocity.Y * (float)gt.ElapsedGameTime.TotalSeconds;
                }
                else state = AlienState.Waiting;
                
            }
            else if (state == AlienState.Escaping)
            {
                // Escape movement
                if (m_position.Y > 0)
                {
                    tint = Color.Red;

                    m_velocity.X = MathF.Sin(m_position.Y / 10) * 100;
                    m_position.X += m_velocity.X * (float)gt.ElapsedGameTime.TotalSeconds;


                    m_velocity.Y = moveSpeed;
                    m_position.Y -= m_velocity.Y * (float)gt.ElapsedGameTime.TotalSeconds;
                }
                else state = AlienState.Escaped;
            }
        }
    }

    // Predator alien object
    class Predator : Alien 
    {

        private bool canAttack = true;
        private float attackTimer, attackRate;
        private int attackCount = 0;
        private Texture2D projectileTxr;
        private Rectangle screenSize;

        private Camera cam;
        private Rectangle player;

        bool reachedDestination = false;
        Vector2 destination;
        float waitTimer, waitTime;

        // Create predator alien
        public Predator(Rectangle _rect, Texture2D _txr, Texture2D _offScreenIndiTxr, int _fps, int _health, float _moveSpeed, float _attackRate, Texture2D _projectileTxr, Rectangle _mapSize, Rectangle _screenSize) : base(_rect, _txr, _offScreenIndiTxr, _fps, _health, _moveSpeed, _mapSize)
        {
            attackRate = _attackRate;
            projectileTxr = _projectileTxr;
            screenSize = _screenSize;
            destination = new Vector2(500, 700);
            waitTime = 0.3f;
            tint = Color.Gold;
            state = AlienState.Attacking;
            score = 1000;
        }

        // Update predator
        public override void updateme(GameTime gt, Camera _cam, Rectangle _player)
        {
            cam = _cam;
            player = _player;

            // Call update function
            MovementPattern(gt);

            // Call attack function
            if (m_position.X + m_txr.Width / 2 > cam.Position.X && m_position.X + m_txr.Width / 2 < cam.Position.X + screenSize.Width && m_position.Y + m_txr.Height / 2 > cam.Position.Y && m_position.Y + m_txr.Height / 2 < cam.Position.Y + screenSize.Height)
            {
                if (canAttack) AttackPattern(gt);
            }

            base.updateme(gt, _cam, player);
        }

        protected override void MovementPattern(GameTime gt)
        {
            // Apply movement
            m_rotation = m_velocity.X;
            m_rotation = MathHelper.Clamp(m_rotation, -0.4f, 0.4f);

            // Calculate direction
            Vector2 dir = destination - m_position;
            dir.Normalize();
            
            if (reachedDestination == true)
            {
                // When reached destination
                reachedDestination = false;
                waitTimer = 0f;
                m_velocity = Vector2.Zero;
                destination.X = Game1.RNG.Next((int)(cam.Position.X), (int)(cam.Position.X + screenSize.Width));
                destination.Y = Game1.RNG.Next((int)(cam.Position.Y + 200), (int)(cam.Position.Y + screenSize.Height));
            }
            else
            {
                // Move towards destination
                m_velocity = dir;
                
                if (m_position.X > destination.X - 10f && m_position.X < destination.X + 10f)
                {
                    // Wait some time before moving again
                    m_rotation = 0f;
                    waitTimer += (float)gt.ElapsedGameTime.TotalSeconds;
                    if (waitTimer >= waitTime) 
                        reachedDestination = true;
                }
                else
                {
                    // Move
                    m_position += m_velocity * moveSpeed * (float)gt.ElapsedGameTime.TotalSeconds;
                    if (m_position.X + m_txr.Width / 2 < cam.Position.X || m_position.X + m_txr.Width / 2 > cam.Position.X + screenSize.Width || m_position.Y + m_txr.Height / 2 < cam.Position.Y + 200 || m_position.Y + m_txr.Height / 2 > cam.Position.Y + screenSize.Height)
                        reachedDestination = true;
                }

            }

        }

        private void AttackPattern(GameTime gt)
        {
            // Apply attack pattern
            float projectileSpeed = 1200f;
            int projectileSize = 10;

            Vector2 finalDir;
            Vector2 targetPos = new Vector2(player.X, player.Y);
            targetPos.X = targetPos.X - Rect.X;
            targetPos.Y = targetPos.Y - Rect.Y;
            float angle = MathF.Atan2(targetPos.Y, targetPos.X);
            float rotation = angle;

            // Main attack timer
            attackTimer += (float)gt.ElapsedGameTime.TotalSeconds;
            if (attackTimer >= attackRate)
            {
                attackTimer = 0;
                if (attackCount < 3)
                {
                    // Spawn projectile
                    finalDir = new Vector2(MathF.Cos(rotation), MathF.Sin(rotation));
                    finalDir.Normalize();
                    Game1.instance.SpawnProjectile(projectileTxr, 1,
                                    new Rectangle((int)(m_position.X + m_txr.Width / 2 - projectileSize), (int)(m_position.Y + m_txr.Height / 2 - projectileSize), 32, 32),
                                    _collisionSize: projectileSize, _speed: projectileSpeed, rotation, _dir: finalDir,
                                    _tint: Color.Orange, SpriteEffects.None,
                                    _isPlayer: false, _lifeTime: 10f, _damage: 1,
                                    _trailTint: Color.Red,
                                    _hasTrail: true, _rotationSpeed: 10f, _trailFadeOutSpeed: 32, _trailScalingOverTime: 4);
                    attackCount++;
                }
                else
                {
                    //Spawn 3 projectiles
                    //Spawn first projectile
                    finalDir = new Vector2(MathF.Cos(rotation - 0.1f), MathF.Sin(rotation - 0.1f));
                    finalDir.Normalize();

                    Game1.instance.SpawnProjectile(projectileTxr, 1,
                                    new Rectangle((int)(m_position.X + m_txr.Width / 2 - projectileSize), (int)(m_position.Y + m_txr.Height / 2 - projectileSize), 32, 32),
                                    _collisionSize: projectileSize, _speed: projectileSpeed, rotation, _dir: finalDir,
                                    _tint: Color.Orange, SpriteEffects.None,
                                    _isPlayer: false, _lifeTime: 10f, _damage: 1,
                                    _trailTint: Color.Red,
                                    _hasTrail: true, _rotationSpeed: 10f, _trailFadeOutSpeed: 32, _trailScalingOverTime: 4);

                    // Spawn second projectile
                    finalDir = new Vector2(MathF.Cos(rotation), MathF.Sin(rotation));
                    finalDir.Normalize();

                    Game1.instance.SpawnProjectile(projectileTxr, 1,
                                    new Rectangle((int)(m_position.X + m_txr.Width / 2 - projectileSize), (int)(m_position.Y + m_txr.Height / 2 - projectileSize), 32, 32),
                                    _collisionSize: projectileSize, _speed: projectileSpeed, rotation, _dir: finalDir,
                                    _tint: Color.Orange, SpriteEffects.None,
                                    _isPlayer: false, _lifeTime: 10f, _damage: 1,
                                    _trailTint: Color.Red,
                                    _hasTrail: true, _rotationSpeed: 10f, _trailFadeOutSpeed: 32, _trailScalingOverTime: 4);

                    // Spawn third projectile
                    finalDir = new Vector2(MathF.Cos(rotation + 0.1f), MathF.Sin(rotation + 0.1f));
                    finalDir.Normalize();

                    Game1.instance.SpawnProjectile(projectileTxr, 1,
                                    new Rectangle((int)(m_position.X + m_txr.Width / 2 - projectileSize), (int)(m_position.Y + m_txr.Height / 2 - projectileSize), 32, 32),
                                    _collisionSize: projectileSize, _speed: projectileSpeed, rotation, _dir: finalDir,
                                    _tint: Color.Orange, SpriteEffects.None,
                                    _isPlayer: false, _lifeTime: 10f, _damage: 1,
                                    _trailTint: Color.Red,
                                    _hasTrail: true, _rotationSpeed: 10f, _trailFadeOutSpeed: 32, _trailScalingOverTime: 4);

                    attackCount = 0;
                }
            }
        }

        // Draw predator alien
        public override void drawme(SpriteBatch sb, Rectangle screenSize, Rectangle _topBarSize)
        {
            base.drawme(sb, new Vector2(m_txr.Width / 2, m_txr.Height / 2), SpriteEffects.None, 0);
#if DEBUG
            sb.DrawString(Game1.font, $"{reachedDestination}\n{destination}\n{destination - m_position}", m_position + new Vector2(0, 70), Color.White);
#endif
        }

    }

    // Human
    class Human : Animated2D
    {
        private SpriteEffects fliped;
        private Vector2 velocity;
        private float speed = 50f;

        private Rectangle mapSize;

        // Create human object
        public Human(Texture2D _txr, Rectangle _rect, int fps, SpriteEffects _fliped, Rectangle _mapSize) : base(_txr, fps, _rect)
        {
            velocity.X = speed;
            fliped = _fliped;
            if (fliped == SpriteEffects.FlipHorizontally)
            {
                velocity.X *= -1f;
            }
            mapSize = _mapSize;
        }

        // Update human
        public override void UpdateMe(GameTime gt)
        {
            // Change move direction after reaching map boarder
            if (m_position.X < 0 || m_position.X > mapSize.Width - 16)
            {
                velocity.X *= -1;
                fliped++;
                if (fliped == SpriteEffects.FlipVertically) fliped = 0;
            }

            // Apply movement
            m_position.X += velocity.X * (float)gt.ElapsedGameTime.TotalSeconds;

            base.UpdateMe(gt);
            m_collisionSphere.Center.X -= m_txr.Width / 3;
        }

        // Draw human
        public void drawme(SpriteBatch sb)
        {
            m_rect.X = (int)m_position.X;
            m_rect.Y = (int)m_position.Y;

            sb.Draw(m_txr, m_rect, m_srcRect, Color.White, 0, Vector2.Zero, fliped, 0);
        }
    }
}
