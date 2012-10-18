#if WINDOWS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using AwesomiumSharp;

namespace IlluminatiEngine
{
    public class AwesomiumUIManager : DrawableComponentService
    {
        public int thisWidth;
        public int thisHeight;

        protected Effect webEffect;

        public WebView webView;
        public Texture2D webRender;
        
        protected int[] webData;

        public bool TransparentBackground = true;

        bool keyPressed = false;

        protected string _LocalURL;
        public string LocalURL { get { return _LocalURL; } set { _LocalURL = value; URL = value; ReLoad(); } }

        /// <summary>
        /// If false (default), you have to call the AwesomiumUIManager <b>DrawUI</b> call to
        /// render the output. This is so debug and post processing wont effect it.
        /// If you want PP to work on the UI then set this to true.
        /// </summary>
        public bool InRenderPipeline = false;

        public IAssetManager assetManager
        {
            get { return (IAssetManager)Game.Services.GetService(typeof(IAssetManager)); }
        }

        protected InputHandlerService inputHandler
        {
            get { return (InputHandlerService)Game.Services.GetService(typeof(InputHandlerService)); }
        }

        protected SpriteBatch spriteBatch
        {
            get { return (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch)); }
        }

        public string URL;

        public AwesomiumUIManager(Game game, string baseUrl) : base(game) 
        {
            URL = baseUrl;

            DrawOrder = int.MaxValue;
        }

        protected override void LoadContent()
        {
            WebCore.Config config = new WebCore.Config();
            config.enablePlugins = true;
            config.enableJavascript = true;
            WebCore.Initialize(config);

            thisWidth = Game.GraphicsDevice.PresentationParameters.BackBufferWidth;
            thisHeight = Game.GraphicsDevice.PresentationParameters.BackBufferHeight;

            webView = WebCore.CreateWebview(thisWidth, thisHeight);

            webRender = new Texture2D(GraphicsDevice, thisWidth, thisHeight, false, SurfaceFormat.Color);
            webData = new int[thisWidth * thisHeight];

            webEffect = assetManager.GetAsset<Effect>("Shaders/webEffect");

            LocalURL = URL;
            //ReLoad();
        }

        public virtual void LoadFile(string file)
        {
            LoadURL(string.Format("file:///{0}\\{1}", Directory.GetCurrentDirectory(), file).Replace("\\", "/"));
        }

        public virtual void LoadURL(string url)
        {
            URL = url;
            webView.LoadURL(url);
            
            webView.SetTransparent(TransparentBackground);
            
            webView.Focus();
        }

        public virtual void ReLoad()
        {
            if (URL.Contains("http://") || URL.Contains("file:///"))
                LoadURL(URL);
            else
                LoadFile(URL);
        }

        public virtual void CreateObject(string name)
        {
            webView.CreateObject(name);
        }
        public virtual void CreateObject(string name,string method, WebView.JSCallback callback)
        {
            CreateObject(name);
            
            webView.SetObjectCallback(name, method, callback);
        }

        public virtual void PushData(string name, string method, params JSValue[] args)
        {
            webView.CallJavascriptFunction(name, method, args);
        }

        public override void Update(GameTime gameTime)
        {
            if (inputHandler.MouseManager.LeftButtonDown)
                webView.InjectMouseDown(MouseButton.Left);

            if (inputHandler.MouseManager.LeftClicked)
                webView.InjectMouseUp(MouseButton.Left);

            if (inputHandler.MouseManager.Velocity != Vector2.Zero)
                webView.InjectMouseMove((int)inputHandler.MouseManager.Position.X, (int)inputHandler.MouseManager.Position.Y);

            if (inputHandler.MouseManager.ScrollWheelDelta != 0)
                webView.InjectMouseWheel(inputHandler.MouseManager.ScrollWheelDelta);

            if (!keyPressed && inputHandler.KeyboardManager.KeysPressed().Length > 0)
            {
                keyPressed = true;
                InjectKeyboardEvent();
            }

            if (inputHandler.KeyboardManager.KeysPressed().Length == 0)
                keyPressed = false;

            WebCore.Update();

            if (webView.IsDirty())
            {
                Marshal.Copy(webView.Render().GetBuffer(), webData, 0, webData.Length);
                webRender.SetData(webData);
            }

            base.Update(gameTime);
        }

        public void InjectKeyboardEvent()
        {
            Microsoft.Xna.Framework.Input.Keys[] keys = inputHandler.KeyboardManager.KeysPressed();


            WebKeyboardEvent keyEvent = new WebKeyboardEvent();
            
            ushort keyValue = 0;

            List<Microsoft.Xna.Framework.Input.Keys> regularKeys = new List<Microsoft.Xna.Framework.Input.Keys>();
            bool IsShiftDown = false;
            bool IsCtrlDown = false;
            bool IsAltDown = false;
            bool IsWinDown = false;
            
            // what special keys are down?
            for (int k = keys.Length-1; k >= 0; k--)
            {
                switch (keys[k])
                {
                    case Microsoft.Xna.Framework.Input.Keys.LeftShift:
                    case Microsoft.Xna.Framework.Input.Keys.RightShift:
                        IsShiftDown = true;                        
                        break;
                    case Microsoft.Xna.Framework.Input.Keys.LeftControl:
                    case Microsoft.Xna.Framework.Input.Keys.RightControl:
                        IsCtrlDown = true;
                        break;
                    case Microsoft.Xna.Framework.Input.Keys.LeftAlt:
                    case Microsoft.Xna.Framework.Input.Keys.RightAlt:
                        IsAltDown = true;
                        break;
                    case Microsoft.Xna.Framework.Input.Keys.LeftWindows:
                    case Microsoft.Xna.Framework.Input.Keys.RightWindows:
                        IsWinDown = true;
                        break;
                    default:
                        regularKeys.Add(keys[k]);
                        break;
                }
            }
            

            foreach (Microsoft.Xna.Framework.Input.Keys key in regularKeys)
            {
                switch (key)
                {
                    case Microsoft.Xna.Framework.Input.Keys.Add:
                        keyValue = (ushort)'+';
                        InjectKeyPress(keyEvent, keyValue);
                        break;
                    case Microsoft.Xna.Framework.Input.Keys.Back:
                        keyValue = (ushort)'\b';
                        InjectKeyDown(keyEvent, keyValue);
                        break;
                    case Microsoft.Xna.Framework.Input.Keys.Delete:
                        keyValue = (ushort)127;
                        InjectKeyDown(keyEvent, keyValue);
                        break;
                    default:
                        keyValue = (ushort)key;
                        
                        if (!IsShiftDown)
                            keyValue += 32;

                        InjectKeyPress(keyEvent, keyValue);
                        break;
                }
            }

        }

        void InjectKeyPress(WebKeyboardEvent keyEvent, ushort keyValue)
        {
            keyEvent.type = WebKeyType.Char;
            keyEvent.text = new ushort[] { keyValue, 0, 0, 0 };
            webView.InjectKeyboardEvent(keyEvent);
        }

        void InjectKeyDown(WebKeyboardEvent keyEvent, ushort keyValue)
        {
            keyEvent.type = WebKeyType.KeyDown;
            keyEvent.virtualKeyCode = keyValue;
            keyEvent.nativeKeyCode = keyValue;
            webView.InjectKeyboardEvent(keyEvent);
        }

        public override void Draw(GameTime gameTime)
        {
            if (InRenderPipeline)
                DrawUI(gameTime);
        }
        public virtual void DrawUI(GameTime gameTime)
        {
            if (webRender != null)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
                webEffect.CurrentTechnique.Passes[0].Apply();
                spriteBatch.Draw(webRender, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), Color.White);
                spriteBatch.End();

                Game.GraphicsDevice.Textures[0] = null;
            }
        }
        protected void SaveTarget()
        {
            FileStream s = new FileStream("UI.jpg", FileMode.Create);
            webRender.SaveAsJpeg(s, webRender.Width, webRender.Height);
            s.Close();
        }
    }
}
#endif