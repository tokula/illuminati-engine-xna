using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;

namespace IlluminatiEngine
{
    public class GamePadManager : GameComponent, IInputStateManager
    {
        public Dictionary<PlayerIndex, GamePadState> State = new Dictionary<PlayerIndex, GamePadState>();
        public Dictionary<PlayerIndex, GamePadState> LastState = new Dictionary<PlayerIndex, GamePadState>();

        public GamePadManager(Game game) : base(game)
        {
            State.Add(PlayerIndex.One, GamePad.GetState(PlayerIndex.One));
            State.Add(PlayerIndex.Two, GamePad.GetState(PlayerIndex.Two));
            State.Add(PlayerIndex.Three, GamePad.GetState(PlayerIndex.Three));
            State.Add(PlayerIndex.Four, GamePad.GetState(PlayerIndex.Four));

            LastState.Add(PlayerIndex.One, new GamePadState());
            LastState.Add(PlayerIndex.Two, new GamePadState());
            LastState.Add(PlayerIndex.Three, new GamePadState());
            LastState.Add(PlayerIndex.Four, new GamePadState());
        }

        public Nullable<PlayerIndex> GetFirstActiveControler()
        {
            Nullable<PlayerIndex> index = null;

            bool hasPad = false;
            for (index = PlayerIndex.One; index < PlayerIndex.Four; index++)
            {
                if (GamePadConnected(index.Value))
                {
                    hasPad = true;
                    break;
                }
            }

            if (!hasPad)
                index = null;

            return index;
        }
        public bool GamePadConnected(PlayerIndex index)
        {
            return State[index] != null && State[index].IsConnected;
        }

        public override void Update(GameTime gameTime)
        {

#if WINDOWS_PHONE
            State[PlayerIndex.One] = GamePad.GetState(PlayerIndex.One);
#else
            if (SignedInGamer.SignedInGamers.Count == 0)
            {
                State[PlayerIndex.One] = GamePad.GetState(PlayerIndex.One);
                State[PlayerIndex.Two] = GamePad.GetState(PlayerIndex.Two);
                State[PlayerIndex.Three] = GamePad.GetState(PlayerIndex.Three);
                State[PlayerIndex.Four] = GamePad.GetState(PlayerIndex.Four);
            }
            else
            {
                int cnt = SignedInGamer.SignedInGamers.Count;
                for (int g = 0; g < cnt; g++)
                    State[SignedInGamer.SignedInGamers[g].PlayerIndex] = GamePad.GetState(SignedInGamer.SignedInGamers[g].PlayerIndex);
            }
#endif

            base.Update(gameTime);
        }

        
#if WINDOWS_PHONE
        public GamePadState GetState()
        {
            return State[PlayerIndex.One];
        }
#else
        public GamePadState GetStateForPlayer(PlayerIndex index)
        {
            return State[index];
        }
        public GamePadState GetStateForPlayer(SignedInGamer gamer)
        {
            return State[gamer.PlayerIndex];
        }
#endif

        public bool ButtonPress(PlayerIndex index,Buttons button)
        {
            bool retVal = false;
            switch (button)
            {
                case Buttons.A:
                    retVal = State[index].Buttons.A == ButtonState.Released && LastState[index].Buttons.A == ButtonState.Pressed;
                    break;
                case Buttons.B:
                    retVal = State[index].Buttons.B == ButtonState.Released && LastState[index].Buttons.B == ButtonState.Pressed;
                    break;
                case Buttons.X:
                    retVal = State[index].Buttons.X == ButtonState.Released && LastState[index].Buttons.X == ButtonState.Pressed;
                    break;
                case Buttons.Y:
                    retVal = State[index].Buttons.Y == ButtonState.Released && LastState[index].Buttons.Y == ButtonState.Pressed;
                    break;
                case Buttons.Back:
                    retVal = State[index].Buttons.Back == ButtonState.Released && LastState[index].Buttons.Back == ButtonState.Pressed;
                    break;
                case Buttons.BigButton:
                    retVal = State[index].Buttons.BigButton == ButtonState.Released && LastState[index].Buttons.BigButton == ButtonState.Pressed;
                    break;
                case Buttons.DPadDown:
                    retVal = State[index].DPad.Down == ButtonState.Released && LastState[index].DPad.Down == ButtonState.Pressed;
                    break;
                case Buttons.DPadLeft:
                    retVal = State[index].DPad.Left == ButtonState.Released && LastState[index].DPad.Left == ButtonState.Pressed;
                    break;
                case Buttons.DPadRight:
                    retVal = State[index].DPad.Right == ButtonState.Released && LastState[index].DPad.Right == ButtonState.Pressed;
                    break;
                case Buttons.DPadUp:
                    retVal = State[index].DPad.Up == ButtonState.Released && LastState[index].DPad.Up == ButtonState.Pressed;

                    if (retVal) {
                    }
                    break;
                case Buttons.LeftShoulder:
                    retVal = State[index].Buttons.LeftShoulder == ButtonState.Released && LastState[index].Buttons.LeftShoulder == ButtonState.Pressed;
                    break;
                case Buttons.LeftStick:
                    retVal = State[index].Buttons.LeftStick == ButtonState.Released && LastState[index].Buttons.LeftStick == ButtonState.Pressed;
                    break;
                case Buttons.RightShoulder:
                    retVal = State[index].Buttons.RightShoulder == ButtonState.Released && LastState[index].Buttons.RightShoulder == ButtonState.Pressed;
                    break;
                case Buttons.RightStick:
                    retVal = State[index].Buttons.RightStick == ButtonState.Released && LastState[index].Buttons.RightStick == ButtonState.Pressed;
                    break;
                case Buttons.Start:
                    retVal = State[index].Buttons.Start == ButtonState.Released && LastState[index].Buttons.Start == ButtonState.Pressed;
                    break;                
            }
            return retVal;
        }

        public bool ButtonDown(PlayerIndex index, Buttons button)
        {
            bool retVal = false;
            switch (button)
            {
                case Buttons.A:
                    retVal = LastState[index].Buttons.A == ButtonState.Pressed;
                    break;
                case Buttons.B:
                    retVal = LastState[index].Buttons.B == ButtonState.Pressed;
                    break;
                case Buttons.X:
                    retVal = LastState[index].Buttons.X == ButtonState.Pressed;
                    break;
                case Buttons.Y:
                    retVal = LastState[index].Buttons.Y == ButtonState.Pressed;
                    break;
                case Buttons.Back:
                    retVal = LastState[index].Buttons.Back == ButtonState.Pressed;
                    break;
                case Buttons.BigButton:
                    retVal = LastState[index].Buttons.BigButton == ButtonState.Pressed;
                    break;
                case Buttons.DPadDown:
                    retVal = LastState[index].DPad.Down == ButtonState.Pressed;
                    break;
                case Buttons.DPadLeft:
                    retVal = LastState[index].DPad.Left == ButtonState.Pressed;
                    break;
                case Buttons.DPadRight:
                    retVal = LastState[index].DPad.Right == ButtonState.Pressed;
                    break;
                case Buttons.DPadUp:
                    retVal = LastState[index].DPad.Up == ButtonState.Pressed;

                    if (retVal)
                    {
                    }
                    break;
                case Buttons.LeftShoulder:
                    retVal = LastState[index].Buttons.LeftShoulder == ButtonState.Pressed;
                    break;
                case Buttons.LeftStick:
                    retVal = LastState[index].Buttons.LeftStick == ButtonState.Pressed;
                    break;
                case Buttons.RightShoulder:
                    retVal =  LastState[index].Buttons.RightShoulder == ButtonState.Pressed;
                    break;
                case Buttons.RightStick:
                    retVal = LastState[index].Buttons.RightStick == ButtonState.Pressed;
                    break;
                case Buttons.Start:
                    retVal = LastState[index].Buttons.Start == ButtonState.Pressed;
                    break;
            }
            return retVal;
        }

        public void PreUpdate(GameTime gameTime)
        {
            if (SignedInGamer.SignedInGamers.Count == 0)
            {
                LastState[PlayerIndex.One] = State[PlayerIndex.One];
                LastState[PlayerIndex.Two] = State[PlayerIndex.Two];
                LastState[PlayerIndex.Three] = State[PlayerIndex.Three];
                LastState[PlayerIndex.Four] = State[PlayerIndex.Four];
            }
            else
            {
                int cnt = SignedInGamer.SignedInGamers.Count;
                for (int g = 0; g < cnt; g++)
                    LastState[SignedInGamer.SignedInGamers[g].PlayerIndex] = State[SignedInGamer.SignedInGamers[g].PlayerIndex];
            }
        }
    }
}
