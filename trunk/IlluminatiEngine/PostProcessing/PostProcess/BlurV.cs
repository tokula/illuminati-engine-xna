using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.PostProcessing
{
    public class BlurV : BasePostProcess
    {
        float blurAmount = 1;
        public BlurV(Game game, float amount) : base(game) 
        {
            blurAmount = amount;
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
            {
                effect = AssetManager.GetAsset<Effect>("Shaders/PostProcessing/blur");
                effect.CurrentTechnique = effect.Techniques["BlurV"];
            }
            effect.Parameters["g_BlurAmount"].SetValue(blurAmount);
            // Set Params.
            base.Draw(gameTime);
        }
    }
}

