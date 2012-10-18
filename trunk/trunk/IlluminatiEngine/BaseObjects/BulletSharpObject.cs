#if WINDOWS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using IlluminatiEngine;

using BulletSharpPhysics = BulletSharp;

namespace IlluminatiEngine
{
    public class BulletSharpObject : BaseDeferredObject
    {
        public BulletSharpPhysicsComponent bulletPhysics
        {
            get { return (BulletSharpPhysicsComponent)Game.Services.GetService(typeof(BulletSharpPhysicsComponent)); }
        }

        public BulletSharpPhysics.RigidBody RigidBody { get; set; }

        public BulletSharpObject(Game game) : base(game) { }
        public BulletSharpObject(Game game, string modelAssetName) : base(game, modelAssetName) { }

        public BulletSharpObject(Game game, float mass, BulletSharp.MotionState motionState, BulletSharp.CollisionShape collisionShape, Vector3 localInertia)
            : base(game)
        {
            SetUpBulletPhysicsBody(mass, motionState, collisionShape, localInertia);
        }

        public BulletSharpObject(Game game, string modelAssetName, float mass, BulletSharp.MotionState motionState, BulletSharp.CollisionShape collisionShape, Vector3 localInertia)
            : base(game, modelAssetName)
        {
            SetUpBulletPhysicsBody(mass, motionState, collisionShape, localInertia);
        }

        public void SetUpBulletPhysicsBody(float mass, BulletSharp.MotionState motionState, BulletSharp.CollisionShape collisionShape, Vector3 localInertia)
        {
            BulletSharpPhysics.RigidBodyConstructionInfo rbInfo =
                            new BulletSharpPhysics.RigidBodyConstructionInfo(mass, motionState, collisionShape, localInertia);
            RigidBody = new BulletSharpPhysics.RigidBody(rbInfo);

            bulletPhysics.World.AddRigidBody(RigidBody,GetCollisionFlags(),GetCollisionMask());
           
        }


        public override void Update(GameTime gameTime)
        {
            World = RigidBody.WorldTransform;
            Position = World.Translation;
            Orientation = Quaternion.CreateFromRotationMatrix(World);

            base.Update(gameTime);
        }

        public override void TranslateAA(Vector3 distance)
        {
            HasMoved = Moved(distance);
            RigidBody.ActivationState = BulletSharpPhysics.ActivationState.ActiveTag;
            RigidBody.Translate(distance);
        }

        public override void TranslateOO(Vector3 distance)
        {
            HasMoved = Moved(distance);
            RigidBody.ActivationState = BulletSharpPhysics.ActivationState.ActiveTag;
            RigidBody.Translate(GameComponentHelper.Translate3D(distance, rotation));
        }

        public virtual void Accelerate(Vector3 velocity)
        {
            RigidBody.LinearVelocity += velocity;
        }

        public virtual BulletSharp.CollisionFilterGroups GetCollisionFlags()
        {
            return BulletSharp.CollisionFilterGroups.StaticFilter;
        }


        public virtual BulletSharp.CollisionFilterGroups GetCollisionMask()
        {
            return (BulletSharp.CollisionFilterGroups.AllFilter ^ BulletSharp.CollisionFilterGroups.StaticFilter);
        }

        public Vector3 LinearVelocity
        {
            get { return RigidBody.LinearVelocity; }
            set{RigidBody.LinearVelocity = value;}
        }

        public void ApplyImpulse(Vector3 impulse)
        {
            RigidBody.ActivationState = BulletSharp.ActivationState.ActiveTag;
            RigidBody.ApplyCentralImpulse(impulse);
        }

        public override void Rotate(Vector3 axis, float angle)
        {
            callCount++;
            if (callCount == 1 || callCount == 2)
            {
                Matrix m = RigidBody.WorldTransform;
                Quaternion q = Quaternion.CreateFromRotationMatrix(m);
                Quaternion qc = q;
                GameComponentHelper.Rotate(axis, angle, ref q);
                int ibreak = 0;

            }
            rotation = Quaternion.CreateFromRotationMatrix(RigidBody.WorldTransform);
            GameComponentHelper.Rotate(axis, angle, ref rotation);
            Matrix result = Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(Position);
            RigidBody.WorldTransform = result;

            System.Console.WriteLine("RotResult : " + result);
            // nb scale doesn't really count here as it will be the collisionshape not the rigid body that counts?
        }

        int callCount = 0;
 
    }
}
#endif