﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine
{
    public enum FixUpTypes
    {
        Top,
        Left,
        Right,
        Bottom
    }
    public class GeoClipMapFootPrintFixUp : GeoClipMapFootPrintBase
    {
        public FixUpTypes FixUpType = FixUpTypes.Top;
        public GeoClipMapFootPrintFixUp(Game game, short m, bool lr)
            : base(game)
        {
            verts = new VertexPositionColor[m * 3];

            if (!lr)
            {
                width = m;
                height = 3;
            }
            else
            {
                width = 3;
                height = m;
            }

            color = Color.Green;
        }

        public override void Initialize()
        {
            base.Initialize();

            // Make the edge red.
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    switch (FixUpType)
                    {
                        case FixUpTypes.Top:
                            if (x == 0)
                                verts[x + y * width].Color = new Color(1f, 0, 0, 1f);
                            break;
                        case FixUpTypes.Left:
                            if (y == 0)
                                verts[x + y * width].Color = new Color(1f, 1f, 0, 1f);
                            break;
                        case FixUpTypes.Right:
                            if (y == height - 1)
                                verts[x + y * width].Color = new Color(1f, 1f, 1f, 1f);
                            break;
                        case FixUpTypes.Bottom:
                            if (x == width - 1)
                                verts[x + y * width].Color = new Color(1f, 1f, 1f, 0);
                            break;
                    }
                }
            }
        }
    }
}
