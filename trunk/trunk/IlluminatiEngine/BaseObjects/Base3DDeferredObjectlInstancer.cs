using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine
{
    public interface IInstanced
    {
        void Draw(GameTime gameTime);
        void Draw(GameTime gameTime, Effect effect);
    }

    public interface IIGeomInstanced
    {
        Dictionary<int, Base3DDeferredObjectInstance> Instances { get; set; }
        Dictionary<Base3DDeferredObjectInstance, InstanceVertex> instanceTransformMatrices { get; set; }
    }

    public struct InstanceVertex : IVertexType
    {
        public Matrix TransformMatrix { get; set; }
        public Vector4 Extras { get; set; }

        public VertexDeclaration VertexDeclaration
        {
            get
            {
                return new VertexDeclaration
                    (
                    new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
                    new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
                    new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
                    new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3),
                    new VertexElement(64, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 4)
                    );
            }
        }
    }

    public class Base3DDeferredObjectlInstancer : DrawableGameComponent, IDeferredRender, IHasMesh, IInstanced, IIGeomInstanced
    {
        public List<string> TextureMaterials { get; set; }
        public List<string> NormalMaterials { get; set; }
        public List<string> SpeculaMaterials { get; set; }
        public List<string> GlowMaterials { get; set; }
        public List<string> ReflectionMaterials { get; set; }
        protected Texture2D blank;

        protected List<BoundingSphere> aaSphereBounds;
        public List<BoundingSphere> AASphereBounds
        {
            get
            {
                return aaSphereBounds;
            }
            set
            {
                aaSphereBounds = value;
            }
        }

        protected List<BoundingBox> aaBounds;
        public List<BoundingBox> AABoxBounds
        {
            get
            {
                if (aaBounds == null && meshData != null)
                    aaBounds = ((List<BoundingBox>)meshData["BoundingBoxs"]);
                return aaBounds;
            }
            set
            {
                aaBounds = value;
            }
        }

        public IAssetManager AssetManager
        {
            get { return ((IAssetManager)Game.Services.GetService(typeof(IAssetManager))); }
        }

        public ICameraService Camera
        {
            get { return ((ICameraService)Game.Services.GetService(typeof(ICameraService))); }
        }

        protected DynamicVertexBuffer instanceVertexBuffer;

        //public BaseDeferredObject Object;
        public Dictionary<Base3DDeferredObjectInstance, InstanceVertex> instanceTransformMatrices { get; set; }
        public VertexBuffer modelVertexBuffer { get; set; }
        public int vertCount { get; set; }
        public IndexBuffer indexBuffer { get; set; }

        protected Model thisMesh;
        protected string mesh;
        public string Mesh
        {
            get
            {
                return mesh;
            }
            set
            {
                mesh = value;
            }
        }

        public Dictionary<int, Base3DDeferredObjectInstance> Instances { get; set; }

        protected Effect effect;

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
        protected VertexPositionColor[] points;
        protected short[] index;
        protected List<BoundingBox> bounds = new List<BoundingBox>();
        public bool RenderBounds { get; set; }
        protected Dictionary<string, object> meshData;

        protected static VertexDeclaration instanceVertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
            new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3),
            new VertexElement(64, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 4)
        );

        /// <summary>
        /// Specialized vertex structure for the instancing
        /// </summary>
        public struct VertexPositionNormalTextureInstance : IVertexType
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 TexCoord;
            public Vector3 Tangent;
            public Vector4 InstanceData;
            //public Vector3 BlendI;
            //public Vector3 BlendW;
            
            public VertexPositionNormalTextureInstance(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 texcoord)
            {
                Position = position;
                Normal = normal;
                TexCoord = texcoord;
                Tangent = tangent;
                InstanceData = Vector4.Zero;
                //BlendI = Vector3.Zero;
                //BlendW = Vector3.Zero;
            }

            public VertexPositionNormalTextureInstance(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 texcoord, Vector4 data)
            {
                Position = position;
                Normal = normal;
                Tangent = tangent;
                TexCoord = texcoord;
                InstanceData = data;
                //BlendI = Vector3.Zero;
                //BlendW = Vector3.Zero;
            }

            //public VertexPositionNormalTextureInstance(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 texcoord, Vector3 blendI,Vector3 blendW)
            //{
            //    Position = position;
            //    Normal = normal;
            //    Tangent = tangent;
            //    TexCoord = texcoord;
            //    InstanceData = Vector4.Zero;
            //    BlendI = blendI;
            //    BlendW = blendW;
            //}

            //public VertexPositionNormalTextureInstance(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 texcoord, Vector4 data, Vector3 blendI, Vector3 blendW)
            //{
            //    Position = position;
            //    Normal = normal;
            //    Tangent = tangent;
            //    TexCoord = texcoord;
            //    InstanceData = data;
            //    BlendI = blendI;
            //    BlendW = blendW;
            //}

            static public VertexElement[] VertexElements = new VertexElement[]
            {
                new VertexElement(0,VertexElementFormat.Vector3,VertexElementUsage.Position,0),
                new VertexElement(4*3,VertexElementFormat.Vector3,VertexElementUsage.Normal,0),                
                new VertexElement(4*6,VertexElementFormat.Vector2 ,VertexElementUsage.TextureCoordinate,0),
                new VertexElement(4*8,VertexElementFormat.Vector3,VertexElementUsage.Tangent,0),
                new VertexElement(4*11,VertexElementFormat.Vector4,VertexElementUsage.Position,1)
                //new VertexElement(4*15,VertexElementFormat.Vector3,VertexElementUsage.BlendIndices,0),
                //new VertexElement(4*18,VertexElementFormat.Vector3,VertexElementUsage.BlendWeight,0),

            };

            public static int SizeInBytes = (3 + 3 + 2 + 3 + 4 /*+ 3 + 3*/) * 4;

            #region IVertexType Members


            public VertexDeclaration VertexDeclaration
            {
                get
                {
                    return new VertexDeclaration
                            (
                            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                            new VertexElement(4 * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                            new VertexElement(4 * 6, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                            new VertexElement(4 * 8, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
                            new VertexElement(4 * 11, VertexElementFormat.Vector4, VertexElementUsage.Position, 1)
                            //new VertexElement(4 * 15, VertexElementFormat.Vector3, VertexElementUsage.BlendIndices, 0),
                            //new VertexElement(4 * 18, VertexElementFormat.Vector3, VertexElementUsage.BlendWeight, 0)
                            );
                }
            }
            #endregion
        }

        
        public Base3DDeferredObjectlInstancer(Game game, string modelAssetName) : base(game)
        {
            Instances = new Dictionary<int, Base3DDeferredObjectInstance>();
            instanceTransformMatrices = new Dictionary<Base3DDeferredObjectInstance, InstanceVertex>();
        
            vertCount = 0;
        

            mesh = modelAssetName;

            TextureMaterials = new List<string>();
            NormalMaterials = new List<string>();
            SpeculaMaterials = new List<string>();
            GlowMaterials = new List<string>();
            ReflectionMaterials = new List<string>();
        }
        public override void Initialize()
        {
            //Object.Initialize();
            base.Initialize();

            blank = new Texture2D(Game.GraphicsDevice, 1, 1);
            blank.SetData<Color>(new Color[] { Color.Black });            
        }
        Matrix[] transforms;
        protected override void LoadContent()
        {
            base.LoadContent();

            thisMesh = AssetManager.GetAsset<Model>(mesh);

            meshData = (Dictionary<string, object>)thisMesh.Tag;



            transforms = new Matrix[thisMesh.Bones.Count];
            thisMesh.CopyAbsoluteBoneTransformsTo(transforms);

            effect = AssetManager.GetAsset<Effect>("Shaders/Deferred/DeferredInstanceShader");
            effect.CurrentTechnique = effect.Techniques["BasicTextureH"];

            Dictionary<string, object> TagData = (Dictionary<string, object>)thisMesh.Tag;

            Dictionary<int, List<Vector3>> p = (Dictionary<int, List<Vector3>>)TagData["Vertices"];
            Dictionary<int, List<Vector3>> n = (Dictionary<int, List<Vector3>>)TagData["Normals"];
            Dictionary<int, List<Vector2>> t = (Dictionary<int, List<Vector2>>)TagData["TexCoords"];
            Dictionary<int, List<Vector3>> tang = (Dictionary<int, List<Vector3>>)TagData["Tangents"];
            Dictionary<int, List<Vector3>> bin = (Dictionary<int, List<Vector3>>)TagData["BiNormals"];
            Dictionary<int, List<int>> i = (Dictionary<int, List<int>>)TagData["Inicies"];

            // Build Vert Elemets
            List<VertexPositionNormalTextureInstance> verts = new List<VertexPositionNormalTextureInstance>();
            List<int> indx = new List<int>();

            for (int d = 0; d < p.Count; d++)
            {
                for (int v = 0; v < p[d].Count; v++)
                {
                    verts.Add(new VertexPositionNormalTextureInstance(p[d][v], n[d][v], tang[d][v], t[d][v]));
                    indx.Add(i[d][v]);
                }
            }
            vertCount = verts.Count;
            modelVertexBuffer = new VertexBuffer(Game.GraphicsDevice, typeof(VertexPositionNormalTextureInstance), vertCount, BufferUsage.WriteOnly);
            modelVertexBuffer.SetData(verts.ToArray());
            indexBuffer = new IndexBuffer(Game.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indx.Count, BufferUsage.WriteOnly);
            indexBuffer.SetData(indx.ToArray());
        }

        public override void Draw(GameTime gameTime)
        {
            Draw(gameTime, effect);

        }
        public virtual void Draw(GameTime gameTime, Effect thisEffect)
        {
            if (!Visible || !Enabled)
                return;

            if (instanceTransformMatrices.Count == 0)
                return;

            // If we have more instances than room in our vertex buffer, grow it to the neccessary size.
            if ((instanceVertexBuffer == null) ||
                (instanceTransformMatrices.Count > instanceVertexBuffer.VertexCount))
            {
                if (instanceVertexBuffer != null)
                    instanceVertexBuffer.Dispose();

                instanceVertexBuffer = new DynamicVertexBuffer(GraphicsDevice, instanceVertexDeclaration, instanceTransformMatrices.Count, BufferUsage.WriteOnly);
            }

            // Transfer the latest instance transform matrices into the instanceVertexBuffer.
            instanceVertexBuffer.SetData(instanceTransformMatrices.Values.ToArray(), 0, instanceTransformMatrices.Count, SetDataOptions.Discard);

            if (thisEffect.CurrentTechnique.Name == "ShadowMapH") // Shadow Map
            { }
            else
            {
                thisEffect.Parameters["color"].SetValue(Vector3.One);

                // Texture
                if (TextureMaterials.Count > 0)
                    thisEffect.Parameters["textureMat"].SetValue(AssetManager.GetAsset<Texture2D>(TextureMaterials[0]));
                else
                {
                    thisEffect.Parameters["textureMat"].SetValue(blank);
                }

                // Normals
                if (NormalMaterials.Count > 0)
                    thisEffect.Parameters["BumpMap"].SetValue(AssetManager.GetAsset<Texture2D>(NormalMaterials[0]));
                else
                    thisEffect.Parameters["BumpMap"].SetValue(blank);

                // Specula
                if (SpeculaMaterials.Count > 0)
                    thisEffect.Parameters["specularMap"].SetValue(AssetManager.GetAsset<Texture2D>(SpeculaMaterials[0]));
                else
                    thisEffect.Parameters["specularMap"].SetValue(blank);

                // Glow
                if (GlowMaterials.Count > 0)
                    thisEffect.Parameters["glowMap"].SetValue(AssetManager.GetAsset<Texture2D>(GlowMaterials[0]));
                else
                    thisEffect.Parameters["glowMap"].SetValue(blank);

                // Reflection
                if (ReflectionMaterials.Count > 0)
                    thisEffect.Parameters["reflectionMap"].SetValue(AssetManager.GetAsset<Texture2D>(ReflectionMaterials[0]));
                else
                    thisEffect.Parameters["reflectionMap"].SetValue(blank);
                
                thisEffect.Parameters["vp"].SetValue(Camera.View * Camera.Projection);
            }

            
            GraphicsDevice.SetVertexBuffers(
                        new VertexBufferBinding(modelVertexBuffer, 0, 0),
                        new VertexBufferBinding(instanceVertexBuffer, 0, 1)
                    );


            thisEffect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.Indices = indexBuffer;
            Game.GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                                   modelVertexBuffer.VertexCount, 0,
                                                   modelVertexBuffer.VertexCount / 3,
                                                   instanceTransformMatrices.Count);

            if (RenderBounds)
                DrawBounds();
        }
        
        private void DrawBounds()
        {
            if (Instances.Count == 0)
                return;

            if (bounds.Count == 0)
            {
                BoundingBox bb = AABoxBounds[0];

                for (int b = 0; b < Instances.Count; b++)
                {
                    bounds.Add(bb);
                }
            }

            BuildBoxCorners();

            if (shader == null)
                shader = AssetManager.GetAsset<Effect>("Shaders/Deferred/DeferredBasicEffect");


            shader.Parameters["world"].SetValue(Matrix.Identity);
            shader.Parameters["wvp"].SetValue(Matrix.Identity * Camera.View * Camera.Projection);            

            //shader.World = Matrix.Identity;
            //shader.View = Camera.View;
            //shader.Projection = Camera.Projection;
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

                for (int c = 0; c < thisCorners.Length; c++)
                {
                    thisCorners[c] = Vector3.Transform(thisCorners[c], Instances[b].AAWorld);
                }

                points[(b * 8) + 0] = new VertexPositionColor(thisCorners[1], Color.Red);
                points[(b * 8) + 1] = new VertexPositionColor(thisCorners[0], Color.Red);
                points[(b * 8) + 2] = new VertexPositionColor(thisCorners[2], Color.Red);
                points[(b * 8) + 3] = new VertexPositionColor(thisCorners[3], Color.Red);
                points[(b * 8) + 4] = new VertexPositionColor(thisCorners[5], Color.Red);
                points[(b * 8) + 5] = new VertexPositionColor(thisCorners[4], Color.Red);
                points[(b * 8) + 6] = new VertexPositionColor(thisCorners[6], Color.Red);
                points[(b * 8) + 7] = new VertexPositionColor(thisCorners[7], Color.Red);

                for (int i = 0; i < 24; i++)
                {
                    inds[(b * 24) + i] = (short)(indsMap[i] + (b * 8));
                }
            }

            index = inds;
        }

        public List<BoundingBox> GetInstanceAABounds(int idx)
        {
            List<BoundingBox> thisBounds = new List<BoundingBox>();

            int cnt = AABoxBounds.Count;
            for(int b = 0;b<cnt;b++)
                thisBounds.Add(new BoundingBox(Vector3.Transform(AABoxBounds[b].Min, Instances[idx].World), Vector3.Transform(AABoxBounds[b].Max, Instances[idx].World)));

            return thisBounds; 
        }
    }
}
