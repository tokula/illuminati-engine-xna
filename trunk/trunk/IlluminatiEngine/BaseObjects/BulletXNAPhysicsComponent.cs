using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using BulletXNAPhysics = BulletXNA;
using IlluminatiEngine.BaseObjects;
using BulletXNA.BulletDynamics;
using BulletXNA.BulletCollision;
using BulletXNA.LinearMath;
using IlluminatiEngine.Utilities;

namespace IlluminatiEngine
{
    public class BulletXNAPhysicsComponent : GameComponentService
    {
        public static class GravityTypes
        {
            public static Vector3 Sun
            {
                get { return new Vector3(0, -274, 0); }
            }
            public static Vector3 Mercury
            {
                get { return new Vector3(0, -2.78f, 0); }
            }
            public static Vector3 Venus
            {
                get { return new Vector3(0, -8.84f, 0); }
            }
            public static Vector3 Earth
            {
                get { return new Vector3(0, -9.81f, 0); }
            }
            public static Vector3 Moon
            {
                get { return new Vector3(0, -1.62f, 0); }
            }
            public static Vector3 Mars
            {
                get { return new Vector3(0, -3.80f, 0); }
            }
            public static Vector3 Jupiter
            {
                get { return new Vector3(0, -24.9f, 0); }
            }
            public static Vector3 Saturn
            {
                get { return new Vector3(0, -10.4f, 0); }
            }
            public static Vector3 Uranus
            {
                get { return new Vector3(0, -10.4f, 0); }
            }
            public static Vector3 Neptune
            {
                get { return new Vector3(0, -13.8f, 0); }
            }
            public static Vector3 Pluto
            {
                get { return new Vector3(0, -0.29f, 0); }
            }
        }

        DiscreteDynamicsWorld _world;
        BulletXNADeferredDebugDraw _debugDraw;

        public BulletXNA.BulletDynamics.DiscreteDynamicsWorld World
        {
            get { return _world; }
            protected set { _world = value; }
        }

        public BulletXNA.LinearMath.DebugDrawModes DebugDrawMode
        {
            get
            {
                if (_debugDraw != null)
                {
                    return _debugDraw.GetDebugMode();
                }
                return BulletXNA.LinearMath.DebugDrawModes.DBG_NoDebug;
            }
            set
            {
                if (value != BulletXNA.LinearMath.DebugDrawModes.DBG_NoDebug)
                {
                    if (_debugDraw == null)
                    {

                        _debugDraw = new BulletXNADeferredDebugDraw(Game);
                        World.SetDebugDrawer(_debugDraw);
                        Game.Components.Add(_debugDraw);
                    }
                    _debugDraw.SetDebugMode(value);
                }

            }
        }


        public BulletXNAPhysicsComponent(Game game, ICollisionConfiguration collisionConf, IConstraintSolver solver, Vector3 gravity)
            : base(game)
        {
            CollisionDispatcher Dispatcher = new CollisionDispatcher(collisionConf);

            IndexedVector3 worldMin = new IndexedVector3(-1000, -1000, -1000);
            IndexedVector3 worldMax = -worldMin;
            IBroadphaseInterface broadphase = new AxisSweep3Internal(ref worldMin, ref worldMax, 0xfffe, 0xffff, 16384, null, false);
            //broadphase = new DbvtBroadphase();


            World = new DiscreteDynamicsWorld(Dispatcher, broadphase, solver, collisionConf);
            IndexedVector3 iv3Gravity = new IndexedVector3(gravity.X, gravity.Y, gravity.Z);
            World.SetGravity(ref iv3Gravity);
        }

        public BulletXNAPhysicsComponent(Game game, ICollisionConfiguration collisionConf, Vector3 gravity)
            : this(game, new DefaultCollisionConfiguration(), new SequentialImpulseConstraintSolver(), gravity) { }

        public BulletXNAPhysicsComponent(Game game, Vector3 gravity) : this(game, new DefaultCollisionConfiguration(), gravity) { }

        public BulletXNAPhysicsComponent(Game game) : this(game, GravityTypes.Earth) { }

        public override void Update(GameTime gameTime)
        {
            SimpleProfiler.StartProfileBlock("BulletXNA");

            lock (addRemoveLock)
            {

                // go through and add / remove new objects in thread safe way.
                // FIXME MAN - should do the same for constraints at somepoint.
                int cnt = m_removeList.Count;
                for (int r = 0; r < cnt; r++)
                {
                    if (m_removeList[r] is RigidBody)
                    {
                        _world.RemoveRigidBody(m_removeList[r] as RigidBody);
                    }
                    else
                    {
                        _world.RemoveCollisionObject(m_removeList[r]);
                    }
                }
                m_removeList.Clear();

                cnt = m_addList.Count;
                for (int a = 0; a < cnt; a++)
                {
                    if (m_addList[a].collisionObject is RigidBody)
                    {
                        _world.AddRigidBody(m_addList[a].collisionObject as RigidBody, m_addList[a].filterGroup, m_addList[a].filterMask);
                    }
                    else
                    {
                        _world.AddCollisionObject(m_addList[a].collisionObject, m_addList[a].filterGroup, m_addList[a].filterMask);
                    }
                }
                m_addList.Clear();

                if (Enabled)
                {

                _world.StepSimulation((float)gameTime.ElapsedGameTime.TotalMilliseconds, 1);
                _world.DebugDrawWorld();
                }

            }
            SimpleProfiler.EndProfileBlock("BulletXNA");

        }

        public bool RayCastSingle(Vector3 from, Vector3 to, int filterMask, int filterGroup, ref Vector3 contactPoint, ref Vector3 contactNormal)
        {

            bool hasHit = false;
            ClosestRayResultCallback callback = new ClosestRayResultCallback(from, to);
            callback.m_collisionFilterGroup = (CollisionFilterGroups)filterGroup;
            callback.m_collisionFilterMask = (CollisionFilterGroups)filterMask;

            hasHit = callback.HasHit();
            if (hasHit)
            {
                contactPoint = callback.m_hitPointWorld;
                contactNormal = callback.m_hitPointWorld;
            }
            return hasHit;
        }

        public bool RayCastAll(Vector3 from, Vector3 to, int filterMask, int filterGroup, List<Vector3> contactPoints, List<Vector3> contactNormals)
        {

            bool hasHit = false;
            AllHitsRayResultCallback callback = new AllHitsRayResultCallback(from, to);
            callback.m_collisionFilterGroup = (CollisionFilterGroups)filterGroup;
            callback.m_collisionFilterMask = (CollisionFilterGroups)filterMask;

            hasHit = callback.HasHit();
            if (hasHit)
            {
                int numHits = callback.m_hitNormalWorld.Count;
                for (int i = 0; i < numHits; ++i)
                {
                    contactPoints.Add(callback.m_hitPointWorld[i]);
                    contactNormals.Add(callback.m_hitNormalWorld[i]);
                }
            }
            return hasHit;
        }

        public void AddCollisionObject(CollisionObject collisionObject, CollisionFilterGroups fg, CollisionFilterGroups fm)
        {
            lock (addRemoveLock)
            {
                m_addList.Add(new ColObjectHolder(collisionObject,fg,fm));
            }
        }

        public void RemoveCollisionObject(CollisionObject collisionObject)
        {
            lock (addRemoveLock)
            {
                m_removeList.Add(collisionObject);
            }
        }

        public void ResetWorld()
        {
            lock (addRemoveLock)
            {
                m_removeList.Clear();
                ObjectArray<CollisionObject> allObjects = new ObjectArray<CollisionObject>();
                allObjects.AddRange(_world.GetCollisionObjectArray());
                foreach(CollisionObject co in allObjects)
                {
                    if(co is RigidBody)
                    {
                        _world.RemoveRigidBody(co as RigidBody);
                    }
                    else
                    {
                        _world.RemoveCollisionObject(co);
                    }
                }

                ObjectArray<TypedConstraint> allConstraints = new ObjectArray<TypedConstraint>();
                allConstraints.AddRange(_world.GetConstraintsObjectArray());
                foreach (TypedConstraint typedConstraint in allConstraints)
                {
                    _world.RemoveConstraint(typedConstraint);
                }
            }
            // that should be it..... but we may want to re-create broadphases and the like...
        }


        public struct ColObjectHolder
        {
            public ColObjectHolder(CollisionObject co,CollisionFilterGroups fg, CollisionFilterGroups fm)
            {
                collisionObject = co;
                filterGroup = fg;
                filterMask = fm;
            }
            public CollisionObject collisionObject;
            public CollisionFilterGroups filterGroup;
            public CollisionFilterGroups filterMask;
        }


        // these should really be blocking collections. 
        private object addRemoveLock = new object();
        protected List<ColObjectHolder> m_addList = new List<ColObjectHolder>();
        protected List<CollisionObject> m_removeList = new List<CollisionObject>();

    }
}
