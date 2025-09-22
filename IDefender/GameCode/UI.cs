using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace IDefender
{
    class UI : StaticGraphic
    {

        private SpriteBatch spriteBatch;

        private Rectangle screenSize;
        private Rectangle mapSize;
        private Rectangle topBarSize;
        private Rectangle minimapSize;
        private Texture2D minimapTxr;

        // Create UI element
        public UI(Rectangle rect, Texture2D txr, Rectangle _screenSize, Rectangle _mapSize, Rectangle _topBarSize, Rectangle _minimapSize, Texture2D _minimapTxr) : base(rect, txr)
        {
            screenSize = _screenSize;
            mapSize = _mapSize;
            topBarSize = _topBarSize;
            minimapSize = _minimapSize;
            minimapTxr = _minimapTxr;
        }

        public void DrawUI(SpriteBatch _spriteBatch, Player player)
        {

            spriteBatch = _spriteBatch;

            spriteBatch.Draw(Game1.pixel, new Rectangle(topBarSize.X, topBarSize.Y, (int)topBarSize.Width, (int)topBarSize.Height), Color.Black);
            spriteBatch.Draw(Game1.pixel, new Rectangle(topBarSize.X, topBarSize.Height, (int)topBarSize.Width, 1), Color.Gray);
            spriteBatch.Draw(Game1.pixel, new Rectangle(minimapSize.X - 1, 0, minimapSize.Width + 2, minimapSize.Height), Color.Gray);
            spriteBatch.Draw(Game1.pixel, new Rectangle(minimapSize.X, 0, minimapSize.Width, minimapSize.Height), Color.Black);

            int x = screenSize.Width - minimapSize.Width;
            // Top
            spriteBatch.Draw(Game1.pixel, new Rectangle(minimapSize.X, 1, screenSize.Width / 2, 1), Color.Gray);
            // Left
            spriteBatch.Draw(Game1.pixel, new Rectangle(minimapSize.X, 1, 1, minimapSize.Height - minimapSize.Height / 3), Color.Gray);
            // Right
            spriteBatch.Draw(Game1.pixel, new Rectangle(minimapSize.X * 2 + screenSize.Width, 1, 1, minimapSize.Height - minimapSize.Height / 3), Color.Gray);

            spriteBatch.Draw(minimapTxr, new Rectangle(minimapSize.X, minimapSize.Y, (int)minimapSize.Width, (int)minimapSize.Height), Color.White);

            // Draw player on a minimap
            if (player.Rect.X >= 0 && player.Rect.X <= mapSize.Width && player.Rect.Y <= mapSize.Height - 5)
                spriteBatch.Draw(Game1.pixel, new Rectangle((int)(player.Rect.X / 4) + minimapSize.X, (int)(player.Rect.Y / 10.5), 8, 3), Color.MediumPurple);
        }
        public void DrawMapAliens(List<Alien> aliens)
        {
            // Draw aliens on a minimap
            for (int i = 0; i < aliens.Count; i++)
            {
                if (aliens[i].Rect.X >= 0 && aliens[i].Rect.X <= mapSize.Width && aliens[i].Rect.Y <= mapSize.Height - 5)
                    spriteBatch.Draw(Game1.pixel, new Rectangle((aliens[i].Rect.X / 4) + minimapSize.X, (int)(aliens[i].Rect.Y / 10.5f) + 2, 3, 3), aliens[i].Color);
            }
        }
        public void DrawMapHumans(List<Human> humans)
        {
            // Draw humans on a minimap
            for (int i = 0; i < humans.Count; i++)
            {
                spriteBatch.Draw(Game1.pixel, new Rectangle((int)(humans[i].Rect.X / 4) + minimapSize.X, (int)((humans[i].Rect.Y / 10.5f) + 2), 3, 3), Color.White);
            }
        }
        public void DrawMapProjectiles(List<Projectile> projectiles)
        {
            // Draw projectiles on a minimap
            for (int i = 0; i < projectiles.Count; i++)
            {
                if (projectiles[i].Rect.X >= 0 && projectiles[i].Rect.X <= mapSize.Width && projectiles[i].Rect.Y <= mapSize.Height - 5) spriteBatch.Draw(Game1.pixel, new Rectangle((int)(projectiles[i].Rect.X / 4) + minimapSize.X, (int)(projectiles[i].Rect.Y / 10.5), 1, 1), projectiles[i].TrailTint);
            }
        }
        public void DrawMapPickups(List<Pickup> pickups)
        {
            // Draw pickups on a minimap
            for (int i = 0; i < pickups.Count; i++)
            {
                if (pickups[i].Rect.X >= 0 && pickups[i].Rect.X <= mapSize.Width && pickups[i].Rect.Y <= mapSize.Height - 5) spriteBatch.Draw(Game1.pixel, new Rectangle((int)(pickups[i].Rect.X / 4) + minimapSize.X, (int)(pickups[i].Rect.Y / 10.5), 1, 1), Color.White);
            }
        }

        public void DrawPlayerStats(Player player, Texture2D healthTxtTxr, Texture2D ammoTxtTxr)
        {
            // Draw player statistics
            // Draw reload bar
            spriteBatch.Draw(Game1.pixel, new Rectangle((int)minimapSize.X + minimapSize.Width + 74, (int)topBarSize.Height - 34, 64, (int)((player.ReloadTimer / player.ReloadSpeed) * 12)), Color.White);

            // Draw current bullet limit
            for (int i = 0; i < player.BulletLimit; i++)
            {
                spriteBatch.Draw(Game1.pixel, new Rectangle((int)minimapSize.X + minimapSize.Width + 8 + (6 * i), (int)topBarSize.Height - 16, 3, 9), Color.White * 0.3f);
                if (i < player.BulletCount) spriteBatch.Draw(Game1.pixel, new Rectangle((int)minimapSize.X + minimapSize.Width + 8 + (6 * i), (int)topBarSize.Height - 16, 3, 9), Color.White);
            }
            // Draw current player health
            for (int i = 0; i < player.MaxHealth; i++)
            {
                spriteBatch.Draw(Game1.pixel, new Rectangle((int)minimapSize.X + minimapSize.Width + 8 + (6 * i), (int)topBarSize.Height - 52, 3, 9), Color.White * 0.3f);
                if (i < player.CurrentHealth) spriteBatch.Draw(Game1.pixel, new Rectangle((int)minimapSize.X + minimapSize.Width + 8 + (6 * i), (int)topBarSize.Height - 52, 3, 9), Color.White);
            }

            // Draw icons
            spriteBatch.Draw(healthTxtTxr, new Rectangle((int)minimapSize.X + minimapSize.Width + 8, (int)topBarSize.Height - 72, 96, 16), Color.White);
            spriteBatch.Draw(ammoTxtTxr, new Rectangle((int)minimapSize.X + minimapSize.Width + 8, (int)topBarSize.Height - 36, 64, 16), Color.White);
        }

        public void DrawInvasionBar(int invasionCurrent, int invasionTotal, List<int> invInfo)
        {
            // Draw invasion bar and informations
            if (invasionCurrent > 0) spriteBatch.Draw(Game1.pixel, new Rectangle((int)minimapSize.X, 0, (invasionTotal / (minimapSize.Width) * invasionCurrent - 40), 16), Color.Green);
            for (int i = 0; i < invInfo.Count; i++)
            {
                if ((int)(20 * i - 1) < minimapSize.Height) spriteBatch.DrawString(Game1.font, $"+{invInfo[i]}", new Vector2((int)minimapSize.X, (int)minimapSize.Y + 20 * (i + 1)), Color.Green);
            }
        }

    }
}
