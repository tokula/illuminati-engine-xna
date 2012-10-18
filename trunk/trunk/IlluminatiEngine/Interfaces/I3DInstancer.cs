using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine
{
    public interface I3DInstancer
    {
        List<string> TextureMaterials { get; set; }
        List<string> NormalMaterials { get; set; }
        List<string> SpeculaMaterials { get; set; }
        List<string> GlowMaterials { get; set; }
        List<string> ReflectionMaterials { get; set; }

        List<BoundingBox> AABoxBounds { get; set; }

        IAssetManager AssetManager { get; }
        ICameraService Camera { get; }

        Dictionary<Base3DDeferredObjectInstance, Matrix> instanceTransformMatrices { get; set; }
        VertexBuffer modelVertexBuffer { get; set; }
        int vertCount { get; set; }
        IndexBuffer indexBuffer { get; set; }

        string Mesh { get; set; }

        Dictionary<int, Base3DDeferredObjectInstance> Instances { get; set; }

        bool RenderBounds { get; set; }
    }
}
