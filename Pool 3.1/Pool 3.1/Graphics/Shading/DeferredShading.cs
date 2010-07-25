using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Graphics.Shadows;
using Microsoft.Xna.Framework.Graphics;
using TextureInUse = XNA_PoolGame.Graphics.PostProcessManager.TextureInUse;
namespace XNA_PoolGame.Graphics.Shading
{
    public class DeferredShading : BaseShading
    {
        private PresentationParameters pp;
        public TextureInUse diffuseColorTIU, normalTIU, lightTIU, depthTIU, combineTIU;
        
        public DeferredShading()
        {
            halfPixel.X = 0.5f / (float)PoolGame.device.PresentationParameters.BackBufferWidth;
            halfPixel.Y = 0.5f / (float)PoolGame.device.PresentationParameters.BackBufferHeight;

            pp = PoolGame.device.PresentationParameters;
            stencilBuffer = new DepthStencilBuffer(PoolGame.device, PoolGame.Width, PoolGame.Height, PoolGame.device.DepthStencilBuffer.Format, pp.MultiSampleType, pp.MultiSampleQuality);
        }
        public override void Draw(GameTime gameTime)
        {
            SetGBuffer();
            ClearGBuffer();

            PostProcessManager.ChangeRenderMode(RenderMode.RenderGBuffer);
            World.scenario.DrawScene(gameTime);
            
            ResolveGBuffer();

            shadows.Draw(gameTime);
            
            DepthStencilBuffer oldBuffer = PoolGame.device.DepthStencilBuffer;
            PoolGame.device.DepthStencilBuffer = stencilBuffer;
            
            DrawLights(gameTime);
            CombineFinal();
            PoolGame.device.DepthStencilBuffer = oldBuffer;

            if (World.doSSAO)
            {
                PostProcessManager.ssao.normalTIU = normalTIU;
            }
        }

        private void DrawLights(GameTime gameTime)
        {
            lightTIU = PostProcessManager.GetIntermediateTexture(PoolGame.Width, PoolGame.Height, SurfaceFormat.HalfVector4, pp.MultiSampleType, pp.MultiSampleQuality);


            PoolGame.device.SetRenderTarget(0, lightTIU.renderTarget);

            //clear all components to 0
            PoolGame.device.Clear(Color.TransparentBlack);
            PoolGame.device.RenderState.AlphaBlendEnable = true;
            //use additive blending, and make sure the blending factors are as we need them
            PoolGame.device.RenderState.AlphaBlendOperation = BlendFunction.Add;
            PoolGame.device.RenderState.SourceBlend = Blend.One;
            PoolGame.device.RenderState.DestinationBlend = Blend.One;
            //use the same operation on the alpha channel
            PoolGame.device.RenderState.SeparateAlphaBlendEnabled = false;
            PoolGame.device.RenderState.DepthBufferEnable = false;

            //Color gris = new Color(128, 128, 128);
            Color gris = new Color(255, 255, 255);
            for (int i = 0; i < LightManager.totalLights; ++i)
            {
                //Vector3 dir = (LightManager.lights[i].LookAt - LightManager.lights[i].Position);
                Vector3 dir = LightManager.lights[i].Position;
                //dir.Normalize();
                DrawDirectionalLight(dir, gris);
            }

            //DrawDirectionalLight(new Vector3(1, -1, 1), Color.White);
            //DrawDirectionalLight(new Vector3(-1, 1, -1), Color.White);
            //DrawDirectionalLight(Vector3.Up, gris);
            //DrawDirectionalLight(Vector3.Left, gris);
            //DrawDirectionalLight(Vector3.Right, gris);
            //DrawDirectionalLight(Vector3.Forward, gris);
            //DrawDirectionalLight(Vector3.Backward, gris);
        }
        private void DrawDirectionalLight(Vector3 lightDirection, Color color)
        {
            //set all parameters
            PostProcessManager.directionalLightEffect.Parameters["colorMap"].SetValue(diffuseColorTIU.renderTarget.GetTexture());
            PostProcessManager.directionalLightEffect.Parameters["normalMap"].SetValue(normalTIU.renderTarget.GetTexture());
            PostProcessManager.directionalLightEffect.Parameters["depthMap"].SetValue(PostProcessManager.depthTIU.renderTarget.GetTexture());
            PostProcessManager.directionalLightEffect.Parameters["lightDirection"].SetValue(lightDirection);
            PostProcessManager.directionalLightEffect.Parameters["Color"].SetValue(color.ToVector3());
            PostProcessManager.directionalLightEffect.Parameters["cameraPosition"].SetValue(World.camera.CameraPosition);
            PostProcessManager.directionalLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(World.camera.ViewProjection));
            PostProcessManager.directionalLightEffect.Parameters["halfPixel"].SetValue(halfPixel);
            
            PostProcessManager.directionalLightEffect.Begin();
            PostProcessManager.directionalLightEffect.Techniques[0].Passes[0].Begin();
            //draw a full-screen quad
            PostProcessManager.quad.Draw(PoolGame.device);
            PostProcessManager.directionalLightEffect.Techniques[0].Passes[0].End();
            PostProcessManager.directionalLightEffect.End();
        }
        private void SetGBuffer()
        {
            diffuseColorTIU = PostProcessManager.GetIntermediateTexture(PoolGame.Width, PoolGame.Height, SurfaceFormat.HalfVector4, pp.MultiSampleType, pp.MultiSampleQuality);
            normalTIU = PostProcessManager.GetIntermediateTexture(PoolGame.Width, PoolGame.Height, SurfaceFormat.HalfVector4, pp.MultiSampleType, pp.MultiSampleQuality);
            PostProcessManager.depthTIU.Use();
            PoolGame.device.SetRenderTarget(0, diffuseColorTIU.renderTarget);
            PoolGame.device.SetRenderTarget(1, normalTIU.renderTarget);
            PoolGame.device.SetRenderTarget(2, PostProcessManager.depthRT);
        }

        private void ClearGBuffer()
        {
            
            //PostProcessManager.DrawQuad(PostProcessManager.whiteTexture, PostProcessManager.clearGBuffer_DefEffect);

            PostProcessManager.clearGBuffer_DefEffect.Begin();
            PostProcessManager.clearGBuffer_DefEffect.Techniques[0].Passes[0].Begin();
            PostProcessManager.quad.Draw(PoolGame.device);
            PostProcessManager.clearGBuffer_DefEffect.Techniques[0].Passes[0].End();
            PostProcessManager.clearGBuffer_DefEffect.End();

            PoolGame.device.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1.0f, 0);

            PoolGame.device.RenderState.DepthBufferEnable = true;
            PoolGame.device.RenderState.DepthBufferWriteEnable = true;
            PoolGame.device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            PoolGame.device.RenderState.AlphaBlendEnable = false;
        }

        private void CombineFinal()
        {
            combineTIU = PostProcessManager.GetIntermediateTexture();
            PoolGame.device.RenderState.AlphaBlendEnable = false;
            PoolGame.device.RenderState.DepthBufferEnable = false;
            PoolGame.device.RenderState.DepthBufferWriteEnable = false;
            PoolGame.device.RenderState.StencilEnable = false;

            PoolGame.device.SetRenderTarget(0, combineTIU.renderTarget);

            //set the effect parameters
            PostProcessManager.combineFinal_DefEffect.Parameters["colorMap"].SetValue(diffuseColorTIU.renderTarget.GetTexture());
            PostProcessManager.combineFinal_DefEffect.Parameters["lightMap"].SetValue(lightTIU.renderTarget.GetTexture());
            PostProcessManager.combineFinal_DefEffect.Parameters["halfPixel"].SetValue(halfPixel);
            PostProcessManager.combineFinal_DefEffect.Parameters["shadowOcclusion"].SetValue(shadows.ShadowRT.GetTexture());
            //PostProcessManager.combineFinal_DefEffect.Parameters["shadowOcclusion"].SetValue(PostProcessManager.whiteTexture);

            PostProcessManager.combineFinal_DefEffect.Begin();
            PostProcessManager.combineFinal_DefEffect.Techniques[0].Passes[0].Begin();

            //render a full-screen quad
            PostProcessManager.quad.Draw(PoolGame.device);

            PostProcessManager.combineFinal_DefEffect.Techniques[0].Passes[0].End();
            PostProcessManager.combineFinal_DefEffect.End();

            //PostProcessManager.DrawQuad(diffuseColorTIU.renderTarget.GetTexture(), PostProcessManager.combineFinal_DefEffect);
            normalTIU.DontUse();
            lightTIU.DontUse();
            diffuseColorTIU.DontUse();


            resultTIU = combineTIU;
        }

        private void ResolveGBuffer()
        {
            PoolGame.device.SetRenderTarget(0, null);
            PoolGame.device.SetRenderTarget(1, null);
            PoolGame.device.SetRenderTarget(2, null);
        }

        public override void DrawTextured(GameTime gameTime)
        {
            
        }

        public override string GetBasicRenderTechnique()
        {
            return "Technique1";
        }
    }
}
