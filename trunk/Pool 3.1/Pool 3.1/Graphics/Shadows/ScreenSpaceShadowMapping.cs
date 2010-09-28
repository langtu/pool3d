using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TextureInUse = XNA_PoolGame.Graphics.PostProcessManager.TextureInUse;

namespace XNA_PoolGame.Graphics.Shadows
{
    /// <summary>
    /// Screen Space Shadow Mapping with PCF Samples.
    /// </summary>
    public class ScreenSpaceShadowMapping : Shadow
    {

        PresentationParameters pp;
        public ScreenSpaceShadowMapping()
        {
            shadowMapSize = World.shadowMapSize;

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

            pp = PoolGame.device.PresentationParameters;

            ShadowMapRT = new RenderTarget2D[2];
            ShadowMapRT[0] = new RenderTarget2D(PoolGame.device, shadowMapSize, shadowMapSize, 1, pp.BackBufferFormat);
            ShadowMapRT[1] = new RenderTarget2D(PoolGame.device, shadowMapSize, shadowMapSize, 1, pp.BackBufferFormat);

            shadowMapTIU = new TextureInUse[2];
            shadowMapTIU[0] = new TextureInUse(ShadowMapRT[0], false);
            shadowMapTIU[1] = new TextureInUse(ShadowMapRT[1], false);

            PostProcessManager.renderTargets.Add(shadowMapTIU[0]);
            PostProcessManager.renderTargets.Add(shadowMapTIU[1]);

            ShadowRT = new RenderTarget2D(PoolGame.device, pp.BackBufferWidth, pp.BackBufferHeight, 1, PoolGame.device.DisplayMode.Format,
                pp.MultiSampleType, pp.MultiSampleQuality);

            shadowTIU = new TextureInUse(ShadowRT, false);
            PostProcessManager.renderTargets.Add(shadowTIU);

            stencilBuffer = new DepthStencilBuffer(PoolGame.device, shadowMapSize, shadowMapSize, PoolGame.device.DepthStencilBuffer.Format);
            
        }

        public override void Draw(GameTime gameTime)
        {
            ///////////////// PASS 1 - Depth Map ////////
            PostProcessManager.ChangeRenderMode(RenderMode.ShadowMapRender);

            oldBuffer = PoolGame.device.DepthStencilBuffer;
            lightpass = 0;
            for (int i = 0; i < LightManager.totalLights; ++i)
            {
                RenderShadowMap(i);
                SetDepthMapParameters(LightManager.lights[i]);
                shadowMapTIU[i].Use();
                World.scenario.DrawScene(gameTime);
                ++lightpass;
            }

            ///////////////// PASS 2 - PCF //////////////
            PostProcessManager.ChangeRenderMode(RenderMode.PCFShadowMapRender);
            RenderPCFShadowMap();
            SetPCFParameters(ref LightManager.lights);
            shadowTIU.Use();

            World.scenario.DrawScene(gameTime);

            shadowOcclussionTIU = shadowTIU;
        }

        public void RenderDEM()
        {
            PoolGame.device.RenderState.DepthBufferEnable = true;
            PoolGame.device.RenderState.DepthBufferWriteEnable = true;
            PoolGame.device.RenderState.DepthBufferFunction = CompareFunction.LessEqual;
            PoolGame.device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            PoolGame.device.SetRenderTarget(0, null);
            PoolGame.device.SetRenderTarget(1, null);
            PoolGame.device.SetRenderTarget(2, null);
        }

        public void RenderShadowMap(int lightindex)
        {
            PoolGame.device.RenderState.DepthBufferEnable = true;
            PoolGame.device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            //Render Shadow Map
            PoolGame.device.DepthStencilBuffer = stencilBuffer;
            PoolGame.device.SetRenderTarget(0, ShadowMapRT[lightindex]);
            PoolGame.device.Clear(Color.White);
        }

        public void RenderPCFShadowMap()
        {
            //Render PCF Shadow Map
            PoolGame.device.DepthStencilBuffer = oldBuffer;
            PoolGame.device.SetRenderTarget(0, ShadowRT);
            PoolGame.device.Clear(Color.White);

            PoolGame.device.RenderState.CullMode = CullMode.None;
        }

        public void RenderSoftShadow()
        {
            // Soft
            if (PostProcessManager.shadowBlurTech == ShadowBlurTechnnique.SoftShadow)
            {
                TextureInUse tmp = PostProcessManager.GetIntermediateTexture();
                PostProcessManager.SetBlurEffectParameters(0.5f / PoolGame.device.Viewport.Width, 0.0f, PostProcessManager.GBlurHEffect);
                PostProcessManager.SetBlurEffectParameters(0.0f, 0.5f / PoolGame.device.Viewport.Height, PostProcessManager.GBlurVEffect);

                //Gaussian Blur H
                PoolGame.device.SetRenderTarget(0, tmp.renderTarget);
                PostProcessManager.DrawQuad(shadowOcclussionTIU.renderTarget.GetTexture(), PostProcessManager.GBlurHEffect);

                //Guassian Blur V
                PoolGame.device.SetRenderTarget(0, shadowOcclussionTIU.renderTarget);
                PostProcessManager.DrawQuad(tmp.renderTarget.GetTexture(), PostProcessManager.GBlurVEffect);

                //PostProcessManager.SetBlurEffectParameters(0.5f / PoolGame.device.Viewport.Width, 0.0f, PostProcessManager.GBlurHEffect);
                //PostProcessManager.SetBlurEffectParameters(0.0f, 0.5f / PoolGame.device.Viewport.Height, PostProcessManager.GBlurVEffect);

                tmp.DontUse();
            }

            //Screen Space Shadow
            PostProcessManager.mainTIU.Use();
            PoolGame.device.SetRenderTarget(0, PostProcessManager.mainRT);
            
            if (World.motionblurType != MotionBlurType.None || World.dofType != DOFType.None)
            {
                PostProcessManager.depthTIU.Use(); PostProcessManager.velocityTIU.Use(); PostProcessManager.velocityLastFrameTIU.Use();
                PoolGame.device.SetRenderTarget(1, PostProcessManager.depthRT);
                PoolGame.device.SetRenderTarget(2, PostProcessManager.velocityRT);


                PostProcessManager.clearGBufferEffect.CurrentTechnique = PostProcessManager.clearGBufferEffect.Techniques["ClearGBufferTechnnique"];
                PostProcessManager.DrawQuad(PostProcessManager.whiteTexture, PostProcessManager.clearGBufferEffect);

                //PoolGame.device.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1.0f, 0);
                PoolGame.device.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            }
            else
            {
                PostProcessManager.depthTIU.DontUse(); PostProcessManager.velocityTIU.DontUse(); PostProcessManager.velocityLastFrameTIU.DontUse();

                {
                    PoolGame.device.SetRenderTarget(1, null);
                    PoolGame.device.SetRenderTarget(2, null);

                    //PoolGame.device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target | ClearOptions.Stencil, Color.CornflowerBlue, 1.0f, 0);
                    PoolGame.device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, Color.CornflowerBlue, 1.0f, 0);
                }
            }

            PoolGame.device.RenderState.DepthBufferEnable = true;
            PoolGame.device.RenderState.DepthBufferWriteEnable = true;
            PoolGame.device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            resultTIU = PostProcessManager.mainTIU;
        }

        public override void Dispose()
        {

            base.Dispose();
        }

        public override void PostDraw()
        {
            /////////////////////////////////////////////
            PoolGame.device.RenderState.StencilEnable = false;
            for (int i = 0; i < LightManager.totalLights; ++i)
                shadowMapTIU[i].DontUse();
            shadowTIU.DontUse();


        }

        public override void Pass3(GameTime gameTime)
        {
            ///////////////// PASS 3 - DEM //////////////
            if (World.EM == EnvironmentType.Dynamic)
            {
                PostProcessManager.ChangeRenderMode(RenderMode.DEMPass);
                RenderDEM();

                World.scenario.DrawDEMObjects(gameTime);
            }
            else if (World.EM == EnvironmentType.DualParaboloid)
            {
                PostProcessManager.ChangeRenderMode(RenderMode.DualParaboloidRenderMaps);
                RenderDEM();

                World.scenario.DrawDEMObjects(gameTime);
            }
            
        }

        public override void Pass4(GameTime gameTime)
        {
            ///////////////// PASS 4 - SSSM /////////////
            World.camera.ItemsDrawn = 0;
            PostProcessManager.ChangeRenderMode(RenderMode.ScreenSpaceSoftShadowRender);
            RenderSoftShadow();

            World.scenario.DrawScene(gameTime);

        }

        public override string GetDepthMapTechnique()
        {
            return "DepthMap";
        }

        public override void SetPCFParameters(ref List<Light> lights)
        {
            PostProcessManager.PCFShadowMap.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);

            PostProcessManager.PCFShadowMap.Parameters["LightViewProjs"].SetValue(LightManager.viewprojections);
            PostProcessManager.PCFShadowMap.Parameters["MaxDepths"].SetValue(LightManager.maxdepths);
            PostProcessManager.PCFShadowMap.Parameters["totalLights"].SetValue(LightManager.totalLights);
            for (int i = 0; i < LightManager.totalLights; ++i)
            {
                PostProcessManager.PCFShadowMap.Parameters["ShadowMap" + i].SetValue(ShadowMapRT[i].GetTexture());
            }

            PostProcessManager.PCFShadowMap.Parameters["PCFSamples"].SetValue(pcfSamples);
            PostProcessManager.PCFShadowMap.Parameters["depthBias"].SetValue(LightManager.depthbias);
        }

        public override void SetDepthMapParameters(Light light)
        {
            PostProcessManager.DepthEffect.Parameters["ViewProj"].SetValue(light.LightViewProjection);
            PostProcessManager.DepthEffect.Parameters["MaxDepth"].SetValue(light.LightFarPlane);
        }

    }
}
