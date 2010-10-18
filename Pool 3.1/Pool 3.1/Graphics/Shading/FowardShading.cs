using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNA_PoolGame.Graphics.Shading
{
    public class FowardShading : BaseShading
    {
        PresentationParameters pp;

        /// <summary>
        /// Create a new instance of FowardShading class.
        /// </summary>
        public FowardShading()
        {
            format = SurfaceFormat.Color;
            pp = PoolGame.device.PresentationParameters;
            stencilBuffer = new DepthStencilBuffer(PoolGame.device, PoolGame.Width, PoolGame.Height, PoolGame.device.DepthStencilBuffer.Format, pp.MultiSampleType, pp.MultiSampleQuality);
        }

        public override void Draw(GameTime gameTime)
        {
            #region SSAO PREPASS

            if (World.doSSAO || World.doNormalPositionPass)
            {
                PostProcessManager.depthTIU.Use();
                PostProcessManager.ssao.normalTIU = PostProcessManager.GetIntermediateTexture(PoolGame.Width, PoolGame.Height, SurfaceFormat.HalfVector4, PoolGame.device.PresentationParameters.MultiSampleType, PoolGame.device.PresentationParameters.MultiSampleQuality);
                PostProcessManager.ssao.viewTIU = PostProcessManager.GetIntermediateTexture(PoolGame.Width, PoolGame.Height, SurfaceFormat.Single, PoolGame.device.PresentationParameters.MultiSampleType, PoolGame.device.PresentationParameters.MultiSampleQuality);

                DepthStencilBuffer oldbuffer = PoolGame.device.DepthStencilBuffer;

                PoolGame.device.DepthStencilBuffer = stencilBuffer;
                //PoolGame.device.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.CornflowerBlue, 1.0f, 0);
                PoolGame.device.Clear(ClearOptions.DepthBuffer, Color.CornflowerBlue, 1.0f, 0);


                PoolGame.device.SetRenderTarget(0, PostProcessManager.ssao.normalTIU.renderTarget);
                PoolGame.device.SetRenderTarget(1, PostProcessManager.ssao.viewTIU.renderTarget);

                PostProcessManager.clearGBufferEffect.CurrentTechnique = PostProcessManager.clearGBufferEffect.Techniques["SSAOClearGBufferTechnnique"];
                PostProcessManager.DrawQuad(PostProcessManager.whiteTexture, PostProcessManager.clearGBufferEffect);


                PoolGame.device.RenderState.DepthBufferEnable = true;
                PoolGame.device.RenderState.DepthBufferWriteEnable = true;
                PoolGame.device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;


                PostProcessManager.ChangeRenderMode(RenderPassMode.SSAOPrePass);
                World.scenario.DrawScene(gameTime);

                PoolGame.device.DepthStencilBuffer = oldbuffer;

                PoolGame.device.SetRenderTarget(0, null);
                PoolGame.device.SetRenderTarget(1, null);
            }

            #endregion

            shadows.Draw(gameTime);

            shadows.Pass3(gameTime);
            shadows.Pass4(gameTime);

            shadows.PostDraw();
            if (World.motionblurType != MotionBlurType.None || World.dofType != DOFType.None)
            {
                PoolGame.device.SetRenderTarget(1, null);
                PoolGame.device.SetRenderTarget(2, null);
            }

            resultTIU = shadows.resultTIU;
        }

        public override void DrawTextured(GameTime gameTime)
        {
            #region SSAO PREPASS

            if (World.doSSAO || World.doNormalPositionPass)
            {
                PostProcessManager.ssao.normalTIU = PostProcessManager.GetIntermediateTexture(PoolGame.Width, PoolGame.Height, SurfaceFormat.HalfVector4, PoolGame.device.PresentationParameters.MultiSampleType, PoolGame.device.PresentationParameters.MultiSampleQuality);
                PostProcessManager.ssao.viewTIU = PostProcessManager.GetIntermediateTexture(PoolGame.Width, PoolGame.Height, SurfaceFormat.HalfVector4, PoolGame.device.PresentationParameters.MultiSampleType, PoolGame.device.PresentationParameters.MultiSampleQuality);

                DepthStencilBuffer oldbuffer = PoolGame.device.DepthStencilBuffer;

                PoolGame.device.DepthStencilBuffer = stencilBuffer;
                PoolGame.device.Clear(ClearOptions.DepthBuffer, Color.CornflowerBlue, 1.0f, 0);


                PoolGame.device.SetRenderTarget(0, PostProcessManager.ssao.normalTIU.renderTarget);
                PoolGame.device.SetRenderTarget(1, PostProcessManager.ssao.viewTIU.renderTarget);

                PostProcessManager.clearGBufferEffect.CurrentTechnique = PostProcessManager.clearGBufferEffect.Techniques["SSAOClearGBufferTechnnique"];
                PostProcessManager.DrawQuad(PostProcessManager.whiteTexture, PostProcessManager.clearGBufferEffect);


                PoolGame.device.RenderState.DepthBufferEnable = true;
                PoolGame.device.RenderState.DepthBufferWriteEnable = true;
                PoolGame.device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;


                PostProcessManager.ChangeRenderMode(RenderPassMode.SSAOPrePass);
                World.scenario.DrawScene(gameTime);

                PoolGame.device.DepthStencilBuffer = oldbuffer;

                PoolGame.device.SetRenderTarget(0, null);
                PoolGame.device.SetRenderTarget(1, null);
            }

            #endregion

            shadows.DrawTextured(gameTime);

            resultTIU = PostProcessManager.mainTIU;
        }

        public override void Dispose()
        {
            if (stencilBuffer != null) stencilBuffer.Dispose();
            stencilBuffer = null;
            base.Dispose();
        }

        public override string GetBasicRenderTechnique()
        {
            return "ModelTechnique";
        }

        public override void SetParameters()
        {

        }

        public override void FreeStuff()
        {
            
        }
    }
}
