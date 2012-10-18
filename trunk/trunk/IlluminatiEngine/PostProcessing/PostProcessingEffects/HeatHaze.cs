using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.PostProcessing
{
    public class HeatHazeEffect : BasePostProcessingEffect
    {
        public BumpmapDistort distort;

        public HeatHazeEffect(Game game, string bumpasset, bool high)
            : base(game)
        {
            distort = new BumpmapDistort(game, bumpasset, high);

            AddPostProcess(distort);
        }
    }
}
