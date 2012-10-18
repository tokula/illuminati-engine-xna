using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.PostProcessing
{
    public class MotionBlur : BasePostProcess
    {
        Matrix lastVP;

        public MotionBlur(Game game)
            : base(game)
        {
            UsesVertexShader = true;
            newSceneSurfaceFormat = SurfaceFormat.Vector4;
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
            {
                effect = AssetManager.GetAsset<Effect>("Shaders/PostProcessing/MotionBlur");
                effect.CurrentTechnique = effect.Techniques["MotionBlur"];
            }

            
            effect.Parameters["depthMap"].SetValue(DepthBuffer);
            effect.Parameters["g_ViewProjectionInverseMatrix"].SetValue(Matrix.Invert(camera.View * camera.Projection));
            effect.Parameters["g_previousViewProjectionMatrix"].SetValue(lastVP);
            effect.Parameters["halfPixel"].SetValue(HalfPixel);

            lastVP = camera.View * camera.Projection;

            Game.GraphicsDevice.BlendState = BlendState.Opaque;
            // Set Params.
            base.Draw(gameTime);
        }
    }
}
