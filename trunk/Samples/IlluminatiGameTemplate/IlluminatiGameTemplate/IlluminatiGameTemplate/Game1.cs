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


namespace IlluminatiGameTemplate
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

            box2= new BaseDeferredObject(this, "Models/Box");
            box2.TextureMaterials.Add("Textures/BoxColor02");
            box2.NormalMaterials.Add("Textures/BoxNormal02");
            box2.Position = new Vector3(0, 4.5f, -10);
            Components.Add(box2);

            SunPosition = new Vector3(0, 300, 200);

            renderer.DirectionalLights.Add(new DeferredDirectionalLight(this, SunPosition, Vector3.Zero, Color.White, 1, true));            
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
            spriteBatch.DrawString(font, "F2         - Soft Shadows On/Off", new Vector2(0, font.LineSpacing * 5), Color.Gold);           

            spriteBatch.End();
        }
    }
}
