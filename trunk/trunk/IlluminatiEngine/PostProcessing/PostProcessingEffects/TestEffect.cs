using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.PostProcessing
{
    public class TesterEffect : BasePostProcessingEffect
    {

        public MotionBlur mb;
        
        
        public TesterEffect(Game game)
            : base(game)
        {

            mb = new MotionBlur(game);

            AddPostProcess(mb);        
        }        
    }
}
