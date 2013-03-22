using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IlluminatiEngine.Interfaces;
using System.ComponentModel;

namespace IlluminatiEngine
{
    public class BaseDeferredObject : DrawableGameComponent, I3DBaseObject, IDeferredRender, IHasMesh, IImplementsCustomShader, IGridRegisterable, IDebuggable, ICanCastShadow
    {
        public List<string> TextureMaterials { get; set; }
        public List<string> NormalMaterials { get; set; }
        public List<string> SpeculaMaterials { get; set; }
        public List<string> GlowMaterials { get; set; }
        public List<string> ReflectionMaterials { get; set; }
        protected Texture2D blank;
        protected Texture2D blank_normal;


        [BrowsableAttribute(false)]
        public IAssetManager AssetManager
        {
            get { return ((IAssetManager)Game.Services.GetService(typeof(IAssetManager))); }
        }

        [BrowsableAttribute(false)]
        public ICameraService Camera
        {
            get { return ((ICameraService)Game.Services.GetService(typeof(ICameraService))); }
        }

        protected Quaternion rotation = Quaternion.Identity;
        public virtual Quaternion Orientation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
            }
        }

        protected Vector3 position;
        public virtual Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        protected Vector3 scale = Vector3.One;
        public Vector3 Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
            }
        }

        protected Matrix world;
        public virtual Matrix World
        {
            get
            {
                return world;
            }
            set
            {
                world = value;
            }
        }

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
                return aaBounds;
            }
            set
            {
                aaBounds = value;
            }
        }
        
        protected string effect;
        public string Effect
        {
            get { return effect; }
            set { effect = value; }
        }

        protected Matrix[] transforms;
        protected Matrix meshWorld;
        protected Matrix meshWVP;
        public Color color = Color.White;

        protected Dictionary<string, object> meshData;
        protected List<BoundingBox> myBounds = new List<BoundingBox>();
        public List<BoundingBox> Bounds
        {
            get
            {
                if (myBounds.Count == 0 && meshData != null)
                {
                    myBounds = ((List<BoundingBox>)meshData["BoundingBoxs"]);
                }
                return myBounds;
            }
        }

        public Color boxCol = Color.Red;
        protected VertexPositionColor[] points;
        protected short[] index;
        protected Effect lineShader;
        public bool RenderBounds = false;

        public bool HasMoved
        { get; set; }


        public BaseDeferredObject(Game game)  : base(game)
        {
            effect = "shaders/deferred/DeferredModelRender";

            TextureMaterials = new List<string>();
            NormalMaterials = new List<string>();
            SpeculaMaterials = new List<string>();
            GlowMaterials = new List<string>();
            ReflectionMaterials = new List<string>();
        }

        //public BaseDeferredObject(Game game, string modelAssetName, string textureAssetName, string bumpAssetName, string specularAssetName, string glowAssetName,string reflectionAssetName) : this(game)
        //{
        //    mesh = modelAssetName;
        //    colorMap = textureAssetName;
        //    normalMap = bumpAssetName;
        //    specularMap = specularAssetName;
        //    glowMap = glowAssetName;
        //    relflectionMap = reflectionAssetName; 
        //}

        public BaseDeferredObject(Game game, string modelAssetName) : this(game)
        {
            mesh = modelAssetName;
        }

        public virtual void ChangeEffect(string effectAsset)
        {
            effect = effectAsset;
        }

        public virtual void TranslateAA(Vector3 distance)
        {
            HasMoved = Moved(distance);
            position += GameComponentHelper.Translate3D(distance);
        }

        public virtual void TranslateOO(Vector3 distance)
        {
            HasMoved = Moved(distance);
            position += GameComponentHelper.Translate3D(distance, rotation);
        }

        protected virtual bool Moved(Vector3 distance)
        {
            return distance != Vector3.Zero;
        }

        public virtual void Rotate(Vector3 axis, float angle)
        {
            GameComponentHelper.Rotate(axis, angle, ref rotation);
        }

        public virtual void RotateAA(Vector3 axis, float angle)
        {
            GameComponentHelper.RotateAA(axis, angle, ref rotation);
        }

        public override void Update(GameTime gameTime)
        {
            if (!Enabled)
                return;

            world = Matrix.CreateScale(scale) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(position);
            base.Update(gameTime);

            if (blank == null)
            {
                blank = AssetManager.GetAsset<Texture2D>(BaseAssets.BlankTexture);
            }

            if (blank_normal == null)
            {
                //blank_normal = AssetManager.GetAsset<Texture2D>(BaseAssets.NormalTexture);
                blank_normal = AssetManager.GetAsset<Texture2D>(BaseAssets.BlankTexture);
            }
        }

        public virtual void Draw(GameTime gameTime, Effect effect)
        {
            if (Visible == false)
            {
                int ibreak = 0;
                // Need to make sure this won't break anything else
                return;
            }

            if (thisMesh == null)
            {
                thisMesh = AssetManager.GetAsset<Model>(mesh);
                transforms = new Matrix[thisMesh.Bones.Count];
                thisMesh.CopyAbsoluteBoneTransformsTo(transforms);

                meshData = (Dictionary<string, object>)thisMesh.Tag;

                Dictionary<int, List<Vector3>> verts = (Dictionary<int, List<Vector3>>)meshData["Vertices"];

                foreach (int m in verts.Keys)
                    _vertices.AddRange(verts[m]);
            }

            int cnt = thisMesh.Meshes.Count;
            for(int m = 0;m<cnt;m++)
            {
                meshWorld = transforms[thisMesh.Meshes[m].ParentBone.Index] * World;
                meshWVP = meshWorld * Camera.View * Camera.Projection;

                int pcnt = 0;
                int cnt2 = thisMesh.Meshes[m].MeshParts.Count;
                for(int mp =0;mp<cnt2;mp++)
                {
                    effect.Parameters["world"].SetValue(meshWorld);

                    if (effect.CurrentTechnique.Name == "ShadowMap") // Shadow Map
                    { }
                    else
                    {
                        effect.CurrentTechnique = effect.Techniques["Deferred"];
                        effect.Parameters["wvp"].SetValue(meshWVP);

                        effect.Parameters["color"].SetValue(color.ToVector3());

                        effect.Parameters["clipPlane"].SetValue(new Vector4(GameComponentHelper.WaterReflectionPane.Normal, GameComponentHelper.WaterReflectionPane.D));

                        // Texture
                        if (TextureMaterials.Count > 0 && pcnt < TextureMaterials.Count)
                            effect.Parameters["textureMat"].SetValue(AssetManager.GetAsset<Texture2D>(TextureMaterials[pcnt]));
                        else
                        {
                            if (thisMesh.Meshes[m].MeshParts[mp].Effect is SkinnedEffect)
                            {
                                if (((SkinnedEffect)thisMesh.Meshes[m].MeshParts[mp].Effect).Texture != null)
                                    effect.Parameters["textureMat"].SetValue(((SkinnedEffect)thisMesh.Meshes[m].MeshParts[mp].Effect).Texture);
                                else
                                    effect.Parameters["textureMat"].SetValue(blank);
                            }
                            else
                            {
                                if (thisMesh.Meshes[m].MeshParts[mp].Effect is BasicEffect)
                                {
                                    if (((BasicEffect)thisMesh.Meshes[m].MeshParts[mp].Effect).Texture != null)
                                        effect.Parameters["textureMat"].SetValue(((BasicEffect)thisMesh.Meshes[m].MeshParts[mp].Effect).Texture);
                                    else
                                        effect.Parameters["textureMat"].SetValue(blank);
                                }
                            }
                        }

                        // Normals
                        if (NormalMaterials.Count > 0 && pcnt < NormalMaterials.Count)
                            effect.Parameters["BumpMap"].SetValue(AssetManager.GetAsset<Texture2D>(NormalMaterials[pcnt]));
                        else
                            effect.Parameters["BumpMap"].SetValue(blank_normal);

                        // Specula
                        if (SpeculaMaterials.Count > 0 && pcnt < SpeculaMaterials.Count)
                            effect.Parameters["specularMap"].SetValue(AssetManager.GetAsset<Texture2D>(SpeculaMaterials[pcnt]));
                        else
                            effect.Parameters["specularMap"].SetValue(blank);

                        // Glow
                        if (GlowMaterials.Count > 0 && pcnt < GlowMaterials.Count)
                            effect.Parameters["glowMap"].SetValue(AssetManager.GetAsset<Texture2D>(GlowMaterials[pcnt]));
                        else
                            effect.Parameters["glowMap"].SetValue(blank);

                        // Reflection
                        if (ReflectionMaterials.Count > 0 && pcnt < ReflectionMaterials.Count)
                            effect.Parameters["reflectionMap"].SetValue(AssetManager.GetAsset<Texture2D>(ReflectionMaterials[pcnt]));
                        else
                            effect.Parameters["reflectionMap"].SetValue(blank);

                        pcnt++;
                    }

                    int pCnt = effect.CurrentTechnique.Passes.Count;
                    for (int p = 0; p < pCnt; p++)
                    {
                        effect.CurrentTechnique.Passes[p].Apply();

                        try
                        {
                            Game.GraphicsDevice.SetVertexBuffer(thisMesh.Meshes[m].MeshParts[mp].VertexBuffer);
                            Game.GraphicsDevice.Indices = thisMesh.Meshes[m].MeshParts[mp].IndexBuffer;
                            Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, thisMesh.Meshes[m].MeshParts[mp].VertexOffset, 0, thisMesh.Meshes[m].MeshParts[mp].NumVertices, thisMesh.Meshes[m].MeshParts[mp].StartIndex, thisMesh.Meshes[m].MeshParts[mp].PrimitiveCount);
                        }
                        catch (Exception e)
                        {

                        }
                    }
                }
            }
            //if (effect.CurrentTechnique.Name == "ShadowMap") // Shadow Map
            //{ }
            //else
            //{
            //    effect.CurrentTechnique = effect.Techniques["Deferred"];


            //    if (effect.Parameters["vp"] != null)
            //        effect.Parameters["vp"].SetValue(Camera.View * Camera.Projection);
            //    if (effect.Parameters["timer"] != null)
            //        effect.Parameters["timer"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);

            //    if (!string.IsNullOrEmpty(colorMap))
            //        effect.Parameters["textureMat"].SetValue(AssetManager.GetAsset<Texture2D>(colorMap));
            //    else
            //        effect.Parameters["textureMat"].SetValue(AssetManager.GetAsset<Texture2D>(BaseAssets.BlankTexture));

            //    if (!string.IsNullOrEmpty(normalMap))
            //        effect.Parameters["BumpMap"].SetValue(AssetManager.GetAsset<Texture2D>(normalMap));
            //    else
            //        effect.Parameters["BumpMap"].SetValue(AssetManager.GetAsset<Texture2D>(BaseAssets.BlankTexture));

            //    if (!string.IsNullOrEmpty(specularMap))
            //        effect.Parameters["specularMap"].SetValue(AssetManager.GetAsset<Texture2D>(specularMap));
            //    else
            //        effect.Parameters["specularMap"].SetValue(AssetManager.GetAsset<Texture2D>(BaseAssets.BlankTexture));

            //    if (!string.IsNullOrEmpty(glowMap))
            //        effect.Parameters["glowMap"].SetValue(AssetManager.GetAsset<Texture2D>(glowMap));
            //    else
            //        effect.Parameters["glowMap"].SetValue(AssetManager.GetAsset<Texture2D>(BaseAssets.BlankTexture));
            //}

            

            //foreach (ModelMesh meshM in thisMesh.Meshes)
            //{
            //    // Do the world stuff. 
            //    // Scale * transform * pos * rotation
            //    meshWorld = transforms[meshM.ParentBone.Index] * World;
            //    meshWVP = meshWorld * Camera.View * Camera.Projection;

            //    if (effect.Parameters["world"] != null)
            //        effect.Parameters["world"].SetValue(meshWorld);
            //    if (effect.Parameters["wvp"] != null)
            //        effect.Parameters["wvp"].SetValue(meshWVP);

            //    effect.CurrentTechnique.Passes[0].Apply();

            //    foreach (ModelMeshPart meshPart in meshM.MeshParts)
            //    {
            //        Game.GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
            //        Game.GraphicsDevice.Indices = meshPart.IndexBuffer;
            //        Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshPart.VertexOffset, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
            //    }
            //}
        }
        public override void Draw(GameTime gameTime)
        {
            Draw(gameTime, AssetManager.GetAsset<Effect>(Effect));

            if (RenderBounds)
            {
                for (int b = 0; b < Bounds.Count; b++)
                    DrawBox(Bounds[b]);
            }
        }
        protected void DrawBox(BoundingBox box)
        {
            BuildBox(box, boxCol);
            DrawLines(12);
        }
        protected void BuildBox(BoundingBox box, Color lineColor)
        {
            points = new VertexPositionColor[8];

            Vector3[] corners = box.GetCorners();

            points[0] = new VertexPositionColor(corners[1], lineColor); // Front Top Right
            points[1] = new VertexPositionColor(corners[0], lineColor); // Front Top Left
            points[2] = new VertexPositionColor(corners[2], lineColor); // Front Bottom Right
            points[3] = new VertexPositionColor(corners[3], lineColor); // Front Bottom Left
            points[4] = new VertexPositionColor(corners[5], lineColor); // Back Top Right
            points[5] = new VertexPositionColor(corners[4], lineColor); // Back Top Left
            points[6] = new VertexPositionColor(corners[6], lineColor); // Back Bottom Right
            points[7] = new VertexPositionColor(corners[7], lineColor); // Bakc Bottom Left

            index = new short[] {
	            0, 1, 0, 2, 1, 3, 2, 3,
	            4, 5, 4, 6, 5, 7, 6, 7,
	            0, 4, 1, 5, 2, 6, 3, 7
                };
        }
        protected void DrawLines(int primativeCount)
        {
            if (lineShader == null)
                lineShader = AssetManager.GetAsset<Effect>("Shaders/Deferred/DeferredBasicEffect");


            lineShader.Parameters["world"].SetValue(Matrix.CreateScale(Scale) * Matrix.CreateTranslation(position));
            lineShader.Parameters["wvp"].SetValue((Matrix.CreateScale(Scale) * Matrix.CreateTranslation(position)) * Camera.View * Camera.Projection);
            
            lineShader.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.LineList, points, 0, points.Length, index, 0, primativeCount);
        }

        protected String m_debugId;
        public virtual String DebugId
        {
            get { return m_debugId; }
            set { m_debugId = value; }
        }

        [BrowsableAttribute(false)]
        public new GraphicsDevice GraphicsDevice
        {
            get { return base.GraphicsDevice; }
        }


        public virtual void DumpDebugInfo(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine("BaseDeferredObject ["+this.GetType()+"][" + m_debugId + "]");
            stringBuilder.AppendFormat("Enabled[{0}] Visible[{1}] Mesh[{2}]\n", Enabled, Visible, Mesh);
            GameComponentHelper.PrintMatrix(stringBuilder, "World", World);
        }


        #region ICanCastShadow Members

        List<Vector3> _vertices = new List<Vector3>();
        
        public List<Vector3> vertices
        {
            get
            {
                return _vertices;
            }
        }


        #endregion
    }
}
