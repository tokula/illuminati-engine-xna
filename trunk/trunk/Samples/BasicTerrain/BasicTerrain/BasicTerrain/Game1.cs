using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using IlluminatiEngine;
using IlluminatiEngine.Renderer;
using IlluminatiEngine.Renderer.Deferred;
using IlluminatiEngine.Utilities;

using Bullet = BulletXNA;
using BulletXNA.BulletCollision;

namespace BasicTerrain
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : BaseDeferredRenderGame
    {

        Base3DCamera camera;
        BruteForceTerrain terrain;
        SpriteFont font;

        List<BulletXNAObject> boxs = new List<BulletXNAObject>();

        public Game1()
            : base()
        {
            graphics.PreferredBackBufferHeight = 600;
            graphics.PreferredBackBufferWidth = 800;

            camera = new Base3DCamera(this, .1f, 20000);

            DeferredSkyBox sb = new DeferredSkyBox(this, "Textures/SkyBox/cubeMap");
            Components.Add(sb);

            terrain = new BruteForceTerrain(this, "Textures/Terrain/Maps/heightMap3");
            terrain.Position = new Vector3(-128,-30,-128);
            Components.Add(terrain);
          
            SunPosition = new Vector3(225, 220, -400);

            renderer.DirectionalLights.Add(new DeferredDirectionalLight(this, SunPosition, Vector3.Zero, Color.White, 1, true));

            GodRays.Enabled = true;
            GodRays.LightSourceSize = 1500;
            GodRays.bp.Enabled = false;
            GodRays.lightSourceasset = "Textures/flare2";
            SunPosition *= 2.5f;
            GodRays.Decay = .96f;
            GodRays.Density = 1f;
            GodRays.Exposure = .04f;
            GodRays.Weight = 1;

            UseBulletPhysics = true;  
        }

        // create 125 (5x5x5) dynamic objects
        int ArraySizeX = 3, ArraySizeY = 3, ArraySizeZ = 3;

        // scaling of the objects (0.1 = 20 centimeter boxes )
        float StartPosX = -5;
        float StartPosY = -5;
        float StartPosZ = -3;



        public override void InitializePhysics()
        {
            base.InitializePhysics();

            CollisionFilterGroups colGroup = CollisionFilterGroups.DefaultFilter;
            CollisionFilterGroups colMask = CollisionFilterGroups.AllFilter;

            BulletXNAObject ball = new BulletXNAObject(this, "Models/Sphere", 5, new Bullet.DefaultMotionState(Matrix.CreateTranslation(camera.Position + new Vector3(0, 10, -5)), Matrix.Identity), new Bullet.BulletCollision.SphereShape(1), Vector3.Zero,colGroup,colMask);
            ball.TranslateAA(camera.Position + new Vector3(0, 10, -5));
            ball.TextureMaterials.Add("Textures/core1");
            ball.NormalMaterials.Add("Textures/core1Normal");
            ball.RigidBody.SetFriction(1);
            boxs.Add(ball);
            Components.Add(ball);

            // create a few dynamic rigidbodies
            float mass = 1.0f;

            Bullet.BulletCollision.CollisionShape colShape = new Bullet.BulletCollision.BoxShape(Vector3.One);
            

            //CollisionShapes.Add(colShape);
            Vector3 localInertia = Vector3.Zero;
            Bullet.LinearMath.IndexedVector3 li = Bullet.LinearMath.IndexedVector3.Zero;
            colShape.CalculateLocalInertia(mass, out li);
            localInertia = li;

            float start_x = StartPosX - ArraySizeX / 2;
            float start_y = StartPosY;
            float start_z = StartPosZ - ArraySizeZ / 2;

            start_z -= 8;
            Random rnd = new Random();

            int k, i, j;
            for (k = 0; k < ArraySizeY; k++)
            {
                for (i = 0; i < ArraySizeX; i++)
                {
                    for (j = 0; j < ArraySizeZ; j++)
                    {
                        Matrix startTransform = Matrix.CreateTranslation(
                            2 * i + start_x,
                            2 * k + start_y,
                            2 * j + start_z
                        );

                        // using motionstate is recommended, it provides interpolation capabilities
                        // and only synchronizes 'active' objects
                        BulletXNAObject box = new BulletXNAObject(this, "Models/Box", mass, new Bullet.DefaultMotionState(startTransform,Matrix.Identity), colShape, localInertia,colGroup,colMask);
                        box.TextureMaterials.Add("Textures/BoxColor");
                        box.NormalMaterials.Add("Textures/BoxNormal");
                        
                        box.TranslateAA(new Vector3(0, 7, 0));
                        boxs.Add(box);
                        Components.Add(box);
                    }
                }
            }

            start_x -= 8;

            for (k = 0; k < 2; k++)
            {
                for (i = 0; i < 2; i++)
                {
                    for (j = 0; j < 2; j++)
                    {
                        Matrix startTransform = Matrix.CreateTranslation(
                            2 * i + start_x,
                            2 * k + start_y,
                            2 * j + start_z
                        );

                        // using motionstate is recommended, it provides interpolation capabilities
                        // and only synchronizes 'active' objects
                        BulletXNAObject box = new BulletXNAObject(this, "Models/Box", mass, new Bullet.DefaultMotionState(startTransform,Matrix.Identity), colShape, localInertia,colGroup,colMask);
                        box.TextureMaterials.Add("Textures/BoxColor01");
                        box.NormalMaterials.Add("Textures/BoxNormal01");
                        
                        box.TranslateAA(new Vector3(0, 7, 0));
                        boxs.Add(box);
                        Components.Add(box);
                    }
                }
            }

            


            IsPhysicsEnabled = false;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            base.Initialize();

            font = assetManager.GetAsset<SpriteFont>("Fonts/font");
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (inputHandler.KeyboardManager.KeyPress(Keys.Escape))
                this.Exit();

            float speedTran = .1f;
            float speedRot = .01f;

            if (inputHandler.KeyboardManager.KeyPress(Keys.F1))
                renderer.DebugDeferred = !renderer.DebugDeferred;

            if (inputHandler.KeyboardManager.KeyPress(Keys.Space))
                renderer.DirectionalLights[0].CastShadow = !renderer.DirectionalLights[0].CastShadow;

            if (inputHandler.KeyboardManager.KeyDown(Keys.W) || inputHandler.GamePadManager.ButtonDown(PlayerIndex.One, Buttons.DPadUp))
                camera.Translate(Vector3.Forward * speedTran);
            if (inputHandler.KeyboardManager.KeyDown(Keys.S) || inputHandler.GamePadManager.ButtonDown(PlayerIndex.One, Buttons.DPadDown))
                camera.Translate(Vector3.Backward * speedTran);
            if (inputHandler.KeyboardManager.KeyDown(Keys.A) || inputHandler.GamePadManager.ButtonDown(PlayerIndex.One, Buttons.DPadLeft))
                camera.Translate(Vector3.Left * speedTran);
            if (inputHandler.KeyboardManager.KeyDown(Keys.D) || inputHandler.GamePadManager.ButtonDown(PlayerIndex.One, Buttons.DPadRight))
                camera.Translate(Vector3.Right * speedTran);

            if (inputHandler.KeyboardManager.KeyDown(Keys.Left) || inputHandler.GamePadManager.State[PlayerIndex.One].ThumbSticks.Right.X < 0)
                camera.Rotate(Vector3.Up, speedRot);
            if (inputHandler.KeyboardManager.KeyDown(Keys.Right) || inputHandler.GamePadManager.State[PlayerIndex.One].ThumbSticks.Right.X > 0)
                camera.Rotate(Vector3.Up, -speedRot);
            if (inputHandler.KeyboardManager.KeyDown(Keys.Up) || inputHandler.GamePadManager.State[PlayerIndex.One].ThumbSticks.Right.Y > 0)
                camera.Rotate(Vector3.Right, speedRot);
            if (inputHandler.KeyboardManager.KeyDown(Keys.Down) || inputHandler.GamePadManager.State[PlayerIndex.One].ThumbSticks.Right.Y < 0)
                camera.Rotate(Vector3.Right, -speedRot);

            if (inputHandler.KeyboardManager.KeyDown(Keys.NumPad8))
                boxs[0].LinearVelocity += new Vector3(0, 0, -1);
            if (inputHandler.KeyboardManager.KeyDown(Keys.NumPad2))
                boxs[0].LinearVelocity += new Vector3(0, 0, 1);
            if (inputHandler.KeyboardManager.KeyDown(Keys.NumPad4))
                boxs[0].LinearVelocity += new Vector3(-1, 0, 0);
            if (inputHandler.KeyboardManager.KeyDown(Keys.NumPad6))
                boxs[0].LinearVelocity += new Vector3(1, 0, 0);

            if (inputHandler.KeyboardManager.KeyDown(Keys.NumPad0))
                boxs[0].LinearVelocity += new Vector3(0, 1, 0);

            if (inputHandler.KeyboardManager.KeyPress(Keys.P))
                IsPhysicsEnabled = !IsPhysicsEnabled;

            base.Update(gameTime);
        }

        bool tDone = false;
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (terrain.RigidBody != null && !tDone)
            {
                tDone = true;
                ((BulletXNAPhysicsComponent)Services.GetService(typeof(BulletXNAPhysicsComponent))).World.AddRigidBody(terrain.RigidBody as BulletXNA.BulletDynamics.RigidBody);
            }

            base.Draw(gameTime);

            spriteBatch.Begin();

            spriteBatch.DrawString(font, "Esc           - Exit", Vector2.Zero, Color.Gold);
            spriteBatch.DrawString(font, "F1            - Deferred Debug On/Off", new Vector2(0, font.LineSpacing), Color.Gold);
            spriteBatch.DrawString(font, "WASD          - Translate Camera", new Vector2(0, font.LineSpacing * 2), Color.Gold);
            spriteBatch.DrawString(font, "Arrow Keys    - Translate Camera", new Vector2(0, font.LineSpacing * 3), Color.Gold);
            spriteBatch.DrawString(font, "Space         - Shadows On/Off", new Vector2(0, font.LineSpacing * 4), Color.Gold);
            spriteBatch.DrawString(font, "NumPad Arrows - Translate Sphere", new Vector2(0, font.LineSpacing * 5), Color.Gold);
            spriteBatch.DrawString(font, "NumPad 0      - Translate Sphere Up", new Vector2(0, font.LineSpacing * 6), Color.Gold);
            spriteBatch.DrawString(font, "P             - Switch Physics On/Off", new Vector2(0, font.LineSpacing * 7), Color.Gold);

            spriteBatch.End();
        }
    }
}
