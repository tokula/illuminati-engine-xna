using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace IlluminatiEngine
{
    public class JitterPhysicsComponent : GameComponentService
    {

        public JitterPhysicsComponent(Game game) : base(game)
        {
            m_collisionSystem = new Jitter.Collision.CollisionSystemPersistentSAP();

            m_world = new Jitter.World(CollisionSystem); 
            World.AllowDeactivation = true;
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Update(gameTime);
            if (Enabled)
            {
                float step = (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (step > 1.0f / 100.0f) step = 1.0f / 100.0f;
                bool multiThread = false;
                World.Step(step, multiThread);
            }
        }

        private Jitter.Collision.CollisionSystem m_collisionSystem;
        public Jitter.Collision.CollisionSystem CollisionSystem
        {
            get { return m_collisionSystem; }
        }


        private Jitter.World m_world;
        public Jitter.World World
        {
            get { return m_world; }
        }



    }
}
