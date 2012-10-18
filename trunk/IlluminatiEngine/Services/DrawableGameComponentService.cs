using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine
{
    public class DrawableComponentService : DrawableGameComponent
    {
        public DrawableComponentService(Game game) : base(game)
        {
            game.Components.Add(this);
            game.Services.AddService(this.GetType(), this);
        }
    }
}
