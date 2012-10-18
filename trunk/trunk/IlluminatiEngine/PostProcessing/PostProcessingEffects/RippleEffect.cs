using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.PostProcessing
{
    public class RippleEffect : BasePostProcessingEffect
    {
        public Ripple r;

        public RippleEffect(Game game)
            : base(game)
        {
            r = new Ripple(game);

            AddPostProcess(r);
        }
    }
}
