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

        public Vector3 pMods = -Vector3.One * 10000;
        public Vector3 AdditionalCPUData;

        public Base3DParticleInstance(Game game, Vector3 position, Vector3 scale, ref Base3DParticleInstancer instancer,Quaternion orientation) : this(game, position, scale, ref instancer)
        {
            Orientation = orientation;
            this.Update(null);
        }
        public Base3DParticleInstance(Game game, Vector3 position, Vector3 scale, Vector3 mods, ref Base3DParticleInstancer instancer)
            : this(game,position, scale, ref instancer)
        {
            pMods = mods;
            this.Update(null);
        }
        public Base3DParticleInstance(Game game, Vector3 position, Vector3 scale, ref Base3DParticleInstancer instancer)
            : this(game)
        {

            rnd = new Random(DateTime.Now.Millisecond);
            Position = position;
            Scale = scale;

            Instancer = instancer;
            Instancer.instanceTransformMatrices.Add(this, World);
            Instancer.Instances.Add(myID, this);

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

                    if (pMods == -Vector3.One * 10000)
                    {
                        World.M12 = (float)rnd.NextDouble();
                        World.M23 = (float)rnd.NextDouble();
                        World.M34 = (float)rnd.NextDouble();
                    }
                    else
                    {
                        World.M12 = pMods.X;
                        World.M23 = pMods.Y;
                        World.M34 = pMods.Z;
                    }
                }
                Instancer.instanceTransformMatrices[this] = World;
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
                        Instancer.instanceTransformMatrices.Add(this, World);
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