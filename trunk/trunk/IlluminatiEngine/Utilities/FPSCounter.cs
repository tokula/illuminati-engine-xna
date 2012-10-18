using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine
{
    public class FPSCounter : DrawableComponentService
    {
        /// <summary>
        /// Time passed since last call.
        /// </summary>
        //static float elapsedTime;
        static TimeSpan elapsedTime;
        
        /// <summary>
        /// Resulting frame rate.
        /// </summary>
        public int frameRate;
        public int frameCounter;

        public FPSCounter(Game game) : base(game)
        { }

        public override void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }

            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            frameCounter++;
        }
    }
}
