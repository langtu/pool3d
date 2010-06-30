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

        public static void Load()
        {
            bounds = new BoundingSphere();
            
            totalLights = lights.Count;

        }

    }
}
