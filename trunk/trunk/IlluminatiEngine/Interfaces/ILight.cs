using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine
{
    public interface ILight
    {
        string Name { get; set; }
        Vector3 Position { get; set; }
        Color Color { get; set; }
        float Intensity { get; set; }
        bool CastShadow { get; set; }
        Matrix View { get; }
        Matrix Projection { get; }
        RenderTarget2D ShadowMap { get; set; }

        double ShadowMod { get; set; }
    }
    public interface IDirectionalLight : ILight
    {
        Vector3 Target { get; set; }
        Vector3 Direction { get; }
    }
    public interface IPointLight : ILight
    {
        float Radius { get; set; }
    }
    public interface IConeLight : ILight
    {
        float Angle { get; set; }
        float Decay { get; set; }
        Vector3 Target { get; set; }
        Vector3 Direction { get; }

        void Rotate(Vector3 axis, float angle);
    }
}
