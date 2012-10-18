using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IlluminatiEngine
{
    public interface IImplementsCustomShader
    {
        string Effect { get; }
        void ChangeEffect(string effectAsset);
    }
}
