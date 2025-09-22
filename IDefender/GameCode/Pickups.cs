using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace IDefender
{
    enum PickupType
    {
        MaxAmmo,
        ReloadSpeed,
        CurrentHealth,
        MaxHealth,
        MoveSpeed,
    }

    class Pickup : MotionGraphic
    {

        private PickupType pickupType;
        private bool isAlive = true;
        private float lifeTimer, lifeTime;

        public Pickup(Texture2D _txr, Rectangle _rect, PickupType _type, float _lifeTime) : base(_rect, _txr)
        {
            pickupType = _type;
            lifeTime = _lifeTime;
        }

        public override void UpdateMe(GameTime gt)
        {
            // Destroy game object after some time
            lifeTimer += (float)gt.ElapsedGameTime.TotalSeconds;
            if (lifeTimer >= lifeTime) isAlive = false;
        }

        public void drawme(SpriteBatch sb)
        {
            // Draw pickup object
            sb.Draw(Game1.pixel, new Rectangle(Rect.X, Rect.Y + Rect.Height, (int)(((lifeTime - lifeTimer) / lifeTime) * Rect.Width), 6), Color.White);
#if DEBUG
            sb.DrawString(Game1.font, $"{lifeTimer}/{lifeTime}", new Vector2(Rect.X, Rect.Y + Rect.Height + 20), Color.White);
#endif

            base.drawme(sb, Vector2.Zero, SpriteEffects.None, 0);
        }

        public PickupType PickupType
        {
            get { return pickupType; }
        }
        public bool IsAlive
        {
            get { return isAlive; }
            set { isAlive = value; }
        }
    }
}
