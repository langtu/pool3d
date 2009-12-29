using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Graphics.Shadows;
using Microsoft.Xna.Framework.Graphics;
using XNA_PoolGame.Models;

namespace XNA_PoolGame.Graphics
{
    public static class LightManager
    {
        public static BasicModel sphereModel = null;
        public static Light lights;
        public static BoundingSphere bounds;
        public static int totalLights;

        public static void Load()
        {
            //lights = new Light(new Vector3(0, 1000/2, -600.0f/2));
            //lights = new Light(new Vector3(0, 1000, -200.0f));

            bounds = new BoundingSphere();
            //lights = new Light(new Vector3(-286 , 500, 144));
            lights = new Light(new Vector3(0, 500, 144));
            lights.LightFarPlane = 1000.0f;

            //lights = new Light(new Vector3(-800, 500, 0));
            //lights.LightFarPlane = 2000.0f;
            totalLights = 1;

        }

        #region Helpers
        public static double AngleBetweenVectors(Vector3 first, Vector3 second)
        {
            float dot = Vector3.Dot(first, second);
            float magnitude = first.Length() * second.Length();
            return Math.Acos(dot / magnitude);
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
        #endregion
    }
}
