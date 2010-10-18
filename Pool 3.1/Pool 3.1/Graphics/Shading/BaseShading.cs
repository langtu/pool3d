using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Graphics.Shadows;
using TextureInUse = XNA_PoolGame.Graphics.PostProcessManager.TextureInUse;
using Microsoft.Xna.Framework.Graphics;

namespace XNA_PoolGame.Graphics.Shading
{
    /// <summary>
    /// Abstract class.
    /// </summary>
    public abstract class BaseShading
    {
        protected Vector2 halfPixel;
        protected Shadow shadows;
        public TextureInUse resultTIU;
        protected DepthStencilBuffer stencilBuffer;
        protected SurfaceFormat format;

        public Shadow Shadows 
        { 
            get { return shadows; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                shadows = value;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public SurfaceFormat Format
        { 
            get { return format; } 
        }

        public virtual void Dispose()
        {
            if (shadows != null) shadows.Dispose();
            shadows = null;
        }

        public abstract void SetParameters();

        public abstract void Draw(GameTime gameTime);

        public abstract void DrawTextured(GameTime gameTime);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract string GetBasicRenderTechnique();


        /// <summary>
        /// 
        /// </summary>
        public abstract void FreeStuff();
    }
}
