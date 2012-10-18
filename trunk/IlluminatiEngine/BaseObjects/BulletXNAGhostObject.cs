//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Microsoft.Xna.Framework;
//using BulletXNA.BulletCollision;
//using BulletXNA.LinearMath;

//namespace IlluminatiEngine.BaseObjects
//{
//    public class BulletXNAGhostObject : BaseDeferredObject,IDisposable
//    {
//        public BulletXNAPhysicsComponent bulletPhysics
//        {
//            get { return (BulletXNAPhysicsComponent)Game.Services.GetService(typeof(BulletXNAPhysicsComponent)); }
//        }

//        public GhostObject GhostObject { get; set; }

//        public BulletXNAGhostObject(Game game) : base(game) { }
//        public BulletXNAGhostObject(Game game, string modelAssetName) : base(game, modelAssetName) { }

//        public BulletXNAGhostObject(Game game, CollisionShape collisionShape)
//            : base(game)
//        {
//            SetUpBulletPhysicsBody(collisionShape);
//        }


//        public virtual void SetUpBulletPhysicsBody(CollisionShape collisionShape)
//        {
//            m_ghostObject = new BulletXNA.BulletCollision.GhostObject();
//            m_ghostObject.SetCollisionShape(collisionShape);

//            BulletXNA.BulletCollision.CollisionFilterGroups bflags = GetCollisionFlags();
//            BulletXNA.BulletCollision.CollisionFilterGroups bmask = GetCollisionMask();

//            m_ghostObject.SetCollisionFlags(BulletXNA.BulletCollision.CollisionFlags.CF_NO_CONTACT_RESPONSE);		// We can choose to make it "solid" if we want...
//            m_ghostObject.SetWorldTransform(BulletXNA.LinearMath.IndexedMatrix.CreateTranslation(Position));
//            // let us know what it is..

//            bulletPhysics.World.AddCollisionObject(GhostObject, GetCollisionFlags(), GetCollisionMask());
//        }


//        public override void Update(GameTime gameTime)
//        {
//            World = GhostObject.GetWorldTransform();
//            Position = World.Translation;
//            Orientation = Quaternion.CreateFromRotationMatrix(World); 
//            base.Update(gameTime);
//        }

//        public override Vector3 Position
//        {
//            get
//            {
//                return GhostObject.GetWorldTransform()._origin;
//            }
//            set
//            {
//                base.Position = value;
//                IndexedMatrix im = GhostObject.GetWorldTransform();
//                im._origin = value;
//                GhostObject.SetWorldTransform(ref im);
//            }
//        }


//        public virtual CollisionFilterGroups GetCollisionFlags()
//        {
//            return CollisionFilterGroups.StaticFilter;
//        }


//        public virtual CollisionFilterGroups GetCollisionMask()
//        {
//            return (CollisionFilterGroups.AllFilter ^ CollisionFilterGroups.StaticFilter);
//        }

//        public virtual void Dispose()
//        {
//            // make sure we're removed from the world when we 'finish'
//            bulletPhysics.World.RemoveCollisionObject(this.GhostObject);
//        }


       
//        private GhostObject m_ghostObject;
//    }
//}
