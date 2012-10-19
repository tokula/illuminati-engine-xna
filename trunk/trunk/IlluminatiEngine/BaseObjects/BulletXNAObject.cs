using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using IlluminatiEngine;

using BulletXNAPhysics = BulletXNA;
using BulletXNA.BulletDynamics;
using BulletXNA;
using BulletXNA.BulletCollision;
using BulletXNA.LinearMath;
using System.Diagnostics;
using System.Text;
using System.ComponentModel;

namespace IlluminatiEngine
{
    public class BulletXNAObject : BaseDeferredObject, IDisposable
    {
        [BrowsableAttribute(false)]
        public BulletXNAPhysicsComponent bulletPhysics
        {
            get { return (BulletXNAPhysicsComponent)Game.Services.GetService(typeof(BulletXNAPhysicsComponent)); }
        }

        public RigidBody RigidBody { get; set; }
        public GhostObject GhostObject { get; set; }
        public CollisionObject CollisionObject { get; set; }

        public BulletXNAObject(Game game) : base(game) { }
        public BulletXNAObject(Game game, string modelAssetName) : base(game, modelAssetName) { }

        public BulletXNAObject(Game game, float mass, DefaultMotionState motionState, CollisionShape collisionShape, Vector3 localInertia) : this(game, mass, motionState, collisionShape, localInertia, false) { }
        public BulletXNAObject(Game game, float mass, DefaultMotionState motionState, CollisionShape collisionShape, Vector3 localInertia,bool ghost)
            : base(game)
        {
            m_motionState = motionState;
            m_ghost = ghost;
            SetUpBulletPhysicsBody(mass, motionState, collisionShape, localInertia);
        }

        public BulletXNAObject(Game game, string modelAssetName, float mass, DefaultMotionState motionState, CollisionShape collisionShape, Vector3 localInertia) : this(game, modelAssetName, mass, motionState, collisionShape, localInertia, false) { }
        public BulletXNAObject(Game game, string modelAssetName, float mass, DefaultMotionState motionState, CollisionShape collisionShape, Vector3 localInertia, bool ghost)
            : base(game, modelAssetName)
        {
            m_motionState = motionState;
            IsGhost = ghost;
            SetUpBulletPhysicsBody(mass, motionState, collisionShape, localInertia);
        }

        public BulletXNAObject(Game game, string modelAssetName, float mass, DefaultMotionState motionState, CollisionShape collisionShape, Vector3 localInertia, CollisionFilterGroups collisionGroup, CollisionFilterGroups collisionMask) : this(game, modelAssetName, mass, motionState, collisionShape, localInertia, false, collisionGroup, collisionMask) { }
        public BulletXNAObject(Game game, string modelAssetName, float mass, DefaultMotionState motionState, CollisionShape collisionShape, Vector3 localInertia, bool ghost, CollisionFilterGroups collisionGroup, CollisionFilterGroups collisionMask)
            : base(game, modelAssetName)
        {
            m_motionState = motionState;
            CollisionGroups = collisionGroup;
            CollisionMask = collisionMask;
            IsGhost = ghost;
            SetUpBulletPhysicsBody(mass, motionState, collisionShape, localInertia);
        }


        public virtual void SetUpBulletPhysicsBody(float mass, IMotionState motionState, CollisionShape collisionShape, Vector3 localInertia)
        {
            Mass = mass;
            CollisionShape = collisionShape;
            Inertia = localInertia;
            SetUpBulletPhysicsBody();
        }

        public virtual void SetUpBulletPhysicsBody()
        {
            if (!IsGhost)
            {
                RigidBodyConstructionInfo rbInfo =
                                new RigidBodyConstructionInfo(Mass, MotionState, CollisionShape, Inertia);
                RigidBody = new RigidBody(rbInfo);
                CollisionObject = RigidBody;
            }
            else
            {
                GhostObject = new BulletXNA.BulletCollision.PairCachingGhostObject();
                GhostObject.SetCollisionShape(CollisionShape);
                CollisionObject = GhostObject;

                GhostObject.SetCollisionFlags(BulletXNA.BulletCollision.CollisionFlags.CF_NO_CONTACT_RESPONSE);		// We can choose to make it "solid" if we want...
                GhostObject.SetWorldTransform(BulletXNA.LinearMath.IndexedMatrix.CreateTranslation(Position));
            }

            bulletPhysics.AddCollisionObject(CollisionObject, CollisionGroups, CollisionMask);
            CollisionObject.SetUserPointer(this);
        }


        public override void Update(GameTime gameTime)
        {
            if (CollisionObject != null)
            {
                world = CollisionObject.GetWorldTransform();
                position = world.Translation;
                rotation = Quaternion.CreateFromRotationMatrix(world);
            }
            base.Update(gameTime);
        }

        public override void TranslateAA(Vector3 distance)
        {
            HasMoved = Moved(distance);
            CollisionObject.SetActivationState(ActivationState.ACTIVE_TAG);
            CollisionObject.Translate(distance);
        }

        public override void TranslateOO(Vector3 distance)
        {
            HasMoved = Moved(distance);
            CollisionObject.SetActivationState(ActivationState.ACTIVE_TAG);
            CollisionObject.Translate(GameComponentHelper.Translate3D(distance, rotation));
        }

        public virtual void Accelerate(Vector3 velocity)
        {
            Debug.Assert(RigidBody != null);
            RigidBody.SetLinearVelocity(RigidBody.GetLinearVelocity() + velocity);
 
        }


        public virtual CollisionFilterGroups CollisionGroups
        {
            get
            {
                return m_collisionGroups;
            }
            set
            {
                m_collisionGroups = value;
                if (CollisionObject != null && CollisionObject.GetBroadphaseHandle() != null)
                {
                    CollisionObject.GetBroadphaseHandle().m_collisionFilterGroup = m_collisionGroups;
                }
            }
        }


        public virtual CollisionFilterGroups CollisionMask
        {
            get
            {
                return m_collisionMask;
            }
            set
            {
                m_collisionMask = value;
                if (CollisionObject != null && CollisionObject.GetBroadphaseHandle() != null)
                {
                    CollisionObject.GetBroadphaseHandle().m_collisionFilterMask = m_collisionMask;
                }
            }
        }


        public virtual CollisionShape CollisionShape
        {
            get
            {
                return m_collisionShape;
            }
            set
            {
                m_collisionShape = value;
                if (CollisionObject != null)
                {
                    CollisionObject.SetCollisionShape(m_collisionShape);
                }
            }
        }


        public virtual float Mass
        {
            get
            {
                return m_mass;
            }
            set
            {
                m_mass = value;
                if(RigidBody != null)
                {
                    RigidBody.SetMassProps(Mass,Inertia);
                }
            }
        }


        public Vector3 Inertia
        {
            get
            {
                return m_inertia;
            }
            set
            {
                m_inertia = value;
                if(RigidBody != null)
                {
                    RigidBody.SetMassProps(Mass,Inertia);
                }
            }
        }


        public bool IsGhost
        {
            get { return m_ghost; }
            set { m_ghost = value; }
        }

        public IMotionState MotionState
        {
            get
            {
                return m_motionState;
            }
            set
            {
                m_motionState = value;
                if (RigidBody != null)
                {
                    RigidBody.SetMotionState(MotionState);
                }
            }
        }


        public override Vector3 Position
        {
            get
            {
                if (m_motionState != null)
                {
                    IndexedMatrix im;
                    m_motionState.GetWorldTransform(out im);
                    return im._origin;
                }
                else
                {
                    if (CollisionObject != null)
                    {
                        return CollisionObject.GetWorldTransform()._origin;
                    }
                    return Vector3.Zero;
                }
            }
            set
            {
                base.Position = value;
                if (m_motionState != null)
                {
                    IndexedMatrix im;
                    m_motionState.GetWorldTransform(out im);
                    im._origin = value;
                    m_motionState.SetWorldTransform(ref im);
                }
                else
                {
                    if (CollisionObject != null)
                    {
                        IndexedMatrix im = CollisionObject.GetWorldTransform();
                        im._origin = value;
                        CollisionObject.SetWorldTransform(ref im);
                    }
                }
            }
        }


        public override Quaternion Orientation
        {
            get
            {
                if (m_motionState != null)
                {
                    IndexedMatrix im;
                    m_motionState.GetWorldTransform(out im);
                    return Quaternion.CreateFromRotationMatrix(im);
                }
                else
                {
                    if (CollisionObject != null)
                    {
                        return Quaternion.CreateFromRotationMatrix(CollisionObject.GetWorldTransform());
                    }
                    return Quaternion.Identity;
                }
            }
            set
            {
                base.Orientation = value;
                if (m_motionState != null)
                {
                    IndexedBasisMatrix ibm = new IndexedBasisMatrix(value);
                    IndexedMatrix im;
                    m_motionState.GetWorldTransform(out im);
                    im._basis = ibm;
                    m_motionState.SetWorldTransform(im);
                }
                else
                {
                    if (CollisionObject != null)
                    {
                        IndexedBasisMatrix ibm = new IndexedBasisMatrix(value);
                        IndexedMatrix im;
                        CollisionObject.GetWorldTransform(out im);
                        im._basis = ibm;
                        CollisionObject.SetWorldTransform(im);
                    }
                }
            }
        }



       
        public Vector3 LinearVelocity
        {
            get 
            {
                if (CollisionObject != null)
                {
                    return CollisionObject.GetInterpolationLinearVelocity();

                }
                return Vector3.Zero;
            }
            set 
            {
                if (RigidBody != null)
                {
                    RigidBody.SetLinearVelocity(value);
                }
            }
        }

        public Vector3 AngularVelocity
        {
            get
            {
                if (CollisionObject != null)
                {
                    return CollisionObject.GetInterpolationAngularVelocity();
                }
                return Vector3.Zero;
            }
            set
            {
                if (RigidBody != null)
                {
                    RigidBody.SetAngularVelocity(value);
                }
            }
        }


        public void ApplyImpulse(Vector3 impulse)
        {
            Debug.Assert(RigidBody != null);
            RigidBody.SetActivationState(ActivationState.ACTIVE_TAG);
            IndexedVector3 iv3Impulse = impulse;
            RigidBody.ApplyCentralImpulse(ref iv3Impulse); 
        }

        public override void Rotate(Vector3 axis, float angle)
        {
            Quaternion rot = Orientation;
            GameComponentHelper.Rotate(axis, angle, ref rot);
            Orientation = rot;
        }


        public bool CCDEnabled
        {
            get
            {
                if (RigidBody != null)
                {
                    return RigidBody.GetCcdMotionThreshold() == 0.0f;
                }
                return false;
            }
            set
            {
                if (RigidBody != null)
                {

                    if (value)
                    {
                        RigidBody.SetCcdMotionThreshold(1f * Scale.X);
                        RigidBody.SetCcdSweptSphereRadius(0.2f * Scale.X);
                    }
                    else
                    {
                        RigidBody.SetCcdMotionThreshold(0f);
                        RigidBody.SetCcdSweptSphereRadius(0f);
                    }
                }
            }
        }


        public virtual void Dispose()
        {
            bulletPhysics.RemoveCollisionObject(this.CollisionObject);
        }


        public override void DumpDebugInfo(StringBuilder stringBuilder)
        {
            base.DumpDebugInfo(stringBuilder);
            stringBuilder.AppendFormat("BulletXNAObject type[{0}] actiState[{1}] mask[{2}] flags[{3}]",RigidBody!=null?"RB":"G",CollisionObject.GetActivationState(),CollisionMask,CollisionGroups);
            GameComponentHelper.PrintVector3(stringBuilder, "LinVel", LinearVelocity);
            if (CollisionObject.GetBroadphaseHandle() != null)
            {
                GameComponentHelper.PrintVector3(stringBuilder, "aaBMin", CollisionObject.GetBroadphaseHandle().m_aabbMin);
                GameComponentHelper.PrintVector3(stringBuilder, "aaBMax", CollisionObject.GetBroadphaseHandle().m_aabbMax);
            }
        }

        protected CollisionShape m_collisionShape;
        protected CollisionFilterGroups m_collisionGroups = CollisionFilterGroups.StaticFilter;
        protected CollisionFilterGroups m_collisionMask = (CollisionFilterGroups.AllFilter ^ CollisionFilterGroups.StaticFilter);
        protected float m_mass = 0f;
        protected Vector3 m_inertia;
        protected IMotionState m_motionState;
        // whether or not it's a ghost object.
        protected bool m_ghost;
        protected bool m_inPhysicsWorld;
    }
}