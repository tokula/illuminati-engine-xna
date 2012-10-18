using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.PostProcessing
{
    public class STHardPoints : BasePostProcess
    {
        public Color Color;
        public Color BGColor;

        public Texture2D normalMap;
        public float angle = 1.0f / 2.0f;
        public float sobelWeight = 0;

        public STHardPoints(Game game, Color color, Color bgColor)
            : base(game)
        {
            Color = color;
            BGColor = bgColor;
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
                effect = AssetManager.GetAsset<Effect>("Shaders/PostProcessing/STHardPoint");

            effect.Parameters["depthMap"].SetValue(DepthBuffer);
            effect.Parameters["color"].SetValue(Color.ToVector4());
            effect.Parameters["bgColor"].SetValue(BGColor.ToVector4());

            effect.Parameters["camPos"].SetValue(camera.Position);
            effect.Parameters["viewProjectionInv"].SetValue(Matrix.Invert(camera.View * camera.Projection));

            effect.Parameters["bumpMap"].SetValue(normalMap);

            effect.Parameters["angle"].SetValue(angle);
            effect.Parameters["sobelWeight"].SetValue(sobelWeight);

            effect.Parameters["imageIncrement"].SetValue(new Vector2(1.0f / BackBuffer.Width, 1.0f / BackBuffer.Height / 2));

            // Set Params.
            base.Draw(gameTime);
        }
    }
}