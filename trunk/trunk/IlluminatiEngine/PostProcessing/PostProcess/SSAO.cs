using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.PostProcessing
{
    public class SSAO : BasePostProcess
    {
        public float rad = .1f;
        public float intensity = 1;//2.5f;
        public float scale = .5f;//5;
        public float bias = 1f;

        public SSAO(Game game, float radius,float intensity,float scale,float bias)
            : base(game)
        {
            rad = radius;
            this.intensity = intensity;
            this.scale = scale;
            this.bias = bias;

            UsesVertexShader = true;
        }


        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
                effect = AssetManager.GetAsset<Effect>("Shaders/PostProcessing/SSAO2");
            
            effect.CurrentTechnique = effect.Techniques["ssao"];
            /*
            effect.Parameters["halfPixel"].SetValue(HalfPixel);
            
            effect.Parameters["depthMap"].SetValue(DepthBuffer);
            effect.Parameters["NoiseTexture"].SetValue(AssetManager.GetAsset<Texture2D>("Textures/random"));
            //effect.Parameters["vecViewPort"].SetValue(new Vector4(Game.GraphicsDevice.Viewport.Width,Game.GraphicsDevice.Viewport.Height,Game.GraphicsDevice.Viewport.MinDepth,Game.GraphicsDevice.Viewport.MaxDepth));
            effect.Parameters["vecViewPort"].SetValue(new Vector4(Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height, 1.0f / Game.GraphicsDevice.Viewport.Width, 1.0f / Game.GraphicsDevice.Viewport.Height));
            */

            //effect.Parameters["ViewProjectionInv"].SetValue(Matrix.Invert(camera.View * camera.Projection));
            effect.Parameters["halfPixel"].SetValue(HalfPixel);
            effect.Parameters["g_screen_size"].SetValue(new Vector2(camera.Viewport.Width, camera.Viewport.Height));

            effect.Parameters["g_far_clip"].SetValue(camera.Viewport.MaxDepth);

            effect.Parameters["View"].SetValue(camera.View);
            
            effect.Parameters["normal"].SetValue(normalBuffer);
            effect.Parameters["position"].SetValue(BackBuffer);
            effect.Parameters["random"].SetValue(AssetManager.GetAsset<Texture2D>("Textures/random"));
            effect.Parameters["random_size"].SetValue(new Vector2(AssetManager.GetAsset<Texture2D>("Textures/random").Width, AssetManager.GetAsset<Texture2D>("Textures/random").Height));

            effect.Parameters["g_sample_rad"].SetValue(rad);
            effect.Parameters["g_intensity"].SetValue(intensity);
            effect.Parameters["g_scale"].SetValue(scale);
            effect.Parameters["g_bias"].SetValue(bias);

            // Set Params.
            base.Draw(gameTime);

        }
    }
}
