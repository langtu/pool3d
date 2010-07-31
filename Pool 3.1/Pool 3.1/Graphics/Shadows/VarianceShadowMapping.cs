using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TextureInUse = XNA_PoolGame.Graphics.PostProcessManager.TextureInUse;

namespace XNA_PoolGame.Graphics.Shadows
{
    /// <summary>
    /// Variance Shadow Mapping
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
            RenderVSMShadowMap();
            SetPCFParameters(ref LightManager.lights);
            shadowTIU.Use();

            World.scenario.DrawScene(gameTime);

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
            
        }

        public override string GetDepthMapTechnique()
        {
            return "DepthMap_VSM";
        }

        public override void SetPCFParameters(ref List<Light> lights)
        {
            PostProcessManager.VSMEffect.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);
            PostProcessManager.VSMEffect.Parameters["TexProj"].SetValue(textproj);
            PostProcessManager.VSMEffect.Parameters["LightViews"].SetValue(LightManager.views);
            PostProcessManager.VSMEffect.Parameters["Projection"].SetValue(lights[0].LightProjection);

            PostProcessManager.VSMEffect.Parameters["MaxDepths"].SetValue(LightManager.maxdepths);
            PostProcessManager.VSMEffect.Parameters["totalLights"].SetValue(LightManager.totalLights);
            for (int i = 0; i < LightManager.totalLights; ++i)
            {
                PostProcessManager.VSMEffect.Parameters["ShadowMap" + i].SetValue(ShadowMapRT[i].GetTexture());
            }

            //PostProcessManager.vsmEffect.Parameters["PCFSamples"].SetValue(pcfSamples);
            PostProcessManager.VSMEffect.Parameters["depthBias"].SetValue(LightManager.depthbias);
        }

        public override void SetDepthMapParameters(Light light)
        {
            PostProcessManager.DepthEffect.Parameters["ViewProj"].SetValue(light.LightViewProjection);
            PostProcessManager.DepthEffect.Parameters["MaxDepth"].SetValue(light.LightFarPlane);
        }
    }
}
