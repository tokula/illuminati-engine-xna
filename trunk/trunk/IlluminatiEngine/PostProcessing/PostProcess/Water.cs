using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.PostProcessing
{
    public class Water : BasePostProcess
    {
        public Texture2D LightMap { get; set; }
        public float WaterLevel = -25f;

        public Water(Game game,float waterLevel) : base(game)
        {
            WaterLevel = waterLevel;
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
                effect = AssetManager.GetAsset<Effect>("Shaders/PostProcessing/Water");


            effect.Parameters["waterLevel"].SetValue(WaterLevel);

            //effect.Parameters["backBufferMapTex"].SetValue(orgBuffer);
            effect.Parameters["lightMap"].SetValue(LightMap);

            //effect.Parameters["reflectionMapTex"].SetValue(reflectionMap);
            //effect.Parameters["sgrMap"].SetValue(reflectionSRGMap);

            effect.Parameters["heightMapTex"].SetValue(AssetManager.GetAsset<Texture2D>("Textures/water_caustics"));
            effect.Parameters["positionMapTex"].SetValue(DepthBuffer);
            effect.Parameters["normalMapTex"].SetValue(AssetManager.GetAsset<Texture2D>("Textures/BumpMap"));
            effect.Parameters["foamMapTex"].SetValue(AssetManager.GetAsset<Texture2D>("Textures/water_foam"));

            effect.Parameters["matViewInverse"].SetValue((camera.View));
            effect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(camera.View * camera.Projection));
            effect.Parameters["cameraPos"].SetValue(camera.Position);
            //effect.Parameters["timer"].SetValue(f+=50);
            effect.Parameters["timer"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            effect.Parameters["halfPixel"].SetValue(HalfPixel);
            effect.Parameters["matViewProj"].SetValue(camera.View * camera.Projection);

            // Set Params.
            base.Draw(gameTime);
        }
    }
}
