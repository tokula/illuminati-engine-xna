using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.PostProcessing
{
    public class Ripple : BasePostProcess
    {
        float divisor = .5f;
        float distortion = 2.5f;

        public Ripple(Game game)
            : base(game)
        { }


        DateTime lastVisit = new DateTime();
        DateTime thisVisit = new DateTime();

        public override void Draw(GameTime gameTime)
        {
            thisVisit = DateTime.Now;
            TimeSpan duration = thisVisit - lastVisit;
            //0.026
            //0.033
            // 0.21
            //0.017
            //Game.Window.Title = duration.ToString() + " : " + ((float)gameTime.ElapsedGameTime.TotalSeconds * 0.5f).ToString();
            lastVisit = thisVisit;

            if (effect == null)
                effect = AssetManager.GetAsset<Effect>("Shaders/PostProcessing/Ripple");

            if (divisor > 1.25f)
                divisor = .4f;

            divisor += (float)gameTime.ElapsedGameTime.TotalSeconds * 0.5f;

            effect.Parameters["wave"].SetValue(MathHelper.Pi / divisor);
            effect.Parameters["distortion"].SetValue(distortion);
            effect.Parameters["centerCoord"].SetValue(new Vector2(.5f, .5f));

            // Set Params.
            base.Draw(gameTime);
        }
    }
}
