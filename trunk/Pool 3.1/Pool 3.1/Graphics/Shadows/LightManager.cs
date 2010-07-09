using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Graphics.Shadows;
using Microsoft.Xna.Framework.Graphics;
using XNA_PoolGame.Graphics;
using XNA_PoolGame.Graphics.Models;

namespace XNA_PoolGame.Graphics
{
    public static class LightManager
    {
        public static Entity sphereModel = null;
        public static List<Light> lights;
        public static BoundingSphere bounds;
        public static int totalLights;
        public static Matrix[] viewprojections;
        public static float[] maxdepths;
        public static Vector4[] positions;
        public static Vector4[] diffuse;
        public static Vector4[] ambient;
        public static Vector4[] specular;
        public static Vector4[] nospecular;
        public static float[] depthbias;

        public static void Load()
        {
            bounds = new BoundingSphere();

            //Light li = lights[0];lights[0] = lights[1];lights[1] = li;
            totalLights = lights.Count;
            viewprojections = new Matrix[totalLights];
            maxdepths = new float[totalLights];
            positions = new Vector4[totalLights];
            diffuse = new Vector4[totalLights];
            ambient = new Vector4[totalLights];
            specular = new Vector4[totalLights];
            nospecular = new Vector4[totalLights];
            depthbias = new float[totalLights];
            totalLights = 1;

            UpdateLights();
            
        }

        public static void UpdateLights()
        {
            for (int i = 0; i < totalLights; ++i)
            {
                viewprojections[i] = lights[i].LightViewProjection;
                maxdepths[i] = lights[i].LightFarPlane;
                positions[i] = new Vector4(lights[i].Position.X, lights[i].Position.Y, lights[i].Position.Z, 1.0f);
                diffuse[i] = lights[i].DiffuseColor;
                ambient[i] = lights[i].AmbientColor;
                specular[i] = lights[i].SpecularColor;
                nospecular[i] = new Vector4(0, 0, 0, 1);
                depthbias[i] = lights[i].DepthBias;
            }
        }

    }
}
