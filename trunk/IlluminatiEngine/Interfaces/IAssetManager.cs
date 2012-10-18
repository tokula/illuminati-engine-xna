using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine
{
    public interface IAssetManager
    {
        void AddAsset<T>(string key, object asset) where T : class;
        T GetAsset<T>(string key) where T : class;
        T GetAsset<T>(BaseAssets key) where T : class;
        void ClearAssets();
    }
}
