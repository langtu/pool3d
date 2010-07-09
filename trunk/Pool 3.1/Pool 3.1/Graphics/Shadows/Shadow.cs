using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TextureInUse = XNA_PoolGame.Graphics.PostProcessManager.TextureInUse;

namespace XNA_PoolGame.Graphics.Shadows
{
    public abstract class Shadow
    {
        public Vector2[] pcfSamples = new Vector2[9];
        public float[] depthBias;


        public DepthStencilBuffer stencilBuffer;

        /// <summary>
        /// 
        /// </summary>
        public DepthStencilBuffer oldBuffer;


        public TextureInUse[] shadowMapTIU;
        public TextureInUse shadowTIU;


        public int shadowMapSize;

        /// <summary>
        /// One RenderTarget per light
        /// </summary>
        public RenderTarget2D[] ShadowMapRT;

        /// <summary>
        /// 
        /// </summary>
        public RenderTarget2D ShadowRT;

        public abstract void Draw(GameTime gameTime);
        public virtual void Dispose()
        {
            if (ShadowMapRT != null)
            {
                foreach (RenderTarget2D rt2d in ShadowMapRT)
                    rt2d.Dispose();
            }

            if (shadowMapTIU != null)
            {
                for (int i = 0; i < shadowMapTIU.Length; ++i)
                {
                    PostProcessManager.renderTargets.Remove(shadowMapTIU[i]);
                    shadowMapTIU[i] = null;
                }
                shadowMapTIU = null;
            }
            if (shadowTIU != null)
            {
                PostProcessManager.renderTargets.Remove(shadowTIU);
                shadowTIU = null;

            }
            if (stencilBuffer != null) stencilBuffer.Dispose();

            depthBias = null;
        }
        

        
    }
}
