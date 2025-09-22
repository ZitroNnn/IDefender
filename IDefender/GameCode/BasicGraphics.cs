using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.MediaFoundation;

namespace IDefender
{
    class StaticGraphic
    {
        // Basic static graphic object
        protected Rectangle m_rect;
        protected Texture2D m_txr;
        protected Color m_tint = Color.White;

        public StaticGraphic(Rectangle rectPosition, Texture2D txrImage)
        {
            m_rect = rectPosition;
            m_txr = txrImage;
        }

        public virtual void drawme(SpriteBatch sBatch, Vector2 rotationOrigin, SpriteEffects spriteEffects, int layerDepth)
        {
            sBatch.Draw(m_txr, m_rect, m_tint);
        }

        public Rectangle Rect
        {
            get { return m_rect; }
        }
    }

    abstract class MotionGraphic : StaticGraphic
    {
        // Motion graphic object
        protected Vector2 m_position;
        protected Vector2 m_velocity;
        protected BoundingSphere m_collisionSphere;
        protected int CSoffset;

        public MotionGraphic(Rectangle rect, Texture2D txr)
            : base(rect, txr)
        {
            m_position = new Vector2(rect.X, rect.Y);
            m_velocity = Vector2.Zero;

            // Calculatae collision sphere
            if (m_txr.Height >= m_txr.Width)
            {
                CSoffset = (m_txr.Height - m_txr.Width) / 2;
                m_collisionSphere = new BoundingSphere(new Vector3(m_position.X, m_position.Y + CSoffset, 0), txr.Width);
            }
            else if (m_txr.Height < m_txr.Width)
            {
                CSoffset = (m_txr.Width - m_txr.Height) / 2;
                m_collisionSphere = new BoundingSphere(new Vector3(m_position.X + CSoffset, m_position.Y, 0), txr.Height);
            }
        }

        public virtual void UpdateMe(GameTime gt)
        {
            // Update collision sphere
            if (m_txr.Height >= m_txr.Width)
            {
                m_collisionSphere.Center = new Vector3(m_position.X, m_position.Y + CSoffset, 0);
            }
            else if (m_txr.Height < m_txr.Width)
            {
                m_collisionSphere.Center = new Vector3(m_position.X + CSoffset, m_position.Y, 0);
            }
        }

        public override void drawme(SpriteBatch sBatch, Vector2 rotationOrigin, SpriteEffects spriteEffects, int layerDepth)
        {
            // Draw motion graphic object
            m_rect.X = (int)m_position.X;
            m_rect.Y = (int)m_position.Y;

            sBatch.Draw(m_txr, m_rect, m_tint);
        }

        public Vector2 Velocity
        {
            get { return m_velocity; }
            set { m_velocity = value; }
        }
        public BoundingSphere CollisionSphere
        {
            get { return m_collisionSphere; }
            set { m_collisionSphere = value; }
        }
    }

    abstract class Animated2D : MotionGraphic
    {
        // Animated graphic object
        protected float m_rotation;
        protected bool playAnim = true;
        protected Rectangle m_srcRect;
        protected float m_updateTrigger;
        protected int m_framesPerSecond;

        public Animated2D(Texture2D spriteSheet, int fps, Rectangle rect)
            : base(rect, spriteSheet)
        {
            m_srcRect = new Rectangle(0, 0, rect.Width, rect.Height);
            m_updateTrigger = 0;
            m_framesPerSecond = fps;

            m_position = new Vector2(rect.X, rect.Y);
            m_velocity = Vector2.Zero;
        }

        public override void UpdateMe(GameTime gt)
        {
            base.UpdateMe(gt);
            
            if (playAnim) m_updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * m_framesPerSecond;

            // Play animation
            if (m_updateTrigger >= 1)
            {
                m_updateTrigger = 0;
                m_srcRect.X += m_srcRect.Width;
                if (m_srcRect.X == m_txr.Width)
                    m_srcRect.X = 0;
            }
        }

        public override void drawme(SpriteBatch sBatch, Vector2 rotationOrigin, SpriteEffects spriteEffects, int layerDepth)
        {
            // Draw animatied object
            m_rect.X = (int)m_position.X;
            m_rect.Y = (int)m_position.Y;
            sBatch.Draw(m_txr, new Rectangle((int)(m_rect.X + rotationOrigin.X), (int)(m_rect.Y + rotationOrigin.Y), m_rect.Width, m_rect.Height), m_srcRect, m_tint, m_rotation, rotationOrigin, spriteEffects, layerDepth);
#if DEBUG
            sBatch.DrawString(Game1.font, $"{m_srcRect.X},{m_srcRect.Y},{m_srcRect.Width},{m_srcRect.Height} \n{m_velocity} \n{m_rotation}", m_position,Color.White);
#endif
        }
    }
}
