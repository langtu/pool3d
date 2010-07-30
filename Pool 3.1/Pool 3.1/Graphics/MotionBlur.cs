using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using XNA_PoolGame.Cameras;
using Microsoft.Xna.Framework;

namespace XNA_PoolGame.Graphics
{
    public class MotionBlur
    {
        public float blurScale;
        protected Vector3[] frustumCornersWS = new Vector3[8];
        protected Vector3[] frustumCornersVS = new Vector3[8];
        protected Vector3[] farFrustumCornersVS = new Vector3[4];
        protected RenderTarget2D[] singleSourceArray = new RenderTarget2D[1];
        protected RenderTarget2D[] doubleSourceArray = new RenderTarget2D[2];
        protected RenderTarget2D[] tripleSourceArray = new RenderTarget2D[3];
        
        public MotionBlur()
        {
            
            this.blurScale = 1.0f;
        }

        public void DoMotionBlur(RenderTarget2D source,
                                RenderTarget2D result,
                                RenderTarget2D depthTexture,
                                RenderTarget2D velocityTexture,
                                RenderTarget2D prevVelocityTexture,
                                Camera camera,
                                Matrix prevViewProj,
                                MotionBlurType mbType)
        {
            // Get corners of the main camera's bounding frustum
            Matrix viewMatrix = World.camera.View;
            camera.FrustumCulling.GetCorners(frustumCornersWS);

            Vector3.Transform(frustumCornersWS, ref viewMatrix, frustumCornersVS);
            for (int i = 0; i < 4; i++)
                farFrustumCornersVS[i] = frustumCornersVS[i + 4];

            // Set the technique according to the motion blur type
            PostProcessManager.motionblurEffect.CurrentTechnique = PostProcessManager.motionblurEffect.Techniques[mbType.ToString()];

            PostProcessManager.motionblurEffect.Parameters["g_fBlurAmount"].SetValue(this.blurScale);
            PostProcessManager.motionblurEffect.Parameters["g_matInvView"].SetValue(camera.InvView);
            PostProcessManager.motionblurEffect.Parameters["g_matLastViewProj"].SetValue(prevViewProj);

            RenderTarget2D[] sources;
            if (mbType == MotionBlurType.DepthBuffer4Samples
                || mbType == MotionBlurType.DepthBuffer8Samples
                || mbType == MotionBlurType.DepthBuffer12Samples)
            {
                sources = doubleSourceArray;
                sources[0] = source;
                sources[1] = depthTexture;
            }
            else
            {
                sources = tripleSourceArray;
                sources[0] = source;
                sources[1] = velocityTexture;
                sources[2] = prevVelocityTexture;
            }

            PostProcess(sources, result, PostProcessManager.motionblurEffect);
        }

        protected void PostProcess(RenderTarget2D[] sources, RenderTarget2D result, Effect effect)
        {
            PoolGame.device.SetRenderTarget(0, result);
            PoolGame.device.Clear(Color.Black);

            for (int i = 0; i < sources.Length; i++)
                effect.Parameters["SourceTexture" + Convert.ToString(i)].SetValue(sources[i].GetTexture());
            effect.Parameters["g_vSourceDimensions"].SetValue(new Vector2(sources[0].Width, sources[0].Height));
            if (result == null)
                effect.Parameters["g_vDestinationDimensions"].SetValue(new Vector2(PoolGame.device.PresentationParameters.BackBufferWidth, PoolGame.device.PresentationParameters.BackBufferHeight));
            else
                effect.Parameters["g_vDestinationDimensions"].SetValue(new Vector2(result.Width, result.Height));
            effect.Parameters["g_vFrustumCornersVS"].SetValue(farFrustumCornersVS);

            
            // Begin effect
            effect.Begin(SaveStateMode.SaveState);
            effect.CurrentTechnique.Passes[0].Begin();

            // Draw the quad
            PostProcessManager.quad.Draw(PoolGame.device);
            

            // We're done
            effect.CurrentTechnique.Passes[0].End();
            effect.End();
        }
    }
}
