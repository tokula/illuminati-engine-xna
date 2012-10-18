using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace IlluminatiEngine
{
    public interface IGGRService
    {
        Dictionary<Vector3, BoundingBox> theGrid { get; set; }
        Vector3 gridDimensions { get; set; }
        BoundingBox gridElementSize { get; set; }

        List<Vector3> GetGridRef(IGridRegisterable obj);
        List<BoundingBox> GetRefBounds(List<Vector3> refs);
        List<IGridRegisterable> GetObjectsInMyZone(IGridRegisterable obj);

    }
}
