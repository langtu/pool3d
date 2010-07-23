using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA_PoolGame.Cameras
{
    public class EmptyCamera : Camera
    {
        public EmptyCamera(Game game)
            : base(game)
        {

        }

        public override void SetMouseCentered()
        {
            
        }


        public override void UpdateCameraMatrices()
        {
            
        }

        public override Matrix View
        {
            get { return viewMatrix; }
            set
            {
                viewMatrix = value;
                frustum.Matrix = viewMatrix * projectionMatrix;
            }
        }

        public override Matrix Projection
        {
            get { return projectionMatrix; }
            set
            {
                projectionMatrix = value;
                frustum.Matrix = viewMatrix * projectionMatrix;
            }
        }

        public override Matrix ViewProjection
        {
            get { return viewProjectionMatrix; }
            set
            {
                viewProjectionMatrix = value;
                frustum.Matrix = viewProjectionMatrix;
            }
        }
        public override BoundingFrustum FrustumCulling
        {
            get
            {
                return frustum;
            }
        }
        protected override void Dispose(bool disposing)
        {
            World.emptycamera = null;
        }
    }
}
