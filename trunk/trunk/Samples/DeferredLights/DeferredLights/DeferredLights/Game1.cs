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

namespace DeferredLights
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : BaseDeferredRenderGame
    {

        Base3DCamera camera;

        SpriteFont font;

        BaseDeferredObject BasicColumnScene;

        BaseDeferredSkinnedObject dude;

        List<BaseDeferredObject> shots = new List<BaseDeferredObject>();
        Dictionary<BaseDeferredObject, Vector3> shotVelocity = new Dictionary<BaseDeferredObject, Vector3>();
        Dictionary<BaseDeferredObject, IPointLight> shotLights = new Dictionary<BaseDeferredObject, IPointLight>();

        float coneAngle = .7f;
        float codeDecay = 4;

        float directionalIntensity = .125f;

        public Game1()
            : base()
        {
            graphics.PreferredBackBufferHeight = 600;
            graphics.PreferredBackBufferWidth = 800;

            camera = new Base3DCamera(this, .1f, 20000);

            BasicColumnScene = new BaseDeferredObject(this);
            BasicColumnScene.Position = new Vector3(0, -1, 0);
            BasicColumnScene.Scale = new Vector3(.5f, 1, .5f);
            BasicColumnScene.Mesh = "Models/BasicColumnScene";
            BasicColumnScene.TextureMaterials.Add("Textures/floor_color");
            BasicColumnScene.TextureMaterials.Add("Textures/bricks_color");
            BasicColumnScene.NormalMaterials.Add("Textures/floor_normal");
            BasicColumnScene.NormalMaterials.Add("Textures/bricks_normal");
            Components.Add(BasicColumnScene);

            dude = new BaseDeferredSkinnedObject(this);
            dude.Position = new Vector3(0, -1, -5);
            dude.AnimationClip = "Take 001";
            dude.Scale = Vector3.One * .05f;
            dude.Mesh = "Models/dude";
            Components.Add(dude);


            SunPosition = new Vector3(100, 300, -200);

            renderer.DirectionalLights.Add(new DeferredDirectionalLight(this, SunPosition, Vector3.Zero, Color.White, directionalIntensity, true));

            renderer.ConeLights.Add(new DeferredConeLight(this, Vector3.Zero + Vector3.Up * 5, new Vector3(0, 0, -20), Color.White, 1.5f, coneAngle, codeDecay, true));
            renderer.ConeLights.Add(new DeferredConeLight(this, Vector3.Zero + Vector3.Up * 5, new Vector3(0, 0, 20), Color.White, 1.5f, coneAngle, codeDecay, true));
            renderer.ConeLights.Add(new DeferredConeLight(this, Vector3.Zero + Vector3.Up * 5, new Vector3(20, 0, 0), Color.White, 1.5f, coneAngle, codeDecay, true));
            renderer.ConeLights.Add(new DeferredConeLight(this, Vector3.Zero + Vector3.Up * 5, new Vector3(-20, 0, 0), Color.White, 1.5f, coneAngle, codeDecay, true));

            renderer.PointLights.Add(new DeferredPointLight(new Vector3(-10, 5, -10), Color.White, 10, 2, false));
            renderer.PointLights.Add(new DeferredPointLight(new Vector3(10, 5, -10), Color.Red, 10, 2, false));
            renderer.PointLights.Add(new DeferredPointLight(new Vector3(0, 5, 0), Color.Green, 11, 2, false));
            renderer.PointLights.Add(new DeferredPointLight(new Vector3(-10, 5, 10), Color.Blue, 10, 2, false));
            renderer.PointLights.Add(new DeferredPointLight(new Vector3(10, 5, 10), Color.Gold, 10, 2, false));
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

            if (inputHandler.KeyboardManager.KeyDown(Keys.T))
            {
                directionalIntensity += .01f;
                if (directionalIntensity > 1)
                    directionalIntensity = 1;

                renderer.DirectionalLights[0].Intensity = directionalIntensity;
            }
            if (inputHandler.KeyboardManager.KeyDown(Keys.G))
            {
                directionalIntensity -= .01f;
                if (directionalIntensity < 0)
                    directionalIntensity = 0;

                renderer.DirectionalLights[0].Intensity = directionalIntensity;
            }

            if (inputHandler.KeyboardManager.KeyDown(Keys.Y))
            {
                coneAngle += .01f;
                if (coneAngle > .999f)
                    coneAngle = .999f;

                foreach (IConeLight cone in renderer.ConeLights)
                    cone.Angle = coneAngle;
            }
            if (inputHandler.KeyboardManager.KeyDown(Keys.H))
            {
                coneAngle -= .01f;
                if (coneAngle < .001f)
                    coneAngle = .001f;

                foreach (IConeLight cone in renderer.ConeLights)
                    cone.Angle = coneAngle;
            }

            if (inputHandler.KeyboardManager.KeyDown(Keys.U))
            {
                codeDecay += .1f;
                if (codeDecay > 10)
                    codeDecay = 10;

                foreach (IConeLight cone in renderer.ConeLights)
                    cone.Decay = codeDecay;
            }
            if (inputHandler.KeyboardManager.KeyDown(Keys.J))
            {
                codeDecay -= .1f;
                if (codeDecay < 0)
                    codeDecay = 0;

                foreach (IConeLight cone in renderer.ConeLights)
                    cone.Decay = codeDecay;
            }

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

            if (inputHandler.KeyboardManager.KeyPress(Keys.Q) || inputHandler.GamePadManager.ButtonPress(PlayerIndex.One, Buttons.A))
            {
                shots.Add(new BaseDeferredObject(this, "Models/sphere"));
                shots[shots.Count - 1].TextureMaterials.Add("Textures/white");
                shots[shots.Count - 1].GlowMaterials.Add("Textures/white");
                shots[shots.Count - 1].Scale = Vector3.One * .125f;
                shots[shots.Count - 1].color = Color.DodgerBlue;


                shotVelocity.Add(shots[shots.Count - 1], camera.World.Forward * .25f);
                
                shots[shots.Count - 1].Position = camera.Position;

                shotLights.Add(shots[shots.Count - 1], new DeferredPointLight(shots[shots.Count - 1].Position, shots[shots.Count - 1].color, 10, 1, false));
                renderer.PointLights.Add(shotLights[shots[shots.Count - 1]]);
                Components.Add(shots[shots.Count - 1]);
            }

            List<BaseDeferredObject> dropShots = new List<BaseDeferredObject>();
            foreach (BaseDeferredObject shot in shots)
            {
                if (shot.Visible)
                {
                    shot.TranslateAA(shotVelocity[shot]);
                    shotLights[shot].Position = shot.Position;
                    if (shot.Position.Length() > 30)
                    {
                        Components.Remove(shot);
                        shotVelocity.Remove(shot);
                        
                        renderer.PointLights.Remove(shotLights[shot]);
                        shotLights.Remove(shot);

                        dropShots.Add(shot);
                    }
                }
            }

            foreach (BaseDeferredObject shot in dropShots)
                shots.Remove(shot);

            dropShots.Clear();


            dude.TranslateOO(Vector3.Forward * .0325f);

            if (dude.Position.Z < -25)
                dude.Position = new Vector3(MathHelper.Lerp(-10,10,(float)rnd.NextDouble()), -1, 25);

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
            spriteBatch.DrawString(font, "Q          - Shoot", new Vector2(0, font.LineSpacing * 5), Color.Gold);
            spriteBatch.DrawString(font, string.Format("T/G        - Directional Light Intensity {0:0.00}" , directionalIntensity), new Vector2(0, font.LineSpacing * 6), Color.Gold);
            spriteBatch.DrawString(font, string.Format("Y/H        - Cone Light Angle {0:0.00}", coneAngle), new Vector2(0, font.LineSpacing * 7), Color.Gold);
            spriteBatch.DrawString(font, string.Format("U/J        - Cone Light Decay {0:0.00}", codeDecay), new Vector2(0, font.LineSpacing * 8), Color.Gold);

            spriteBatch.End();
        }
    }
}
