using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.Renderer.Deferred
{
    public class DeferredConeLight : BaseLight, IConeLight
    {
        protected float angle;
        protected float decay;
        protected Vector3 target;
        protected Quaternion rotation;

        public DeferredConeLight(Game game, Vector3 position, Vector3 target, Color color, float intensity, float angle, float decay, bool castShadow)
            : base(game, position, color, intensity, castShadow)
        {
            this.angle = angle;
            this.decay = decay;
            this.target = target;

            this.rotation = Quaternion.Identity;

            this.ShadowMod = .0001d;
        }

        protected Matrix RotationMatrix
        {
            get { return Matrix.CreateFromQuaternion(rotation); }
        }
        protected Matrix TraslationMatrix
        {
            get { return Matrix.CreateTranslation(position); }
        }
        protected Matrix World
        {
            get { return RotationMatrix * TraslationMatrix; }
        }
        public Vector3 Direction
        {
            get
            {
                return target - position;
            }
        }
        public Quaternion Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value;
                target = Vector3.Transform(Vector3.Forward, World);
            }
        }
        public void Rotate(Vector3 axis, float angle)
        {
            axis = Vector3.Transform(axis, Matrix.CreateFromQuaternion(rotation));
            rotation = Quaternion.Normalize(Quaternion.CreateFromAxisAngle(axis, angle) * rotation);
        }
        public new Matrix View
        {
            get
            {
                return Matrix.CreateLookAt(position, target, Vector3.Transform(Vector3.Forward, Matrix.Invert(World)));
            }
        }
        public new Matrix Projection
        {
            get
            {
                return Matrix.CreatePerspectiveFieldOfView((float)Math.Acos(angle) * 2, camera.Viewport.AspectRatio, camera.Viewport.MinDepth, camera.Viewport.MaxDepth);
            }
        }

        #region IConeLight Members

        public float Angle
        {
            get { return angle; }
            set { angle = value; }
        }

        public float Decay
        {
            get { return decay; }
            set { decay = value; }
        }

        public Vector3 Target
        {
            get { return target; }
            set { target = value; }
        }

        #endregion

    }
}
