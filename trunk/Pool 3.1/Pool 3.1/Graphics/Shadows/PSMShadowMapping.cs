using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TextureInUse = XNA_PoolGame.Graphics.PostProcessManager.TextureInUse;
using Microsoft.Xna.Framework.Graphics;

namespace XNA_PoolGame.Graphics.Shadows
{
    public class PSMShadowMapping : Shadow
    {
        public Effect ShadowFinalPCM;
        public PSMShadowMapping()
        {
            shadowMapSize = World.shadowMapSize;
            //depthBias = 0.0042f; // 0.0035f;

            float texelSize = 0.75f / (float)shadowMapSize;

            pcfSamples[0] = new Vector2(0.0f, 0.0f);
            pcfSamples[1] = new Vector2(-texelSize, 0.0f);
            pcfSamples[2] = new Vector2(texelSize, 0.0f);
            pcfSamples[3] = new Vector2(0.0f, -texelSize);
            pcfSamples[4] = new Vector2(-texelSize, -texelSize);
            pcfSamples[5] = new Vector2(texelSize, -texelSize);
            pcfSamples[6] = new Vector2(0.0f, texelSize);
            pcfSamples[7] = new Vector2(-texelSize, texelSize);
            pcfSamples[8] = new Vector2(texelSize, texelSize);

            PresentationParameters pp = PoolGame.device.PresentationParameters;

            ShadowMapRT = new RenderTarget2D[2];
            ShadowMapRT[0] = new RenderTarget2D(PoolGame.device, shadowMapSize, shadowMapSize, 1, pp.BackBufferFormat);


            PostProcessManager.renderTargets.Add(new TextureInUse(ShadowMapRT[0], false));

            ShadowRT = new RenderTarget2D(PoolGame.device, pp.BackBufferWidth, pp.BackBufferHeight, 1, PoolGame.device.DisplayMode.Format,
                pp.MultiSampleType, pp.MultiSampleQuality);

            shadowTIU = new TextureInUse(ShadowRT, false);
            PostProcessManager.renderTargets.Add(shadowTIU);

            ShadowFinalPCM = PoolGame.content.Load<Effect>("Effects\\ShadowFinal");
            stencilBuffer = new DepthStencilBuffer(PoolGame.device, shadowMapSize, shadowMapSize, PoolGame.device.DepthStencilBuffer.Format);
        }

        public override void Draw(GameTime gameTime)
        {
            PostProcessManager.ChangeRenderMode(RenderMode.ShadowMapRender);
            RenderShadowMap();

            World.scenario.DrawScene(gameTime);

            PostProcessManager.ChangeRenderMode(RenderMode.PCFShadowMapRender);
            RenderPSM();

            World.scenario.DrawScene(gameTime);


            PostProcessManager.ChangeRenderMode(RenderMode.BasicRender);
            RenderFinal();

            World.scenario.DrawScene(gameTime);

            PoolGame.device.SetRenderTarget(0, null);
            //PoolGame.device.SetRenderTarget(1, null);
            //PoolGame.device.SetRenderTarget(2, null);

            Texture2D tex = PostProcessManager.mainRT.GetTexture();
            PoolGame.device.SetRenderTarget(0, PostProcessManager.mainRT);
            
            ShadowFinalPCM.Parameters["ShadowPCM"].SetValue(ShadowRT.GetTexture());
            PostProcessManager.DrawQuad(tex, ShadowFinalPCM);
        }
        private void RenderShadowMap()
        {
            PoolGame.device.RenderState.DepthBufferEnable = true;
            PoolGame.device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            //Render Shadow Map
            oldBuffer = PoolGame.device.DepthStencilBuffer;
            PoolGame.device.DepthStencilBuffer = stencilBuffer;
            PoolGame.device.SetRenderTarget(0, ShadowMapRT[0]);
            PoolGame.device.Clear(Color.White);

            // UPGRADE (MULTIPLE LIGHTS)
        }

        private void RenderPSM()
        {
            PoolGame.device.DepthStencilBuffer = oldBuffer;
            PoolGame.device.SetRenderTarget(0, ShadowRT);
            PoolGame.device.Clear(Color.White);

            PoolGame.device.RenderState.CullMode = CullMode.None;
        }


        private void RenderFinal()
        {
            PoolGame.device.SetRenderTarget(0, PostProcessManager.mainRT);
            PoolGame.device.SetRenderTarget(1, PostProcessManager.depthRT);
            PoolGame.device.SetRenderTarget(2, PostProcessManager.velocityRT);
            PoolGame.device.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1.0f, 0);
            PostProcessManager.DrawQuad(PostProcessManager.whiteTexture, PostProcessManager.clearGBufferEffect);

            PoolGame.device.RenderState.DepthBufferEnable = true;
            PoolGame.device.RenderState.DepthBufferWriteEnable = true;
            PoolGame.device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
        }
        public override void Dispose()
        {

            base.Dispose();
        }

        public override void PostDraw()
        {
            throw new NotImplementedException();
        }

        public override void Pass3(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public override void Pass4(GameTime gameTime)
        {
            throw new NotImplementedException();
        }
    }
}
