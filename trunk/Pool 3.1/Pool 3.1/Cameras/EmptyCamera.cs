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

        /// <summary>
        /// Do nothing.
        /// </summary>
        public override void SetMouseCentered()
        {
            
        }

        /// <summary>
        /// Do nothing.
        /// </summary>
        public override void UpdateCameraMatrices()
        {
            
        }

        public override Matrix View
        {
            get { return viewMatrix; }
            set
            {
                viewMatrix = value;
                viewProjectionMatrix = viewMatrix * projectionMatrix;
                if (enableFrustum) frustum.Matrix = viewProjectionMatrix;
            }
        }

        public override Matrix Projection
        {
            get { return projectionMatrix; }
            set
            {
                projectionMatrix = value;
                viewProjectionMatrix = viewMatrix * projectionMatrix;
                if (enableFrustum) frustum.Matrix = viewProjectionMatrix;
            }
        }

        public override Matrix ViewProjection
        {
            get { return viewProjectionMatrix; }
            set
            {
                viewProjectionMatrix = value;
                if (enableFrustum) frustum.Matrix = viewProjectionMatrix;
            }
        }
        public override BoundingFrustum Frustum
        {
            get
            {
                return frustum;
            }
        }

        protected override void Dispose(bool disposing)
        {
            //World.emptycamera = null;
        }
    }
}
