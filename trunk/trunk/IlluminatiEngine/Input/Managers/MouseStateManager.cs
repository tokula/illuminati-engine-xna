using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace IlluminatiEngine
{
    public class MouseStateManager : GameComponent, IInputStateManager
    {
        public MouseState State;
        public MouseState LastState;

        public Vector2 Position;
        public Vector2 LastPosition;
        public Vector2 Direction;
        public Vector2 Velocity;

        public MouseStateManager(Game game) : base(game) { }

        public bool LeftClicked
        {
            get { return (State.LeftButton == ButtonState.Released && LastState.LeftButton == ButtonState.Pressed); }
        }
        public bool RightClicked
        {
            get { return (State.RightButton == ButtonState.Released && LastState.RightButton == ButtonState.Pressed); }
        }
        public bool MiddleClicked
        {
            get { return (State.MiddleButton == ButtonState.Released && LastState.MiddleButton == ButtonState.Pressed); }
        }
        public bool XButton1Clicked
        {
            get { return (State.XButton1 == ButtonState.Released && LastState.XButton1 == ButtonState.Pressed); }
        }
        public bool XButton2Clicked
        {
            get { return (State.XButton2 == ButtonState.Released && LastState.XButton2 == ButtonState.Pressed); }
        }

        public ButtonState LeftButtonState
        {
            get { return State.LeftButton; }
        }

        public ButtonState RighttButtonState
        {
            get { return State.RightButton; }
        }

        public ButtonState MiddleButtonState
        {
            get { return State.MiddleButton; }
        }

        public bool LeftButtonDown
        {
            get { return State.LeftButton == ButtonState.Pressed; }
        }
        public bool RightButtonDown
        {
            get { return State.RightButton == ButtonState.Pressed; }
        }
        public bool MiddleButtonDown
        {
            get { return State.MiddleButton == ButtonState.Pressed; }
        }

        public bool XButton1ButtonDown
        {
            get { return State.XButton1 == ButtonState.Pressed; }
        }

        public bool XButton2ButtonDown
        {
            get { return State.XButton2 == ButtonState.Pressed; }
        }

        public int ScrollWheelValue
        {
            get { return State.ScrollWheelValue; }
        }

        private int lastScrollVal = 0;
        public int ScrollWheelDelta
        {
            get { return ScrollWheelValue - lastScrollVal; }
        }

        public override void Update(GameTime gameTime)
        {
            State = Mouse.GetState();
            Position = new Vector2(State.X, State.Y);
            base.Update(gameTime);
        }

        public void PreUpdate(GameTime gameTime)
        {
            LastState = State;
            lastScrollVal = LastState.ScrollWheelValue;
            Velocity = Position - LastPosition;

            if (Velocity != Vector2.Zero)
                Direction = Vector2.Normalize(Velocity);
            else
                Direction = Velocity;

            LastPosition = Position;
        }
    }
}
