using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.PostProcessing
{
    public class SSAOEffect : BasePostProcessingEffect
    {
        public SSAO ssao;
        public WorldPositionMap posMap;
        public PoissonDiscBlur blur;
        public SceneBlend blend;

        public float rad
        {
            get { return ssao.rad; }
            set { ssao.rad = value; }
        }
        public float intensity
        {
            get { return ssao.intensity; }
            set { ssao.intensity = value; }
        }
        public float scale
        {
            get { return ssao.scale; }
            set { ssao.scale = value; }
        }
        public float bias
        {
            get { return ssao.bias; }
            set { ssao.bias = value; }
        }

        public SSAOEffect(Game game, float radius, float intensity, float scale, float bias)
            : base(game)
        {
            ssao = new SSAO(game, radius, intensity, scale, bias);

            posMap = new WorldPositionMap(game);

            blur = new PoissonDiscBlur(game);
            blur.Sampler = SamplerState.PointClamp;

            blend = new SceneBlend(game);
            blend.Blend = true;
            blend.Sampler = SamplerState.PointClamp;


            AddPostProcess(posMap);
            AddPostProcess(ssao);
            AddPostProcess(blur);
            AddPostProcess(blend);
        }
        public override void Draw(GameTime gameTime, Texture2D scene, Texture2D depth, Texture2D normal)
        {
            if (!Enabled)
                return;

            orgScene = scene;

            int maxProcess = postProcesses.Count;
            lastScene = null;

            for (int p = 0; p < maxProcess; p++)
            {
                if (postProcesses[p].Enabled)
                {
                    // Set Half Pixel value.
                    if (postProcesses[p].HalfPixel == Vector2.Zero)
                        postProcesses[p].HalfPixel = HalfPixel;

                    // Set original scene
                    postProcesses[p].orgBuffer = orgScene;

                    // Ready render target if needed.
                    if (postProcesses[p].newScene == null)
                    {
                        if (postProcesses[p] is WorldPositionMap)
                            postProcesses[p].newScene = new RenderTarget2D(Game.GraphicsDevice, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height, false, SurfaceFormat.Vector4, DepthFormat.None);
                        else
                            postProcesses[p].newScene = new RenderTarget2D(Game.GraphicsDevice, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
                    }

                    Game.GraphicsDevice.SetRenderTarget(postProcesses[p].newScene);

                    // Has the scene been rendered yet (first effect may be disabled)
                    if (lastScene == null)
                        lastScene = orgScene;

                    postProcesses[p].BackBuffer = lastScene;

                    postProcesses[p].DepthBuffer = depth;
                    postProcesses[p].normalBuffer = normal;
                    Game.GraphicsDevice.Textures[0] = postProcesses[p].BackBuffer;
                    postProcesses[p].Draw(gameTime);

                    Game.GraphicsDevice.SetRenderTarget(null);

                    lastScene = postProcesses[p].newScene;
                }
            }

            if (lastScene == null)
                lastScene = scene;
        }
    }
}
