using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using IlluminatiEngine;
using IlluminatiEngine.PostProcessing;
using IlluminatiEngine.Renderer;
using IlluminatiEngine.Renderer.Deferred;


namespace IlluminatiEngine
{
    public class BaseDeferredRenderGame : Game
    {
        public bool firstFrameComplete = false;
        public GraphicsDeviceManager graphics;
        protected SpriteBatch spriteBatch;

        protected AssetManager assetManager;
        protected DeferredRender renderer;

        public DeferredRender Renderer { get { return renderer; } /*set { renderer = value; }*/ }

        protected InputHandlerService inputHandler;
        protected Random rnd;

        

        //protected GeographicGridRegistrationSystem ggr;

        public bool UseBulletPhysics { get; set; }

        protected PostProcessingManager ppManager;
        protected SunEffect sun;
        protected DepthOfFieldEffect dof;
        protected BloomEffect bloom;
        protected HeatHazeEffect haze;
        protected RadialBlurEffect radialBlur;
        protected RippleEffect ripple;
        protected FogEffect fog;
        protected WaterEffect water;

        protected STHardPointEffect stHPe;

        protected CrepuscularRays godRays;
        protected SSAOEffect SSAO;
        protected TesterEffect test;

        public STHardPointEffect STHpEffect
        {
            get { return stHPe; }
        }

        public SunEffect Sun { get { return sun; } }
        public DepthOfFieldEffect DoF { get { return dof; } }
        public CrepuscularRays GodRays { get { return godRays; } }
        public BloomEffect Bloom { get { return bloom; } }
        public HeatHazeEffect HeatHaze { get { return haze; } }
        public RadialBlurEffect RadialBlur { get { return radialBlur; } }
        public RippleEffect Ripple { get { return ripple; } }
        public FogEffect Fog { get { return fog; } }
        public WaterEffect Water { get { return water; } }

        protected Vector3 sunPosition = Vector3.Zero;
        public Vector3 SunPosition
        {
            get { return sunPosition; }
            set 
            { 
                sunPosition = value; 
                if(sun != null)
                    sun.Position = sunPosition;

                if (godRays != null)
                    godRays.lightSource = sunPosition;
            }
        }

        public Texture2D DepthMap
        {
            get { return renderer.depthMap; }
        }

        public enum ScreenResolution
        {
            UserDefined,
            Tiny256x128,
            WP7,
            HD720,
            HD1080p
        }

        public ScreenResolution Resolution = ScreenResolution.UserDefined;

        public bool IsPhysicsEnabled
        {
            get 
        { 
            bool enabled = false;
#if BULLETSHARP
                enabled = ((BulletSharpPhysicsComponent)Services.GetService(typeof(BulletSharpPhysicsComponent))).Enabled; 
#elif BULLETXNA
                enabled = ((BulletXNAPhysicsComponent)Services.GetService(typeof(BulletXNAPhysicsComponent))).Enabled;
#elif JITTER
                enabled = ((JitterPhysicsComponent)Services.GetService(typeof(JitterPhysicsComponent))).Enabled;
#endif
                return enabled;
        }
            set 
            { 
#if BULLETSHARP            
            ((BulletSharpPhysicsComponent)Services.GetService(typeof(BulletSharpPhysicsComponent))).Enabled = value; 

#elif BULLETXNA
            ((BulletXNAPhysicsComponent)Services.GetService(typeof(BulletXNAPhysicsComponent))).Enabled = value;
#elif JITTER
            ((JitterPhysicsComponent)Services.GetService(typeof(JitterPhysicsComponent))).Enabled = value;
#endif
            }
        }

        public BaseDeferredRenderGame() : base()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
            Content.RootDirectory = "Content";

            inputHandler = new InputHandlerService(this);

            assetManager = new AssetManager(this);

            renderer = new DeferredRender(this);

            rnd = new Random(DateTime.Now.Millisecond);

            //ggr = new GeographicGridRegistrationSystem(this, new Vector3(10, 5, 20), new BoundingBox(-Vector3.One * 2f, Vector3.One * 2f));

            ppManager = new PostProcessingManager(this);

            test = new TesterEffect(this);
            test.Enabled = false;
            ppManager.AddEffect(test);

            fog = new FogEffect(this, 50, 100, Color.DarkSlateGray);
            fog.Enabled = false;
            ppManager.AddEffect(fog);

            SSAO = new SSAOEffect(this, .1f, 1f, .5f, 1f);
            SSAO.Enabled = false;
            ppManager.AddEffect(SSAO);

            //stHPe = new STHardPointEffect(this, 25, 30, new Color(48, 89, 122));
            stHPe = new STHardPointEffect(this, 25, 30, new Color(41, 77, 107), new Color(.125f, .125f, .125f,1.0f));
            stHPe.Enabled = false;
            ppManager.AddEffect(stHPe);
                        

            sun = new SunEffect(this, SunPosition);
            sun.Enabled = false;
            ppManager.AddEffect(sun);

            dof = new DepthOfFieldEffect(this, 5, 30);
            dof.Enabled = false;
            ppManager.AddEffect(dof);

            bloom = new BloomEffect(this, 1.25f, 1f, 1f, 1f, .25f, 4f);
            bloom.Enabled = false;
            ppManager.AddEffect(bloom);

            haze = new HeatHazeEffect(this, "Textures/bumpmap", false);
            haze.Enabled = false;
            ppManager.AddEffect(haze);

            radialBlur = new RadialBlurEffect(this, 0.009f);
            radialBlur.Enabled = false;
            ppManager.AddEffect(radialBlur);

            ripple = new RippleEffect(this);
            ripple.Enabled = false;
            ppManager.AddEffect(ripple);

            water = new WaterEffect(this);
            water.waterHeight = -25f;
            water.Enabled = false;
            ppManager.AddEffect(water);

            godRays = new CrepuscularRays(this, SunPosition, "Textures/flare", 1500, 1f, .99f, 1f, .15f, .25f);
            //godRays = new CrepuscularRays(this, SunPosition, "Textures/flare", 1500, 1f, .99f, .1f, 0.12f, .25f);
            godRays.Enabled = false;
            ppManager.AddEffect(godRays);
        }

        public delegate void DeviceCreationEvent(object sender, PreparingDeviceSettingsEventArgs e);

        public PresentationParameters PresentationParameters = null;

        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            if (PresentationParameters != null)
                e.GraphicsDeviceInformation.PresentationParameters = PresentationParameters;            
        }

        public virtual void InitializePhysics()
        {
#if BULLETSHARP
            BulletSharpPhysicsComponent bulletPys = new BulletSharpPhysicsComponent(this);
#endif
#if BULLETXNA
            BulletXNAPhysicsComponent bulletPysXNA = new BulletXNAPhysicsComponent(this);
#endif
#if JITTER
            JitterPhysicsComponent jitterPhysics = new JitterPhysicsComponent(this);  
#endif
        
        }

#if BULLETSHARP

        public BulletSharpPhysicsComponent BulletPhysics
        {
            get { return (BulletSharpPhysicsComponent)Services.GetService(typeof(BulletSharpPhysicsComponent)); }
        }

#endif
#if BULLETXNA
        public BulletXNAPhysicsComponent BulletPhysicsXNA
        {
            get { return (BulletXNAPhysicsComponent)Services.GetService(typeof(BulletXNAPhysicsComponent)); }
        }
#endif
#if JITTER
        public JitterPhysicsComponent JitterPhysics
        {
            get { return (JitterPhysicsComponent)Services.GetService(typeof(JitterPhysicsComponent)); }
        }

#endif

        protected override void Initialize()
        {
            if (UseBulletPhysics)
                InitializePhysics();

            switch (Resolution)
            {
                case ScreenResolution.UserDefined:
                    break;
                case ScreenResolution.Tiny256x128:
                    graphics.PreferredBackBufferHeight = 128;
                    graphics.PreferredBackBufferWidth = 256;
                    graphics.ApplyChanges();
                    break;
                case ScreenResolution.WP7:
                    graphics.PreferredBackBufferHeight = 480;
                    graphics.PreferredBackBufferWidth = 800;
                    graphics.ApplyChanges();
                    break;
                case ScreenResolution.HD720:
                    graphics.PreferredBackBufferHeight = 720;
                    graphics.PreferredBackBufferWidth = 1208;
                    graphics.ApplyChanges();
                    break;
                case ScreenResolution.HD1080p:
                    graphics.PreferredBackBufferHeight = 1080;
                    graphics.PreferredBackBufferWidth = 1920;
                    graphics.ApplyChanges();
                    break;
            }

            base.Initialize();

            Services.AddService(typeof(SpriteBatch), spriteBatch);
        }

        protected override void Update(GameTime gameTime)
        {
            inputHandler.PreUpdate(gameTime);

            base.Update(gameTime);

            ppManager.Update(gameTime);
        }

        public bool PostProcessingOn = true;

        protected override bool BeginDraw()
        {
            if (renderer.StopRender && firstFrameComplete)
                firstFrameComplete = false;


            return base.BeginDraw();
        }
        protected override void Draw(GameTime gameTime)
        {
            renderer.Draw(gameTime);

            if (PostProcessingOn)
            {
                STHpEffect.normalMap = renderer.normalMap;
                ppManager.Draw(gameTime, renderer.finalBackBuffer, renderer.depthMap, renderer.normalMap);
            }
            else
            {
                if (!renderer.StopRender)
                {
                    GraphicsDevice.Clear(renderer.ClearColor);
                    spriteBatch.Begin();
                    spriteBatch.Draw(renderer.finalBackBuffer, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
                    spriteBatch.End();
                }
            }


            renderer.RenderDebug();

            if (hWnd != IntPtr.Zero)
                GraphicsDevice.Present(null, null, hWnd);
            
        }

        protected override void EndDraw()
        {
            if (!renderer.StopRender && !firstFrameComplete)
                firstFrameComplete = true;

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            spriteBatch.Draw(assetManager.GetAsset<Texture2D>("EngineIcon"), new Rectangle(GraphicsDevice.Viewport.TitleSafeArea.Right - 70, GraphicsDevice.Viewport.TitleSafeArea.Bottom - 70, 64, 64), new Color(1, 1, 1, .25f));
            spriteBatch.End();

           base.EndDraw();
        }

        public void SaveJpg(Texture2D tex, string name)
        {
            FileStream f = new FileStream(name, FileMode.Create);
            tex.SaveAsJpeg(f, tex.Width, tex.Height);
            f.Close();
        }

        public IntPtr hWnd = IntPtr.Zero;

        
        GraphicsDevice _GraphicsDevice = null;
        public new GraphicsDevice GraphicsDevice
        {
            get
            {
                if (_GraphicsDevice == null)
                    _GraphicsDevice = base.GraphicsDevice;

                return _GraphicsDevice;
            }
            set
            {
                _GraphicsDevice = value;
            }
        }
    }
}
