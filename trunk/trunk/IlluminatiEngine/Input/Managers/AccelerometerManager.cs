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
using Microsoft.Devices.Sensors;

namespace IlluminatiEngine
{
    public class AccelerometerManager : GameComponent, IInputStateManager
    {
        public Accelerometer Accelerometer;
        public Vector3 State;
        public Vector3 LastState;
        
        object accelerometerLockObject = new object();

        public AccelerometerManager(Game game) : base(game)
        { }
        public override void Initialize()
        {
            base.Initialize();
            Accelerometer = new Accelerometer();
            if (Accelerometer.State == SensorState.Ready)
            {
                Accelerometer.ReadingChanged += AccelerometerReadingChanged;
                Accelerometer.Start();
            }
        }
        
        public virtual void AccelerometerReadingChanged(object sender, AccelerometerReadingEventArgs args)
        {
            // this event fires on a different thread than the game loop, and the 
            // class is composed of multiple non-atomic data members, so even though
            // there's only one writer, we don't want to update the value while it's
            // being read elsewhere.
            lock (accelerometerLockObject)
            {
                LastState = State;
                State = new Vector3((float)args.X, (float)args.Y, (float)args.Z);
            }
        }


        public void PostUpdate(GameTime gameTime)
        {
            throw new NotImplementedException();
        }
    }
}
#endif