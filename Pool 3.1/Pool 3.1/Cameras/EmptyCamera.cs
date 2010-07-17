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
                viewDirty = false;
                invViewDirty = true;
                viewProjDirty = true;

                viewMatrix = value; 
            }
        }

        public override Matrix Projection
        {
            get { return projectionMatrix; }
            set
            {
                projDirty = false;
                viewProjDirty = true;

                projectionMatrix = value;
            }
        }

        public override Matrix ViewProjection
        {
            get { return base.ViewProjection; }
            set
            {
                viewProjDirty = false;
                invViewProjDirty = true;

                viewProjectionMatrix = View * Projection;
            }
        }

        protected override void Dispose(bool disposing)
        {
            World.emptycamera = null;
        }
    }
}
