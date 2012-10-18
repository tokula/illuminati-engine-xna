using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace IlluminatiEngine
{
    public interface IGridRegisterable
    {
        List<BoundingBox> Bounds { get; }
        bool HasMoved { get; set; }
    }
}
