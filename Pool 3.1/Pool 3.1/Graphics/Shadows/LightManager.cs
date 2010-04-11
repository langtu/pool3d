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
            //lights = new Light(new Vector3(0, 350, 144));

            //lights.LightFarPlane = 1350.0f;
            lights.LightFarPlane = 1000.0f;

            //lights = new Light(new Vector3(-800, 500, 0));
            //lights.LightFarPlane = 2000.0f;
            totalLights = 1;

        }

        #region Helpers
        
        #endregion
    }
}
