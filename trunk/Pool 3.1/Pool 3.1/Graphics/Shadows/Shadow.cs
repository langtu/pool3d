﻿#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TextureInUse = XNA_PoolGame.Graphics.PostProcessManager.TextureInUse;
#endregion

namespace XNA_PoolGame.Graphics.Shadows
{
    /// <summary>
    /// Abstract class.
    /// </summary>
    public abstract class Shadow
    {
        public Vector2[] pcfSamples = new Vector2[9];
        public float[] depthBias;
        public TextureInUse shadowOcclussionTIU;

        public DepthStencilBuffer stencilBuffer;

        /// <summary>
        /// Old depth buffer.
        /// </summary>
        public DepthStencilBuffer oldBuffer;


        public TextureInUse[] shadowMapTIU;
        public TextureInUse shadowTIU;


        public int shadowMapSize;

        /// <summary>
        /// Depth map from light sources (0 nearest, 1 farest). One RenderTarget per light.
        /// </summary>
        public RenderTarget2D[] ShadowMapRT;

        /// <summary>
        /// Shadow occlussion (white fragment = litted).
        /// </summary>
        public RenderTarget2D ShadowRT;

        public TextureInUse resultTIU;

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
            stencilBuffer = null;

            depthBias = null;
        }

        public abstract void PostDraw();
        public abstract void Pass3(GameTime gameTime);
        public abstract void Pass4(GameTime gameTime);

        public abstract string GetDepthMapTechnique();
        public abstract void SetPCFParameters(ref List<Light> lights);
        public abstract void SetDepthMapParameters(Light light);

        /// <summary>
        /// Draw the scene without shadows
        /// </summary>
        public virtual void DrawTextured(GameTime gameTime)
        {
            World.camera.ItemsDrawn = 0;
            PostProcessManager.ChangeRenderMode(RenderPassMode.BasicRender);

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
