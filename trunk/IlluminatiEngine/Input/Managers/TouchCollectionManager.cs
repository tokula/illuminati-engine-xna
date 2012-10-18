#if WINDOWS_PHONE
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Microsoft.Xna.Framework.Input.Touch;

namespace IlluminatiEngine
{
    public class TouchCollectionManager : GameComponent, IInputStateManager
    {
        public TouchCollection State;
        public TouchCollection LastState;
        public GestureSample Gesture;
        public List<GestureType> RequiredGestures = new List<GestureType>();

        public TouchCollectionManager(Game game)  : base(game)
        { }
        public TouchCollectionManager(Game game, params GestureType[] gestures) : base(game)
        {
            UpdateEnabledGestures(gestures);
        }

        public void UpdateEnabledGestures(GestureType[] gestures)
        {
            RequiredGestures.AddRange(gestures);
            for (int g = 0; g < gestures.Length; g++)
                TouchPanel.EnabledGestures |= gestures[g];
        }
        public override void Update(GameTime gameTime)
        {
            State = TouchPanel.GetState();

            if (TouchPanel.IsGestureAvailable)
                Gesture = TouchPanel.ReadGesture();
            

            base.Update(gameTime);
        }

        public void PostUpdate(GameTime gameTime)
        {
            LastState = State;
        }
    }
}
#endif