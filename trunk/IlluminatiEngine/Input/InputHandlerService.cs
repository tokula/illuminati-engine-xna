using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#if WINDOWS_PHONE
using Microsoft.Xna.Framework.Input.Touch;
#endif

namespace IlluminatiEngine
{
    /// <summary>
    /// This class will manage all the input for a project, it is a self registering component service as
    /// it is implementing the XNAUKLib.BaseService.BaseComponentService.
    /// 
    /// When implemented in the game loop after all required input has been handled you must call the
    /// PostUpdate to ensure that the input managers are all uptodate.
    /// </summary>
    public class InputHandlerService : GameComponentService, IInputStateManager
    {
        /// <summary>
        /// Manager used for keyboard input, avaialable on all platforms
        /// </summary>
        public KeyboardStateManager KeyboardManager;
        /// <summary>
        /// Manager for gamepad input, avaialable on all platforms
        /// </summary>
        public GamePadManager GamePadManager;
#if WINDOWS
        /// <summary>
        /// Manager used for mouse input, available in Windows only
        /// </summary>
        public MouseStateManager MouseManager;
#endif
#if WINDOWS_PHONE
        /// <summary>
        /// Manager for accelerometer input, WP7 only
        /// </summary>
        public AccelerometerManager AccelerometerManager;
        /// <summary>
        /// Manager for touch input, WP7 only
        /// </summary>
        public TouchCollectionManager TouchManager;
        
#endif
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="game">Game associated with the manager</param>
        public InputHandlerService(Game game) : base(game)
        {
            KeyboardManager = new KeyboardStateManager(game);
            GamePadManager = new GamePadManager(game);
#if WINDOWS
            MouseManager = new MouseStateManager(game);
#endif
#if WINDOWS_PHONE
        AccelerometerManager = new AccelerometerManager(game);
        TouchManager = new TouchCollectionManager(game);
        GamePadManager = new GamePadManager(game);
#endif
            //#if XBOX
            //        GamePadManager = new GamePadManager(game);
            //#endif
        }

        /// <summary>
        /// Initializes managers
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
#if WINDOWS_PHONE
            AccelerometerManager.Initialize();
#endif
            // Add our own service to the Game Service container
            //Game.Services.AddService(typeof(InputHandlerService), this);
        }
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (Game.IsActive)
            {
                KeyboardManager.Update(gameTime);
                GamePadManager.Update(gameTime);
#if WINDOWS
                MouseManager.Update(gameTime);
#endif
                base.Update(gameTime);
            }            
        }

        /// <summary>
        /// Call this after all input has been maanged, this will ensure the
        /// managers are all uptodate.
        /// </summary>
        /// <param name="gameTime"></param>
        public void PreUpdate(GameTime gameTime)
        {
            if (Game.IsActive)
            {
                KeyboardManager.PreUpdate(gameTime);
                GamePadManager.PreUpdate(gameTime);
#if WINDOWS
                MouseManager.PreUpdate(gameTime);
#endif
            }
        }
    }
}
