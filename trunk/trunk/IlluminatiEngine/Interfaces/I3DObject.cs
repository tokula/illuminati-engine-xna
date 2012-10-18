using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine
{
    public interface I3DBaseObject
    {
        IAssetManager AssetManager
        { get; }
        ICameraService Camera
        { get; }

        Quaternion Orientation
        { get; set; }

        Vector3 Position
        { get; set; }

        Vector3 Scale
        { get; set; }

        Matrix World
        { get; set; }

        List<BoundingBox> AABoxBounds
        { get; set; }

        void TranslateAA(Vector3 distance);
        void TranslateOO(Vector3 distance);
        void Rotate(Vector3 axis, float angle);
        void Draw(GameTime gameTime, Effect effect);
    }
}
