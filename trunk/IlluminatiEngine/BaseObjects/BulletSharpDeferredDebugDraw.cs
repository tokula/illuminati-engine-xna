#if WINDOWS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletSharp;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace IlluminatiEngine.BaseObjects
{
    public class BulletSharpDeferredDebugDraw : Microsoft.Xna.Framework.DrawableGameComponent, IDebugDraw, IDeferredRender
    {
        public static List<string> ErrorList = new List<string>();

        public BulletSharpDeferredDebugDraw(Microsoft.Xna.Framework.Game game)
            : base(game)
        {

        }

        public DebugDrawModes DebugMode
        {
            get
            {
                return m_debugDrawModes;
            }
            set
            {
                m_debugDrawModes = value;
            }
        }

        public void Draw3dText(ref Vector3 location, string textString)
        {
            throw new NotImplementedException();
        }

        public void DrawAabb(ref Vector3 from, ref Vector3 to, Microsoft.Xna.Framework.Color color)
        {
            DrawBox(from, to, color);
        }

        public void DrawArc(ref Vector3 center, ref Vector3 normal, ref Vector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle, Microsoft.Xna.Framework.Color color, bool drawSect, float stepDegrees)
        {
            Vector3 vx = axis;
            Vector3 vy = Vector3.Cross(normal, axis);
            float step = stepDegrees * SIMD_RADS_PER_DEG;
            int nSteps = (int)((maxAngle - minAngle) / step);
            if (nSteps == 0)
            {
                nSteps = 1;
            }
            Vector3 prev = center + radiusA * vx * (float)Math.Cos(minAngle) + radiusB * vy * (float)Math.Sin(minAngle);
            if (drawSect)
            {
                DrawLine(center, prev, color);
            }
            for (int i = 1; i <= nSteps; i++)
            {
                float angle = minAngle + (maxAngle - minAngle) * i / nSteps;
                Vector3 next = center + radiusA * vx * (float)Math.Cos(angle) + radiusB * vy * (float)Math.Sin(angle);
                DrawLine(prev, next, color);
                prev = next;
            }
            if (drawSect)
            {
                DrawLine(center, prev, color);
            }
 
        }

        public void DrawArc(ref Vector3 center, ref Vector3 normal, ref Vector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle, Microsoft.Xna.Framework.Color color, bool drawSect)
        {
            DrawArc(ref center, ref normal, ref axis, radiusA, radiusB, minAngle, maxAngle, color, drawSect, 10f);
        }

        public void DrawBox(ref Vector3 bbMin, ref Vector3 bbMax, Microsoft.Xna.Framework.Color color)
        {
            Matrix im = Matrix.Identity;
            DrawBox(ref bbMin, ref bbMax, ref im, color);
        }

        public void DrawBox(Vector3 bbMin, Vector3 bbMax, Matrix transform, Microsoft.Xna.Framework.Color color)
        {
            DrawBox(ref bbMin, ref bbMax, ref transform, color);
        }

        public void DrawBox(ref Vector3 bbMin, ref Vector3 bbMax, ref Matrix transform, Microsoft.Xna.Framework.Color color)
        {
            DrawLine(Vector3.Transform(new Vector3(bbMin.X, bbMin.Y, bbMin.Z),transform), Vector3.Transform(new Vector3(bbMax.X, bbMin.Y, bbMin.Z),transform), color);
            DrawLine(Vector3.Transform(new Vector3(bbMax.X, bbMin.Y, bbMin.Z),transform), Vector3.Transform(new Vector3(bbMax.X, bbMax.Y, bbMin.Z),transform), color);
            DrawLine(Vector3.Transform(new Vector3(bbMax.X, bbMax.Y, bbMin.Z),transform), Vector3.Transform(new Vector3(bbMin.X, bbMax.Y, bbMin.Z),transform), color);
            DrawLine(Vector3.Transform(new Vector3(bbMin.X, bbMax.Y, bbMin.Z),transform), Vector3.Transform(new Vector3(bbMin.X, bbMin.Y, bbMin.Z),transform), color);
            DrawLine(Vector3.Transform(new Vector3(bbMin.X, bbMin.Y, bbMin.Z),transform), Vector3.Transform(new Vector3(bbMin.X, bbMin.Y, bbMax.Z),transform), color);
            DrawLine(Vector3.Transform(new Vector3(bbMax.X, bbMin.Y, bbMin.Z),transform), Vector3.Transform(new Vector3(bbMax.X, bbMin.Y, bbMax.Z),transform), color);
            DrawLine(Vector3.Transform(new Vector3(bbMax.X, bbMax.Y, bbMin.Z),transform), Vector3.Transform(new Vector3(bbMax.X, bbMax.Y, bbMax.Z),transform), color);
            DrawLine(Vector3.Transform(new Vector3(bbMin.X, bbMax.Y, bbMin.Z),transform), Vector3.Transform(new Vector3(bbMin.X, bbMax.Y, bbMax.Z),transform), color);
            DrawLine(Vector3.Transform(new Vector3(bbMin.X, bbMin.Y, bbMax.Z),transform), Vector3.Transform(new Vector3(bbMax.X, bbMin.Y, bbMax.Z),transform), color);
            DrawLine(Vector3.Transform(new Vector3(bbMax.X, bbMin.Y, bbMax.Z),transform), Vector3.Transform(new Vector3(bbMax.X, bbMax.Y, bbMax.Z),transform), color);
            DrawLine(Vector3.Transform(new Vector3(bbMax.X, bbMax.Y, bbMax.Z),transform), Vector3.Transform(new Vector3(bbMin.X, bbMax.Y, bbMax.Z),transform), color);
            DrawLine(Vector3.Transform(new Vector3(bbMin.X, bbMax.Y, bbMax.Z),transform), Vector3.Transform(new Vector3(bbMin.X, bbMin.Y, bbMax.Z),transform), color);
        }

        public void DrawBox(Vector3 bbMin, Vector3 bbMax, Microsoft.Xna.Framework.Color color)
        {
            Matrix im = Matrix.Identity;
            DrawBox(ref bbMin, ref bbMax, ref im, color);
        }

        public void DrawCapsule(float radius, float halfHeight, int upAxis, ref Matrix transform, Microsoft.Xna.Framework.Color color)
        {
            throw new NotImplementedException();
        }

        public void DrawCone(float radius, float height, int upAxis, ref Matrix transform, Microsoft.Xna.Framework.Color color)
        {
            throw new NotImplementedException();
        }

        public void DrawContactPoint(ref Vector3 pointOnB, ref Vector3 normalOnB, float distance, int lifeTime, Microsoft.Xna.Framework.Color color)
        {
            Vector3 to = pointOnB + (normalOnB * 1f);
            DrawLine(pointOnB, to, color);
        }

        public void DrawCylinder(float radius, float halfHeight, int upAxis, ref Matrix transform, Microsoft.Xna.Framework.Color color)
        {
            throw new NotImplementedException();
        }

        public void DrawLine(Vector3 from, Vector3 to, Microsoft.Xna.Framework.Color color)
        {
            DrawLine(ref from, ref to, color);
        }
        public void DrawLine(ref Vector3 from, ref Vector3 to, Microsoft.Xna.Framework.Color color, Microsoft.Xna.Framework.Color color2)
        {
            DrawLine(ref from, ref to, color);
        }

        public void DrawLine(ref Vector3 from, ref Vector3 to, Microsoft.Xna.Framework.Color color)
        {
            if (m_lineIndex < m_lineVertexMaxSize - 2)
            {
                m_lineVertices[m_lineIndex].Position = from;
                m_lineVertices[m_lineIndex++].Color = DrawingColorToXNAColor(color);

                m_lineVertices[m_lineIndex].Position = to;
                m_lineVertices[m_lineIndex++].Color = DrawingColorToXNAColor(color);
            }
        }

        public void DrawLine(Vector3 from, Vector3 to, Microsoft.Xna.Framework.Color fromColor, Microsoft.Xna.Framework.Color toColor)
        {
            throw new NotImplementedException();
        }

        public void DrawPlane(ref Vector3 planeNormal, float planeConst, ref Matrix transform, Microsoft.Xna.Framework.Color color)
        {
            Vector3 planeOrigin = planeNormal * planeConst;
            Vector3 vec0, vec1;
            PlaneSpace1(ref planeNormal, out vec0, out vec1);
            float vecLen = 100f;
            Vector3 pt0 = Vector3.Transform(planeOrigin + vec0 * vecLen,transform);
            Vector3 pt1 = Vector3.Transform(planeOrigin - vec0 * vecLen,transform);
            Vector3 pt2 = Vector3.Transform(planeOrigin + vec1 * vecLen,transform);
            Vector3 pt3 = Vector3.Transform(planeOrigin - vec1 * vecLen,transform);
            
            DrawLine(pt0, pt1, color);
            DrawLine(pt2, pt3, color);
        }

        public void DrawSphere(ref Vector3 p, float radius, Microsoft.Xna.Framework.Color color)
        {
            throw new NotImplementedException();
        }

        public void DrawSphere(float radius, ref Matrix transform, Microsoft.Xna.Framework.Color color)
        {
            throw new NotImplementedException();
        }

        public void DrawSpherePatch(ref Vector3 center, ref Vector3 up, ref Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, Microsoft.Xna.Framework.Color color, float stepDegrees)
        {
            Vector3[] vA;
            Vector3[] vB;
            Vector3[] pvA, pvB, pT;
            Vector3 npole = center + up * radius;
            Vector3 spole = center - up * radius;
            Vector3 arcStart = Vector3.Zero;
            float step = stepDegrees * SIMD_RADS_PER_DEG;
            Vector3 kv = up;
            Vector3 iv = axis;

            Vector3 jv = Vector3.Cross(kv, iv);
            bool drawN = false;
            bool drawS = false;
            if (minTh <= -SIMD_HALF_PI)
            {
                minTh = -SIMD_HALF_PI + step;
                drawN = true;
            }
            if (maxTh >= SIMD_HALF_PI)
            {
                maxTh = SIMD_HALF_PI - step;
                drawS = true;
            }
            if (minTh > maxTh)
            {
                minTh = -SIMD_HALF_PI + step;
                maxTh = SIMD_HALF_PI - step;
                drawN = drawS = true;
            }
            int n_hor = (int)((maxTh - minTh) / step) + 1;
            if (n_hor < 2) n_hor = 2;
            float step_h = (maxTh - minTh) / (n_hor - 1);
            bool isClosed = false;
            if (minPs > maxPs)
            {
                minPs = -SIMD_PI + step;
                maxPs = SIMD_PI;
                isClosed = true;
            }
            else if ((maxPs - minPs) >= SIMD_PI * 2f)
            {
                isClosed = true;
            }
            else
            {
                isClosed = false;
            }
            int n_vert = (int)((maxPs - minPs) / step) + 1;
            if (n_vert < 2) n_vert = 2;

            vA = new Vector3[n_vert];
            vB = new Vector3[n_vert];
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

        public void DrawSpherePatch(ref Vector3 center, ref Vector3 up, ref Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, Microsoft.Xna.Framework.Color color)
        {
            throw new NotImplementedException();
        }

        public void DrawTransform(ref Matrix transform, float orthoLen)
        {
            throw new NotImplementedException();
        }

        public void DrawTriangle(ref Vector3 v0, ref Vector3 v1, ref Vector3 v2, Microsoft.Xna.Framework.Color color, float __unnamed004)
        {
            DrawLine( v0,  v1,  color);
            DrawLine( v1,  v2,  color);
            DrawLine( v2,  v0,  color);
        }

        public void DrawTriangle(ref Vector3 v0, ref Vector3 v1, ref Vector3 v2, ref Vector3 __unnamed003, ref Vector3 __unnamed004, ref Vector3 __unnamed005, Microsoft.Xna.Framework.Color color, float alpha)
        {
            DrawLine(v0, v1, color);
            DrawLine(v1, v2, color);
            DrawLine(v2, v0, color);
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

        public void Draw(Microsoft.Xna.Framework.GameTime gameTime)
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


        private Microsoft.Xna.Framework.Color DrawingColorToXNAColor(Microsoft.Xna.Framework.Color color)
        {
            return new Microsoft.Xna.Framework.Color(color.R,color.G,color.B,color.A);
        }


        public void PlaneSpace1(ref Vector3 n, out Vector3 p, out Vector3 q)
        {
            if (Math.Abs(n.Z) > SIMDSQRT12)
            {
                // choose p in y-z plane
                float a = n.Y * n.Y + n.Z * n.Z;
                float k = RecipSqrt(a);
                p = new Vector3(0, -n.Z * k, n.Y * k);
                // set q = n x p
                q = new Vector3(a * k, -n.X * p.Z, n.X * p.Y);
            }
            else
            {
                // choose p in x-y plane
                float a = n.X * n.X + n.Y * n.Y;
                float k = RecipSqrt(a);
                p = new Vector3(-n.Y * k, n.X * k, 0);
                // set q = n x p
                q = new Vector3(-n.Z * p.Y, n.Z * p.X, a * k);
            }
        }

        public float RecipSqrt(float a)
        {
            return (float)(1 / Math.Sqrt(a));
        }



        private const int m_lineVertexMaxSize = 500000;

        private int m_lineIndex = 0;

        private VertexPositionColor[] m_lineVertices = new VertexPositionColor[m_lineVertexMaxSize];
        private List<String> m_emptyList = new List<String>();
        private BulletSharp.DebugDrawModes m_debugDrawModes;
        private Effect m_basicEffect;


        public const float SIMD_2_PI = 6.283185307179586232f;
        public const float SIMD_PI = SIMD_2_PI * 0.5f;
        public const float SIMD_HALF_PI = SIMD_PI * 0.5f;
        public const float SIMD_QUARTER_PI = SIMD_PI * 0.25f;

        public const float SIMD_INFINITY = float.MaxValue;
        public const float SIMD_RADS_PER_DEG = (SIMD_2_PI / 360.0f);
        public const float SIMD_DEGS_PER_RAD = (360.0f / SIMD_2_PI);

        public const float SIMDSQRT12 = 0.7071067811865475244008443621048490f;
    }
}
#endif