using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine
{
    public interface ICanCastShadow 
    {
        List<Vector3> vertices { get; }
        Matrix World { get; }
    }
}
