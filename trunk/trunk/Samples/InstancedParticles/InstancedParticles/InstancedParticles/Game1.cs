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

namespace InstancedParticles
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : BaseDeferredRenderGame
    {

        Base3DCamera camera;

        BaseDeferredObject floor;

        BaseDeferredObject box;
        BaseDeferredObject box1;
        BaseDeferredObject box2;

        SpriteFont font;

        Base3DParticleInstancer snowInstancer;
        Base3DParticleInstancer tailInstancer;

        Base3DParticleInstancer smokeInstancer;

        List<Base3DParticleInstance> particles = new List<Base3DParticleInstance>();

        public Game1()
            : base()
        {
            graphics.PreferredBackBufferHeight = 600;
            graphics.PreferredBackBufferWidth = 800;

            camera = new Base3DCamera(this, .1f, 20000);

            floor = new BaseDeferredObject(this, "Models/plane");
            floor.TextureMaterials.Add("Textures/FloorColor");
            floor.NormalMaterials.Add("Textures/FloorNormal");
            floor.Position = new Vector3(0, -.75f, -10);
            Components.Add(floor);

            box = new BaseDeferredObject(this, "Models/Box");
            box.TextureMaterials.Add("Textures/BoxColor");
            box.NormalMaterials.Add("Textures/BoxNormal");
            box.Position = new Vector3(0, 0, -10);
            Components.Add(box);

            box1 = new BaseDeferredObject(this, "Models/Box");
            box1.TextureMaterials.Add("Textures/BoxColor01");
            box1.NormalMaterials.Add("Textures/BoxNormal01");
            box1.Position = new Vector3(0, 2.25f, -10);
            Components.Add(box1);

            box2 = new BaseDeferredObject(this, "Models/Box");
            box2.TextureMaterials.Add("Textures/BoxColor02");
            box2.NormalMaterials.Add("Textures/BoxNormal02");
            box2.Position = new Vector3(0, 4.5f, -10);
            Components.Add(box2);

            snowInstancer = new Base3DParticleInstancer(this, "Shaders/Particles/FallingWeatherParticleInstance");
            snowInstancer.TextureMaterials.Add("Textures/Particles/snow");
            snowInstancer.Position = new Vector3(0, -1, 0);
            //instancer2.Size = Vector2.One * .1f;
            Components.Add(snowInstancer);

            float sqr = 100;
            for (int a = 0; a < 100000; a++)
            {
                float x = MathHelper.Lerp(-sqr, sqr, (float)rnd.NextDouble());
                float y = MathHelper.Lerp(0, sqr / 2, (float)rnd.NextDouble());
                float z = MathHelper.Lerp(-sqr, sqr, (float)rnd.NextDouble());

                particles.Add(new Base3DParticleInstance(this, new Vector3(x, y, z), Vector3.One, snowInstancer));
            }

            tailInstancer = new Base3DParticleInstancer(this, "Shaders/Particles/Trail");
            tailInstancer.TextureMaterials.Add("Textures/Particles/bling2");
            tailInstancer.Color = Color.White;
            tailInstancer.Rotate(Vector3.Forward, (float)Math.PI / 2);
            Components.Add(tailInstancer);

            for (int a = 0; a < 1000; a++)
            {
                float y = MathHelper.Lerp(0, sqr / 2, (float)rnd.NextDouble());
                float x = MathHelper.Lerp(0, .2f, (float)rnd.NextDouble());
                float z = MathHelper.Lerp(0, .2f, (float)rnd.NextDouble());

                System.Threading.Thread.Sleep(1);
                particles.Add(new Base3DParticleInstance(this, new Vector3(x, y, z), Vector3.One * 1f, tailInstancer));
            }

            smokeInstancer = new Base3DParticleInstancer(this, "Shaders/Particles/RisingSmokeInstanced");
            smokeInstancer.TextureMaterials.Add("Textures/Particles/smoke");
            Components.Add(smokeInstancer);

            for (int s = 0; s < 100; s++)
            {
                float x = MathHelper.Lerp(-sqr, sqr, (float)rnd.NextDouble());
                float z = MathHelper.Lerp(-sqr, sqr, (float)rnd.NextDouble());

                PlaceRisingsmoke(new Vector3(x, -.75f, z), 5);
            }

            SunPosition = new Vector3(0, 300, 200);

            renderer.DirectionalLights.Add(new DeferredDirectionalLight(this, SunPosition, Vector3.Zero, Color.White, 1, true));
        }

        void PlaceRisingsmoke(Vector3 position, int size)
        {
            for (int gx = 0; gx < size; gx++)
            {
                for (int gz = 0; gz < size; gz++)
                {

                    Vector3 gp = position + (new Vector3((float)Math.Cos(rnd.Next(0, 360)), -.5f * 4, (float)Math.Sin(rnd.Next(0, 360))) * .25f);
                    Vector4 mods = new Vector4(rnd.Next(64, 128) * .001f, rnd.Next(64, 128), size + 1,0);
                    particles.Add(new Base3DParticleInstance(this, gp, Vector3.One * 2, mods, smokeInstancer));
                }
            }
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

            float x = (float)Math.Cos(gameTime.TotalGameTime.TotalSeconds) * 10;
            float y = 1;// MathHelper.Clamp((float)Math.Cos(gameTime.TotalGameTime.TotalSeconds), 0, 1);
            float z = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds) * 10;

            Vector3 lp = tailInstancer.Position;
            tailInstancer.Position = new Vector3(x, y, z - 10);
            GameComponentHelper.LookAt(lp, 1, tailInstancer.Position, ref tailInstancer.Rotation, Vector3.Down);

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

            if (inputHandler.KeyboardManager.KeyPress(Keys.F2))
                snowInstancer.Enabled = !snowInstancer.Enabled;

            if (inputHandler.KeyboardManager.KeyPress(Keys.F3))
                tailInstancer.Enabled = !tailInstancer.Enabled;

            if (inputHandler.KeyboardManager.KeyPress(Keys.F4))
                smokeInstancer.Enabled = !smokeInstancer.Enabled;

           
            box.Rotate(Vector3.Up, .01f);
            box1.Rotate(Vector3.Up + Vector3.Left, .01f);
            box2.Rotate(Vector3.Left, .01f);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            spriteBatch.Begin();

            spriteBatch.DrawString(font, "Esc        - Exit", Vector2.Zero, Color.Gold);
            spriteBatch.DrawString(font, "F1         - Deferred Debug On/Off", new Vector2(0, font.LineSpacing), Color.Gold);
            spriteBatch.DrawString(font, "WASD       - Translate Camera", new Vector2(0, font.LineSpacing * 2), Color.Gold);
            spriteBatch.DrawString(font, "Arrow Keys - Rotate Camera", new Vector2(0, font.LineSpacing * 3), Color.Gold);
            spriteBatch.DrawString(font, "Space      - Shadows On/Off", new Vector2(0, font.LineSpacing * 4), Color.Gold);
            spriteBatch.DrawString(font, "F2         - Snow On/Off", new Vector2(0, font.LineSpacing * 5), Color.Gold);
            spriteBatch.DrawString(font, "F3         - Tail On/Off", new Vector2(0, font.LineSpacing * 6), Color.Gold);
            spriteBatch.DrawString(font, "F4         - Smoke On/Off", new Vector2(0, font.LineSpacing * 7), Color.Gold);

            spriteBatch.End();
        }
    }
}
