using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace IlluminatiEngine
{
    /// <summary>
    /// Enforces that input state magers have the required elements
    /// to run the state managers they look after..
    /// </summary>
    public interface IInputStateManager
    {
        void PreUpdate(GameTime gameTime);
    }
}
