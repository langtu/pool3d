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
        public float depthBias;
        public DepthStencilBuffer stencilBuffer;
        public DepthStencilBuffer oldBuffer;
        public TextureInUse shadowMapTIU;
        public TextureInUse shadowTIU;
        public int shadowMapSize;
        public RenderTarget2D[] ShadowMapRT;
        public RenderTarget2D ShadowRT;

        public abstract void Draw(GameTime gameTime);
        public abstract void Dispose();
        

        
    }
}
