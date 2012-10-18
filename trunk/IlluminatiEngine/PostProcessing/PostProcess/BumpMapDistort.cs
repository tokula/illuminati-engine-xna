using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.PostProcessing
{
    public class BumpmapDistort : BasePostProcess
    {
        private double elapsedTime = 0;
        public bool High = true;
        public string BumpAsset;

        public BumpmapDistort(Game game, string bumpAsset, bool high)
            : base(game)
        {
            BumpAsset = bumpAsset;
            High = high;
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
            {
                effect = AssetManager.GetAsset<Effect>("Shaders/PostProcessing/BumpMapDistort");
            }

            if (High)
                effect.CurrentTechnique = effect.Techniques["High"];
            else
                effect.CurrentTechnique = effect.Techniques["Low"];

            elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;

            if (elapsedTime >= 10.0f)
                elapsedTime = 0.0f;

            effect.Parameters["Offset"].SetValue((float)elapsedTime * .1f);
            effect.Parameters["Bumpmap"].SetValue(AssetManager.GetAsset<Texture2D>(BumpAsset));

            effect.Parameters["halfPixel"].SetValue(HalfPixel);

            // Set Params.
            base.Draw(gameTime);
        }
    }
}
