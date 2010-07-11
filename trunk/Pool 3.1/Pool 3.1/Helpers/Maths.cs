using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNA_PoolGame.Helpers
{
    
    public static class Maths
    {
        public static Random random = new Random(Environment.TickCount);
        public static double AngleBetweenVectors(Vector3 first, Vector3 second)
        {
            float dot = Vector3.Dot(first, second);
            float magnitude = first.Length() * second.Length();
            return Math.Acos(dot / magnitude);
        }
        public static Vector3 RandomPointOnCircle()
        {
            const float radius = 100;
            const float height = 400;

            double angle = random.NextDouble() * Math.PI * 2;

            float x = (float)Math.Cos(angle);
            float y = (float)Math.Sin(angle);

            return new Vector3(x * radius, y * radius + height, 0);
        }

        public static Vector3 RandomPointOnCube(Vector3 center, float side)
        {
            float signX = random.NextDouble() < 0.5 ? -1 : 1;
            float signY = random.NextDouble() < 0.5 ? -1 : 1;
            float signZ = random.NextDouble() < 0.5 ? -1 : 1;

            float x = signX * (float)random.NextDouble() * side;
            float y = signY * (float)random.NextDouble() * side;
            float z = signZ * (float)random.NextDouble() * side;

            return new Vector3(x + center.X, y + center.Y, z + center.Z);
        }
        public static float RamdomNumberBetween(float fmin, float fmax)
        {

            return fmin + ((float)random.NextDouble() * (fmax - fmin));
        }
        public static Matrix CalcLightProjection(Vector3 lightPos, BoundingSphere bounds, Viewport viewport)
        {
            // temp3 a vector that intersects the light
            // and is tangent to the bounds
            Vector3 temp = lightPos - bounds.Center;
            Vector3 temp2 = Vector3.Cross(temp, Vector3.Up);
            temp2 = Vector3.Cross(temp, temp2);
            temp2.Normalize();
            temp2 = temp2 * bounds.Radius;
            Vector3 temp3 = lightPos - (bounds.Center + temp2);

            // The angle between the tangent and the center of the bounds
            // is half of our field of view
            float angle = (float)AngleBetweenVectors(-temp, -temp3) * 2;
            float near = temp.Length() - bounds.Radius;
            float far = temp.Length() + bounds.Radius;

            // If the light actually gets into the scene, the projection
            // matrix could throw an exception.  These clamping operations
            // prevent that
            angle = MathHelper.Clamp(angle, 0.01f, 179.99f);
            near = MathHelper.Max(near, 0.01f);

            return Matrix.CreatePerspectiveFieldOfView(angle,
                (float)viewport.Width / viewport.Height, 0.01f, far);
        }
    }
}
