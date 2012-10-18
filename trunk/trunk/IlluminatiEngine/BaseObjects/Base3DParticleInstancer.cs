using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BulletXNA.LinearMath;

namespace IlluminatiEngine
{
    public class Base3DParticleInstancer : DrawableGameComponent, IInstanced
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Scale = Vector3.One;
        public Quaternion Rotation = Quaternion.Identity;
        public Vector2 Size = Vector2.One * .5f;
        public Matrix World = Matrix.Identity;

        public Vector3 FixedCameraPos = Vector3.One * 10000;

        public List<string> TextureMaterials { get; set; }
        public List<string> NormalMaterials { get; set; }

        public bool PseudoVoxel = false;

        protected Texture2D blank;
        protected Texture2D blank_normal;

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
        public ObjectArray<Matrix> tempMatrixList = new ObjectArray<Matrix>();
        public Dictionary<Base3DParticleInstance, Matrix> instanceTransformMatrices = new Dictionary<Base3DParticleInstance, Matrix>();
        public VertexBuffer modelVertexBuffer;
        public int vertCount = 0;
        public IndexBuffer indexBuffer;

        public Dictionary<int, Base3DParticleInstance> Instances = new Dictionary<int, Base3DParticleInstance>();

        protected Effect effect;

        protected static VertexDeclaration instanceVertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
            new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3)
        );

        string effectAsset;
        public Color Color = Color.White;

        public Base3DParticleInstancer(Game game, string effectAsset)
            : base(game)
        {
            this.effectAsset = effectAsset;
            TextureMaterials = new List<string>();
            NormalMaterials = new List<string>();
            rnd = new Random(DateTime.Now.Millisecond);
        }
        public override void Initialize()
        {
            //Object.Initialize();
            base.Initialize();

            blank = new Texture2D(Game.GraphicsDevice, 1, 1);
            blank.SetData<Color>(new Color[] { Color.Black });

            blank_normal = new Texture2D(Game.GraphicsDevice, 1, 1);
            blank_normal.SetData<Color>(new Color[] { new Color(128, 128, 255) });        
        }
        protected Random rnd;
        protected override void LoadContent()
        {
            base.LoadContent();

            effect = AssetManager.GetAsset<Effect>(effectAsset);
            effect.CurrentTechnique = effect.Techniques["BasicTextureH"];

            List<Vector2> t = new List<Vector2>();

            // Build Vert Elemets
            List<VertexPositionTexture> verts = new List<VertexPositionTexture>();
            List<int> indx = new List<int>();
            Vector2 texCoord = Vector2.Zero;
            Vector4 extras = new Vector4((float)rnd.NextDouble(), Size.X, Size.Y, (float)rnd.NextDouble());

            indx.Add(0);
            indx.Add(1);
            indx.Add(2);
            indx.Add(2);
            indx.Add(3);
            indx.Add(0);

            for (int d = 0; d < 4; d++)
            {
                switch (d)
                {
                    case 0:
                        texCoord = new Vector2(1, 1);
                        break;
                    case 1:
                        texCoord = new Vector2(0, 1);
                        break;
                    case 2:
                        texCoord = new Vector2(0, 0);
                        break;
                    case 3:
                        texCoord = new Vector2(1, 0);
                        break;
                }
                verts.Add(new VertexPositionTexture(Vector3.Zero, texCoord));
            }

            vertCount = verts.Count;
            modelVertexBuffer = new VertexBuffer(Game.GraphicsDevice, typeof(VertexPositionTexture), vertCount, BufferUsage.WriteOnly);
            modelVertexBuffer.SetData(verts.ToArray());
            indexBuffer = new IndexBuffer(Game.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indx.Count, BufferUsage.WriteOnly);
            indexBuffer.SetData(indx.ToArray());
        }

        public override void Draw(GameTime gameTime)
        {
            Draw(gameTime, effect);
        }
        float timer;
        public bool UpdateInstances = false;
        public virtual void Draw(GameTime gameTime, Effect thisEffect)
        {
            if (!Visible || !Enabled)
                return;

            World = Matrix.CreateScale(Scale) * Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(Position);
            timer = (float)gameTime.TotalGameTime.TotalSeconds;

            if (instanceTransformMatrices.Count == 0)
                return;

            // If we have more instances than room in our vertex buffer, grow it to the neccessary size.
            // only grow if we need more room?
            if ((instanceVertexBuffer == null) || (instanceTransformMatrices.Count > instanceVertexBuffer.VertexCount))
            {
                if (instanceVertexBuffer != null)
                    instanceVertexBuffer.Dispose();

                instanceVertexBuffer = new DynamicVertexBuffer(GraphicsDevice, instanceVertexDeclaration, instanceTransformMatrices.Count, BufferUsage.WriteOnly);

                // Transfer the latest instance transform matrices into the instanceVertexBuffer.
            }

            if (UpdateInstances)
            {
                tempMatrixList.Clear();
                tempMatrixList.EnsureCapacity(instanceTransformMatrices.Values.Count);
                instanceTransformMatrices.Values.CopyTo(tempMatrixList.GetRawArray(), 0);
                instanceVertexBuffer.SetData(tempMatrixList.GetRawArray(), 0, instanceTransformMatrices.Count, SetDataOptions.Discard);
            }



            if (thisEffect.CurrentTechnique.Name == "ShadowMapH") // Shadow Map
            {
                thisEffect.CurrentTechnique = thisEffect.Techniques["ShadowMapHP"];

                if (FixedCameraPos == Vector3.One * 10000)
                    thisEffect.Parameters["EyePosition"].SetValue(Camera.Position);
                else
                    thisEffect.Parameters["EyePosition"].SetValue(FixedCameraPos);

                thisEffect.Parameters["World"].SetValue(World);
            }
            else
            {
                thisEffect.Parameters["color"].SetValue(Color.ToVector4());
                
                thisEffect.Parameters["time"].SetValue(timer);

                // Texture
                if (TextureMaterials.Count > 0)
                    thisEffect.Parameters["textureMat"].SetValue(AssetManager.GetAsset<Texture2D>(TextureMaterials[0]));
                else
                    thisEffect.Parameters["textureMat"].SetValue(blank);

                if (thisEffect.Parameters["BumpMap"] != null)
                {
                    if (NormalMaterials.Count > 0)
                        thisEffect.Parameters["BumpMap"].SetValue(AssetManager.GetAsset<Texture2D>(NormalMaterials[0]));
                    else
                        thisEffect.Parameters["BumpMap"].SetValue(blank_normal);
                }

                if (FixedCameraPos == Vector3.One * 10000)
                    thisEffect.Parameters["EyePosition"].SetValue(Camera.Position);
                else
                    thisEffect.Parameters["EyePosition"].SetValue(FixedCameraPos);

                thisEffect.Parameters["vp"].SetValue(Camera.View * Camera.Projection);
                thisEffect.Parameters["World"].SetValue(World);

                if (!DepthContribution)
                    thisEffect.Parameters["depthMap"].SetValue(((BaseDeferredRenderGame)Game).DepthMap);

            }

            VertexBufferBindingArray[0] = new VertexBufferBinding(modelVertexBuffer, 0, 0);
            VertexBufferBindingArray[1] = new VertexBufferBinding(instanceVertexBuffer, 0, 1);

            GraphicsDevice.SetVertexBuffers(VertexBufferBindingArray);

            Game.GraphicsDevice.BlendState = thisBlendState;
            Game.GraphicsDevice.DepthStencilState = thisDepthStencilState;
            
            thisEffect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.Indices = indexBuffer;

            Game.GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                                   modelVertexBuffer.VertexCount, 0,
                                                   2,
                                                   instanceTransformMatrices.Count);

        }

        VertexBufferBinding[] VertexBufferBindingArray = new VertexBufferBinding[2];

        public bool DepthContribution = false;
        public BlendState thisBlendState = BlendState.Additive;
        public DepthStencilState thisDepthStencilState = DepthStencilState.None;
        /// <summary>
        /// Method to translate object
        /// </summary>
        /// <param name="distance"></param>
        public void TranslateOO(Vector3 distance)
        {
            Position += Vector3.Transform(distance, Rotation);
        }
        public void TranslateAA(Vector3 distance)
        {
            Position += Vector3.Transform(distance, Quaternion.Identity);
        }
        /// <summary>
        /// Method to rotate object
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="angle"></param>
        public void Rotate(Vector3 axis, float angle)
        {
            axis = Vector3.Transform(axis, Rotation);
            Rotation = Quaternion.Normalize(Quaternion.CreateFromAxisAngle(axis, angle) * Rotation);
        }
    }
}
