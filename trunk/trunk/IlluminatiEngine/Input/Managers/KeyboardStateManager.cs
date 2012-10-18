using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace IlluminatiEngine
{
    public class KeyboardStateManager : GameComponent, IInputStateManager
    {
        public KeyboardState State;
        public KeyboardState LastState;

        protected Keys[] keysPressed = new Keys[] { Keys.None };
        protected Keys[] lastKeysPressed = new[] { Keys.None };


        public KeyboardStateManager(Game game) : base(game) { }

        public Keys[] KeysPressed()
        {
            return keysPressed;
        }

        public bool KeyDown(Keys key)
        {
            return State.IsKeyDown(key);
        }
        
        public bool KeyPress(Keys key)
        {
            return (State.IsKeyUp(key) && LastState.IsKeyDown(key));
        }

        public override void Update(GameTime gameTime)
        {
            State = Keyboard.GetState();
            keysPressed = State.GetPressedKeys();
            base.Update(gameTime);
        }

        public void PreUpdate(GameTime gameTime)
        {
            LastState = State;
            lastKeysPressed = keysPressed;
        }
    }
}
