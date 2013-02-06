using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine
{
    public class DeferredSkyBox : BaseDeferredObject
    {
        public float Alpha = 1;

        public string textureAsset;

        public DeferredSkyBox(Game game, string textureAsset) : base(game)
        {
            this.textureAsset = textureAsset;
            mesh = "Models/SkyBox";

            rotation = Quaternion.Identity;
            scale = new Vector3(99, 99, 99);
            effect = "Shaders/Deferred/DeferredSkyBoxRender";
        }

        public override void Draw(GameTime gameTime)
        {
            this.Draw(gameTime, AssetManager.GetAsset<Effect>(effect));
        }
        public override void Draw(GameTime gameTime, Effect effect)
        {
            if (effect.CurrentTechnique.Name.Contains("Shadow"))
                return;
            if (Enabled && Visible)
            {
                if (thisMesh == null)
                {
                    thisMesh = AssetManager.GetAsset<Model>(mesh);
                }

                Matrix World = Matrix.CreateScale(Scale) *
                                Matrix.CreateFromQuaternion(rotation) * 
                                Matrix.CreateTranslation(Camera.Position);

                effect.Parameters["World"].SetValue(World);
                effect.Parameters["View"].SetValue(Camera.View);
                effect.Parameters["Projection"].SetValue(Camera.Projection);
                effect.Parameters["surfaceTexture"].SetValue(AssetManager.GetAsset<TextureCube>(textureAsset));

                effect.Parameters["EyePosition"].SetValue(Camera.Position);
                effect.Parameters["alpha"].SetValue(Alpha);

                effect.CurrentTechnique.Passes[0].Apply();

                int cnt = thisMesh.Meshes.Count;
                for (int p = 0; p < cnt; p++)
                {
                    int cnt2 = thisMesh.Meshes[p].MeshParts.Count;
                    for (int mp = 0; mp < cnt2; mp++)
                    {
                        Game.GraphicsDevice.SetVertexBuffer(thisMesh.Meshes[p].MeshParts[mp].VertexBuffer);
                        Game.GraphicsDevice.Indices = thisMesh.Meshes[p].MeshParts[mp].IndexBuffer;
                        Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, thisMesh.Meshes[p].MeshParts[mp].VertexOffset, 0, thisMesh.Meshes[p].MeshParts[mp].NumVertices, thisMesh.Meshes[p].MeshParts[mp].StartIndex, thisMesh.Meshes[p].MeshParts[mp].PrimitiveCount);
                    }
                }
            }
        }
    }
}
