using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace IDefender
{
    // This controlls game states
    enum GameState
    {
        MainMenuState,
        PlayState,
        GameOverState
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private Vector2 resolution = new Vector2(1920, 1080);
        private Rectangle screenSize;
        private Rectangle mapSize;
        private Rectangle topBarSize;
        private Rectangle minimapSize;

        private GameState gameState = GameState.MainMenuState;

        public static Random RNG = new Random();

        // Controllers
        private GamePadState pad1_curr, pad1_old;

        private UI ui;

        private Camera cam;

        public static SpriteFont font;
        public static Texture2D pixel;

        // Textures
        private Texture2D titleTxr, beginTxr;
        private Texture2D healthTxtTxr, ammoTxtTxr;
        private Texture2D scoreTxr, highScoreTxr;

        private StaticGraphic background;

        // Lists for objects like projectiles and trails
        private List<Projectile> projectiles = new List<Projectile>();
        private List<Trail> trails = new List<Trail>();

        // All statistics in the game
        #region Statistics
        private float gameTimer;
        private int totalGameTime;
        private int score, highScore;
        private int aliensKilled;
        private List<int> table = new List<int>();
        private int invasionCurrent, invasionTotal;
        private List<int> invInfo = new List<int>();
        private float invInfoTimer, invInfoRate;
        #endregion

        // Player
        private bool godMode = false;
        private Player player;


        #region Humans
        private List<Human> humans = new List<Human>();
        private float humanTimer, humanRate;
        private int humanLimit;
        #endregion

        #region Aliens
        private bool spawnAliens = true;
        private List<Alien> aliens = new List<Alien>();

        private int alienLimit;

        // Timers for different alien types
        private float invaderTimer, invaderSpawnRate;
        private float predatorTimer, predatorSpawnRate;
        #endregion

        private List<Pickup> pickups = new List<Pickup>();

        public static Game1 instance;

        public Game1()
        {
            if (instance == null) instance = this;

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Application settings
            IsFixedTimeStep = false;
            graphics.PreferredBackBufferWidth = (int)resolution.X;
            graphics.PreferredBackBufferHeight = (int)resolution.Y;
            graphics.IsFullScreen = true;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            // Set screen and map size
            screenSize = graphics.GraphicsDevice.Viewport.Bounds;
            mapSize = new Rectangle(0, 0, screenSize.Width * 2, (int)(screenSize.Height * 1.5));
            // UI size
            topBarSize = new Rectangle(0, 0, screenSize.Width, 160);
            minimapSize = new Rectangle(screenSize.Width / 4, 0, screenSize.Width / 2, topBarSize.Height);

            ui = new UI(new Rectangle(), null, screenSize, mapSize, topBarSize, minimapSize, Content.Load<Texture2D>("Textures\\map2"));

            // Create camera
            cam = new Camera(new Vector2(mapSize.Width / 4, 0), 1f);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Load textures
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("Fonts\\debugFont");
            pixel = Content.Load<Texture2D>("Textures\\pixel");

            titleTxr = Content.Load<Texture2D>("Textures\\titleTxt");
            beginTxr = Content.Load<Texture2D>("Textures\\beginTxt");
            healthTxtTxr = Content.Load<Texture2D>("Textures\\healthTxt");
            ammoTxtTxr = Content.Load<Texture2D>("Textures\\ammoTxt");
            scoreTxr = Content.Load<Texture2D>("Textures\\scoreTxt");
            highScoreTxr = Content.Load<Texture2D>("Textures\\highScoreTxt");

            background = new StaticGraphic(new Rectangle(0, 0, mapSize.Width, mapSize.Height), Content.Load<Texture2D>("Textures\\map2"));

            // Spawn Player
            player = new Player(new Rectangle((int)screenSize.Width / 2, screenSize.Height / 2, 80, 64),
                Content.Load<Texture2D>("Textures\\playerShip"),
                Content.Load<Texture2D>("Textures\\arrow"),
                pixel);

            ResetSettings();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Update gamepad
            pad1_curr = GamePad.GetState(0);

            // Controll game states
            switch (gameState)
            {
                case GameState.MainMenuState:
                    UpdateMainMenu(gameTime);
                    break;
                case GameState.PlayState:
                    UpdatePlay(gameTime);
                    break;
                case GameState.GameOverState:
                    UpdateGameOver(gameTime);
                    break;
            }

            pad1_old = pad1_curr;

            base.Update(gameTime);
        }

        // Update main menu
        public void UpdateMainMenu(GameTime gameTime)
        {
            cam.UpdateMe(gameTime, mapSize, screenSize, new Rectangle(screenSize.Width, 0, 0, 0), pad1_curr, gameState);

            if (pad1_curr.Buttons.A == ButtonState.Pressed && pad1_old.Buttons.A != ButtonState.Pressed)
            {
                ResetSettings();

                gameState = GameState.PlayState;
            }
        }

        public void UpdatePlay(GameTime gameTime)
        {
            // Update camera
            cam.UpdateMe(gameTime, mapSize, screenSize, new Rectangle((int)(player.Rect.X + player.Velocity.X * 1.5f), (int)(player.Rect.Y + player.Velocity.Y), player.Rect.Width, player.Rect.Height), pad1_curr, gameState);
            //cam.UpdateMe(gameTime, mapSize, screenSize, new Rectangle((int)(player.Rect.X), (int)(player.Rect.Y), player.Rect.Width, player.Rect.Height), pad1_curr);
            
            // Update player
            player.UpdateMe(gameTime, pad1_curr, cam);

            gameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (gameTimer >= 1f)
            {
                gameTimer = 0f;
                totalGameTime++;
                invaderSpawnRate -= 0.01f;
                invaderSpawnRate = MathHelper.Clamp(invaderSpawnRate, 0.2f, 10f);
            }

#if DEBUG
            if (pad1_curr.DPad.Up == ButtonState.Pressed) invasionCurrent += 1;
            if (pad1_curr.DPad.Down == ButtonState.Pressed) invasionCurrent -= 1;
            if (pad1_curr.DPad.Left == ButtonState.Pressed && pad1_old.DPad.Left != ButtonState.Pressed) player.BulletLimit -= 1;
            if (pad1_curr.DPad.Right == ButtonState.Pressed && pad1_old.DPad.Right != ButtonState.Pressed) player.BulletLimit += 1;
#endif

            invasionCurrent = MathHelper.Clamp(invasionCurrent, 0, invasionTotal);

            // Show invasion bar increase amount
            if (invInfo.Count > 0)
            {
                invInfoTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (invInfoTimer >= invInfoRate)
                {
                    invInfoTimer = 0f;
                    invInfo.RemoveAt(0);
                }
            }

            #region [PLAYER]
            // Spawn smoke trail behind player
            if (player.Velocity.X >= 50f || player.Velocity.X <= -50f)
            {
                Vector2 dir = player.Velocity;
                dir.Normalize();
                trails.Add(new Trail(Content.Load<Texture2D>("Textures\\pixel"),
                                     new Rectangle((int)(player.Rect.X + player.Rect.Width / 2 - dir.X * 20), player.Rect.Y + player.Rect.Height / 2, 1, 1),
                                     player.Rotation, 20f, Color.Gray));
            }
            #endregion

            // Spawn aliens
            if (aliens.Count < alienLimit && spawnAliens)
            {
                // Spawn invader ships
                invaderTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (invaderTimer >= invaderSpawnRate)
                {
                    invaderTimer = 0f;
                    int x = RNG.Next(64, mapSize.Width - 64);
                    aliens.Add(new Invader(new Rectangle(x, 0, 110, 72),
                    Content.Load<Texture2D>("Textures\\alienShip2"), Content.Load<Texture2D>("Textures\\warning"),
                    1, 1, 64, mapSize));
                }
                // Spawn predator ships
                predatorTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (predatorTimer >= predatorSpawnRate)
                {
                    predatorTimer = 0f; // XDDD
                    int rnd = RNG.Next(0, 2);
                    int x = (rnd == 1) ? mapSize.X - 100 : mapSize.Width + 100;
                    aliens.Add(new Predator(new Rectangle(x, mapSize.Height / 2, 110, 70),
                    Content.Load<Texture2D>("Textures\\alienShip2"), Content.Load<Texture2D>("Textures\\warning"),
                    _fps: 1, _health: 3, _moveSpeed: 500f, _attackRate: 0.8f, Content.Load<Texture2D>("Textures\\predatorBullet"), mapSize, screenSize));
                }
            }

            // Update everything related to aliens
            for (int i = 0; i < aliens.Count; i++)
            {
                aliens[i].updateme(gameTime, cam, player.Rect);
                if (aliens[i].CollisionSphere.Intersects(player.CollisionSphere))
                {
                    score -= 10;
                    if (!godMode) player.CurrentHealth--;
                    aliens[i].Health--;
                }
                for (int j = 0; j < humans.Count; j++)
                {
                    if (aliens[i].State == AlienState.Waiting && aliens[i].Rect.Intersects(humans[j].Rect))
                    {
                        invasionCurrent += 10;
                        invInfo.Add(10);
                        aliens[i].State = AlienState.Escaping;
                        humans.RemoveAt(j);
                    }
                }
                if (aliens[i].State == AlienState.Escaped)
                {
                    // When alien escapes
                    invasionCurrent += 50;
                    invInfo.Add(50);
                    aliens.RemoveAt(i);
                }
                if (aliens[i].Health <= 0)
                {
                    // When alien is destroyed:
                    AlienState state = aliens[i].State;
                    switch (state)
                    {
                        case AlienState.Descending:
                            score += aliens[i].Score;
                            break;
                        case AlienState.Waiting:
                            score += aliens[i].Score * 2;
                            break;
                        case AlienState.Escaping:
                            score += aliens[i].Score * 3;
                            break;
                        case AlienState.Attacking:
                            score += aliens[i].Score;
                            break;
                    }

                    int pickupChance = RNG.Next(0, 20);
                    if (pickupChance == 0)
                    {
                        // Increase bullet limit
                        if (player.BulletLimit < 79)
                            pickups.Add(new Pickup(Content.Load<Texture2D>("Textures\\addMaxAmmo"), new Rectangle(aliens[i].Rect.X + aliens[i].Rect.Width / 2, aliens[i].Rect.Y + aliens[i].Rect.Height / 2, 48, 48),
                                           PickupType.MaxAmmo, 5f));

                    }
                    if (pickupChance == 1)
                    {
                        // Increase reload speed
                        pickups.Add(new Pickup(Content.Load<Texture2D>("Textures\\addReloadSpeed"), new Rectangle(aliens[i].Rect.X + aliens[i].Rect.Width / 2, aliens[i].Rect.Y + aliens[i].Rect.Height / 2, 48, 48),
                                           PickupType.ReloadSpeed, 5f));
                    }
                    if (pickupChance == 2)
                    {
                        // Icrease max health
                        pickups.Add(new Pickup(Content.Load<Texture2D>("Textures\\addMaxHealth"), new Rectangle(aliens[i].Rect.X + aliens[i].Rect.Width / 2, aliens[i].Rect.Y + aliens[i].Rect.Height / 2, 48, 48),
                                           PickupType.MaxHealth, 5f));
                    }
                    if (pickupChance == 3 && player.CurrentHealth < player.MaxHealth)
                    {
                        // Increase current health
                        pickups.Add(new Pickup(Content.Load<Texture2D>("Textures\\addHealth"), new Rectangle(aliens[i].Rect.X + aliens[i].Rect.Width / 2, aliens[i].Rect.Y + aliens[i].Rect.Height / 2, 48, 48),
                                           PickupType.CurrentHealth, 5f));
                    }
                    if (pickupChance == 4 && player.CurrentSpeed < player.MaxSpeed)
                    {

                        // Increase move speed
                        pickups.Add(new Pickup(Content.Load<Texture2D>("Textures\\addHealth"), new Rectangle(aliens[i].Rect.X + aliens[i].Rect.Width / 2, aliens[i].Rect.Y + aliens[i].Rect.Height / 2, 48, 48),
                                           PickupType.MoveSpeed, 5f));
                    }

                    // Remove alien
                    aliens.RemoveAt(i);
                    aliensKilled++;
                }
            }

            // Spawn humans
            if (humans.Count < humanLimit)
            {
                humanTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (humanTimer >= humanRate)
                {
                    humanTimer = 0f;
                    SpriteEffects se = RNG.Next(0, 2) == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                    humans.Add(new Human(Content.Load<Texture2D>("Textures\\human"), new Rectangle(RNG.Next(100, mapSize.Width - 100), mapSize.Height - 40, 16, 16), 16, se, mapSize));
                }
            }
            // Update humans
            for (int i = 0; i < humans.Count; i++)
            {
                humans[i].UpdateMe(gameTime);
            }
            // Update projectiles
            for (int i = 0; i < projectiles.Count; i++)
            {
                if (projectiles[i].HasTrail)
                    trails.Add(new Trail(Content.Load<Texture2D>("Textures\\pixel"), new Rectangle(projectiles[i].Rect.X + projectiles[i].Rect.Width / 2, projectiles[i].Rect.Y + projectiles[i].Rect.Height / 2, 2, 2),
                    projectiles[i].Rotation, 1, projectiles[i].TrailTint,
                    projectiles[i].TrailFadeOutSpeed, projectiles[i].TrailScalingOverTime));

                projectiles[i].UpdateMe(gameTime);
                if (projectiles[i].IsPlayer)
                {
                    // When collide with an alien
                    for (int j = 0; j < aliens.Count; j++)
                    {
                        if (projectiles[i].CollisionSphere.Intersects(aliens[j].CollisionSphere))
                        {
                            score += 20;
                            projectiles.RemoveAt(i);
                            aliens[j].Health--;
                            return;
                        }
                    }
                    if (!projectiles[i].IsAlive) projectiles.RemoveAt(i);
                }
                else
                {
                    // When collide with player
                    if (projectiles[i].CollisionSphere.Intersects(player.CollisionSphere))
                    {
                        score -= 20;
                        projectiles.RemoveAt(i);
                        if (!godMode) player.CurrentHealth--;
                        return;
                    }
                }

            }
            // Update trails
            for (int i = 0; i < trails.Count; i++)
            {
                trails[i].UpdateMe(gameTime);
                if (!trails[i].IsAlive) trails.RemoveAt(i);
            }

            // Pickup manager
            for (int i = 0; i < pickups.Count; i++)
            {
                pickups[i].UpdateMe(gameTime);
                if (pickups[i].Rect.Intersects(player.Rect))
                {
                    // Check pickup type and apply effect
                    PickupType type = pickups[i].PickupType;
                    switch (type)
                    {
                        case PickupType.MaxAmmo:
                            player.BulletLimit += 2;
                            player.BulletCount += 2;
                            player.ReloadSpeed += 0.3f;
                            break;
                        case PickupType.ReloadSpeed:
                            player.ReloadSpeed -= 0.2f;
                            break;
                        case PickupType.CurrentHealth:
                            player.CurrentHealth += 2;
                            break;
                        case PickupType.MaxHealth:
                            player.MaxHealth += 1;
                            player.CurrentHealth += 1;
                            break;
                        case PickupType.MoveSpeed:
                            player.CurrentSpeed += 1f;
                            break;
                    }
                    pickups[i].IsAlive = false;
                }

                // Remove pickup object
                if (!pickups[i].IsAlive) pickups.RemoveAt(i);
            }


            // Change to game over state
            if (player.CurrentHealth <= 0 || invasionCurrent >= invasionTotal)
            {
                gameState = GameState.GameOverState;
                for (int k = 0; k < table.Count; k++)
                {
                    if (table[k] == aliensKilled) return;
                }
                table.Add(aliensKilled);
                table.Sort();
                table.Reverse();
            }

#if DEBUG
            if (pad1_curr.Buttons.Y == ButtonState.Pressed && pad1_old.Buttons.Y != ButtonState.Pressed)
            {
                gameState = GameState.GameOverState;
                for (int k = 0; k < table.Count; k++)
                {
                    if (table[k] == aliensKilled) return;
                }
                table.Add(aliensKilled);
                table.Sort();
                table.Reverse();
            }

#endif

        }

        public void UpdateGameOver(GameTime gameTime)
        {
            // Move to main menu
            if (pad1_curr.Buttons.A == ButtonState.Pressed && pad1_old.Buttons.A != ButtonState.Pressed)
            {
                gameState = GameState.MainMenuState;
            }
        }

        // Spawns projectile and adds it to the main projectile list
        public void SpawnProjectile(Texture2D _txr, int _fps, Rectangle _rect, int _collisionSize, float _speed, float _rotation, Vector2 _dir, Color _tint, SpriteEffects _fliped, bool _isPlayer, float _lifeTime, int _damage, Color _trailTint, bool _hasTrail = false, float _rotationSpeed = 1f, float _trailFadeOutSpeed = 8f, float _trailScalingOverTime = 10f, bool _hasGravity = false, float _gravityStrength = 10f)
        {
            projectiles.Add(new Projectile(_txr, _fps, _rect, _collisionSize, _speed, _rotation, _dir, _tint, _fliped, _isPlayer, _lifeTime, _damage, _trailTint, _hasTrail, _rotationSpeed, _trailFadeOutSpeed, _trailScalingOverTime, _hasGravity, _gravityStrength));
        }

        // Resets every setting in the game
        public void ResetSettings()
        {
            if (player != null)
            {
                player.SetPosition(new Vector2(screenSize.Width / 2, screenSize.Height / 2));
                player.Velocity = new Vector2(0, 1);
                
            }
            player.ResetSettings();
            score = 0;
            aliensKilled = 0;
            invInfo.Clear();

            invasionCurrent = 0;
            invasionTotal = 1000;
            invInfoRate = 2f;

            humanRate = 2f;
            humanLimit = 20;

            alienLimit = 25;
            invaderSpawnRate = 2f;
            predatorSpawnRate = 30f;
            predatorTimer = 0f;

            aliens.Clear();
            humans.Clear();
            projectiles.Clear();
            trails.Clear();
            pickups.Clear();

        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, cam.GetCam());

            // Draw different screens depending on the game state
            switch (gameState)
            {
                case GameState.MainMenuState:
                    DrawMainMenu();
                    break;
                case GameState.PlayState:
                    DrawPlay(gameTime);
                    break;
                case GameState.GameOverState:
                    DrawGameOver();
                    break;
            }

            spriteBatch.End();



            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp);

            // Draw UI elements
            if (gameState == GameState.PlayState)
            {
                ui.DrawUI(spriteBatch, player);
                ui.DrawMapAliens(aliens);
                ui.DrawMapHumans(humans);
                ui.DrawMapProjectiles(projectiles);
                ui.DrawMapPickups(pickups);
                ui.DrawPlayerStats(player, healthTxtTxr, ammoTxtTxr);
                ui.DrawInvasionBar(invasionCurrent, invasionTotal, invInfo);
            }
            
            spriteBatch.End();


            base.Draw(gameTime);
        }

        // Draws Main Menu Scene
        public void DrawMainMenu()
        {
            spriteBatch.Draw(titleTxr, new Rectangle((int)(screenSize.Width / 2 - titleTxr.Width / 2 + mapSize.Width / 4), (int)(screenSize.Height / 2 - titleTxr.Height / 2), titleTxr.Width ,titleTxr.Height), Color.White);
            spriteBatch.Draw(beginTxr, new Rectangle((int)(screenSize.Width / 2 - beginTxr.Width / 2 + mapSize.Width / 4), (int)(screenSize.Height / 2 - beginTxr.Height / 2) + 32, beginTxr.Width, beginTxr.Height), Color.White);
        }

        // Draws Game Scene
        public void DrawPlay(GameTime gameTime)
        {
            // Draw background
            background.drawme(spriteBatch, Vector2.Zero, SpriteEffects.None, 0);

            // Draw trails
            #region Trails
            for (int i = 0; i < trails.Count; i++)
            {
                trails[i].drawme(spriteBatch);
            }
            #endregion

            // Draw Humans
            #region Humans
            for (int i = 0; i < humans.Count; i++)
            {
                humans[i].drawme(spriteBatch);
#if DEBUG
                //spriteBatch.Draw(debugPixel, humans[i].Rect, Color.Red * 0.3f);
                spriteBatch.Draw(pixel, new Rectangle((int)humans[i].CollisionSphere.Center.X, (int)humans[i].CollisionSphere.Center.Y, (int)humans[i].CollisionSphere.Radius, (int)humans[i].CollisionSphere.Radius), Color.Blue * 0.3f);
                spriteBatch.Draw(pixel, new Rectangle(humans[i].Rect.X, humans[i].Rect.Y, 1, 1), Color.White);
#endif
            }
            #endregion

            // Draw Aliens
            #region Aliens
            int descendingCount = 0;
            int waitingCount = 0;
            int escapingCount = 0;
            for (int i = aliens.Count - 1; i >= 0; i--)
            {
#if DEBUG
                spriteBatch.Draw(pixel, aliens[i].Rect, Color.Red * 0.3f);
                spriteBatch.Draw(pixel, new Rectangle((int)aliens[i].CollisionSphere.Center.X, (int)aliens[i].CollisionSphere.Center.Y, (int)aliens[i].CollisionSphere.Radius, (int)aliens[i].CollisionSphere.Radius), Color.Blue * 0.3f);
                spriteBatch.Draw(pixel, new Rectangle(aliens[i].Rect.X, aliens[i].Rect.Y, 1, 1), Color.White);
#endif

                aliens[i].drawme(spriteBatch, screenSize, topBarSize);

                // Calculates amount of aliens
                if (aliens[i].State == AlienState.Descending) descendingCount++;
                if (aliens[i].State == AlienState.Waiting) waitingCount++;
                if (aliens[i].State == AlienState.Escaping) escapingCount++;
            }
            #endregion

            // Draw Pickup Objects
            #region Pickups
            for (int i = 0; i < pickups.Count; i++)
            {
                pickups[i].drawme(spriteBatch);
#if DEBUG
                spriteBatch.Draw(pixel, pickups[i].Rect, Color.Red * 0.3f);
                spriteBatch.Draw(pixel, new Rectangle((int)pickups[i].CollisionSphere.Center.X, (int)pickups[i].CollisionSphere.Center.Y, (int)pickups[i].CollisionSphere.Radius, (int)pickups[i].CollisionSphere.Radius), Color.Blue * 0.3f);
                spriteBatch.Draw(pixel, new Rectangle(pickups[i].Rect.X, pickups[i].Rect.Y, 1, 1), Color.White);
#endif
            }
            #endregion

            // Draw Player
            #region [PLAYER]
            player.Drawme(spriteBatch, screenSize, topBarSize);
#if DEBUG
            spriteBatch.Draw(pixel, player.Rect, Color.Red * 0.3f);
            spriteBatch.Draw(pixel, new Rectangle((int)player.CollisionSphere.Center.X, (int)player.CollisionSphere.Center.Y, (int)player.CollisionSphere.Radius, (int)player.CollisionSphere.Radius), Color.Blue * 0.3f);
            spriteBatch.Draw(pixel, new Rectangle(player.Rect.X, player.Rect.Y, 1, 1), Color.White);
#endif
#endregion

            // Draw Projectiles
            #region Projectiles
            for (int i = 0; i < projectiles.Count; i++)
            {
                projectiles[i].drawme(spriteBatch);

#if DEBUG
                spriteBatch.Draw(pixel, new Rectangle((int)projectiles[i].CollisionSphere.Center.X, (int)projectiles[i].CollisionSphere.Center.Y, (int)projectiles[i].CollisionSphere.Radius, (int)projectiles[i].CollisionSphere.Radius), Color.Blue * 0.3f);
                spriteBatch.Draw(pixel, new Rectangle(projectiles[i].Rect.X, projectiles[i].Rect.Y, 1, 1), Color.White);
#endif
            }
            #endregion





#if DEBUG
            float fps = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;
            spriteBatch.DrawString(font, $"[DEBUG]\n" +
                                              $"FPS: {fps}\n" +
                                              $"Resolution: {resolution.X}x{resolution.Y}\n" +
                                              $"Map Size: {mapSize.Width}x{mapSize.Height} (4:3)\n" +
                                              $"Camera Position: {cam.Position}\n" +
                                              $"Player Position: {player.Rect.X},{player.Rect.Y}\n" +
                                              $"Player Veloctiy: {player.Velocity}\n", cam.Position, Color.White);

            spriteBatch.DrawString(font, $"Human Count: {humans.Count}/{humanLimit}\n" +
                                              $"Alien Count: {aliens.Count}/{alienLimit} ({descendingCount}/{waitingCount}/{escapingCount})\n" +
                                              $"Invader Spawn Rate: {invaderSpawnRate}\n" +
                                              $"Smoke Trail Count: {trails.Count}\n" +
                                              $"Projectile Count: {projectiles.Count}\n", cam.Position + new Vector2(300, 0), Color.White);

            spriteBatch.DrawString(font, $"Time: {totalGameTime}\n" +
                                              $"Score: {score}\n" +
                                              $"Invasion Meter: {invasionCurrent}/{invasionTotal}\n", cam.Position + new Vector2(1500, 0), Color.White);

            spriteBatch.DrawString(font, $"Player Health: {player.CurrentHealth} ({player.MaxHealth})\n" +
                                              $"Player Speed: {player.CurrentSpeed} ({player.MaxSpeed})\n" +
                                              $"Bullets: {player.BulletCount}/{player.BulletLimit}\n" +
                                              $"Bullet Reload: {player.ReloadSpeed}\n", cam.Position + new Vector2(1700, 0), Color.White);
#endif
        }

        public void DrawGameOver()
        {
            // Draw game over informations on screen
            if (score > highScore) highScore = score;
            spriteBatch.Draw(highScoreTxr, new Vector2(cam.Position.X + screenSize.Width / 2 - highScoreTxr.Width / 2, cam.Position.Y + screenSize.Height / 2 - 164), Color.White);
            spriteBatch.DrawString(font, $"{highScore}", new Vector2(cam.Position.X + screenSize.Width / 2 - 10, cam.Position.Y + screenSize.Height / 2 - 132), Color.White);

            spriteBatch.Draw(scoreTxr, new Vector2(cam.Position.X + screenSize.Width / 2 - scoreTxr.Width / 2, cam.Position.Y + screenSize.Height / 2 - 64), Color.White);
            spriteBatch.DrawString(font, $"{score}", new Vector2(cam.Position.X + screenSize.Width / 2 - 10, cam.Position.Y + screenSize.Height / 2 - 32), Color.White);

            spriteBatch.DrawString(font, $"Enemies Killed (TOP 5)", new Vector2(cam.Position.X + screenSize.Width / 2 - 60, cam.Position.Y + screenSize.Height / 2 + 10), Color.White);
            for (int i = 0; i < table.Count; i++)
            {
                // Draw top 5 scores for killed enemies
                if (i >= 5) return;
                Color col;
                if (table[i] == aliensKilled) col = Color.Green;
                else col = Color.White;
                spriteBatch.DrawString(font, $"{table[i]}", new Vector2(cam.Position.X + screenSize.Width / 2 - 4, cam.Position.Y + screenSize.Height / 2 + 30 + (20 * i)), col);
            }

        }
    }
}