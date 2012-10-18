using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.PostProcessing
{
    public class RadialBlurEffect : BasePostProcessingEffect
    {
        public RadialBlur rb;

        public RadialBlurEffect(Game game, float scale)
            : base(game)
        {
            rb = new RadialBlur(game, scale);

            AddPostProcess(rb);
        }
    }
}
