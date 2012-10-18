using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IlluminatiEngine.Interfaces
{
    public interface IDebuggable
    {
        void DumpDebugInfo(StringBuilder stringBuilder);
    }
}
