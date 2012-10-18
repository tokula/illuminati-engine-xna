using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.Renderer.Deferred
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class DeferredPointLight : BaseLight, IPointLight
    {
        protected float radius;

        public DeferredPointLight(Vector3 position, Color color, float radius, float intensity, bool castShadow) : base(position, color, intensity, castShadow)
        {
            this.radius = radius;
        }

        #region IPointLight Members

        public float Radius
        {
            get { return radius; }
            set { radius = value; }
        }

        #endregion

    }
}
