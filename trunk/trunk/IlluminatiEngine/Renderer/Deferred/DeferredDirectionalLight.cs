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
    public class DeferredDirectionalLight : BaseLight, IDirectionalLight
    {
        protected Vector3 target;

        public DeferredDirectionalLight(Game game, Vector3 position, Vector3 target, Color color, float intensity, bool castShadow)
            : base(game, position, color, intensity, castShadow)
        {
            this.target = target;
            this.ShadowMod = .000000509d;
        }

        public new Matrix View
        {
            get
            {
                return Matrix.CreateLookAt(position, target, Vector3.Transform(Vector3.Forward, Matrix.Invert(Matrix.CreateTranslation(position))));
            }
        }
        public new Matrix Projection
        {
            get
            {
                return Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4 / 2, camera.Viewport.AspectRatio, camera.Viewport.MinDepth, camera.Viewport.MaxDepth);
                //return Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, camera.Viewport.AspectRatio, camera.Viewport.MinDepth, camera.Viewport.MaxDepth);                
            }
        }

        #region IDirectionalLight Members

        public Vector3 Target
        {
            get { return target; }
            set { target = value; }
        }

        public Vector3 Direction
        {
            get { return target - position; }
        }

        #endregion
    }
}
