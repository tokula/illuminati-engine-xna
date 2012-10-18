using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.PostProcessing
{
    public class WaterEffect : BasePostProcessingEffect
    {
        public Water water;

        public Texture2D LightMap { set { water.LightMap = value; } }
        public float WaterLevel { set { water.WaterLevel = value; } }

        public WaterEffect(Game game, float waterLevel)
            : base(game)
        {
            water = new Water(game, waterLevel);

            AddPostProcess(water);
        }
    }
}
