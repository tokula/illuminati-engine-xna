using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using IlluminatiContentClasses;

namespace IlluminatiEngine
{
    public class BaseDeferredSkinnedObject : BaseDeferredObject 
    {
        AnimationPlayer animationPlayer;
        Matrix[] bones;

        public string AnimationClip;

        public BaseDeferredSkinnedObject(Game game) : base(game)
        {
            effect = "shaders/deferred/DeferredSkinnedModelRender";
        }

        public override void Update(GameTime gameTime)
        {
            if(animationPlayer != null)
                animationPlayer.Update(gameTime.ElapsedGameTime, true, World);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, Effect effect)
        {
            if (meshData != null)
            {
                if (meshData.Keys.Contains("SkinningData") && animationPlayer == null)
                {
                    animationPlayer = new AnimationPlayer((SkinningData)meshData["SkinningData"]);
                    if (!string.IsNullOrEmpty(AnimationClip))
                        animationPlayer.StartClip(animationPlayer.skinningDataValue.AnimationClips[AnimationClip]);
                }
            }

            if (animationPlayer != null)
            {
                bones = animationPlayer.GetSkinTransforms();
                if (effect.Parameters["Bones"] != null)
                    effect.Parameters["Bones"].SetValue(bones);

                if (effect.CurrentTechnique.Name == "ShadowMap") // Shadow Map
                { }
                else
                {
                    if (effect.Parameters["vp"] != null)
                        effect.Parameters["vp"].SetValue(Camera.View * Camera.Projection);

                    if (effect.Parameters["v"] != null)
                        effect.Parameters["v"].SetValue(Camera.View);

                    if (effect.Parameters["p"] != null)
                        effect.Parameters["p"].SetValue(Camera.Projection);
                }
            }

            base.Draw(gameTime, effect);            
        }

    }
}
