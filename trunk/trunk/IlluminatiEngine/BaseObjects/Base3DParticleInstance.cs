using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine
{
    public class Base3DParticleInstance : GameComponent
    {
        static int ID;
        private int id;
        public int myID
        {
            get { return id; }
        }
        public Vector3 Position = Vector3.Zero;
        public Vector3 Scale = Vector3.One * .5f;
        public Quaternion Orientation = Quaternion.Identity;

        protected bool IsActive = true;

        public Vector4 MyExtras = -Vector4.One * 10000;
        public Matrix World;
        public Matrix AAWorld;

        public Base3DParticleInstancer Instancer;

        public bool HasMoved
        { get; set; }

        public Base3DParticleInstance(Game game)
            : base(game)
        {
            ID++;
            id = ID;
        }

        Random rnd;

        public Vector3 AdditionalCPUData;

        public Base3DParticleInstance(Game game, Vector3 position, Vector3 scale, Base3DParticleInstancer instancer,Quaternion orientation) : this(game, position, scale, instancer)
        {
            Orientation = orientation;
            this.Update(null);
        }
        
        public Base3DParticleInstance(Game game, Vector3 position, Vector3 scale, Base3DParticleInstancer instancer)
            : this(game)
        {

            rnd = new Random(DateTime.Now.Millisecond);
            Position = position;
            Scale = scale;

            Instancer = instancer;
            Instancer.instanceTransformMatrices.Add(this, new InstanceVertex() { TransformMatrix = World, Extras = MyExtras });
            Instancer.Instances.Add(myID, this);

            this.Update(null);
        }
        public Base3DParticleInstance(Game game, Vector3 position, Vector3 scale, Vector4 extras, Base3DParticleInstancer instancer)
            : this(game, position, scale, instancer)
        {
            MyExtras = extras;

            this.Update(null);
        }
        public override void Update(GameTime gameTime)
        {
            if (Active)
            {
                World = Matrix.CreateScale(Scale) * Matrix.CreateFromQuaternion(Orientation) * Matrix.CreateTranslation(Position);

                if (!Instancer.PseudoVoxel)
                {
                    World.M13 = Scale.X * .5f;
                    World.M24 = Scale.Y * .5f;

                    if (MyExtras == -Vector4.One * 10000)
                    {
                        MyExtras = new Vector4((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble());                        
                    }                    
                }
                Instancer.instanceTransformMatrices[this] = new InstanceVertex() { TransformMatrix = World, Extras = MyExtras };
                Instancer.UpdateInstances = true;
            }

        }

        public bool Active
        {
            get
            {
                return IsActive;
            }
            set
            {
                if (IsActive != value)
                {
                    IsActive = value;
                    if (value)
                    {
                        Instancer.instanceTransformMatrices.Add(this, new InstanceVertex() { TransformMatrix = World, Extras = MyExtras });
                        Instancer.Instances.Add(myID, this);
                        Enabled = true;
                    }
                    else
                    {
                        Instancer.instanceTransformMatrices.Remove(this);
                        Instancer.Instances.Remove(myID);
                        Enabled = false;
                    }
                }
            }
        }


        public void TranslateAA(Vector3 distance)
        {
            HasMoved = Moved(distance);
            Position += Vector3.Transform(distance, Matrix.Identity);
        }

        protected bool Moved(Vector3 distance)
        {
            return distance != Vector3.Zero;
        }
    }
}