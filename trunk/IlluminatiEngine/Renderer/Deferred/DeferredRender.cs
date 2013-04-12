using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.Renderer.Deferred
{
    public class DeferredRender : DrawableComponentService
    {
        public float WaterHeight { get; set; }
        public bool StopRender { get; set; }
        public DrawableGameComponent CanDrawNonDeferred { get; set; }
        public DrawableGameComponent CanDrawDeferred { get; set; }
        public RenderTarget2D colorMap;
        public RenderTarget2D SGRMap;
        public RenderTarget2D normalMap;
        public RenderTarget2D depthMap;
        public RenderTarget2D lightMap;
        public RenderTarget2D finalBackBuffer;
        //RenderTarget2D db2;
        public RenderTarget2D blendedDepthBuffer;
        public RenderTarget2D finalDepthBuffer;

        Effect clearDeferredBufferEffect;
        Effect deferredDirectionalLightEffect;
        Effect deferredConeLightEffect;
        Effect deferredPointLightEffect;
        Effect deferredSceneRenderEffect;

        Effect deferredShadowEffect;
        Effect deferredSkinnedShadowEffect;

        Model pointLightVolumeModel;

        public List<IDirectionalLight> DirectionalLights = new List<IDirectionalLight>();
        public List<IPointLight> PointLights = new List<IPointLight>();
        public List<IConeLight> ConeLights = new List<IConeLight>();

        public Color ClearColor = Color.Black;

        ScreenQuad sceneQuad;
        Vector2 halfPixel;
        Texture2D t;
        int w, h;

        public bool DebugDeferred = false;

        SpriteBatch spriteBatch;

        IAssetManager AssetManager
        {
            get { return ((IAssetManager)Game.Services.GetService(typeof(IAssetManager))); }
        }

        ICameraService Camera
        {
            get { return ((ICameraService)Game.Services.GetService(typeof(ICameraService))); }
        }

        public DeferredRender(Game game) : base(game) 
        {
            sceneQuad = new ScreenQuad(game);
        }

        public override void Draw(GameTime gameTime)
        {
            // Get reflection map
            if (((BaseDeferredRenderGame)Game).CreateWaterReflectionMap)
                GetReflectionMap(gameTime, WaterHeight);

            RenderDeferred(gameTime);            
        }
        public override void Initialize()
        {
            sceneQuad.Initialize();
            base.Initialize();
        }
        protected override void LoadContent()
        {
            colorMap = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            SGRMap = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            normalMap = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Rgba1010102, DepthFormat.Depth24Stencil8);
            depthMap = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
            lightMap =    new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            GameComponentHelper.reflectionMap = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            GameComponentHelper.reflectionSGRMap = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

            preLightMap = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            SSSMBase = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            SSSMBlur = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            SSSM = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

            finalBackBuffer = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            finalDepthBuffer = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
            blendedDepthBuffer = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Rg32, DepthFormat.Depth24Stencil8);

            halfPixel = -new Vector2(.5f / (float)GraphicsDevice.Viewport.Width,
                                     .5f / (float)GraphicsDevice.Viewport.Height);

            w = GraphicsDevice.Viewport.Width / 7;
            h = GraphicsDevice.Viewport.Height / 5;

            t = new Texture2D(GraphicsDevice, 1, 1);
            Color[] c = new Color[] { Color.DarkGray };
            t.SetData(c);

            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }
        
        public void RenderDeferred(GameTime gameTime)
        {
            if(!StopRender)
                InitializeDeferredRender();
            //ClearDeferredRenderBuffer();
            GraphicsDevice.Clear(ClearColor);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;            

            if (!StopRender)
            {
                int cnt = Game.Components.Count;
                for (int c = 0; c < cnt; c++)
                {
                    // Do draw
                    if (Game.Components[c] is IDeferredRender)
                        ((IDeferredRender)Game.Components[c]).Draw(gameTime);                                     
                }
            }
            else
            {
                if (CanDrawDeferred != null && CanDrawDeferred is IDeferredRender)
                    CanDrawDeferred.Draw(gameTime);
            }
            
            ResolveDeferredRender();

            // Clear the light map...
            //ClearDeferredRenderBuffer();

            // Create shadow Map
            if (!StopRender)
            {
                DeferredShaddows(gameTime);                
                DeferredLighting(gameTime);


                GraphicsDevice.SetRenderTargets(finalBackBuffer, blendedDepthBuffer);
                DrawDeferred();

                int cnt = Game.Components.Count;
                for (int c = 0; c < cnt; c++)
                {
                    if (Game.Components[c] != this && Game.Components[c] is DrawableGameComponent && !(Game.Components[c] is IDeferredRender))
                        ((DrawableGameComponent)Game.Components[c]).Draw(gameTime);
                }

                GraphicsDevice.SetRenderTarget(null);

                // Combine the depth buffers..
                BlendDepth(blendedDepthBuffer, depthMap);
            }
            else
            {
                if (CanDrawNonDeferred != null && !(CanDrawNonDeferred is IDeferredRender))
                    CanDrawNonDeferred.Draw(gameTime);
            }
        }

        public bool ScreenSpaceShadows = false;
        RenderTarget2D SSSMBase;
        RenderTarget2D SSSMBlur;
        RenderTarget2D SSSM;
        Effect SSSMS;
        Effect SSSMB;
        public void DeferredScreenSpaceShadowDLRT(ILight light)
        {
            // Now V an H blur the image..
            if (SSSMB == null)
                SSSMB = AssetManager.GetAsset<Effect>("Shaders/PostProcessing/Blur");

            if (SSSMS == null)
                SSSMS = AssetManager.GetAsset<Effect>("Shaders/SSSM");

            GraphicsDevice.SetRenderTarget(SSSMBase);
            GraphicsDevice.Clear(Color.Transparent);

            SSSMS.Parameters["CastShadow"].SetValue(light.CastShadow);
            SSSMS.Parameters["shadowMod"].SetValue((float)light.ShadowMod);
            SSSMS.Parameters["intensity"].SetValue(light.Intensity);
            
            SSSMS.Parameters["viewProjectionInv"].SetValue(Matrix.Invert(Camera.View * Camera.Projection));
            SSSMS.Parameters["lightViewProjection"].SetValue(light.View * light.Projection);

            SSSMS.Parameters["shadowMap"].SetValue(light.ShadowMap);
            SSSMS.Parameters["depthMap"].SetValue(depthMap);

            //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            SSSMS.CurrentTechnique.Passes[0].Apply();
            sceneQuad.Draw(-Vector2.One, Vector2.One);
            //spriteBatch.Draw(t, GraphicsDevice.Viewport.TitleSafeArea, Color.White);
            //spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            // Blur it.
            GraphicsDevice.SetRenderTarget(SSSMBlur);
            GraphicsDevice.Clear(Color.Transparent);

            SSSMB.Parameters["g_BlurAmount"].SetValue(1);

            SSSMB.CurrentTechnique = SSSMB.Techniques["BlurH"];
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            SSSMB.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(SSSMBase, GraphicsDevice.Viewport.TitleSafeArea, Color.White);
            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            GraphicsDevice.SetRenderTarget(light.SoftShadowMap);
            GraphicsDevice.Clear(Color.Transparent);

            SSSMB.CurrentTechnique = SSSMB.Techniques["BlurV"];
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            SSSMB.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(SSSMBlur, GraphicsDevice.Viewport.TitleSafeArea, Color.White);
            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            
        }

        
        Effect depthBlender;
        public void BlendDepth(Texture2D buff1, Texture2D buff2)
        {
            if (depthBlender == null)
                depthBlender = AssetManager.GetAsset<Effect>("Shaders/DepthBlender");

            GraphicsDevice.SetRenderTarget(finalDepthBuffer);

            depthBlender.Parameters["buff2"].SetValue(buff2);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            depthBlender.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(buff1, GraphicsDevice.Viewport.TitleSafeArea, Color.White);
            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);   
        }

        Effect lightSoftShadow;
        public void BlendSoftShadowMap(Texture2D shadows)
        {
            if (lightSoftShadow == null)
                lightSoftShadow = AssetManager.GetAsset<Effect>("Shaders/SoftShadowBlend");

            lightSoftShadow.Parameters["sceneMap"].SetValue(shadows);
            //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            lightSoftShadow.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.BlendState = BlendState.Additive;
            sceneQuad.Draw(-Vector2.One, Vector2.One);
            //spriteBatch.Draw(shadows, GraphicsDevice.Viewport.TitleSafeArea, Color.White);
            //spriteBatch.End();
        }

        Effect lightShadow;
        public void BlendLightAndSoftShadowMap(Texture2D light, Texture2D shadows)
        {
            if (lightShadow == null)
                lightShadow = AssetManager.GetAsset<Effect>("Shaders/CombineLightAndSoftShadows");

            lightShadow.Parameters["buff2"].SetValue(shadows);
            lightShadow.Parameters["sceneMap"].SetValue(light);
            //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            GraphicsDevice.BlendState = BlendState.Additive;
            lightShadow.CurrentTechnique.Passes[0].Apply();
            sceneQuad.Draw(-Vector2.One, Vector2.One);
            //spriteBatch.Draw(light, GraphicsDevice.Viewport.TitleSafeArea, Color.White);            
            //spriteBatch.End();
        }
        
        public void InitializeDeferredRender()
        {
            GraphicsDevice.SetRenderTargets(colorMap, SGRMap, normalMap, depthMap);
        }
        
        public void ClearDeferredRenderBuffer()
        {
            if (clearDeferredBufferEffect == null)
                clearDeferredBufferEffect = AssetManager.GetAsset<Effect>("Shaders/Deferred/ClearDeferred");

            GraphicsDevice.Clear(ClearColor);

            clearDeferredBufferEffect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.BlendState = BlendState.Opaque;

            sceneQuad.Draw(-Vector2.One, Vector2.One);
        }

        Effect DepthRender;
        public void RenderDebug()
        {
            if (DebugDeferred)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);

                spriteBatch.Draw(t, new Rectangle(0, 0, w * 7, h + 2), Color.White);

                spriteBatch.Draw(finalBackBuffer, new Rectangle(1, 1, w, h), Color.White);
                spriteBatch.Draw(SGRMap, new Rectangle(w + 2, 1, w, h), Color.White);
                spriteBatch.Draw(normalMap, new Rectangle((w * 2) + 3, 1, w, h), Color.White);

                GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

                // depth maps are here..
                
                //spriteBatch.Draw(GameComponentHelper.reflectionMap, new Rectangle((w * 4) + 5, 1, w, h), Color.White);
                if (SSSM != null)
                    spriteBatch.Draw(SSSM, new Rectangle((w * 4) + 5, 1, w, h), Color.White);

                spriteBatch.Draw(lightMap, new Rectangle((w * 6) + 7, 1, w, h), Color.White);
                //spriteBatch.Draw(preLightMap, new Rectangle((w * 6) + 7, 1, w, h), Color.White);
                

                spriteBatch.End();

                if (DepthRender == null)
                    DepthRender = AssetManager.GetAsset<Effect>("Shaders/depthRender");

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                DepthRender.CurrentTechnique.Passes[0].Apply();
                GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
                //spriteBatch.Draw(blendedDepthBuffer, new Rectangle((w * 4) + 5, 1, w, h), Color.White);
                spriteBatch.Draw(depthMap, new Rectangle((w * 3) + 4, 1, w, h), Color.White);
                spriteBatch.Draw(finalDepthBuffer, new Rectangle((w * 5) + 6, 1, w, h), Color.White);
                spriteBatch.End();
            }
        }
        public void ResolveDeferredRender()
        {
            GraphicsDevice.SetRenderTarget(null);
        }
        List<IDirectionalLight> dLights = new List<IDirectionalLight>();
        List<IPointLight> pLights = new List<IPointLight>();
        List<IConeLight> cLights = new List<IConeLight>();

        RenderTarget2D preLightMap;
        public void DeferredLighting(GameTime gameTime)
        {
            if (ScreenSpaceShadows)
            {
                GraphicsDevice.SetRenderTarget(preLightMap);
            }
            else
                GraphicsDevice.SetRenderTarget(lightMap);

            GraphicsDevice.Clear(Color.Transparent);
            GraphicsDevice.BlendState = BlendState.Additive;
            //use the same operation on the alpha channel
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

            dLights = new List<IDirectionalLight>(DirectionalLights.Where(entity => entity.Intensity > 0 && entity.Color != Color.Black));            
            int cnt = dLights.Count;
            for (int l = 0; l < cnt; l++)
                RenderDirectionalLight(dLights[l]);

            pLights = new List<IPointLight>(PointLights.Where(entity => entity.Intensity > 0 && entity.Color != Color.Black));
            cnt = pLights.Count;
            for (int l = 0; l < cnt; l++)
                RenderPointLight(pLights[l]);

            cLights = new List<IConeLight>(ConeLights.Where(entity => entity.Intensity > 0 && entity.Color != Color.Black));
            cnt = cLights.Count;
            for (int l = 0; l < cnt; l++)
                RenderConeLight(cLights[l]);

            GraphicsDevice.SetRenderTarget(null);

            if (ScreenSpaceShadows)
            {
                dLights = new List<IDirectionalLight>(DirectionalLights.Where(entity => entity.Intensity > 0 && entity.Color != Color.Black && entity.CastShadow == true));
                pLights = new List<IPointLight>(PointLights.Where(entity => entity.Intensity > 0 && entity.Color != Color.Black && entity.CastShadow == true));
                cLights = new List<IConeLight>(ConeLights.Where(entity => entity.Intensity > 0 && entity.Color != Color.Black && entity.CastShadow == true));

                GraphicsDevice.SetRenderTarget(SSSM);
                GraphicsDevice.Clear(Color.Transparent);

                cnt = dLights.Count;
                for (int l = 0; l < cnt; l++)
                    BlendSoftShadowMap(dLights[l].SoftShadowMap);

                cnt = pLights.Count;
                for (int l = 0; l < cnt; l++)
                    BlendSoftShadowMap(pLights[l].SoftShadowMap);

                cnt = cLights.Count;
                for (int l = 0; l < cnt; l++)
                    BlendSoftShadowMap(cLights[l].SoftShadowMap);

                GraphicsDevice.SetRenderTarget(null);

                GraphicsDevice.SetRenderTarget(lightMap);
                GraphicsDevice.Clear(Color.Black);

                BlendLightAndSoftShadowMap(preLightMap, SSSM);
                
                GraphicsDevice.SetRenderTarget(null);
            }
            else
            {
                GraphicsDevice.SetRenderTarget(preLightMap);
                GraphicsDevice.Clear(Color.Transparent);
                GraphicsDevice.SetRenderTarget(null);
            }


            GameComponentHelper.lightMap = lightMap;
        }       

        public void RenderDirectionalLight(IDirectionalLight directionalLight)
        {
            if (deferredDirectionalLightEffect == null)
                deferredDirectionalLightEffect = AssetManager.GetAsset<Effect>("Shaders/Deferred/DeferredDirectionalLight");

            // Call lighting methods.
            // Load Light Params
            deferredDirectionalLightEffect.Parameters["halfPixel"].SetValue(halfPixel);
            deferredDirectionalLightEffect.Parameters["lightDirection"].SetValue(directionalLight.Direction);
            deferredDirectionalLightEffect.Parameters["Color"].SetValue(directionalLight.Color.ToVector3());

            deferredDirectionalLightEffect.Parameters["normalMap"].SetValue(normalMap);
            deferredDirectionalLightEffect.Parameters["depthMap"].SetValue(depthMap);
            deferredDirectionalLightEffect.Parameters["power"].SetValue(directionalLight.Intensity);
            deferredDirectionalLightEffect.Parameters["sgrMap"].SetValue(SGRMap);

            deferredDirectionalLightEffect.Parameters["shadowMod"].SetValue((float)directionalLight.ShadowMod);

            deferredDirectionalLightEffect.Parameters["cameraPosition"].SetValue(Camera.Position);

            if (!ScreenSpaceShadows)
            {
                deferredDirectionalLightEffect.Parameters["CastShadow"].SetValue(directionalLight.CastShadow);
                if (directionalLight.CastShadow)
                {
                    deferredDirectionalLightEffect.Parameters["shadowMap"].SetValue(directionalLight.ShadowMap);
                    //SaveJpg(directionalLight.ShadowMap, "shadows.jpg");
                }
            }
            deferredDirectionalLightEffect.Parameters["viewProjectionInv"].SetValue(Matrix.Invert(Camera.View * Camera.Projection));
            deferredDirectionalLightEffect.Parameters["lightViewProjection"].SetValue(directionalLight.View * directionalLight.Projection);

            deferredDirectionalLightEffect.Techniques[0].Passes[0].Apply();

            sceneQuad.Draw(-Vector2.One, Vector2.One);
        }

        public void RenderConeLight(IConeLight coneLight)
        {
            if (deferredConeLightEffect == null)
                deferredConeLightEffect = AssetManager.GetAsset<Effect>("Shaders/Deferred/DeferredConeLight");

            // Call lighting methods.
            // Load Light Params
            deferredConeLightEffect.Parameters["halfPixel"].SetValue(halfPixel);
            deferredConeLightEffect.Parameters["lightDirection"].SetValue(coneLight.Direction);
            deferredConeLightEffect.Parameters["LightPosition"].SetValue(coneLight.Position);
            deferredConeLightEffect.Parameters["Color"].SetValue(coneLight.Color.ToVector3());

            deferredConeLightEffect.Parameters["ViewProjectionInv"].SetValue(Matrix.Invert(Camera.View * Camera.Projection));
            deferredConeLightEffect.Parameters["LightViewProjection"].SetValue(coneLight.View * coneLight.Projection);

            deferredConeLightEffect.Parameters["normalMap"].SetValue(normalMap);
            deferredConeLightEffect.Parameters["depthMap"].SetValue(depthMap);
            deferredConeLightEffect.Parameters["power"].SetValue(coneLight.Intensity);

            deferredConeLightEffect.Parameters["ConeAngle"].SetValue(coneLight.Angle);
            deferredConeLightEffect.Parameters["ConeDecay"].SetValue(coneLight.Decay);

            deferredConeLightEffect.Parameters["shadowMod"].SetValue((float)coneLight.ShadowMod);

            if (!ScreenSpaceShadows)
            {
                deferredConeLightEffect.Parameters["CastShadow"].SetValue(coneLight.CastShadow);
                if (coneLight.CastShadow)
                    deferredConeLightEffect.Parameters["shadowMap"].SetValue(coneLight.ShadowMap);
            }

            deferredConeLightEffect.CurrentTechnique.Passes[0].Apply();
            // Set sampler state to Point as the Surface type requires it in XNA 4.0
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            sceneQuad.Draw(-Vector2.One, Vector2.One);
        }


        Matrix[] transforms;

        public void RenderPointLight(IPointLight pointLight)
        {
            if (deferredPointLightEffect == null)
                deferredPointLightEffect = AssetManager.GetAsset<Effect>("Shaders/Deferred/DeferredPointLight");

            if (pointLightVolumeModel == null)
                pointLightVolumeModel = AssetManager.GetAsset<Model>("Models/sphere2");

            //set the G-Buffer parameters
            deferredPointLightEffect.Parameters["halfPixel"].SetValue(halfPixel);
            deferredPointLightEffect.Parameters["normalMap"].SetValue(normalMap);
            deferredPointLightEffect.Parameters["depthMap"].SetValue(depthMap);
            deferredPointLightEffect.Parameters["sgrMap"].SetValue(SGRMap);

            //compute the light world matrix
            //scale according to light radius, and translate it to light position
            Matrix sphereWorldMatrix = Matrix.CreateScale(pointLight.Radius) * Matrix.CreateTranslation(pointLight.Position);
            deferredPointLightEffect.Parameters["World"].SetValue(sphereWorldMatrix);
            deferredPointLightEffect.Parameters["View"].SetValue(Camera.View);
            deferredPointLightEffect.Parameters["Projection"].SetValue(Camera.Projection);
            //light position
            deferredPointLightEffect.Parameters["lightPosition"].SetValue(pointLight.Position);

            //set the color, radius and Intensity
            deferredPointLightEffect.Parameters["Color"].SetValue(pointLight.Color.ToVector3());
            deferredPointLightEffect.Parameters["lightRadius"].SetValue(pointLight.Radius);
            deferredPointLightEffect.Parameters["lightIntensity"].SetValue(pointLight.Intensity);

            //parameters for specular computations
            deferredPointLightEffect.Parameters["cameraPosition"].SetValue(Camera.Position);
            deferredPointLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(Camera.View * Camera.Projection));

            if (transforms == null || transforms.Length < pointLightVolumeModel.Bones.Count)
            {
                transforms = new Matrix[pointLightVolumeModel.Bones.Count];
            }
            
            pointLightVolumeModel.CopyAbsoluteBoneTransformsTo(transforms);

            int mc = pointLightVolumeModel.Meshes.Count;
            for(int m = 0;m<mc;m++)
            {
                // Do the world stuff. 
                // Scale * transform * pos * rotation
                Matrix meshWorld;
                Matrix meshWVP;

                meshWorld = transforms[pointLightVolumeModel.Meshes[m].ParentBone.Index] * sphereWorldMatrix;
                meshWVP = meshWorld * Camera.View * Camera.Projection;

                if (deferredPointLightEffect.Parameters["world"] != null)
                    deferredPointLightEffect.Parameters["world"].SetValue(meshWorld);
                if (deferredPointLightEffect.Parameters["wvp"] != null)
                    deferredPointLightEffect.Parameters["wvp"].SetValue(meshWVP);

                deferredPointLightEffect.CurrentTechnique.Passes[0].Apply();

                int mpc = pointLightVolumeModel.Meshes[m].MeshParts.Count;
                for(int mp = 0;mp<mpc;mp++)
                {
                    GraphicsDevice.SetVertexBuffer(pointLightVolumeModel.Meshes[m].MeshParts[mp].VertexBuffer);
                    GraphicsDevice.Indices = pointLightVolumeModel.Meshes[m].MeshParts[mp].IndexBuffer;
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, pointLightVolumeModel.Meshes[m].MeshParts[mp].NumVertices, pointLightVolumeModel.Meshes[m].MeshParts[mp].StartIndex, pointLightVolumeModel.Meshes[m].MeshParts[mp].PrimitiveCount);
                }
            }
        }

        public void DeferredShaddows(GameTime gameTime)
        {
            int cnt = 0;
#if WINDOWS
            List<ILight> lights = new List<ILight>(DirectionalLights.Where(entity => entity.Intensity > 0 && entity.Color != Color.Black && entity.CastShadow));
            lights.AddRange(ConeLights.Where(entity => entity.Intensity > 0 && entity.Color != Color.Black && entity.CastShadow));

            List<ILight> needMaps = new List<ILight>(lights.Where(entity => entity.ShadowMap == null));
#else

            List<ILight> lights = new List<ILight>();
            cnt = DirectionalLights.Count;
            for (int l = 0; l < cnt; l++)
            {
                if (DirectionalLights[l].CastShadow && DirectionalLights[l].Intensity > 0 && DirectionalLights[l].Color != Color.Black)
                    lights.Add(DirectionalLights[l]);
                else
                    DirectionalLights[l].ShadowMap = null;
            }

            cnt = ConeLights.Count;
            for (int l = 0; l < cnt; l++)
            {
                if (ConeLights[l].CastShadow && ConeLights[l].Intensity > 0 && ConeLights[l].Color != Color.Black)
                    lights.Add(ConeLights[l]);
                else
                    ConeLights[l].ShadowMap = null;
            }

            List<ILight> needMaps = new List<ILight>();
            cnt = lights.Count;
            for (int l = 0; l < cnt; l++)
            {
                if (lights[l].ShadowMap == null)
                    needMaps.Add(lights[l]);
            }
#endif
            // set shadow maps;
            SetLightShadowMaps(needMaps);

            // render em
            cnt = lights.Count;
            for (int l = 0; l < cnt; l++)
                RenderLightShadows(gameTime, lights[l]);

            if (ScreenSpaceShadows)
            {
                for (int l = 0; l < cnt; l++)
                    DeferredScreenSpaceShadowDLRT(lights[l]);
            }

            if (cnt == 0 && SSSM != null)
            {
                GraphicsDevice.SetRenderTarget(SSSM);
                GraphicsDevice.Clear(Color.Transparent);
                GraphicsDevice.SetRenderTarget(null);
            }
        }
        public int shadowMapSize = 3;
        public void SetLightShadowMaps(List<ILight> lights)
        {
            int cnt = lights.Count;

            for (int l = 0; l < cnt; l++)
            {
                lights[l].ShadowMap = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width * shadowMapSize, GraphicsDevice.Viewport.Height * shadowMapSize, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
                lights[l].SoftShadowMap = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            }
            
        }

        public void RenderLightShadows(GameTime gameTime, ILight light)
        {
            // Clear shadow map..
            GraphicsDevice.SetRenderTarget(light.ShadowMap);
            GraphicsDevice.Clear(Color.Transparent);

            if (deferredShadowEffect == null)
            {
                deferredShadowEffect = AssetManager.GetAsset<Effect>("Shaders/Deferred/DeferredShadowMap");
                deferredSkinnedShadowEffect = AssetManager.GetAsset<Effect>("Shaders/Deferred/DeferredSkinnedShadowMap");
            }

            deferredShadowEffect.Parameters["vp"].SetValue(light.View * light.Projection);
            deferredSkinnedShadowEffect.Parameters["vp"].SetValue(light.View * light.Projection);

            int cnt = Game.Components.Count;
            
            for (int c = 0; c < cnt; c++)
            {
#if WINDOWS
                // terrain 
                if (Game.Components[c] is BruteForceTerrain)
                {
                    deferredShadowEffect.CurrentTechnique = deferredShadowEffect.Techniques["ShadowMapT"];
                    ((BruteForceTerrain)Game.Components[c]).Draw(gameTime, deferredShadowEffect);
                    continue;
                }
#endif
                // Deferred items
                if (Game.Components[c] is IDeferredRender && !(Game.Components[c] is IInstanced) && !(Game.Components[c] is BaseDeferredSkinnedObject) && !(Game.Components[c] is Base3DDeferredSkinnedObjectInstancer))
                {
                    //rlsDeferredList.Add(gc as IDeferredRender);
                    deferredShadowEffect.CurrentTechnique = deferredShadowEffect.Techniques["ShadowMap"];
                    ((IDeferredRender)Game.Components[c]).Draw(gameTime, deferredShadowEffect);
                    continue;
                }

                // skinned
                if (Game.Components[c] is BaseDeferredSkinnedObject)
                {
                    // move this line out of the loop?
                    deferredSkinnedShadowEffect.CurrentTechnique = deferredSkinnedShadowEffect.Techniques["ShadowMap"];
                    ((BaseDeferredSkinnedObject)Game.Components[c]).Draw(gameTime, deferredSkinnedShadowEffect);
                    //idr.Draw(gameTime, deferredSkinnedShadowEffect);
                    continue;
                }
                // Instanced
                if (Game.Components[c] is IDeferredRender && Game.Components[c] is IInstanced)
                {
                    deferredShadowEffect.CurrentTechnique = deferredShadowEffect.Techniques["ShadowMapH"];
                    ((IDeferredRender)Game.Components[c]).Draw(gameTime, deferredShadowEffect);
                    continue;
                }
                // Skinned and instanced
                if (Game.Components[c] is Base3DDeferredSkinnedObjectInstancer)
                {
                    deferredSkinnedShadowEffect.CurrentTechnique = deferredSkinnedShadowEffect.Techniques["ShadowMapH"];
                    ((Base3DDeferredSkinnedObjectInstancer)Game.Components[c]).Draw(gameTime, deferredSkinnedShadowEffect);
                    continue;
                }                
            }
            Game.GraphicsDevice.SetRenderTarget(null);
        }
        
        public void DrawDeferred()
        {
            GraphicsDevice.Clear(ClearColor);

            if (deferredSceneRenderEffect == null)
                deferredSceneRenderEffect = AssetManager.GetAsset<Effect>("Shaders/Deferred/DeferredSceneRender");

            deferredSceneRenderEffect.Parameters["halfPixel"].SetValue(halfPixel);

            deferredSceneRenderEffect.Parameters["colorMap"].SetValue(colorMap);
            deferredSceneRenderEffect.Parameters["lightMap"].SetValue(lightMap);
            deferredSceneRenderEffect.Parameters["sgrMap"].SetValue(SGRMap);
            
            deferredSceneRenderEffect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            sceneQuad.Draw(-Vector2.One, Vector2.One);            
        }

        public void SaveJpg(Texture2D tex, string name)
        {
            FileStream f = new FileStream(name, FileMode.Create);
            tex.SaveAsJpeg(f, tex.Width, tex.Height);
            f.Close();
        }

        public void ClearAllLights()
        {
            DirectionalLights.Clear();
            PointLights.Clear();
            ConeLights.Clear();
        }

        public void GetReflectionMap(GameTime gameTime, float waterHeight)
        {
            Quaternion orgRot = Camera.Rotation;
            Matrix orgView = Camera.View;

            Vector3 refCamPos = Camera.Position;
            refCamPos.Y = -refCamPos.Y + waterHeight * 2;

            GameComponentHelper.LockRotation(ref orgRot, Vector3.Forward);
            Matrix rot = Matrix.CreateFromQuaternion(orgRot);

            Vector3 refTargetPos = Camera.Position + Vector3.Transform(Vector3.Forward, rot);
            refTargetPos.Y = -refTargetPos.Y + waterHeight * 2;

            Vector3 camRight = Vector3.Transform(Vector3.Right, rot);
            Vector3 invUp = Vector3.Cross(camRight, refCamPos - refTargetPos);

            Camera.View = Matrix.CreateLookAt(refCamPos, refTargetPos, invUp);

            GameComponentHelper.WaterReflectionPane = GameComponentHelper.CreatePlane(waterHeight, new Vector3(0, -1, 0), Camera.View, true, Camera.Projection);
            Game.GraphicsDevice.SetRenderTargets(GameComponentHelper.reflectionMap, GameComponentHelper.reflectionSGRMap);
            Game.GraphicsDevice.BlendState = BlendState.Opaque;
            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            // Draw here.  
            if (!StopRender)
            {
                int cnt = Game.Components.Count;
                for (int c = 0; c < cnt; c++)
                {
                    // Do draw
                    if (Game.Components[c] is IDeferredRender)
                        ((IDeferredRender)Game.Components[c]).Draw(gameTime);
                }
            }
            else
            {
                if (CanDrawDeferred != null && CanDrawDeferred is IDeferredRender)
                    CanDrawDeferred.Draw(gameTime);
            }

            //GraphicsDevice.ClipPlanes[0].IsEnabled = false;
            Game.GraphicsDevice.SetRenderTargets(null);

            Camera.Rotation = orgRot;
            Camera.View = orgView;
            //SaveJpg(GameComponentHelper.reflectionMap, "ReflectionMap.jpg");    
            Game.GraphicsDevice.DepthStencilState = DepthStencilState.None;

            GameComponentHelper.WaterReflectionPane = new Plane();
        }
    }
}
