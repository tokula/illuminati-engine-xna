using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing.Design;
using System.Drawing;
using System.ComponentModel;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

using IlluminatiContentClasses;

// TODO: replace these with the processor input and output types.
using TInput = System.String;
using TOutput = System.String;

namespace IlluminatiContentPipelineExtension
{
    /// <summary>
    /// Model Data Slots
    /// </summary>
    public enum ModelDataSlot
    {
        /// <summary>
        /// Vertex data
        /// </summary>
        Vertices,
        /// <summary>
        /// Index data
        /// </summary>
        Inicies,
        /// <summary>
        /// texcoord data
        /// </summary>
        TexCoords,
        /// <summary>
        /// Tangents
        /// </summary>
        Tangents,
        /// <summary>
        /// BiNormals
        /// </summary>
        BiNormals,
        /// <summary>
        /// Normal data
        /// </summary>
        Normals,
        /// <summary>
        /// Bounding Box Data
        /// </summary>
        BoundingBoxs,
        /// <summary>
        /// Bounding Sphere data.
        /// </summary>
        BoundingSpheres,
        /// <summary>
        /// Skinned Animation Data
        /// </summary>
        SkinningData,
        BlendIndex,
        BlendWeight
    }
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    ///
    /// TODO: change the ContentProcessor attribute to specify the correct
    /// display name for this processor.
    /// </summary>
    [ContentProcessor(DisplayName = "Illuminati Instance Model Processor")]
    public class IlluminatiModelProcessor : ModelProcessor
    {
        Dictionary<int, List<Vector3>> vertices = new Dictionary<int, List<Vector3>>();
        Dictionary<int, List<Vector2>> texCoords = new Dictionary<int, List<Vector2>>();
        Dictionary<int, List<Vector3>> normals = new Dictionary<int, List<Vector3>>();
        Dictionary<int, List<Vector3>> tangents = new Dictionary<int, List<Vector3>>();
        Dictionary<int, List<Vector3>> biNormals = new Dictionary<int, List<Vector3>>();
        Dictionary<int, List<Vector4>> blendIndex = new Dictionary<int, List<Vector4>>();
        Dictionary<int, List<Vector4>> blendWeight = new Dictionary<int, List<Vector4>>();
        Dictionary<int, List<int>> indicies = new Dictionary<int, List<int>>();
        List<BoundingBox> boxs = new List<BoundingBox>();
        List<BoundingSphere> spheres = new List<BoundingSphere>();

        List<Matrix> bindPose = new List<Matrix>();
        List<Matrix> inverseBindPose = new List<Matrix>();
        List<int> skeletonHierarchy = new List<int>();
        Dictionary<string, AnimationClip> animationClips;

        bool enableLogging = false;
        bool skinnedMesh = false;

        ContentProcessorContext context;

        /// <summary>
        /// Enable Logging
        /// </summary>
        [DefaultValue(false)]
        [Description("Set to true if you want logging on"), TypeConverter(typeof(BooleanConverter)), Editor(typeof(UITypeEditor), typeof(UITypeEditor))]
        [Category("Illuminati")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DisplayName("Enable Logging")]
        public bool EnableLogging
        {
            get { return enableLogging; }
            set
            {
                enableLogging = value;
            }
        }

        /// <summary>
        /// Skinned Mesh
        /// </summary>
        [DefaultValue(false)]
        [Description("Set to true if Mesh is skinned"), TypeConverter(typeof(BooleanConverter)), Editor(typeof(UITypeEditor), typeof(UITypeEditor))]
        [Category("Illuminati")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DisplayName("Skinned Mesh")]
        public bool SkinnedMesh
        {
            get { return skinnedMesh; }
            set
            {
                skinnedMesh = value;
            }
        }

        public IlluminatiModelProcessor() : base()
        {
            base.GenerateTangentFrames = true;
        }

        [DefaultValue(true)]
        public override bool GenerateTangentFrames
        {
            get
            {
                return base.GenerateTangentFrames;
            }
            set
            {
                base.GenerateTangentFrames = value;
            }
        }

        /// <summary>
        /// Process method.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {

            if (skinnedMesh)
            {
                ValidateMesh(input, context, null);

                // Find the skeleton.
                BoneContent skeleton = MeshHelper.FindSkeleton(input);

                if (skeleton == null)
                    throw new InvalidContentException("Input skeleton not found.");

                // We don't want to have to worry about different parts of the model being
                // in different local coordinate systems, so let's just bake everything.
                FlattenTransforms(input, skeleton);

                // Read the bind pose and skeleton hierarchy data.
                IList<BoneContent> bones = MeshHelper.FlattenSkeleton(skeleton);

                if (bones.Count > SkinnedEffect.MaxBones)
                {
                    throw new InvalidContentException(string.Format(
                        "Skeleton has {0} bones, but the maximum supported is {1}.",
                        bones.Count, SkinnedEffect.MaxBones));
                }

                foreach (BoneContent bone in bones)
                {
                    bindPose.Add(bone.Transform);
                    inverseBindPose.Add(Matrix.Invert(bone.AbsoluteTransform));
                    skeletonHierarchy.Add(bones.IndexOf(bone.Parent as BoneContent));
                }

                // Convert animation data to our runtime format.
                animationClips = ProcessAnimations(skeleton.Animations, bones);
            }

            string modelName = input.Identity.SourceFilename.Substring(input.Identity.SourceFilename.LastIndexOf("\\") + 1);
            if (EnableLogging)
                LogWriter.WriteToLog(string.Format("Process started for {0}", modelName));

            this.context = context;

            Dictionary<string, object> ModelData = new Dictionary<string, object>();

            ModelContent baseModel = base.Process(input, context);

            GenerateData(input);

            ModelData.Add(ModelDataSlot.Vertices.ToString(), vertices);
            ModelData.Add(ModelDataSlot.Inicies.ToString(), indicies);
            ModelData.Add(ModelDataSlot.TexCoords.ToString(), texCoords);
            ModelData.Add(ModelDataSlot.Tangents.ToString(), tangents);
            ModelData.Add(ModelDataSlot.BiNormals.ToString(), biNormals);
            ModelData.Add(ModelDataSlot.Normals.ToString(), normals);
            ModelData.Add(ModelDataSlot.BoundingBoxs.ToString(), boxs);
            ModelData.Add(ModelDataSlot.BoundingSpheres.ToString(), spheres);

            ModelData.Add(ModelDataSlot.BlendIndex.ToString(), blendIndex);
            ModelData.Add(ModelDataSlot.BlendWeight.ToString(), blendWeight);

            ModelData.Add(ModelDataSlot.SkinningData.ToString(), new SkinningData(animationClips, bindPose,
                                         inverseBindPose, skeletonHierarchy));

            baseModel.Tag = ModelData;

            if (EnableLogging)
                LogWriter.WriteToLog(string.Format("Process completed for {0}", modelName));

            return baseModel;
        }

        private void GenerateData(NodeContent node)
        {
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                MeshHelper.OptimizeForCache(mesh);

                // Look up the absolute transform of the mesh.
                Matrix absoluteTransform = mesh.AbsoluteTransform;

                int i = 0;

                // Loop over all the pieces of geometry in the mesh.
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                    Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

                    #region Logging
                    if (EnableLogging)
                    {
                        LogWriter.WriteToLog(string.Format("{0} : Indicies = {1}", geometry.Name, geometry.Vertices.PositionIndices.Count));
                        LogWriter.WriteToLog(string.Format("{0} : Verts = {1}", geometry.Name, geometry.Vertices.VertexCount));
                        LogWriter.WriteToLog(string.Format("{0} : Positions = {1}", geometry.Name, geometry.Vertices.Positions.Count));
                        LogWriter.WriteToLog(string.Format("{0} : PositionsInd = {1}", geometry.Name, geometry.Vertices.PositionIndices.Count));
                        LogWriter.WriteToLog(string.Format("{0} : Channels = {1}", geometry.Name, geometry.Vertices.Channels.Count));
                        int cnt = 0;
                        foreach (VertexChannel c in geometry.Vertices.Channels)
                        {
                            LogWriter.WriteToLog(string.Format("{0} : Channel[{1}] = {2}", mesh.Name, cnt++, c.Name));
                        }
                    }
                    #endregion
                    // Loop over all the indices in this piece of geometry.
                    // Every group of three indices represents one triangle.
                    List<Vector3> thisVerts = new List<Vector3>();
                    List<int> ind = new List<int>();
                    List<Vector2> tex = new List<Vector2>();
                    List<Vector3> norm = new List<Vector3>();
                    List<Vector3> tang = new List<Vector3>();
                    List<Vector3> biN = new List<Vector3>();
                    List<Vector4> blIn = new List<Vector4>();
                    List<Vector4> blWe = new List<Vector4>();

                    Vector2 tmpTex = Vector2.Zero;
                    Vector3 tmpNorm = Vector3.Zero;
                    Vector3 vertex = Vector3.Zero;
                    Vector3 tangent = Vector3.Zero;
                    Vector3 biNormal = Vector3.Zero;
                    Vector4 bI = new Vector4();
                    Vector4 bw = Vector4.Zero;

                    foreach (int index in geometry.Indices)
                    {
                        // Look up the position of this vertex.
                        vertex = Vector3.Transform(geometry.Vertices.Positions[index], absoluteTransform);

                        if (geometry.Vertices.Channels.Contains("TextureCoordinate0"))
                            tmpTex = (Vector2)geometry.Vertices.Channels["TextureCoordinate0"][index];

                        if (geometry.Vertices.Channels.Contains("Normal0"))
                            tmpNorm = Vector3.Transform((Vector3)geometry.Vertices.Channels["Normal0"][index], absoluteTransform);

                        if (geometry.Vertices.Channels.Contains("Tangent0"))
                            tangent = Vector3.Transform((Vector3)geometry.Vertices.Channels["Tangent0"][index], absoluteTransform);
                        
                        if (geometry.Vertices.Channels.Contains("BlendIndices0"))
                            bI = Vector4.Transform(((Byte4)geometry.Vertices.Channels["BlendIndices0"][index]).ToVector4(), absoluteTransform);                            

                        if (geometry.Vertices.Channels.Contains("BlendWeight0"))
                            bw = Vector4.Transform((Vector4)geometry.Vertices.Channels["BlendWeight0"][index], absoluteTransform);                            

                        if (geometry.Vertices.Channels.Contains("Binormal0"))
                            biNormal = Vector3.Transform((Vector3)geometry.Vertices.Channels["Binormal0"][index], absoluteTransform);

                        // Store this data.
                        min = Vector3.Min(min, vertex);
                        max = Vector3.Max(max, vertex);

                        norm.Add(tmpNorm);
                        tex.Add(tmpTex);
                        thisVerts.Add(vertex);
                        tang.Add(tangent);
                        biN.Add(biNormal);
                        blWe.Add(bw);
                        blIn.Add(bI);

                        ind.Add(i++);
                    }

                    boxs.Add(new BoundingBox(min, max));
                    spheres.Add(BoundingSphere.CreateFromBoundingBox(boxs[boxs.Count - 1]));
                    //spheres.Add(new BoundingSphere(min + max, Vector3.Distance(min,max)/(MathHelper.Pi*1.1f)));

                    texCoords.Add(texCoords.Count, tex);
                    normals.Add(normals.Count, norm);
                    indicies.Add(indicies.Count, ind);
                    vertices.Add(vertices.Count, thisVerts);
                    tangents.Add(tangents.Count, tang);
                    biNormals.Add(biNormals.Count, biN);

                    blendWeight.Add(blendWeight.Count, blWe);
                    blendIndex.Add(blendIndex.Count, blIn);
                }
            }

            // Recursively scan over the children of this node.
            foreach (NodeContent child in node.Children)
            {
                GenerateData(child);
            }
        }

        /// <summary>
        /// Makes sure this mesh contains the kind of data we know how to animate.
        /// </summary>
        static void ValidateMesh(NodeContent node, ContentProcessorContext context,
                                 string parentBoneName)
        {
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                // Validate the mesh.
                if (parentBoneName != null)
                {
                    context.Logger.LogWarning(null, null,
                        "Mesh {0} is a child of bone {1}. SkinnedModelProcessor " +
                        "does not correctly handle meshes that are children of bones.",
                        mesh.Name, parentBoneName);
                }

                if (!MeshHasSkinning(mesh))
                {
                    context.Logger.LogWarning(null, null,
                        "Mesh {0} has no skinning information, so it has been deleted.",
                        mesh.Name);

                    mesh.Parent.Children.Remove(mesh);
                    return;
                }
            }
            else if (node is BoneContent)
            {
                // If this is a bone, remember that we are now looking inside it.
                parentBoneName = node.Name;
            }

            // Recurse (iterating over a copy of the child collection,
            // because validating children may delete some of them).
            foreach (NodeContent child in new List<NodeContent>(node.Children))
                ValidateMesh(child, context, parentBoneName);
        }

        /// <summary>
        /// Checks whether a mesh contains skininng information.
        /// </summary>
        static bool MeshHasSkinning(MeshContent mesh)
        {
            foreach (GeometryContent geometry in mesh.Geometry)
            {
                if (!geometry.Vertices.Channels.Contains(VertexChannelNames.Weights()))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Bakes unwanted transforms into the model geometry,
        /// so everything ends up in the same coordinate system.
        /// </summary>
        static void FlattenTransforms(NodeContent node, BoneContent skeleton)
        {
            foreach (NodeContent child in node.Children)
            {
                // Don't process the skeleton, because that is special.
                if (child == skeleton)
                    continue;

                // Bake the local transform into the actual geometry.
                MeshHelper.TransformScene(child, child.Transform);

                // Having baked it, we can now set the local
                // coordinate system back to identity.
                child.Transform = Matrix.Identity;

                // Recurse.
                FlattenTransforms(child, skeleton);
            }
        }
        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContentDictionary
        /// object to our runtime AnimationClip format.
        /// </summary>
        static Dictionary<string, AnimationClip> ProcessAnimations(
            AnimationContentDictionary animations, IList<BoneContent> bones)
        {
            // Build up a table mapping bone names to indices.
            Dictionary<string, int> boneMap = new Dictionary<string, int>();

            for (int i = 0; i < bones.Count; i++)
            {
                string boneName = bones[i].Name;

                if (!string.IsNullOrEmpty(boneName))
                    boneMap.Add(boneName, i);
            }

            // Convert each animation in turn.
            Dictionary<string, AnimationClip> animationClips;
            animationClips = new Dictionary<string, AnimationClip>();

            foreach (KeyValuePair<string, AnimationContent> animation in animations)
            {
                AnimationClip processed = ProcessAnimation(animation.Value, boneMap);

                animationClips.Add(animation.Key, processed);
            }

            if (animationClips.Count == 0)
            {
                throw new InvalidContentException(
                            "Input file does not contain any animations.");
            }

            return animationClips;
        }


        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContent
        /// object to our runtime AnimationClip format.
        /// </summary>
        static AnimationClip ProcessAnimation(AnimationContent animation,
                                              Dictionary<string, int> boneMap)
        {
            List<Keyframe> keyframes = new List<Keyframe>();

            // For each input animation channel.
            foreach (KeyValuePair<string, AnimationChannel> channel in
                animation.Channels)
            {
                // Look up what bone this channel is controlling.
                int boneIndex;

                if (!boneMap.TryGetValue(channel.Key, out boneIndex))
                {
                    throw new InvalidContentException(string.Format(
                        "Found animation for bone '{0}', " +
                        "which is not part of the skeleton.", channel.Key));
                }

                // Convert the keyframe data.
                foreach (AnimationKeyframe keyframe in channel.Value)
                {
                    keyframes.Add(new Keyframe(boneIndex, keyframe.Time,
                                               keyframe.Transform));
                }
            }

            // Sort the merged keyframes by time.
            keyframes.Sort(CompareKeyframeTimes);

            if (keyframes.Count == 0)
                throw new InvalidContentException("Animation has no keyframes.");

            if (animation.Duration <= TimeSpan.Zero)
                throw new InvalidContentException("Animation has a zero duration.");

            return new AnimationClip(animation.Duration, keyframes);
        }


        /// <summary>
        /// Comparison function for sorting keyframes into ascending time order.
        /// </summary>
        static int CompareKeyframeTimes(Keyframe a, Keyframe b)
        {
            return a.Time.CompareTo(b.Time);
        }

        /// <summary>
        /// Force all the materials to use our skinned model effect.
        /// </summary>
        [DefaultValue(MaterialProcessorDefaultEffect.SkinnedEffect)]
        public override MaterialProcessorDefaultEffect DefaultEffect
        {
            get 
            {
                if (skinnedMesh)
                    return MaterialProcessorDefaultEffect.SkinnedEffect;
                else
                    return MaterialProcessorDefaultEffect.BasicEffect;
            }
            set { }
        }
    }
}