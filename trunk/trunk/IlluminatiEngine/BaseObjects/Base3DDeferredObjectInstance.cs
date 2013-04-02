using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine
{

    public class Base3DDeferredObjectInstance : GameComponent, IGridRegisterable
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Scale = Vector3.One;
        public Quaternion Rotation = Quaternion.Identity;

        public Matrix World;
        public Matrix AAWorld;

        public Vector4 MyExtras { get; set; }

        public Base3DDeferredObjectlInstancer Instancer;

        public List<BoundingBox> Bounds
        {
            get 
            {
                List<BoundingBox> thisBounds = new List<BoundingBox>();

                int cnt = Instancer.AABoxBounds.Count;
                for(int b = 0;b<cnt;b++)
                    thisBounds.Add(new BoundingBox(Vector3.Transform(Instancer.AABoxBounds[b].Min, AAWorld), Vector3.Transform(Instancer.AABoxBounds[b].Max, AAWorld)));

                return thisBounds; 
            }
        }

        public bool HasMoved
        { get; set; }

        public Base3DDeferredObjectInstance(Game game)
            : base(game)
        { }

        public Base3DDeferredObjectInstance(Game game, Vector3 position, Vector3 scale, Quaternion rotation, ref Base3DDeferredObjectlInstancer instancer)
            : base(game)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;

            Instancer = instancer;
            Instancer.instanceTransformMatrices.Add(this, new InstanceVertex() { TransformMatrix = World, Extras = MyExtras });
            Instancer.Instances.Add(Instancer.Instances.Count, this);

            this.Update(null);
        }
        public Base3DDeferredObjectInstance(Game game, Vector3 position, Vector3 scale, Quaternion rotation, Vector4 extras, ref Base3DDeferredObjectlInstancer instancer) :
            this(game, position, scale, rotation, ref instancer)
        {
            MyExtras = extras;
        }
        public override void Update(GameTime gameTime)
        {
            World = Matrix.CreateScale(Scale) *
                      Matrix.CreateFromQuaternion(Rotation) *
                      Matrix.CreateTranslation(Position);

            AAWorld = Matrix.CreateScale(Scale) *                      
                      Matrix.CreateTranslation(Position);

            Instancer.instanceTransformMatrices[this] = new InstanceVertex() { TransformMatrix = World, Extras = MyExtras };        
        }
        public void TranslateAA(Vector3 distance)
        {
            HasMoved = Moved(distance);
            Position += GameComponentHelper.Translate3D(distance);
        }

        public void TranslateOO(Vector3 distance)
        {
            HasMoved = Moved(distance);
            Position += GameComponentHelper.Translate3D(distance, Rotation);
        }
        /// <summary>
        /// Method to rotate object
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="angle"></param>
        public void Rotate(Vector3 axis, float angle)
        {
            GameComponentHelper.Rotate(axis, angle, ref Rotation);
        }

        protected bool Moved(Vector3 distance)
        {
            return distance != Vector3.Zero;
        }
    }
}
