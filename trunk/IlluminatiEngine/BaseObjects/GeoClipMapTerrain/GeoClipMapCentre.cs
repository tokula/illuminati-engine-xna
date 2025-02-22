﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine
{
    public class GeoClipMapCentre : BaseDeferredObject
    {
        protected GeoClipMapFootPrintBlock block;

        protected Effect effect;

        public Vector3 Position = Vector3.Zero;
        public Vector3 Scale = Vector3.One;
        public Quaternion Orientation = Quaternion.Identity;

        public Matrix World = Matrix.Identity;

       
        public GeoClipMapCentre(Game game, short n)
            : base(game)
        {
            short m = (short)((n + 1) / 4);
            short edge = (short)((n / 2) - 1);

            block = new GeoClipMapFootPrintBlock(game, n);

            block.Position = new Vector3(-edge, 0, -edge);
            block.color = Color.MintCream;
        }

        public override void Initialize()
        {
            base.Initialize();

            block.Initialize();
        }

        protected override void LoadContent()
        {
            //effect = Game.Content.Load<Effect>("Shaders/GeoClipMapLayer");
            effect = AssetManager.GetAsset<Effect>("Shaders/Terrain/GeoClipMapLayer");


        }

        public override void Update(GameTime gameTime)
        {
            World = Matrix.CreateScale(Scale) * Matrix.CreateFromQuaternion(Orientation) * Matrix.CreateTranslation(Position);
            base.Update(gameTime);

            block.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            effect.Parameters["world"].SetValue(World);
            effect.Parameters["wvp"].SetValue(World * Camera.View * Camera.Projection);

            effect.CurrentTechnique.Passes[0].Apply();

            //if (camera.Frustum.Intersects(block.getBounds(World)))
            block.Draw(gameTime);

        }

        public void Rotate(Vector3 axis, float angle)
        {
            axis = Vector3.Transform(axis, Matrix.CreateFromQuaternion(Orientation));
            Orientation = Quaternion.Normalize(Quaternion.CreateFromAxisAngle(axis, angle) * Orientation);
        }
    }
}
