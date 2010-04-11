using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Helpers;

namespace XNA_PoolGame.PoolTables
{
    /// <summary>
    /// Pocket
    /// </summary>
    public class Pocket
    {
        public List<Ball> balls;
        public BoundingSphere bounds;
        public Vector3 headpoint;
        public Vector3[] insideNormal;
        public OrientedBoundingBox[] insideBands;

        public Pocket(BoundingSphere bounds)
        {
            this.bounds = bounds;
            balls = new List<Ball>();
            insideNormal = new Vector3[2];
            insideBands = new OrientedBoundingBox[2];
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetReady()
        {
            insideNormal[0].Normalize();
            insideNormal[1].Normalize();
        }

        public void Dispose()
        {
            balls.Clear();
            balls = null;
            insideNormal = null;
            insideBands = null;
            
        }
    }
}
