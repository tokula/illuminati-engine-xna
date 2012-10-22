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
        public float waterLevel = 0;
        public float seaFloor = -30;

        public float maxAmplitude = 3.25f;
        public float refractionScale = .00525f;

        public Water(Game game)
            : base(game)
        {
            UsesVertexShader = true;
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
            {
                effect = AssetManager.GetAsset<Effect>("Shaders/PostProcessing/Water");
                effect.CurrentTechnique = effect.Techniques["Water"];
            }


            effect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(camera.View * camera.Projection));
            effect.Parameters["cameraPos"].SetValue(camera.Position);
            effect.Parameters["lightDir"].SetValue(Vector3.Normalize(new Vector3(50, 50, 1000)));

            effect.Parameters["heightMapTex"].SetValue(AssetManager.GetAsset<Texture2D>("Textures/wavemap"));
            effect.Parameters["normalMapTex"].SetValue(AssetManager.GetAsset<Texture2D>("Textures/bumpmap"));
            effect.Parameters["foamMapTex"].SetValue(AssetManager.GetAsset<Texture2D>("Textures/water_foam"));
            effect.Parameters["positionMapTex"].SetValue(DepthBuffer);
            effect.Parameters["backBufferMapTex"].SetValue(BackBuffer);

            effect.Parameters["seaFloor"].SetValue(seaFloor);

            effect.Parameters["sceneNormal"].SetValue(normalBuffer);

            effect.Parameters["caustics"].SetValue(AssetManager.GetAsset<Texture2D>("Textures/water_caustics2"));

            effect.Parameters["timer"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);

            effect.Parameters["matViewInverse"].SetValue(Matrix.Invert(camera.View));
            effect.Parameters["halfPixel"].SetValue(HalfPixel);
            effect.Parameters["matViewProj"].SetValue(camera.View * camera.Projection);

            effect.Parameters["maxAmplitude"].SetValue(maxAmplitude);
            effect.Parameters["refractionScale"].SetValue(refractionScale);

            effect.Parameters["camMin"].SetValue(camera.Viewport.MinDepth);
            effect.Parameters["camMax"].SetValue(camera.Viewport.MaxDepth);

            effect.Parameters["waterLevel"].SetValue(waterLevel);
            // Set Params.
            base.Draw(gameTime);

        }
    }
}
