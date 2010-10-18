#region Using Statments
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
    /// Variance Shadow Mapping.
    /// </summary>
    public class VarianceShadowMapping : Shadow
    {
        Matrix textproj;
        PresentationParameters pp;

        /// <summary>
        /// Creates a new instance of VarianceShadowMapping.
        /// </summary>
        public VarianceShadowMapping()
        {
            shadowMapSize = World.shadowMapSize;

            pp = PoolGame.device.PresentationParameters;

            ShadowMapRT = new RenderTarget2D[2];
            ShadowMapRT[0] = new RenderTarget2D(PoolGame.device, shadowMapSize, shadowMapSize, 1, SurfaceFormat.Rg32);
            ShadowMapRT[1] = new RenderTarget2D(PoolGame.device, shadowMapSize, shadowMapSize, 1, SurfaceFormat.Rg32);

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

            float text_offset = 0.5f + (0.5f / (float)shadowMapSize);
            textproj = new Matrix(0.5f, 0.0f, 0.0f, 0.0f,
                                  0.0f, -0.5f, 0.0f, 0.0f,
                                  0.0f, 0.0f, 1.0f, 0.0f,
                                  text_offset, text_offset, 1.0f, 1.0f);
        }
        public override void Draw(GameTime gameTime)
        {
            ///////////////// PASS 1 - Depth Map ////////
            PostProcessManager.ChangeRenderMode(RenderPassMode.ShadowMapRender);

            oldBuffer = PoolGame.device.DepthStencilBuffer;
            lightpass = 0;
            for (int i = 0; i < LightManager.totalLights; ++i)
            {
                RenderShadowMap(i);
                shadowMapTIU[i].Use();
                if (World.camera.Frustum.Contains(LightManager.lights[i].Frustum) == ContainmentType.Disjoint)
                {
                    ++lightpass;
                    continue;
                }

                SetDepthMapParameters(LightManager.lights[i]);
                
                World.scenario.DrawScene(gameTime);
                ++lightpass;
            }

            ///////////////// PASS 2 - PCF //////////////
            PostProcessManager.ChangeRenderMode(RenderPassMode.PCFShadowMapRender);
            RenderVSMShadowMap();
            SetPCFParameters(ref LightManager.lights);
            shadowTIU.Use();

            World.scenario.DrawScene(gameTime);

            // Soft
            if (PostProcessManager.shadowBlurTech == ShadowBlurTechnnique.SoftShadow)
            {
                TextureInUse tmp = PostProcessManager.GetIntermediateTexture();
                PostProcessManager.SetBlurEffectParameters(0.5f / (float)PoolGame.device.Viewport.Width, 0.0f, PostProcessManager.GBlurHEffect);
                PostProcessManager.SetBlurEffectParameters(0.0f, 0.5f / (float)PoolGame.device.Viewport.Height, PostProcessManager.GBlurVEffect);

                //Gaussian Blur H
                PoolGame.device.SetRenderTarget(0, tmp.renderTarget);
                PostProcessManager.DrawQuad(shadowTIU.renderTarget.GetTexture(), PostProcessManager.GBlurHEffect);

                //Guassian Blur V
                PoolGame.device.SetRenderTarget(0, shadowTIU.renderTarget);
                PostProcessManager.DrawQuad(tmp.renderTarget.GetTexture(), PostProcessManager.GBlurVEffect);

                PostProcessManager.SetBlurEffectParameters(0.5f / (float)PoolGame.device.Viewport.Width, 0.0f, PostProcessManager.GBlurHEffect);
                PostProcessManager.SetBlurEffectParameters(0.0f, 0.5f / (float)PoolGame.device.Viewport.Height, PostProcessManager.GBlurVEffect);

                tmp.DontUse();
                shadowTIU.Use();
            }
            shadowOcclussionTIU = shadowTIU;
        }
        
        public void RenderVSMShadowMap()
        {
            //Render VSM Shadow Map
            PoolGame.device.DepthStencilBuffer = oldBuffer;
            PoolGame.device.SetRenderTarget(0, ShadowRT);
            PoolGame.device.Clear(Color.White);

            PoolGame.device.RenderState.CullMode = CullMode.None;
        }

        public void RenderShadowMap(int lightindex)
        {
            PoolGame.device.RenderState.DepthBufferEnable = true;
            PoolGame.device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            //Render Shadow Map

            // UPGRADE (MULTIPLE LIGHTS)
            PoolGame.device.DepthStencilBuffer = stencilBuffer;
            PoolGame.device.SetRenderTarget(0, ShadowMapRT[lightindex]);
            PoolGame.device.Clear(Color.White);
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
            
        }

        public override void Pass4(GameTime gameTime)
        {
            ///////////////// PASS 4 - SSSM /////////////
            World.camera.ItemsDrawn = 0;
            PostProcessManager.ChangeRenderMode(RenderPassMode.ScreenSpaceSoftShadowRender);
            RenderSoftShadow();

            World.scenario.DrawScene(gameTime);
        }

        private void RenderSoftShadow()
        {
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

        public override string GetDepthMapTechnique()
        {
            return "DepthMap_VSM";
        }

        public override void SetPCFParameters(ref List<Light> lights)
        {
            PostProcessManager.VSMEffect.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);
            
            PostProcessManager.VSMEffect.Parameters["LightViewProjs"].SetValue(LightManager.viewprojections);
            
            PostProcessManager.VSMEffect.Parameters["MaxDepths"].SetValue(LightManager.maxdepths);
            PostProcessManager.VSMEffect.Parameters["totalLights"].SetValue(LightManager.totalLights);
            for (int i = 0; i < LightManager.totalLights; ++i)
            {
                PostProcessManager.VSMEffect.Parameters["ShadowMap" + i].SetValue(ShadowMapRT[i].GetTexture());
            }

            PostProcessManager.VSMEffect.Parameters["depthBias"].SetValue(LightManager.depthbias);
        }

        public override void SetDepthMapParameters(Light light)
        {
            PostProcessManager.DepthEffect.Parameters["ViewProj"].SetValue(light.LightViewProjection);
            PostProcessManager.DepthEffect.Parameters["MaxDepth"].SetValue(light.LightFarPlane);
        }
    }
}
