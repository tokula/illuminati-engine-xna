using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.PostProcessing
{
    public class STHardPointEffect : BasePostProcessingEffect
    {
        public STHardPoints sthp;

        public Texture2D normalMap
        {
            set { sthp.normalMap = value; }
        }

        public float Angle
        {
            set { sthp.angle = value; }
            get { return sthp.angle; }
        }

        public float SobelWeight
        {
            set { sthp.sobelWeight = value; }
            get { return sthp.sobelWeight; }
        }


        public STHardPointEffect(Game game, float distance, float range, Color color, Color bgColor)
            : base(game)
        {
            sthp = new STHardPoints(game, color, bgColor);

            AddPostProcess(sthp);
        }
    }
}