using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine
{
    public interface IDeferredRender
    {
        List<string> TextureMaterials { get; set; }
        List<string> NormalMaterials { get; set; }
        List<string> SpeculaMaterials { get; set; }
        List<string> GlowMaterials { get; set; }
        List<string> ReflectionMaterials { get; set; }

        void Draw(GameTime gameTime);
        void Draw(GameTime gameTime, Effect effect);
    }
}
