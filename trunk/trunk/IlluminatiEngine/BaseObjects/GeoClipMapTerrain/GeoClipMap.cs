#if WINDOWS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using IlluminatiEngine.Kinect;

namespace IlluminatiEngine
{
    // http://http.developer.nvidia.com/GPUGems2/gpugems2_chapter02.html

    public class GeoClipMap : BaseDeferredObject
    {
        protected GeoClipMapLayer[] layers = new GeoClipMapLayer[3];
        protected GeoClipMapCentre centre;

       public Vector3 Offset = new Vector3(1, 0, -1);

        public string TerrainMap;

        public GeoClipMap(Game game, short n, string terrainAsset)
            : base(game)
        {

            Game.Services.AddService(this.GetType(), this);

            TerrainMap = terrainAsset;
            for (int l = 0; l < layers.Length; l++)
            {
                layers[l] = new GeoClipMapLayer(game, n, TerrainMap);
                layers[l].Scale = Scale;

                for (int s = 0; s < l; s++)
                    layers[l].Scale /= 2;

                if (l != 0)
                    layers[l].CorseScale = layers[l].Scale;

                if (l != 0 && ((l + 1) % 2) == 0)
                {
                    //layers[l].Rotate(Vector3.Up, MathHelper.Pi);
                }
            }

            centre = new GeoClipMapCentre(game, n);
            centre.Scale = Scale;

            int lc = layers.Length;
            for (int s = 0; s < lc; s++)
                centre.Scale /= 2;
        }

#if WINDOWS
        public GeoClipMap(Game game, short n)
            : base(game)
        {

            Game.Services.AddService(this.GetType(), this);

            for (int l = 0; l < layers.Length; l++)
            {
                layers[l] = new GeoClipMapLayer(game, n);
                layers[l].Scale = Scale;

                for (int s = 0; s < l; s++)
                    layers[l].Scale /= 2;

                if (l != 0)
                    layers[l].CorseScale = layers[l].Scale;

                if (l != 0 && ((l + 1) % 2) == 0)
                {
                    //layers[l].Rotate(Vector3.Up, MathHelper.Pi);
                }
            }

            centre = new GeoClipMapCentre(game, n);
            centre.Scale = Scale;

            int lc = layers.Length;
            for (int s = 0; s < lc; s++)
                centre.Scale /= 2;
        }
#endif
        public override void Initialize()
        {
            base.Initialize();

            for (int l = 0; l < layers.Length; l++)
                layers[l].Initialize();

            centre.Initialize();

        }
        //Texture2D heightMap;
        protected override void LoadContent()
        {
            //effect = Game.Content.Load<Effect>("Shaders/GeoClipMapLayer");

            //heightMap = Game.Content.Load<Texture2D>("Textures/TerrainMaps/Level1Section1");

            //Color[] data = new Color[heightMap.Width * heightMap.Height];
            //heightMap.GetData<Color>(data);

            //heightMap = new Texture2D(Game.GraphicsDevice, heightMap.Width, heightMap.Height, true, SurfaceFormat.Vector4);
            //Vector4[] data2 = new Vector4[data.Length];
            //for (int x = 0; x < data.Length; x++)
            //    data2[x] = new Vector4(data[x].R / 255f, data[x].G / 255f, data[x].B / 255f, data[x].A / 255f);
            //heightMap.SetData<Vector4>(data2);

            //effect.Parameters["heightMap"].SetValue(heightMap);

        }
        Vector4[] data;
        public float GetHeightAt(Vector3 pos)
        {
            if (layers[0].heightMap == null)
                return 0;

            float val = 0;

            // put position into text coords.
            Vector2 xy = new Vector2(MathHelper.Clamp(Math.Abs(pos.X*8), 0, layers[0].heightMap.Width-1), MathHelper.Clamp(Math.Abs(pos.Z*8), 0, layers[0].heightMap.Height-1));
            

            if(data == null)
            {
                data = new Vector4[layers[0].heightMap.Width * layers[0].heightMap.Height];
                layers[0].heightMap.GetData<Vector4>(data);
            }

            val = data[(int)(xy.X * xy.Y)].X;
             

            return val;
        }

        public override void Update(GameTime gameTime)
        {
            Position = new Vector3((int)Camera.Position.X, Position.Y, (int)Camera.Position.Z);

            for (uint l = 0; l < layers.Length; l++)
            {
                layers[l].Position = Position;

                if (l != 0)
                {
                    if (l == 1)
                        layers[l].Position = Position - new Vector3(.5f, 0, -.5f);
                    if (l == 2)
                        layers[l].Position = Position - new Vector3(.75f, 0, -.75f);

                }

                layers[l].Update(gameTime);
            }

            centre.Position = Position - new Vector3(1f, 0, -.75f);
            centre.Update(gameTime);
        }
        public override void Draw(GameTime gameTime, Effect effect)
        {
            this.Draw(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            if (!Visible)
                return;
            //effect.Parameters["world"].SetValue(Matrix.Identity);
            //effect.Parameters["wvp"].SetValue(Matrix.Identity * camera.View * camera.Projection);
            //effect.Parameters["vp"].SetValue(camera.View * camera.Projection);
            //effect.Parameters["camPos"].SetValue(camera.Position);

            //effect.Parameters["sqrt"].SetValue(new Vector2(heightMap.Width, heightMap.Height) / 4);

            //effect.Parameters["halfPixel"].SetValue(-new Vector2(.5f / (float)Game.GraphicsDevice.Viewport.Width,
            //                         .5f / (float)Game.GraphicsDevice.Viewport.Height));
            //effect.Parameters["hp"].SetValue(new Vector2(heightMap.Width, heightMap.Height) / 2);
            //effect.Parameters["OneOverWidth"].SetValue(1f / heightMap.Width);

            //effect.CurrentTechnique.Passes[0].Apply();

            for (int l = 0; l < layers.Length; l++)
                layers[l].Draw(gameTime);

            centre.Draw(gameTime);
        }
    }
}
#endif
