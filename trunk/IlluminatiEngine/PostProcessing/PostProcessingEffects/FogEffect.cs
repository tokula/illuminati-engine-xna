using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.PostProcessing
{
    public class FogEffect : BasePostProcessingEffect
    {
        public Fog fog;

        public float FogDistance
        {
            get { return fog.FogDistance; }
            set { fog.FogDistance = MathHelper.Clamp(value, camera.Viewport.MinDepth, camera.Viewport.MaxDepth); }
        }

        public float FogRange
        {
            get { return fog.FogRange; }
            set { fog.FogRange = MathHelper.Clamp(value, camera.Viewport.MinDepth, camera.Viewport.MaxDepth); }
        }

        public Color Colour
        {
            get { return fog.FogColor; }
            set { fog.FogColor = value; }
        }

        public FogEffect(Game game, float distance, float range, Color color)
            : base(game)
        {
            fog = new Fog(game, distance, range, color);

            AddPostProcess(fog);
        }
    }
}
