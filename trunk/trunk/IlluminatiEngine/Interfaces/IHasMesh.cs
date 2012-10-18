using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine
{
    public interface IHasMesh
    {
        string Mesh { get; set; }
        List<BoundingBox> AABoxBounds
        { get; set; }
        List<BoundingSphere> AASphereBounds
        { get; set; }
    }
}
