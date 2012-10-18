using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.PostProcessing
{
    public class SceneBlend : BasePostProcess
    {
        public bool Blend = false;

        public SceneBlend(Game game)
            : base(game)
        {
            UsesVertexShader = true;            
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
                effect = AssetManager.GetAsset<Effect>("Shaders/PostProcessing/SceneBlend");

            if(Blend)
                effect.CurrentTechnique = effect.Techniques["Blend"];
            else
                effect.CurrentTechnique = effect.Techniques["Aditive"];

            effect.Parameters["OrgScene"].SetValue(orgBuffer);
            // Set Params.
            base.Draw(gameTime);

        }
    }
}
