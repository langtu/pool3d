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

        public int lightpass = 0;
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


        /// <summary>
        /// Draw the scene without shadows
        /// </summary>
        public void DrawTextured(GameTime gameTime)
        {
            World.camera.ItemsDrawn = 0;
            PostProcessManager.ChangeRenderMode(RenderMode.BasicRender);

            PostProcessManager.mainTIU.Use();
            PoolGame.device.SetRenderTarget(0, PostProcessManager.mainRT);
            if (World.dofType == DOFType.None && World.motionblurType == MotionBlurType.None)
            {
                PoolGame.device.SetRenderTarget(1, null);
                PoolGame.device.SetRenderTarget(2, null);
                PoolGame.device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, Color.CornflowerBlue, 1.0f, 0);
            }
            else
            {
                PostProcessManager.depthTIU.Use(); PostProcessManager.velocityTIU.Use(); PostProcessManager.velocityLastFrameTIU.Use();
                PoolGame.device.SetRenderTarget(1, PostProcessManager.depthRT);
                PoolGame.device.SetRenderTarget(2, PostProcessManager.velocityRT);

                //PoolGame.device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1.0f, 0);
                PoolGame.device.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1.0f, 0);
                PostProcessManager.DrawQuad(PostProcessManager.whiteTexture, PostProcessManager.clearGBufferEffect);
            }

            World.scenario.DrawScene(gameTime);

            if (World.motionblurType != MotionBlurType.None || World.dofType != DOFType.None)
            {
                PoolGame.device.SetRenderTarget(1, null);
                PoolGame.device.SetRenderTarget(2, null);
            }
        }
    }
}
