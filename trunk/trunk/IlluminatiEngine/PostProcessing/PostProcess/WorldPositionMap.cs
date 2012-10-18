using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.PostProcessing
{
    public class WorldPositionMap : BasePostProcess
    {
        public WorldPositionMap(Game game) : base(game) 
        {
            UsesVertexShader = true;
            newSceneSurfaceFormat = SurfaceFormat.Vector4;
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
            {
                effect = AssetManager.GetAsset<Effect>("Shaders/PostProcessing/Position");
                effect.CurrentTechnique = effect.Techniques["PositionMap"];
            }

            effect.Parameters["depthMap"].SetValue(DepthBuffer);
            effect.Parameters["ProjectionInv"].SetValue(Matrix.Invert(camera.Projection));
            effect.Parameters["halfPixel"].SetValue(HalfPixel);

            
            // Set Params.
            base.Draw(gameTime);
        }
    }
}
