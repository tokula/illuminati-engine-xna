using System;
using System.Collections.Generic;
using BulletXNA;
using Microsoft.Xna.Framework.Graphics;
using BulletXNA.LinearMath;
using Microsoft.Xna.Framework;

namespace IlluminatiEngine.BaseObjects
{
    public class BulletXNADeferredDebugDraw : Microsoft.Xna.Framework.DrawableGameComponent, IDebugDraw, IDeferredRender
    {
        public static List<string> ErrorList = new List<string>();

        public BulletXNADeferredDebugDraw(Microsoft.Xna.Framework.Game game)
            : base(game)
        {

        }

        protected override void LoadContent()
        {
        }

        public void DrawText(String text, IndexedVector3 position, IndexedVector3 color)
        {
            DrawText(text, ref position, ref color);
        }

        public void DrawText(String text, ref IndexedVector3 position, ref IndexedVector3 color)
        {
        }


        public void SetDebugMode(DebugDrawModes debugMode)
        {
            m_debugDrawModes = debugMode;
        }

        public DebugDrawModes GetDebugMode()
        {
            return m_debugDrawModes;
        }

        public void Draw3dText(ref IndexedVector3 location, string textString)
        {
            throw new NotImplementedException();
        }

        public void DrawAabb(IndexedVector3 from, IndexedVector3 to, IndexedVector3 color)
        {
            DrawAabb(ref from, ref to, ref color);
        }

        public void DrawAabb(ref IndexedVector3 from, ref IndexedVector3 to, ref IndexedVector3 color)
        {
            IndexedMatrix identity = IndexedMatrix.Identity;
            DrawBox(ref from, ref to, ref identity, ref color, 0f);
        }

        public void DrawArc(ref IndexedVector3 center, ref IndexedVector3 normal, ref IndexedVector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle, ref IndexedVector3 color, bool drawSect)
        {
            DrawArc(ref center, ref normal, ref axis, radiusA, radiusB, minAngle, maxAngle, ref color, drawSect, 10f);
        }

        public void DrawArc(ref IndexedVector3 center, ref IndexedVector3 normal, ref IndexedVector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle, ref IndexedVector3 color, bool drawSect, float stepDegrees)
        {
            IndexedVector3 vx = axis;
            IndexedVector3 vy = IndexedVector3.Cross(normal, axis);
            float step = stepDegrees * MathUtil.SIMD_RADS_PER_DEG;
            int nSteps = (int)((maxAngle - minAngle) / step);
            if (nSteps == 0)
            {
                nSteps = 1;
            }
            IndexedVector3 prev = center + radiusA * vx * (float)Math.Cos(minAngle) + radiusB * vy * (float)Math.Sin(minAngle);
            if (drawSect)
            {
                DrawLine(ref center, ref prev, ref color);
            }
            for (int i = 1; i <= nSteps; i++)
            {
                float angle = minAngle + (maxAngle - minAngle) * i / nSteps;
                IndexedVector3 next = center + radiusA * vx * (float)Math.Cos(angle) + radiusB * vy * (float)Math.Sin(angle);
                DrawLine(ref prev, ref next, ref color);
                prev = next;
            }
            if (drawSect)
            {
                DrawLine(ref center, ref prev, ref color);
            }
        }

        public void DrawBox(ref IndexedVector3 boxMin, ref IndexedVector3 boxMax, ref IndexedMatrix trans, ref IndexedVector3 color)
        {
            DrawBox(ref boxMin, ref boxMax, ref trans, ref color, 1f);
        }

        public void DrawBox(ref IndexedVector3 boxMin, ref IndexedVector3 boxMax, ref IndexedMatrix transform, ref IndexedVector3 color, float alpha)
        {
            DrawLine(transform * (new IndexedVector3(boxMin.X, boxMin.Y, boxMin.Z)), transform * (new IndexedVector3(boxMax.X, boxMin.Y, boxMin.Z)), color);
            DrawLine(transform * (new IndexedVector3(boxMax.X, boxMin.Y, boxMin.Z)), transform * (new IndexedVector3(boxMax.X, boxMax.Y, boxMin.Z)), color);
            DrawLine(transform * (new IndexedVector3(boxMax.X, boxMax.Y, boxMin.Z)), transform * (new IndexedVector3(boxMin.X, boxMax.Y, boxMin.Z)), color);
            DrawLine(transform * (new IndexedVector3(boxMin.X, boxMax.Y, boxMin.Z)), transform * (new IndexedVector3(boxMin.X, boxMin.Y, boxMin.Z)), color);
            DrawLine(transform * (new IndexedVector3(boxMin.X, boxMin.Y, boxMin.Z)), transform * (new IndexedVector3(boxMin.X, boxMin.Y, boxMax.Z)), color);
            DrawLine(transform * (new IndexedVector3(boxMax.X, boxMin.Y, boxMin.Z)), transform * (new IndexedVector3(boxMax.X, boxMin.Y, boxMax.Z)), color);
            DrawLine(transform * (new IndexedVector3(boxMax.X, boxMax.Y, boxMin.Z)), transform * (new IndexedVector3(boxMax.X, boxMax.Y, boxMax.Z)), color);
            DrawLine(transform * (new IndexedVector3(boxMin.X, boxMax.Y, boxMin.Z)), transform * (new IndexedVector3(boxMin.X, boxMax.Y, boxMax.Z)), color);
            DrawLine(transform * (new IndexedVector3(boxMin.X, boxMin.Y, boxMax.Z)), transform * (new IndexedVector3(boxMax.X, boxMin.Y, boxMax.Z)), color);
            DrawLine(transform * (new IndexedVector3(boxMax.X, boxMin.Y, boxMax.Z)), transform * (new IndexedVector3(boxMax.X, boxMax.Y, boxMax.Z)), color);
            DrawLine(transform * (new IndexedVector3(boxMax.X, boxMax.Y, boxMax.Z)), transform * (new IndexedVector3(boxMin.X, boxMax.Y, boxMax.Z)), color);
            DrawLine(transform * (new IndexedVector3(boxMin.X, boxMax.Y, boxMax.Z)), transform * (new IndexedVector3(boxMin.X, boxMin.Y, boxMax.Z)), color);

        }


        public void DrawBox(IndexedVector3 bbMin, IndexedVector3 bbMax, IndexedVector3 color)
        {
            IndexedMatrix im = IndexedMatrix.Identity;
            DrawBox(ref bbMin, ref bbMax, ref im, ref color);
        }

        public void DrawCapsule(float radius, float halfHeight, int upAxis, IndexedMatrix transform, IndexedVector3 color)
        {
            throw new NotImplementedException();
        }

        public void DrawCone(float radius, float height, int upAxis, IndexedMatrix transform, IndexedVector3 color)
        {
            throw new NotImplementedException();
        }

        public void DrawContactPoint(IndexedVector3 PointOnB, IndexedVector3 normalOnB, float distance, int lifeTime, IndexedVector3 color)
        {
            DrawContactPoint(ref PointOnB, ref normalOnB, distance, lifeTime, ref color);
        }

        public void DrawContactPoint(ref IndexedVector3 PointOnB, ref IndexedVector3 normalOnB, float distance, int lifeTime, ref IndexedVector3 color)
        {
            IndexedVector3 from = PointOnB;
            IndexedVector3 to = PointOnB + (normalOnB * 1f);
            DrawLine(ref from, ref to, ref color);
        }

        public void DrawCylinder(float radius, float halfHeight, int upAxis, IndexedMatrix transform, IndexedVector3 color)
        {
            throw new NotImplementedException();
        }

        public void DrawLine(IndexedVector3 from, IndexedVector3 to, IndexedVector3 fromColor)
        {
            DrawLine(ref from, ref to, ref fromColor);
        }

        public void DrawLine(ref IndexedVector3 from, ref IndexedVector3 to, ref IndexedVector3 fromColor)
        {
            DrawLine(ref from, ref to, ref fromColor, ref fromColor);
        }

        public void DrawLine(ref IndexedVector3 from, ref IndexedVector3 to, ref IndexedVector3 fromColor, ref IndexedVector3 toColor)
        {
            if (m_lineIndex < m_lineVertexMaxSize - 2)
            {
                m_lineVertices[m_lineIndex].Position = from.ToVector3();
                m_lineVertices[m_lineIndex++].Color = new Color(fromColor.ToVector3());

                m_lineVertices[m_lineIndex].Position = to.ToVector3();
                m_lineVertices[m_lineIndex++].Color = new Color(toColor.ToVector3());
            }
        }

        public void DrawPlane(IndexedVector3 planeNormal, float planeConst, IndexedMatrix transform, IndexedVector3 color)
        {
            IndexedVector3 planeOrigin = planeNormal * planeConst;
            IndexedVector3 vec0, vec1;
            PlaneSpace1(ref planeNormal, out vec0, out vec1);
            float vecLen = 100f;
            IndexedVector3 pt0 = transform * (planeOrigin + vec0 * vecLen);
            IndexedVector3 pt1 = transform * (planeOrigin - vec0 * vecLen);
            IndexedVector3 pt2 = transform * (planeOrigin + vec1 * vecLen);
            IndexedVector3 pt3 = transform * (planeOrigin - vec1 * vecLen);

            DrawLine(pt0, pt1, color);
            DrawLine(pt2, pt3, color);
        }

        public void DrawSphere(IndexedVector3 p, float radius, IndexedVector3 color)
        {
            DrawSphere(ref p, radius, ref color);
        }

        public void DrawSphere(ref IndexedVector3 p, float radius, ref IndexedVector3 color)
        {
            //throw new NotImplementedException();
        }

        public void DrawSpherePatch(ref IndexedVector3 center, ref IndexedVector3 up, ref IndexedVector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, ref IndexedVector3 color)
        {
            DrawSpherePatch(ref center, ref up, ref axis, radius, minTh, maxTh, minPs, maxPs, ref color, 10);
        }
        
        public void DrawSpherePatch(ref IndexedVector3 center, ref IndexedVector3 up, ref IndexedVector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, ref IndexedVector3 color, float stepDegrees)
        {
            IndexedVector3[] vA;
            IndexedVector3[] vB;
            IndexedVector3[] pvA, pvB, pT;
            IndexedVector3 npole = center + up * radius;
            IndexedVector3 spole = center - up * radius;
            IndexedVector3 arcStart = IndexedVector3.Zero;
            float step = stepDegrees * MathUtil.SIMD_RADS_PER_DEG;
            IndexedVector3 kv = up;
            IndexedVector3 iv = axis;

            IndexedVector3 jv = IndexedVector3.Cross(kv, iv);
            bool drawN = false;
            bool drawS = false;
            if (minTh <= -MathUtil.SIMD_HALF_PI)
            {
                minTh = -MathUtil.SIMD_HALF_PI + step;
                drawN = true;
            }
            if (maxTh >= MathUtil.SIMD_HALF_PI)
            {
                maxTh = MathUtil.SIMD_HALF_PI - step;
                drawS = true;
            }
            if (minTh > maxTh)
            {
                minTh = -MathUtil.SIMD_HALF_PI + step;
                maxTh = MathUtil.SIMD_HALF_PI - step;
                drawN = drawS = true;
            }
            int n_hor = (int)((maxTh - minTh) / step) + 1;
            if (n_hor < 2) n_hor = 2;
            float step_h = (maxTh - minTh) / (n_hor - 1);
            bool isClosed = false;
            if (minPs > maxPs)
            {
                minPs = -MathUtil.SIMD_PI + step;
                maxPs = MathUtil.SIMD_PI;
                isClosed = true;
            }
            else if ((maxPs - minPs) >= MathUtil.SIMD_PI * 2f)
            {
                isClosed = true;
            }
            else
            {
                isClosed = false;
            }
            int n_vert = (int)((maxPs - minPs) / step) + 1;
            if (n_vert < 2) n_vert = 2;

            vA = new IndexedVector3[n_vert];
            vB = new IndexedVector3[n_vert];
            pvA = vA; pvB = vB;

            float step_v = (maxPs - minPs) / (float)(n_vert - 1);
            for (int i = 0; i < n_hor; i++)
            {
                float th = minTh + i * step_h;
                float sth = radius * (float)Math.Sin(th);
                float cth = radius * (float)Math.Cos(th);
                for (int j = 0; j < n_vert; j++)
                {
                    float psi = minPs + (float)j * step_v;
                    float sps = (float)Math.Sin(psi);
                    float cps = (float)Math.Cos(psi);
                    pvB[j] = center + cth * cps * iv + cth * sps * jv + sth * kv;
                    if (i != 0)
                    {
                        DrawLine(pvA[j], pvB[j], color);
                    }
                    else if (drawS)
                    {
                        DrawLine(spole, pvB[j], color);
                    }
                    if (j != 0)
                    {
                        DrawLine(pvB[j - 1], pvB[j], color);
                    }
                    else
                    {
                        arcStart = pvB[j];
                    }
                    if ((i == (n_hor - 1)) && drawN)
                    {
                        DrawLine(npole, pvB[j], color);
                    }
                    if (isClosed)
                    {
                        if (j == (n_vert - 1))
                        {
                            DrawLine(arcStart, pvB[j], color);
                        }
                    }
                    else
                    {
                        if (((i == 0) || (i == (n_hor - 1))) && ((j == 0) || (j == (n_vert - 1))))
                        {
                            DrawLine(center, pvB[j], color);
                        }
                    }
                }
                pT = pvA; pvA = pvB; pvB = pT;
            }

        }


        public void DrawTransform(ref IndexedMatrix transform, float orthoLen)
        {
            IndexedVector3 start = transform._origin;
            IndexedVector3 temp = start + transform._basis * new IndexedVector3(orthoLen, 0, 0);
            IndexedVector3 colour = new IndexedVector3(0.7f, 0, 0);
            DrawLine(ref start, ref temp, ref colour);
            temp = start + transform._basis * new IndexedVector3(0, orthoLen, 0);
            colour = new IndexedVector3(0, 0.7f, 0);
            DrawLine(ref start, ref temp, ref colour);
            temp = start + transform._basis * new IndexedVector3(0, 0, orthoLen);
            colour = new IndexedVector3(0, 0, 0.7f);
            DrawLine(ref start, ref temp, ref colour);
        }

        public void DrawTriangle(ref IndexedVector3 v0, ref IndexedVector3 v1, ref IndexedVector3 v2, ref IndexedVector3 n0, ref IndexedVector3 n1, ref IndexedVector3 n2, ref IndexedVector3 color, float alpha)
        {
            DrawTriangle(ref v0, ref v1, ref v2, ref color, alpha);
        }

        public void DrawTriangle(ref IndexedVector3 v0, ref IndexedVector3 v1, ref IndexedVector3 v2, ref IndexedVector3 color, float alpha)
        {
            DrawLine(ref v0, ref v1, ref color);
            DrawLine(ref v1, ref v2, ref color);
            DrawLine(ref v2, ref v0, ref color);
        }

        public void ReportErrorWarning(string warningString)
        {
            //throw new NotImplementedException();
            ErrorList.Add(string.Format("{0:dd/MM/yyyy HH:mm:ss} - {1}", DateTime.Now, warningString));
        }


        public List<string> TextureMaterials
        {
            get
            {
                return m_emptyList;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<string> NormalMaterials
        {
            get
            {
                return m_emptyList;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<string> SpeculaMaterials
        {
            get
            {
                return m_emptyList;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<string> GlowMaterials
        {
            get
            {
                return m_emptyList;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<string> ReflectionMaterials
        {
            get
            {
                return m_emptyList;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (m_basicEffect == null)
            {
                m_basicEffect = AssetManager.GetAsset<Effect>("Shaders/Deferred/DeferredBasicEffect");
            }
            Draw(gameTime, m_basicEffect);
        }

        public void Draw(Microsoft.Xna.Framework.GameTime gameTime, Effect effect)
        {
            if (m_lineIndex > 0)
            {
                ICameraService camera = ((ICameraService)Game.Services.GetService(typeof(ICameraService)));

                m_basicEffect.Parameters["world"].SetValue(Microsoft.Xna.Framework.Matrix.Identity);
                m_basicEffect.Parameters["wvp"].SetValue(Microsoft.Xna.Framework.Matrix.Identity * camera.View * camera.Projection);

                int cnt = m_basicEffect.CurrentTechnique.Passes.Count;
                for (int p = 0; p < cnt; p++)
                {
                    m_basicEffect.CurrentTechnique.Passes[p].Apply();
                    Game.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, m_lineVertices, 0, m_lineIndex / 2);
                }
            }
            m_lineIndex = 0;
        }


        public SpriteBatch spriteBatch { get { return (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch)); } }

        public IAssetManager AssetManager
        {
            get { return ((IAssetManager)Game.Services.GetService(typeof(IAssetManager))); }
        }

        private IndexedVector3 V4V3(Vector4 v4)
        {
            return new IndexedVector3(v4.X, v4.Y, v4.Z);
        }


        public void PlaneSpace1(ref IndexedVector3 n, out IndexedVector3 p, out IndexedVector3 q)
        {
            if (Math.Abs(n.Z) > MathUtil.SIMDSQRT12)
            {
                // choose p in y-z plane
                float a = n.Y * n.Y + n.Z * n.Z;
                float k = RecipSqrt(a);
                p = new IndexedVector3(0, -n.Z * k, n.Y * k);
                // set q = n x p
                q = new IndexedVector3(a * k, -n.X * p.Z, n.X * p.Y);
            }
            else
            {
                // choose p in x-y plane
                float a = n.X * n.X + n.Y * n.Y;
                float k = RecipSqrt(a);
                p = new IndexedVector3(-n.Y * k, n.X * k, 0);
                // set q = n x p
                q = new IndexedVector3(-n.Z * p.Y, n.Z * p.X, a * k);
            }
        }

        public float RecipSqrt(float a)
        {
            return (float)(1 / Math.Sqrt(a));
        }


        public void DrawCylinder(float radius, float halfHeight, int upAxis, ref IndexedMatrix transform, ref IndexedVector3 color)
        {
        }
        public void DrawCone(float radius, float height, int upAxis, ref IndexedMatrix transform, ref IndexedVector3 color)
        {
        }
        public void DrawPlane(ref IndexedVector3 planeNormal, float planeConst, ref IndexedMatrix transform, ref IndexedVector3 color)
        {
        }

        public void DrawSphere(float radius, ref IndexedMatrix transform, ref IndexedVector3 color)
        {
        }
        public void DrawCapsule(float radius, float halfHeight, int upAxis, ref IndexedMatrix transform, ref IndexedVector3 color)
        {
        }

        public void DrawBox(ref IndexedVector3 bbMin, ref IndexedVector3 bbMax, ref IndexedVector3 color)
        {
        }



        private const int m_lineVertexMaxSize = 500000;

        private int m_lineIndex = 0;

        private VertexPositionColor[] m_lineVertices = new VertexPositionColor[m_lineVertexMaxSize];
        private List<String> m_emptyList = new List<String>();
        private DebugDrawModes m_debugDrawModes;
        private Effect m_basicEffect;
    }
}
