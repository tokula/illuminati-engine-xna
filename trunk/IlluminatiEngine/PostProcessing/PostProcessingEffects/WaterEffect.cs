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
        Water water;

        public float waterHeight
        {
            get { return water.waterLevel; }
            set { water.waterLevel = value; }
        }

        public float seaFloor
        {
            get { return water.seaFloor; }
            set { water.seaFloor = value; }
        }

        public Vector3 foamExistance
        {
            get { return water.foamExistance; }
            set { water.foamExistance = value; }
        }

        public float maxAmplitude { get { return water.maxAmplitude; } set { water.maxAmplitude = value; } }

        public float refractionScale { get { return water.refractionScale; } set { water.refractionScale = value; } }

        public WaterEffect(Game game)
            : base(game)
        {
            water = new Water(game);

            AddPostProcess(water);
        }
    }
}
