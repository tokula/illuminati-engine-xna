#if WINDOWS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using IlluminatiEngine.Kinect;


namespace IlluminatiEngine
{
    public class BruteForceTerrain : BaseDeferredObject
    {
        public Object RigidBody;

        public int height = 512;
        public int width = 512;
        public VertexPositionColor[] verts;
        public int[] indices;

        Texture2D heightMap;

        Texture2D sand;
        Texture2D sandNorm;

        Texture2D grass;
        Texture2D grassNorm;

        Texture2D stone;
        Texture2D stoneNorm;

        Texture2D snow;
        Texture2D snowNorm;

        public Vector3 LightPosition;
        string heightMapAsset;

        bool KinectFeed = false;

        public BruteForceTerrain(Game game, string heightMapAsset)
            : this(game)
        {
            this.heightMapAsset = heightMapAsset;

            KinectFeed = false;
        }

        public BruteForceTerrain(Game game)
            : base(game)
        {
            scale = Vector3.One;
            position = Vector3.Zero;
            rotation = Quaternion.Identity;

            effect = "Shaders/Terrain/GeoClipMapTerrain";

            KinectFeed = true;
        }

        public void LoadHeightMap()
        {
            width = heightMap.Width;
            height = heightMap.Height;

            verts = new VertexPositionColor[width * height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int idx = x + y * width;
                    verts[idx].Position = new Vector3(y, 0, x);
                }
            }

            CreatePhysicsObject();
        }
        public override void Draw(GameTime gameTime, Effect effect)
        {
            if (heightMap == null)
            {
#if WINDOWS
                if (KinectFeed)
                {
                    heightMap = ((KinectComponent)Game.Services.GetService(typeof(KinectComponent))).depthMap;

                    if (heightMap == null)
                        return;
                }
                else
#endif
                {
                    heightMap = AssetManager.GetAsset<Texture2D>(heightMapAsset);
                }

                LoadHeightMap();
                Color[] data = new Color[heightMap.Width * heightMap.Height];
                heightMap.GetData<Color>(data);

                heightMap = new Texture2D(Game.GraphicsDevice, heightMap.Width, heightMap.Height, true, SurfaceFormat.Vector4);
                Vector4[] data2 = new Vector4[data.Length];
                for (int x = 0; x < data.Length; x++)
                    data2[x] = new Vector4(data[x].R / 255f, data[x].G / 255f, data[x].B / 255f, data[x].A / 255f);
                heightMap.SetData<Vector4>(data2);

                sand = AssetManager.GetAsset<Texture2D>("Textures/Terrain/dirt");
                sandNorm = AssetManager.GetAsset<Texture2D>("Textures/Terrain/dirtNormal");

                grass = AssetManager.GetAsset<Texture2D>("Textures/Terrain/grass");
                grassNorm = AssetManager.GetAsset<Texture2D>("Textures/Terrain/grassNormal");

                stone = AssetManager.GetAsset<Texture2D>("Textures/Terrain/stone");
                stoneNorm = AssetManager.GetAsset<Texture2D>("Textures/Terrain/stoneNormal");

                snow = AssetManager.GetAsset<Texture2D>("Textures/Terrain/snow2");
                snowNorm = AssetManager.GetAsset<Texture2D>("Textures/Terrain/snow2Normal");

            }

            effect.Parameters["world"].SetValue(World);
            if (width == height)
                effect.Parameters["sqrt"].SetValue(new Vector2(width + height, width + height) / 2);
            else
                effect.Parameters["sqrt"].SetValue(new Vector2(width, height) / 2);

            effect.Parameters["heightMap"].SetValue(heightMap);

            if (effect.CurrentTechnique.Name == "ShadowMapT") // Shadow Map
            { }
            else
            {
                effect.Parameters["sqrt"].SetValue((width + height) / 2);
                effect.Parameters["mod"].SetValue((width + height) / 2);

                effect.Parameters["EyePosition"].SetValue(Camera.Position);
                effect.Parameters["maxHeight"].SetValue(30);

                effect.Parameters["LayerMap0"].SetValue(sand);
                effect.Parameters["BumpMap0"].SetValue(sandNorm);

                effect.Parameters["LayerMap1"].SetValue(grass);
                effect.Parameters["BumpMap1"].SetValue(grassNorm);

                effect.Parameters["LayerMap2"].SetValue(stone);
                effect.Parameters["BumpMap2"].SetValue(stoneNorm);

                effect.Parameters["LayerMap3"].SetValue(snow);
                effect.Parameters["BumpMap3"].SetValue(snowNorm);

                effect.Parameters["wvp"].SetValue(World * Camera.View * Camera.Projection);
            }

            effect.CurrentTechnique.Passes[0].Apply();
            Game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, verts, 0, width * height, indices, 0, (width - 1) * (height - 1) * 2);
            
        }

        public void CreatePhysicsObject()
        {
#if BULLETXNA
            CreatePhysicsObjectBulletXNA();
#endif
#if BULLETSHARP
            CreatePhysicsObjectBulletSharp();
#endif
#if JITTER
            CreatePhysicsObjectJitter();
#endif
        }

        public void CreatePhysicsObjectBulletXNA()
        {
            Color[] md = new Color[verts.Length];
            heightMap.GetData<Color>(md);

            BulletXNA.LinearMath.ObjectArray<Vector3> realVerts = new BulletXNA.LinearMath.ObjectArray<Vector3>(verts.Length);
            BulletXNA.LinearMath.ObjectArray<int> terrainIndices = new BulletXNA.LinearMath.ObjectArray<int>((width - 1) * (height - 1) * 6);


            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int idx = x + y * width;
                    verts[idx].Position = new Vector3(y, 0, x);
                    realVerts[idx] = new Vector3(x, (md[idx].R / 256f) * 30f, y);
                }
            }

            
            for (int x = 0; x < width - 1; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    terrainIndices[(x + y * (width - 1)) * 6] = ((x + 1) + (y + 1) * width);
                    terrainIndices[(x + y * (width - 1)) * 6 + 1] = ((x + 1) + y * width);
                    terrainIndices[(x + y * (width - 1)) * 6 + 2] = (x + y * width);

                    terrainIndices[(x + y * (width - 1)) * 6 + 3] = ((x + 1) + (y + 1) * width);
                    terrainIndices[(x + y * (width - 1)) * 6 + 4] = (x + y * width);
                    terrainIndices[(x + y * (width - 1)) * 6 + 5] = (x + (y + 1) * width);
                }
            }


            // Build the physics stuff
            BulletXNA.BulletCollision.TriangleIndexVertexArray vertexArray = new BulletXNA.BulletCollision.TriangleIndexVertexArray(((width - 1) * (height - 1) * 2), terrainIndices, 1, realVerts.Count, realVerts, 1);

            BulletXNA.BulletCollision.CollisionShape btTerrain = new BulletXNA.BulletCollision.BvhTriangleMeshShape(vertexArray, true, true);
            BulletXNA.BulletDynamics.RigidBodyConstructionInfo rbInfo = new BulletXNA.BulletDynamics.RigidBodyConstructionInfo(0, new BulletXNA.DefaultMotionState(Matrix.CreateTranslation(Vector3.Zero), Matrix.Identity), btTerrain, Vector3.Zero);

            BulletXNA.BulletDynamics.RigidBody bulletXNARigidBody = new BulletXNA.BulletDynamics.RigidBody(rbInfo);

            bulletXNARigidBody.Translate(Position);
            indices = terrainIndices.GetRawArray();
            RigidBody = bulletXNARigidBody;

        }

        public void CreatePhysicsObjectBulletSharp()
        {
            BulletSharp.TriangleIndexVertexArray vertexArray = new BulletSharp.TriangleIndexVertexArray();
            BulletSharp.IndexedMesh mesh = new BulletSharp.IndexedMesh();

            mesh.Allocate(verts.Length, System.Runtime.InteropServices.Marshal.SizeOf(Vector3.Zero), (width - 1) * (height - 1) * 2, 3 * sizeof(int));
            BulletSharp.DataStream vData = mesh.LockVerts();

            Color[] md = new Color[verts.Length];
            heightMap.GetData<Color>(md);

            Vector3[] realVerts = new Vector3[verts.Length];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int idx = x + y * width;
                    realVerts[idx] = new Vector3(x, (md[idx].R / 256f) * 30f, y);
                }
            }

            for (int v = 0; v < realVerts.Length; v++)
            {
                vData.Write(realVerts[v].X);
                vData.Write(realVerts[v].Y);
                vData.Write(realVerts[v].Z);
            }

            indices = new int[(width - 1) * (height - 1) * 6];
            for (int x = 0; x < width - 1; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    indices[(x + y * (width - 1)) * 6] = ((x + 1) + (y + 1) * width);
                    indices[(x + y * (width - 1)) * 6 + 1] = ((x + 1) + y * width);
                    indices[(x + y * (width - 1)) * 6 + 2] = (x + y * width);

                    indices[(x + y * (width - 1)) * 6 + 3] = ((x + 1) + (y + 1) * width);
                    indices[(x + y * (width - 1)) * 6 + 4] = (x + y * width);
                    indices[(x + y * (width - 1)) * 6 + 5] = (x + (y + 1) * width);
                }
            }



            BulletSharp.IntArray iData = mesh.TriangleIndices;
            for (int idx = 0; idx < indices.Length; idx++)
            {
                iData[idx] = indices[idx];
            }

            vertexArray.AddIndexedMesh(mesh);
            BulletSharp.CollisionShape btTerrain = new BulletSharp.BvhTriangleMeshShape(vertexArray, true);
            BulletSharp.RigidBodyConstructionInfo rbInfo =
                            new BulletSharp.RigidBodyConstructionInfo(0, new BulletSharp.DefaultMotionState(Matrix.Identity), btTerrain, Vector3.Zero);

            BulletSharp.RigidBody bulletSharpRigidBody = new BulletSharp.RigidBody(rbInfo);

            bulletSharpRigidBody.Translate(Position);

            RigidBody = bulletSharpRigidBody;

        }

        public void CreatePhysicsObjectJitter()
        {


        }


    }
}
#endif