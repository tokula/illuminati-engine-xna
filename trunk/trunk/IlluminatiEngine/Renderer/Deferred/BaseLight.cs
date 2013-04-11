using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.Renderer
{
    public class BaseLight : ILight
    {
        public static int ID;
        protected Vector3 position;
        protected Color color;
        protected float intensity;
        protected bool castShadow;
        protected string name;

        protected RenderTarget2D shadowMap;
        protected RenderTarget2D softShadowMap;

        protected Game Game;

        protected ICameraService camera
        {
            get { return ((ICameraService)Game.Services.GetService(typeof(ICameraService))); }
        }

        #region ILight Members

        public RenderTarget2D ShadowMap
        {
            get { return shadowMap; }
            set { shadowMap = value; }
        }

        public RenderTarget2D SoftShadowMap
        {
            get { return softShadowMap; }
            set { softShadowMap = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public float Intensity
        {
            get { return intensity; }
            set { intensity = value; }
        }

        public bool CastShadow
        {
            get { return castShadow; }
            set { castShadow = value; }
        }

        public double ShadowMod { get; set; }

        public Matrix View
        {
            get { return Matrix.Identity; }
        }

        public Matrix Projection
        {
            get { return Matrix.Identity; }
        }

        #endregion

        public BaseLight(Vector3 position, Color color, float intensity, bool castShadow)
        {
            ID++;
            this.position = position;
            this.color = color;
            this.intensity = intensity;
            this.castShadow = castShadow;

            name = string.Format("{0}{1}", this.GetType().ToString(), ID);
        }
        public BaseLight(Game game, Vector3 position, Color color, float intensity, bool castShadow)
            : this(position, color, intensity, castShadow)
        {
            Game = game;
        }
    }
}
