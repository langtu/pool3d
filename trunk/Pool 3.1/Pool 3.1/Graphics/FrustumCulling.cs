//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Microsoft.Xna.Framework;

//namespace XNA_PoolGame.Graphics
//{
//    public enum FrustumTest
//    {
//        Outside,
//        Intersect,
//        Inside
//    }

//    public class FrustumCulling
//    {
        
//        private const int RIGHT = 0;
//        private const int LEFT = 1;
//        private const int BOTTOM = 2;
//        private const int TOP = 3;
//        private const int BACK = 4;
//        private const int FRONT = 5;
//        private const int A = 0;
//        private const int B = 1;
//        private const int C = 2;
//        private const int D = 3;

//        private bool matrixdirty = true;

//        public Matrix Matrix
//        {
//            get { return mvp; }
//            set
//            {
//                mvp = value;
//                matrixdirty = true;
//            }
//        }
//        private Matrix mvp;
//        public float[,] frustumPlanes = new float[6, 4];

//        public FrustumCulling(Matrix viewprojection)
//        {
//            this.mvp = viewprojection;
//            CalcuteFrustum();
//        }

//        private void NormalizePlane(int side)
//        {
//            // Here we calculate the magnitude of the normal to the plane (point A B C)
//            // Remember that (A, B, C) is that same thing as the normal's (X, Y, Z).
//            // To calculate magnitude you use the equation:  magnitude = sqrt( x^2 + y^2 + z^2)
//            float magnitude = (float)Math.Sqrt(frustumPlanes[side, A] * frustumPlanes[side, A] +
//                                           frustumPlanes[side, B] * frustumPlanes[side, B] +
//                                           frustumPlanes[side, C] * frustumPlanes[side, C]);

//            // Then we divide the plane's values by it's magnitude.
//            // This makes it easier to work with.
//            frustumPlanes[side, A] /= magnitude;
//            frustumPlanes[side, B] /= magnitude;
//            frustumPlanes[side, C] /= magnitude;
//            frustumPlanes[side, D] /= magnitude; 
//        }
//        public void CalcuteFrustum(Matrix viewproj)
//        {
//            mvp = viewproj;
//            CalcuteFrustum();
//        }
//        public void CalcuteFrustum()
//        {
//            //
//            // Extract the frustum's right clipping plane and normalize it.
//            //

//            frustumPlanes[RIGHT, A] = mvp.M14 - mvp.M11;
//            frustumPlanes[RIGHT, B] = mvp.M24 - mvp.M21;
//            frustumPlanes[RIGHT, C] = mvp.M34 - mvp.M31;
//            frustumPlanes[RIGHT, D] = mvp.M44 - mvp.M41;

//            NormalizePlane(RIGHT);

//            //
//            // Extract the frustum's left clipping plane and normalize it.
//            //

//            frustumPlanes[LEFT, A] = mvp.M14 + mvp.M11;
//            frustumPlanes[LEFT, B] = mvp.M24 + mvp.M21;
//            frustumPlanes[LEFT, C] = mvp.M34 + mvp.M31;
//            frustumPlanes[LEFT, D] = mvp.M44 + mvp.M41;

//            NormalizePlane(LEFT);

//            //
//            // Extract the frustum's bottom clipping plane and normalize it.
//            //

//            frustumPlanes[BOTTOM, A] = mvp.M14 + mvp.M12;
//            frustumPlanes[BOTTOM, B] = mvp.M24 + mvp.M22;
//            frustumPlanes[BOTTOM, C] = mvp.M34 + mvp.M32;
//            frustumPlanes[BOTTOM, D] = mvp.M44 + mvp.M42;


//            NormalizePlane(BOTTOM);

//            //
//            // Extract the frustum's top clipping plane and normalize it.
//            //

//            frustumPlanes[TOP, A] = mvp.M14 - mvp.M12;
//            frustumPlanes[TOP, B] = mvp.M24 - mvp.M22;
//            frustumPlanes[TOP, C] = mvp.M34 - mvp.M32;
//            frustumPlanes[TOP, D] = mvp.M44 - mvp.M42;

//            NormalizePlane(TOP);

//            //
//            // Extract the frustum's far clipping plane and normalize it.
//            //

//            frustumPlanes[BACK, A] = mvp.M14 - mvp.M13;
//            frustumPlanes[BACK, B] = mvp.M24 - mvp.M23;
//            frustumPlanes[BACK, C] = mvp.M34 - mvp.M33;
//            frustumPlanes[BACK, D] = mvp.M44 - mvp.M43;

//            NormalizePlane(BACK);

//            //
//            // Extract the frustum's near clipping plane and normalize it.
//            //

//            frustumPlanes[FRONT, A] = mvp.M14 + mvp.M13;
//            frustumPlanes[FRONT, B] = mvp.M24 + mvp.M23;
//            frustumPlanes[FRONT, C] = mvp.M34 + mvp.M33;
//            frustumPlanes[FRONT, D] = mvp.M44 + mvp.M43;

//            NormalizePlane(FRONT);

//            matrixdirty = false;
//        }

//        float PlaneDistance(int i, Vector3 p)
//        {
//            return (frustumPlanes[i, D] + (frustumPlanes[i, A] * p.X + frustumPlanes[i, B] * p.Y + frustumPlanes[i, C] * p.Z));
//        }
//        private Vector3 PlaneNormal(int i)
//        {
//            return new Vector3(frustumPlanes[i, A], frustumPlanes[i, B], frustumPlanes[i, C]);
//        }
//        public FrustumTest SphereInFrustum(Vector3 p, float radio)
//        {
//            if (matrixdirty) CalcuteFrustum();
//            FrustumTest result = FrustumTest.Inside;
//            float distance;

//            for (int i = 0; i < 6; i++)
//            {
//                distance = PlaneDistance(i, p);
//                if (distance < -radio)
//                    return FrustumTest.Outside;
//                else if (distance < radio)
//                    result = FrustumTest.Intersect;
//            }
//            return (result);

//        }
//        public FrustumTest OBoxInFrustum(Vector3 min, Vector3 max)
//        {
//            if (matrixdirty) CalcuteFrustum();

//            FrustumTest result = FrustumTest.Inside;
//            for (int i = 0; i < 6; i++)
//            {
//                Vector3 normal = PlaneNormal(i);
//                if (i == TOP) normal.Y= -normal.Y;
//                Vector3 vn;
//                if (normal.X >= 0.0f) vn.X = min.X;
//                else vn.X = max.X;

//                if (normal.Y >= 0.0f) vn.Y = min.Y;
//                else vn.Y = max.Y;

//                if (normal.Z >= 0.0f) vn.Z = min.Z;
//                else vn.Z = max.Z;

//                float a = Vector3.Dot(vn, normal) + frustumPlanes[i, D];
//                if (a > 0.0f) 
//                    return FrustumTest.Outside;

//                Vector3 vp;

//                if (normal.X <= 0.0f) vp.X = min.X;
//                else vp.X = max.X;

//                if (normal.Y <= 0.0f) vp.Y = min.Y;
//                else vp.Y = max.Y;

//                if (normal.Z <= 0.0f) vp.Z = min.Z;
//                else vp.Z = max.Z;

//                float b = Vector3.Dot(vp, normal) + frustumPlanes[i, D];
//                if (b > 0.0f) result = FrustumTest.Intersect;
//            }

//            return result;
//        }

//        public FrustumTest BoxInFrustum(Vector3 min, Vector3 max)
//        {
//            if (matrixdirty) CalcuteFrustum();
//            FrustumTest result = FrustumTest.Inside;

//            for (int i = 0; i < 6; i++)
//            {
//                if ((frustumPlanes[i, A] * min.X + frustumPlanes[i, B] * min.Y + frustumPlanes[i, C] * min.Z + frustumPlanes[i, D]) > 0)
//                    continue;
//                if ((frustumPlanes[i, A] * max.X + frustumPlanes[i, B] * min.Y + frustumPlanes[i, C] * min.Z + frustumPlanes[i, D]) > 0)
//                    continue;
//                if ((frustumPlanes[i, A] * min.X + frustumPlanes[i, B] * max.Y + frustumPlanes[i, C] * min.Z + frustumPlanes[i, D]) > 0)
//                    continue;
//                if ((frustumPlanes[i, A] * max.X + frustumPlanes[i, B] * max.Y + frustumPlanes[i, C] * min.Z + frustumPlanes[i, D]) > 0)
//                    continue;
//                if ((frustumPlanes[i, A] * min.X + frustumPlanes[i, B] * min.Y + frustumPlanes[i, C] * max.Z + frustumPlanes[i, D]) > 0)
//                    continue;
//                if ((frustumPlanes[i, A] * max.X + frustumPlanes[i, B] * min.Y + frustumPlanes[i, C] * max.Z + frustumPlanes[i, D]) > 0)
//                    continue;
//                if ((frustumPlanes[i, A] * min.X + frustumPlanes[i, B] * max.Y + frustumPlanes[i, C] * max.Z + frustumPlanes[i, D]) > 0)
//                    continue;
//                if ((frustumPlanes[i, A] * max.X + frustumPlanes[i, B] * max.Y + frustumPlanes[i, C] * max.Z + frustumPlanes[i, D]) > 0)
//                    continue;

//                // If we get here, it isn't in the frustum
//                return FrustumTest.Outside;
//            }
//            return result;
//         }
//    }
//}
