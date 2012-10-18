#if WINDOWS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using BulletSharpPhysics = BulletSharp;
using IlluminatiEngine.BaseObjects;
using BulletSharp;

namespace IlluminatiEngine
{
    public class BulletSharpPhysicsComponent : GameComponentService
    {

        BulletSharpPhysics.DynamicsWorld _world;
        BulletSharpDeferredDebugDraw _debugDraw;

        public BulletSharpPhysics.DynamicsWorld World
        {
            get { return _world; }
            protected set { _world = value; }
        }

        public BulletSharpPhysics.DebugDrawModes DebugDrawMode
        {
            get 
            {
                if (_debugDraw != null)
                {
                    return _debugDraw.DebugMode;
                }
                return BulletSharpPhysics.DebugDrawModes.None;
            }
            set
            {
                if (value != BulletSharpPhysics.DebugDrawModes.None)
                {
                    if (_debugDraw == null)
                    {

                        _debugDraw = new BulletSharpDeferredDebugDraw(Game);
                        World.DebugDrawer = _debugDraw;
                        Game.Components.Add(_debugDraw);
                    }
                    _debugDraw.DebugMode = value;
                }

            }
        }


        public BulletSharpPhysicsComponent(Game game, BulletSharpPhysics.CollisionConfiguration collisionConf, BulletSharpPhysics.ConstraintSolver solver, Vector3 gravity)
            : base(game)
        {
            BulletSharpPhysics.CollisionDispatcher Dispatcher = new BulletSharpPhysics.CollisionDispatcher(collisionConf);

            World = new BulletSharpPhysics.DiscreteDynamicsWorld(Dispatcher, new BulletSharpPhysics.DbvtBroadphase(), solver, collisionConf);
            //World = new BulletSharpPhysics.DiscreteDynamicsWorld(Dispatcher, new BulletSharpPhysics.AxisSweep3(new Vector3(-1000),new Vector3(1000),16384), solver, collisionConf);
            World.Gravity = gravity;

       
        }

        public BulletSharpPhysicsComponent(Game game, BulletSharpPhysics.CollisionConfiguration collisionConf, Vector3 gravity)
            : this(game, new BulletSharpPhysics.DefaultCollisionConfiguration(), new BulletSharpPhysics.SequentialImpulseConstraintSolver(), gravity) { }

        public BulletSharpPhysicsComponent(Game game, Vector3 gravity) : this(game, new BulletSharpPhysics.DefaultCollisionConfiguration(), gravity) { }

        public BulletSharpPhysicsComponent(Game game) : this(game, new Vector3(0, -10, 0)){}

        public override void Update(GameTime gameTime)
        {
            if (Enabled)
            {
                _world.StepSimulation((float)gameTime.ElapsedGameTime.TotalMilliseconds);
                _world.DebugDrawWorld();
            }
        }

        public bool RayCastSingle(Vector3 from, Vector3 to, int filterMask, int filterGroup, ref Vector3 contactPoint, ref Vector3 contactNormal)
        {

            bool hasHit = false;
            BulletSharp.CollisionWorld.ClosestRayResultCallback callback = new BulletSharp.CollisionWorld.ClosestRayResultCallback(from, to);
            callback.CollisionFilterGroup = (BulletSharp.CollisionFilterGroups)filterGroup;
            callback.CollisionFilterMask = (BulletSharp.CollisionFilterGroups)filterMask;

            hasHit = callback.HasHit;
            if (hasHit)
            {
                contactPoint = callback.HitPointWorld;
                contactNormal = callback.HitNormalWorld;
            }
            return hasHit;
        }

        public bool RayCastAll(Vector3 from, Vector3 to, int filterMask, int filterGroup, List<Vector3> contactPoints, List<Vector3> contactNormals)
        {

            bool hasHit = false;
            BulletSharp.CollisionWorld.AllHitsRayResultCallback callback = new BulletSharp.CollisionWorld.AllHitsRayResultCallback(from, to);
            callback.CollisionFilterGroup = (BulletSharp.CollisionFilterGroups)filterGroup;
            callback.CollisionFilterMask = (BulletSharp.CollisionFilterGroups)filterMask;
            hasHit = callback.HasHit;
            if (hasHit)
            {
                int numHits = callback.HitNormalWorld.Count;
                for (int i = 0; i < numHits; ++i)
                {
                    contactPoints.Add(callback.HitPointWorld[i]);
                    contactNormals.Add(callback.HitNormalWorld[i]);
                }
            }



            return hasHit;
        }
    }

}
#endif