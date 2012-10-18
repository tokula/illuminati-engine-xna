using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine
{
    public class GeographicGridRegistrationSystem : DrawableComponentService, IGGRService
    {
        public IAssetManager AssetManager
        {
            get { return ((IAssetManager)Game.Services.GetService(typeof(IAssetManager))); }
        }
        protected Effect shader;
        protected short[] indsMap = new short[] {
			    0, 1, 0, 
                2, 1, 3, 
                2, 3, 4, 
                5, 4, 6, 
                5, 7, 6, 
                7, 0, 4, 
                1, 5, 2, 
                6, 3, 7
		        };

        public Vector3 gridDimensions { get; set; }
        public BoundingBox gridElementSize { get; set; }

        public Dictionary<Vector3, BoundingBox> theGrid { get; set; }
        public Dictionary<Vector3, List<IGridRegisterable>> gridObjects;
        public List<IGridRegisterable> listObjects;
        public Dictionary<IGridRegisterable, List<Vector3>> objectRegisteredZones;
        public List<BoundingBox> bounds = new List<BoundingBox>();

        protected VertexPositionColor[] points;
        protected short[] index;

        protected Vector3 width;
        protected Vector3 elWidth;
        protected Vector3 min;
        protected Vector3 max;
        protected BoundingBox masterbb;

        protected SpriteFont font;

        protected ICameraService Camera
        {
            get { return (ICameraService)Game.Services.GetService(typeof(ICameraService)); }
        }
        protected SpriteBatch spriteBatch
        {
            get { return (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch)); }
        }

        public GeographicGridRegistrationSystem(Game game, Vector3 dimensions, BoundingBox boxSize)
            : base(game)
        {
            gridDimensions = dimensions;
            gridElementSize = boxSize;

            buildGrid();
        }


        private void buildGrid()
        {
            // Build the grid.
            gridObjects = new Dictionary<Vector3, List<IGridRegisterable>>();
            theGrid = new Dictionary<Vector3, BoundingBox>();
            listObjects = new List<IGridRegisterable>();
            bounds = new List<BoundingBox>();
            objectRegisteredZones = new Dictionary<IGridRegisterable, List<Vector3>>();

            width = ((-gridElementSize.Min + gridElementSize.Max));

            for (int x = 0; x < gridDimensions.X; x++)
            {
                for (int y = 0; y < gridDimensions.Y; y++)
                {
                    for (int z = 0; z < gridDimensions.Z; z++)
                    {
                        Vector3 gridRef = new Vector3(x, y, z);
                        Vector3 pos = gridRef * width;
                        pos -= (width * gridDimensions) / 2 - (width / 2);
                        BoundingBox locBounds = new BoundingBox(gridElementSize.Min + pos, gridElementSize.Max + pos);
                        theGrid.Add(gridRef, locBounds);
                        gridObjects.Add(gridRef, new List<IGridRegisterable>());
                        bounds.Add(locBounds);
                    }
                }
            }

            elWidth = (-gridElementSize.Min + gridElementSize.Max);
            width = elWidth * gridDimensions;

            min = -(width / 2);
            max = (width / 2);
            masterbb = new BoundingBox(min, max);
        }
        public void AddObject(IGridRegisterable obj)
        {
            bool added = false;

            for (int o = 0; o < listObjects.Count; o++)
            {
                if (listObjects[o] == null)
                {
                    listObjects[o] = obj;
                    added = true;
                    break;
                }
            }
            if (!added)
                listObjects.Add(obj);
            objectRegisteredZones.Add(obj, new List<Vector3>());
            RegisterObject(obj);
        }
        protected override void LoadContent()
        {
            font = AssetManager.GetAsset<SpriteFont>("Fonts/font");
            base.LoadContent();
        }
        public override void Update(GameTime gameTime)
        {
            // Check if object has moved. If it has, re register.
            if (Enabled)
            {
                for (int i = 0; i < listObjects.Count; i++)
                {
                    if (listObjects[i] == null)
                        continue;
                    if (listObjects[i].HasMoved)
                        RegisterObject(listObjects[i]);

                }
            }
            base.Update(gameTime);
        }
        private void RegisterObject(IGridRegisterable obj)
        {
            bool oob = false;
            List<Vector3> registeredZones = new List<Vector3>();
            List<Vector3> gridRefs = new List<Vector3>();
            List<IGridRegisterable> objList = new List<IGridRegisterable>();

            for (int b = 0;obj.Bounds != null && b < obj.Bounds.Count; b++)
            {
                if (masterbb.Intersects(obj.Bounds[b]))
                {
                    oob = true;
                    break;
                }
            }
            // Not in the grid anymore :(
            if (!oob)
            {
                // Clean up my registrations.
                registeredZones = objectRegisteredZones[obj];

                for (int u = 0; u < registeredZones.Count; u++)
                {
                    objList = gridObjects[registeredZones[u]];
                    objList.Remove(obj);
                    gridObjects[registeredZones[u]] = objList;

                    registeredZones.Remove(registeredZones[u]);
                }
                return;
            }



            if (objectRegisteredZones.ContainsKey(obj))
                registeredZones = objectRegisteredZones[obj];

            gridRefs = GetGridRef(obj);



            // If not then find my initial place in the grid..
            // Unregister from the grid where we are no longer present.
            for (int u = 0; u < registeredZones.Count; u++)
            {
                if (!gridRefs.Contains(registeredZones[u]))
                {
                    objList = gridObjects[registeredZones[u]];
                    objList.Remove(obj);
                    gridObjects[registeredZones[u]] = objList;

                    registeredZones.Remove(registeredZones[u]);
                }
            }

            // Register with these grid ref.
            for (int r = 0; r < gridRefs.Count; r++)
            {
                if (!registeredZones.Contains(gridRefs[r]))
                {
                    objList = gridObjects[gridRefs[r]];
                    objList.Add(obj);
                    gridObjects[gridRefs[r]] = objList;
                    registeredZones.Add(gridRefs[r]);
                }
            }
            objectRegisteredZones[obj] = registeredZones;
        }

        public List<BoundingBox> GetRefBounds(List<Vector3> refs)
        {
            List<BoundingBox> retVal = new List<BoundingBox>();

            foreach (Vector3 r in refs)
                retVal.Add(theGrid[r]);

            return retVal;
        }
        public List<Vector3> GetGridRef(IGridRegisterable obj)
        {
            List<Vector3> gridRefs = new List<Vector3>();

            int xs = int.MaxValue, ys = int.MaxValue, zs = int.MaxValue;
            int xf = (int)gridDimensions.X, yf = (int)gridDimensions.Y, zf = (int)gridDimensions.Z;
            xf = 0; yf = 0; zf = 0;

            if (obj is ICameraService)
            { }
            else
            {
                // Comvert to Grid position.
                for (int b = 0; b < obj.Bounds.Count; b++)
                {
                    // Am I in the GGR?
                    if (masterbb.Intersects(obj.Bounds[b]))
                    {
                        // Now what box do we first intersect?
                        Vector3 nearestMin = (obj.Bounds[b].Min - min) / elWidth;
                        Vector3 nearestMax = (obj.Bounds[b].Max + max) / elWidth;

                        xs = (int)MathHelper.Clamp(MathHelper.Min(xs, nearestMin.X), 0, gridDimensions.X - 1);
                        ys = (int)MathHelper.Clamp(MathHelper.Min(ys, nearestMin.Y), 0, gridDimensions.Y - 1);
                        zs = (int)MathHelper.Clamp(MathHelper.Min(zs, nearestMin.Z), 0, gridDimensions.Z - 1);

                        xf = (int)MathHelper.Clamp(MathHelper.Max(xf, nearestMax.X), 0, gridDimensions.X - 1);
                        yf = (int)MathHelper.Clamp(MathHelper.Max(yf, nearestMax.Y), 0, gridDimensions.Y - 1);
                        zf = (int)MathHelper.Clamp(MathHelper.Max(zf, nearestMax.Z), 0, gridDimensions.Z - 1);
                    }
                }
            }

            bool outLoop = false;
            for (int x = xs; x <= xf && !outLoop; x++)
            {
                for (int y = ys; y <= yf && !outLoop; y++)
                {
                    for (int z = zs; z <= zf && !outLoop; z++)
                    {
                        Vector3 checkRef = new Vector3(x, y, z);

                        if (obj is ICameraService)
                        {
                            if (theGrid[checkRef].Intersects(((ICameraService)obj).Frustum))
                                gridRefs.Add(checkRef);
                        }
                        else
                        {
                            for (int b = 0; b < obj.Bounds.Count; b++)
                            {
                                if (theGrid[checkRef].Intersects(obj.Bounds[b]))
                                {
                                    gridRefs.Add(checkRef);
                                    // Do do later
                                    //if (obj is ITerrain || obj is IVolumetric || obj is IPlatform3D)
                                    //    outLoop = false;
                                    //else
                                    //outLoop = true;
                                }
                            }
                        }
                    }
                }
            }

            return gridRefs;
        }
        public bool DrawGrid = false;
        public override void Draw(GameTime gameTime)
        {
            if (DrawGrid)
                DrawBounds(bounds);

            //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            //foreach(Vector3 position in theGrid.Keys)
            //{
            //    spriteBatch.DrawString(font, position.ToString(), Get2DCoords((theGrid[position].Min + theGrid[position].Max) / 2) - new Vector2(font.MeasureString(position.ToString()).X / 2, 0), new Color(position));
            //}

            //spriteBatch.End();
        }

        public List<IGridRegisterable> GetObjectsInMyZone(IGridRegisterable obj)
        {
            List<Vector3> registeredZones = new List<Vector3>();
            List<IGridRegisterable> objList = new List<IGridRegisterable>();

            if (objectRegisteredZones.ContainsKey(obj))
                registeredZones = objectRegisteredZones[obj];

            if (registeredZones.Count > 0)
            {
                for (int z = 0; z < registeredZones.Count; z++)
                {
                    for (int o = 0; o < gridObjects[registeredZones[z]].Count; o++)
                    {
                        if (gridObjects[registeredZones[z]][o] != obj && /*!objList.Exists(delegate(IGridRegisterable igr) { return igr == obj; }) &&*/ !objList.Contains(gridObjects[registeredZones[z]][o]))
                            objList.Add(gridObjects[registeredZones[z]][o]);
                    }
                }
            }
            return objList;
        }

        private void DrawBounds(List<BoundingBox> bounds)
        {
            BuildBoxCorners();

            if (shader == null)
                shader = AssetManager.GetAsset<Effect>("Shaders/Deferred/DeferredBasicEffect");


            shader.Parameters["world"].SetValue(Matrix.Identity);
            shader.Parameters["wvp"].SetValue(Matrix.Identity * Camera.View * Camera.Projection);

            //shader.World = Matrix.Identity;
            //shader.View = Camera.View;
            //shader.Projection = Camera.Projection;
            //shader.VertexColorEnabled = true;

            //shader.World = Matrix.Identity;
            //shader.View = camera.View;
            //shader.Projection = camera.Projection;
            //shader.VertexColorEnabled = true;

            shader.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.LineList, points, 0, points.Length, index, 0, 12 * bounds.Count);
        }
        protected void BuildBoxCorners()
        {
            points = new VertexPositionColor[bounds.Count * 8];
            short[] inds = new short[points.Length * 3];

            for (int b = 0; b < bounds.Count; b++)
            {
                Vector3[] thisCorners = bounds[b].GetCorners();

                points[(b * 8) + 0] = new VertexPositionColor(thisCorners[1], Color.DarkBlue);
                points[(b * 8) + 1] = new VertexPositionColor(thisCorners[0], Color.DarkBlue);
                points[(b * 8) + 2] = new VertexPositionColor(thisCorners[2], Color.DarkBlue);
                points[(b * 8) + 3] = new VertexPositionColor(thisCorners[3], Color.DarkBlue);
                points[(b * 8) + 4] = new VertexPositionColor(thisCorners[5], Color.DarkBlue);
                points[(b * 8) + 5] = new VertexPositionColor(thisCorners[4], Color.DarkBlue);
                points[(b * 8) + 6] = new VertexPositionColor(thisCorners[6], Color.DarkBlue);
                points[(b * 8) + 7] = new VertexPositionColor(thisCorners[7], Color.DarkBlue);

                for (int i = 0; i < 24; i++)
                {
                    inds[(b * 24) + i] = (short)(indsMap[i] + (b * 8));
                }
            }

            index = inds;
        }
        public Vector2 Get2DCoords(Vector3 myPosition)
        {
            Matrix ViewProjectionMatrix = Camera.View * Camera.Projection;

            Vector4 result4 = Vector4.Transform(myPosition, ViewProjectionMatrix);

            if (result4.W <= 0)
                return new Vector2(Camera.Viewport.Width, 0);

            Vector3 result = new Vector3(result4.X / result4.W, result4.Y / result4.W, result4.Z / result4.W);

            Vector2 retVal = new Vector2((int)Math.Round(+result.X * (Camera.Viewport.Width / 2)) + (Camera.Viewport.Width / 2), (int)Math.Round(-result.Y * (Camera.Viewport.Height / 2)) + (Camera.Viewport.Height / 2));
            return retVal;
        }
    }
}
