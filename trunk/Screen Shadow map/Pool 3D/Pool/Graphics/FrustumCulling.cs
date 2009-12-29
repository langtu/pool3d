using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA_PoolGame.Graphics
{
    public class FrustumCulling
    {
        public Matrix mvp;
        public float[,] frustumPlanes = new float[6, 4];

        public FrustumCulling(Matrix mvp)
        {
            this.mvp = mvp;
            CalcuteFrustum();

        }

        public void CalcuteFrustum()
        {
            float t;


            //
            // Extract the frustum's right clipping plane and normalize it.
            //

            frustumPlanes[0, 0] = mvp.M14 - mvp.M11;
            frustumPlanes[0, 1] = mvp.M24 - mvp.M21;
            frustumPlanes[0, 2] = mvp.M34 - mvp.M31;
            frustumPlanes[0, 3] = mvp.M44 - mvp.M41;

            t = (float)Math.Sqrt(frustumPlanes[0, 0] * frustumPlanes[0, 0] +
                      frustumPlanes[0, 1] * frustumPlanes[0, 1] +
                      frustumPlanes[0, 2] * frustumPlanes[0, 2]);

            frustumPlanes[0, 0] /= t;
            frustumPlanes[0, 1] /= t;
            frustumPlanes[0, 2] /= t;
            frustumPlanes[0, 3] /= t;

            //
            // Extract the frustum's left clipping plane and normalize it.
            //

            frustumPlanes[1, 0] = mvp.M14 + mvp.M11;
            frustumPlanes[1, 1] = mvp.M24 + mvp.M21;
            frustumPlanes[1, 2] = mvp.M34 + mvp.M31;
            frustumPlanes[1, 3] = mvp.M44 + mvp.M41;

            t = (float)Math.Sqrt(frustumPlanes[1, 0] * frustumPlanes[1, 0] +
                              frustumPlanes[1, 1] * frustumPlanes[1, 1] +
                              frustumPlanes[1, 2] * frustumPlanes[1, 2]);

            frustumPlanes[1, 0] /= t;
            frustumPlanes[1, 1] /= t;
            frustumPlanes[1, 2] /= t;
            frustumPlanes[1, 3] /= t;

            //
            // Extract the frustum's bottom clipping plane and normalize it.
            //

            frustumPlanes[2, 0] = mvp.M14 + mvp.M12;
            frustumPlanes[2, 1] = mvp.M24 + mvp.M22;
            frustumPlanes[2, 2] = mvp.M34 + mvp.M32;
            frustumPlanes[2, 3] = mvp.M44 + mvp.M42;

            t = (float)Math.Sqrt(frustumPlanes[2, 0] * frustumPlanes[2, 0] +
                              frustumPlanes[2, 1] * frustumPlanes[2, 1] +
                              frustumPlanes[2, 2] * frustumPlanes[2, 2]);

            frustumPlanes[2, 0] /= t;
            frustumPlanes[2, 1] /= t;
            frustumPlanes[2, 2] /= t;
            frustumPlanes[2, 3] /= t;

            //
            // Extract the frustum's top clipping plane and normalize it.
            //

            frustumPlanes[3, 0] = mvp.M14 - mvp.M12;
            frustumPlanes[3, 1] = mvp.M24 - mvp.M22;
            frustumPlanes[3, 2] = mvp.M34 - mvp.M32;
            frustumPlanes[3, 3] = mvp.M44 - mvp.M42;

            t = (float)Math.Sqrt(frustumPlanes[3, 0] * frustumPlanes[3, 0] +
                              frustumPlanes[3, 1] * frustumPlanes[3, 1] +
                              frustumPlanes[3, 2] * frustumPlanes[3, 2]);

            frustumPlanes[3, 0] /= t;
            frustumPlanes[3, 1] /= t;
            frustumPlanes[3, 2] /= t;
            frustumPlanes[3, 3] /= t;

            //
            // Extract the frustum's far clipping plane and normalize it.
            //

            frustumPlanes[4, 0] = mvp.M14 - mvp.M13;
            frustumPlanes[4, 1] = mvp.M24 - mvp.M23;
            frustumPlanes[4, 2] = mvp.M34 - mvp.M33;
            frustumPlanes[4, 3] = mvp.M44 - mvp.M43;

            t = (float)Math.Sqrt(frustumPlanes[4, 0] * frustumPlanes[4, 0] +
                              frustumPlanes[4, 1] * frustumPlanes[4, 1] +
                              frustumPlanes[4, 2] * frustumPlanes[4, 2]);

            frustumPlanes[4, 0] /= t;
            frustumPlanes[4, 1] /= t;
            frustumPlanes[4, 2] /= t;
            frustumPlanes[4, 3] /= t;

            //
            // Extract the frustum's near clipping plane and normalize it.
            //

            frustumPlanes[5, 0] = mvp.M14 + mvp.M13;
            frustumPlanes[5, 1] = mvp.M24 + mvp.M23;
            frustumPlanes[5, 2] = mvp.M34 + mvp.M33;
            frustumPlanes[5, 3] = mvp.M44 + mvp.M43;

            t = (float)Math.Sqrt(frustumPlanes[5, 0] * frustumPlanes[5, 0] +
                              frustumPlanes[5, 1] * frustumPlanes[5, 1] +
                              frustumPlanes[5, 2] * frustumPlanes[5, 2]);

            frustumPlanes[5, 0] /= t;
            frustumPlanes[5, 1] /= t;
            frustumPlanes[5, 2] /= t;
            frustumPlanes[5, 3] /= t;
        }

        float PlaneDistance(int i, Vector3 p)
        {
            return (frustumPlanes[i, 3] + (frustumPlanes[i, 0] * p.X + frustumPlanes[i, 1] * p.Y + frustumPlanes[i, 2] * p.Z));
        }
        public FrustumTest SphereInFrustum(Vector3 p, float radio)
        {
            FrustumTest result = FrustumTest.Inside;
            float distance;

            for (int i = 0; i < 6; i++)
            {
                distance = PlaneDistance(i, p);
                if (distance < -radio)
                    return FrustumTest.Outside;
                else if (distance < radio)
                    result = FrustumTest.Intersect;
            }
            return (result);

        }

        public FrustumTest boxInFrustum(Vector3 min, Vector3 max)
        {
            int A = 0;
            int B = 1;
            int C = 2;
            int D = 3;
	        FrustumTest result = FrustumTest.Inside;

            for (int i = 0; i < 6; i++)
            {
                if (frustumPlanes[i, A] * min.X + frustumPlanes[i, B] * min.Y + frustumPlanes[i, C] * min.Z + frustumPlanes[i, D] > 0)
                    continue;
                if (frustumPlanes[i, A] * max.X + frustumPlanes[i, B] * min.Y + frustumPlanes[i, C] * min.Z + frustumPlanes[i, D] > 0)
                    continue;
                if (frustumPlanes[i, A] * min.X + frustumPlanes[i, B] * max.Y + frustumPlanes[i, C] * min.Z + frustumPlanes[i, D] > 0)
                    continue;
                if (frustumPlanes[i, A] * max.X + frustumPlanes[i, B] * max.Y + frustumPlanes[i, C] * min.Z + frustumPlanes[i, D] > 0)
                    continue;
                if (frustumPlanes[i, A] * min.X + frustumPlanes[i, B] * min.Y + frustumPlanes[i, C] * max.Z + frustumPlanes[i, D] > 0)
                    continue;
                if (frustumPlanes[i, A] * max.X + frustumPlanes[i, B] * min.Y + frustumPlanes[i, C] * max.Z + frustumPlanes[i, D] > 0)
                    continue;
                if (frustumPlanes[i, A] * min.X + frustumPlanes[i, B] * max.Y + frustumPlanes[i, C] * max.Z + frustumPlanes[i, D] > 0)
                    continue;
                if (frustumPlanes[i, A] * max.X + frustumPlanes[i, B] * max.Y + frustumPlanes[i, C] * max.Z + frustumPlanes[i, D] > 0)
                    continue;

                // If we get here, it isn't in the frustum
                return FrustumTest.Outside;
            }
            return result;
         }
    }
}
