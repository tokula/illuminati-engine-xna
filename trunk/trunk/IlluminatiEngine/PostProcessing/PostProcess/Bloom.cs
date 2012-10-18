using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.PostProcessing
{
    public class Bloom : BasePostProcess
    {
        public float BloomIntensity;
        public float BloomSaturation;
        public float BaseIntensity;
        public float BaseSaturation;

        public Bloom(Game game, float intensity, float saturation, float baseIntensity, float baseSatration)
            : base(game)
        {
            BloomIntensity = intensity;
            BloomSaturation = saturation;
            BaseIntensity = baseIntensity;
            BaseSaturation = baseSatration;
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
            {
                effect = AssetManager.GetAsset<Effect>("Shaders/PostProcessing/Bloom");
                effect.CurrentTechnique = effect.Techniques["BloomComposite"];
            }
            effect.Parameters["SceneTex"].SetValue(orgBuffer);

            effect.Parameters["BloomIntensity"].SetValue(BloomIntensity);
            effect.Parameters["BloomSaturation"].SetValue(BloomSaturation);
            effect.Parameters["BaseIntensity"].SetValue(BaseIntensity);
            effect.Parameters["BaseSaturation"].SetValue(BaseSaturation);

            // Set Params.
            base.Draw(gameTime);
        }
    }
}
