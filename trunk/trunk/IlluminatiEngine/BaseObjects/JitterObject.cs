using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace IlluminatiEngine.BaseObjects
{
    public class JitterObject : BaseDeferredObject
    {
        [BrowsableAttribute(false)]
        public JitterPhysicsComponent jitterPhysics
        {
            get { return (JitterPhysicsComponent)Game.Services.GetService(typeof(JitterPhysicsComponent)); }
        }

        public JitterObject(Game game)
            : base(game)
        {
        }

        public JitterObject(Game game,String assetName)
            : base(game,assetName)
        {
        }


        public virtual void SetUpBulletPhysicsBody()
        {
            m_rigidBody = new Jitter.Dynamics.RigidBody(Shape);
            jitterPhysics.World.AddBody(m_rigidBody);
        }

        public Jitter.Collision.Shapes.Shape Shape
        {
            get { return m_collisionShape; }
            set { m_collisionShape = value; }
        }

        public Jitter.Dynamics.RigidBody RigidBody
        {
            get { return m_rigidBody; }
            set { m_rigidBody = value; }
        }

        public override void Update(GameTime gameTime)
        {
            Matrix m = ToXNAMatrix(RigidBody.Orientation);
            m.Translation = ToXNAVector(RigidBody.Position);

            World = m;
            Position = m.Translation;
            Orientation = Quaternion.CreateFromRotationMatrix(m);

            base.Update(gameTime);
        }


        public override Vector3 Position
        {
            get
            {
                return JitterObject.ToXNAVector(RigidBody.Position);
            }
            set
            {
                base.Position = value;
                RigidBody.Position = JitterObject.ToJitterVector(value);
            }
        }




        public override void TranslateAA(Vector3 distance)
        {
            HasMoved = Moved(distance);
            RigidBody.Position += ToJitterVector(distance);
        }

        public override void TranslateOO(Vector3 distance)
        {
            HasMoved = Moved(distance);
            RigidBody.Position += ToJitterVector(GameComponentHelper.Translate3D(distance, rotation));
        }



        public static Jitter.LinearMath.JVector ToJitterVector(Vector3 vector)
        {
            return new Jitter.LinearMath.JVector(vector.X, vector.Y, vector.Z);
        }

        public static Matrix ToXNAMatrix(Jitter.LinearMath.JMatrix matrix)
        {
            return new Matrix(matrix.M11,
                            matrix.M12,
                            matrix.M13,
                            0.0f,
                            matrix.M21,
                            matrix.M22,
                            matrix.M23,
                            0.0f,
                            matrix.M31,
                            matrix.M32,
                            matrix.M33,
                            0.0f, 0.0f, 0.0f, 0.0f, 1.0f);
        }

        public static Jitter.LinearMath.JMatrix ToJitterMatrix(Matrix matrix)
        {
            Jitter.LinearMath.JMatrix result;
            result.M11 = matrix.M11;
            result.M12 = matrix.M12;
            result.M13 = matrix.M13;
            result.M21 = matrix.M21;
            result.M22 = matrix.M22;
            result.M23 = matrix.M23;
            result.M31 = matrix.M31;
            result.M32 = matrix.M32;
            result.M33 = matrix.M33;
            return result;

        }


        public static Vector3 ToXNAVector(Jitter.LinearMath.JVector vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }



        protected Jitter.Collision.Shapes.Shape m_collisionShape;
        protected Jitter.Dynamics.RigidBody m_rigidBody;
    }
}
