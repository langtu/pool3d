using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace XNA_PoolGame.Graphics.Shadows
{
    public class ScreenSpaceShadows
    {

        public ScreenSpaceShadows()
        {

        }

        public void Draw(GameTime gameTime)
        {
            PostProcessManager.ChangeRenderMode(RenderMode.ShadowMapRender);
            RenderShadowMap();

            World.scenario.DrawScene(gameTime);

            PostProcessManager.ChangeRenderMode(RenderMode.PCFShadowMapRender);
            RenderPCFShadowMap();

            World.scenario.DrawScene(gameTime);

            World.camera.ItemsDrawn = 0;
            PostProcessManager.ChangeRenderMode(RenderMode.ScreenSpaceSoftShadowRender);
            RenderSSSoftShadow();

            World.scenario.DrawScene(gameTime);

            /////////////////////////////////////
            PoolGame.device.RenderState.StencilEnable = false;

        }

        public void RenderShadowMap()
        {
            PoolGame.device.RenderState.DepthBufferEnable = true;
            PoolGame.device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            //Render Shadow Map
            PostProcessManager.oldBuffer = PoolGame.device.DepthStencilBuffer;
            PoolGame.device.DepthStencilBuffer = PostProcessManager.stencilBuffer;
            PoolGame.device.SetRenderTarget(0, PostProcessManager.ShadowMapRT);
            PoolGame.device.Clear(Color.White);
        }

        public void RenderPCFShadowMap()
        {
            //Render PCF Shadow Map
            PoolGame.device.DepthStencilBuffer = PostProcessManager.oldBuffer;
            PoolGame.device.SetRenderTarget(0, PostProcessManager.ShadowRT);
            PoolGame.device.Clear(Color.White);

            PoolGame.device.RenderState.CullMode = CullMode.None;
        }
        public void RenderSSSoftShadow()
        {
            // Soft
            if (PostProcessManager.ShadowTechnique == ShadowTechnnique.SoftShadow)
            {
                PostProcessManager.SetBlurEffectParameters(0.5f / PoolGame.device.Viewport.Width, 0.0f, PostProcessManager.GBlurH);
                PostProcessManager.SetBlurEffectParameters(0.0f, 0.5f / PoolGame.device.Viewport.Height, PostProcessManager.GBlurV);

                //Gaussian Blur H
                PoolGame.device.SetRenderTarget(0, PostProcessManager.GBlurHRT);
                PostProcessManager.DrawQuad(PostProcessManager.ShadowRT.GetTexture(), PostProcessManager.GBlurH);

                //Guassian Blur V
                PoolGame.device.SetRenderTarget(0, PostProcessManager.GBlurVRT);
                PostProcessManager.DrawQuad(PostProcessManager.GBlurHRT.GetTexture(), PostProcessManager.GBlurV);

                PostProcessManager.SetBlurEffectParameters(1.5f / PoolGame.device.Viewport.Width, 0.0f, PostProcessManager.GBlurH);
                PostProcessManager.SetBlurEffectParameters(0.0f, 1.5f / PoolGame.device.Viewport.Height, PostProcessManager.GBlurV);
            }

            //Screen Space Shadow

            PoolGame.device.SetRenderTarget(0, PostProcessManager.mainRT);
            PoolGame.device.SetRenderTarget(1, PostProcessManager.depthRT);
            PoolGame.device.SetRenderTarget(2, PostProcessManager.velocityRT);
            //PoolGame.device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1.0f, 0);
            PoolGame.device.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1.0f, 0);
            PostProcessManager.DrawQuad(PostProcessManager.whiteTexture, PostProcessManager.clearGBufferEffect);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //PoolGame.device.SetRenderTarget(1, PostProcessManager.DepthOfFieldRT);
            //PostProcessManager.DrawQuad(PostProcessManager.whiteTexture, PostProcessManager.clearGBufferEffect);
            //PoolGame.device.Clear(ClearOptions.DepthBuffer, Color.White, 1.0f, 1);
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            PoolGame.device.RenderState.DepthBufferEnable = true;
            PoolGame.device.RenderState.DepthBufferWriteEnable = true;
            PoolGame.device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
        }
    }
}
