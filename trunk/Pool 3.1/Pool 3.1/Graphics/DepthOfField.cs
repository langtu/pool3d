using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using XNA_PoolGame.Cameras;
using Microsoft.Xna.Framework;
using TextureInUse = XNA_PoolGame.Graphics.PostProcessManager.TextureInUse;

namespace XNA_PoolGame.Graphics
{
    public class DepthOfField
    {
        public float focalDistance, focalWidth;


        protected float blurSigma;
        protected float attenuation;
        protected Vector3[] frustumCornersWS = new Vector3[8];
        protected Vector3[] frustumCornersVS = new Vector3[8];
        protected Vector3[] farFrustumCornersVS = new Vector3[4];
        protected RenderTarget2D[] singleSourceArray = new RenderTarget2D[1];
        protected RenderTarget2D[] doubleSourceArray = new RenderTarget2D[2];
        protected RenderTarget2D[] tripleSourceArray = new RenderTarget2D[3];

        public DepthOfField()
        {
            blurSigma = 2.5f;
            attenuation = 1.0f;
        }

        protected void GenerateDownscaleTargetSW(RenderTarget2D source, RenderTarget2D result)
        {
            string techniqueName = "Downscale4";

            PresentationParameters pp = PoolGame.device.PresentationParameters;

            TextureInUse downscale1 = PostProcessManager.GetIntermediateTexture(source.Width / 4, source.Height / 4, source.Format, pp.MultiSampleType, pp.MultiSampleQuality);
            PostProcessManager.scalingEffect.CurrentTechnique = PostProcessManager.scalingEffect.Techniques[techniqueName];
            PostProcess(source, downscale1.renderTarget, PostProcessManager.scalingEffect);

            PostProcessManager.scalingEffect.CurrentTechnique = PostProcessManager.scalingEffect.Techniques[techniqueName];
            PostProcess(downscale1.renderTarget, result, PostProcessManager.scalingEffect);
            downscale1.DontUse();
        }

        protected void GenerateDownscaleTargetHW(RenderTarget2D source, RenderTarget2D result)
        {
            TextureInUse downscale1 = PostProcessManager.GetIntermediateTexture(source.Width / 2, source.Height / 2, source.Format);
            PostProcessManager.scalingEffect.CurrentTechnique = PostProcessManager.scalingEffect.Techniques["ScaleHW"];
            PostProcess(source, downscale1.renderTarget, PostProcessManager.scalingEffect);

            TextureInUse downscale2 = PostProcessManager.GetIntermediateTexture(source.Width / 2, source.Height / 2, source.Format);
            PostProcessManager.scalingEffect.CurrentTechnique = PostProcessManager.scalingEffect.Techniques["ScaleHW"];
            PostProcess(downscale1.renderTarget, downscale2.renderTarget, PostProcessManager.scalingEffect);
            downscale1.DontUse();

            TextureInUse downscale3 = PostProcessManager.GetIntermediateTexture(source.Width / 2, source.Height / 2, source.Format);
            PostProcessManager.scalingEffect.CurrentTechnique = PostProcessManager.scalingEffect.Techniques["ScaleHW"];
            PostProcess(downscale2.renderTarget, downscale3.renderTarget, PostProcessManager.scalingEffect);
            downscale2.DontUse();

            PostProcessManager.scalingEffect.CurrentTechnique = PostProcessManager.scalingEffect.Techniques["ScaleHW"];
            PostProcess(downscale3.renderTarget, result, PostProcessManager.scalingEffect);
            downscale3.DontUse();
        }

        protected void PostProcess(RenderTarget2D source, RenderTarget2D result, Effect effect)
        {
            RenderTarget2D[] sources = singleSourceArray;
            sources[0] = source;
            PostProcess(sources, result, effect);
        }

        public void Blur(RenderTarget2D source,
                            RenderTarget2D result)
        {
            TextureInUse blurH = PostProcessManager.GetIntermediateTexture(source.Width,
                                                                source.Height,
                                                                source.Format,
                                                                source.MultiSampleType,
                                                                source.MultiSampleQuality);

            string baseTechniqueName = "GaussianBlur";

            // Do horizontal pass first
            PostProcessManager.blurEffect.CurrentTechnique = PostProcessManager.blurEffect.Techniques[baseTechniqueName + "H"];
            PostProcessManager.blurEffect.Parameters["g_fSigma"].SetValue(blurSigma);

            PostProcess(source, blurH.renderTarget, PostProcessManager.blurEffect);

            // Now the vertical pass 
            PostProcessManager.blurEffect.CurrentTechnique = PostProcessManager.blurEffect.Techniques[baseTechniqueName + "V"];

            PostProcess(blurH.renderTarget, result, PostProcessManager.blurEffect);

            blurH.DontUse();
        }

        /// <summary>
        /// Applies a blur to the specified render target, using a depth texture
        /// to prevent pixels from blurring with pixels that are "in front"
        /// </summary>
        /// <param name="source">The render target to use as the source</param>
        /// <param name="result">The render target to use as the result</param>
        /// <param name="depthTexture">The depth texture to use</param>
        /// <param name="sigma">The standard deviation used for gaussian weights</param>
        public void DepthBlur(RenderTarget2D source,
                            RenderTarget2D result,
                            RenderTarget2D depthTexture)
        {
            TextureInUse blurH = PostProcessManager.GetIntermediateTexture(source.Width,
                                                                source.Height,
                                                                source.Format,
                                                                source.MultiSampleType,
                                                                source.MultiSampleQuality);

            string baseTechniqueName = "GaussianDepthBlur";

            // Do horizontal pass first
            PostProcessManager.blurEffect.CurrentTechnique = PostProcessManager.blurEffect.Techniques[baseTechniqueName + "H"];
            PostProcessManager.blurEffect.Parameters["g_fSigma"].SetValue(blurSigma);

            RenderTarget2D[] sources = doubleSourceArray;
            sources[0] = source;
            sources[1] = depthTexture;
            PostProcess(sources, blurH.renderTarget, PostProcessManager.blurEffect);

            // Now the vertical pass 
            PostProcessManager.blurEffect.CurrentTechnique = PostProcessManager.blurEffect.Techniques[baseTechniqueName + "V"];

            sources[0] = blurH.renderTarget;
            sources[1] = depthTexture;
            PostProcess(blurH.renderTarget, result, PostProcessManager.blurEffect);

            blurH.DontUse();
        }


        public void DOF(RenderTarget2D source, 
                        RenderTarget2D result, 
                        RenderTarget2D depthTexture,
                        DOFType dofType)
		{
            if (dofType == DOFType.DiscBlur)
            {
                // Scale tap offsets based on render target size
                float dx = 0.25f / (float)source.Width;
                float dy = 0.25f / (float)source.Height;

                // Generate the texture coordinate offsets for our disc
                Vector2[] discOffsets = new Vector2[12];
                discOffsets[0] = new Vector2(-0.326212f * dx, -0.40581f * dy);
                discOffsets[1] = new Vector2(-0.840144f * dx, -0.07358f * dy);
                discOffsets[2] = new Vector2(-0.840144f * dx, 0.457137f * dy);
                discOffsets[3] = new Vector2(-0.203345f * dx, 0.620716f * dy);
                discOffsets[4] = new Vector2(0.96234f * dx, -0.194983f * dy);
                discOffsets[5] = new Vector2(0.473434f * dx, -0.480026f * dy);
                discOffsets[6] = new Vector2(0.519456f * dx, 0.767022f * dy);
                discOffsets[7] = new Vector2(0.185461f * dx, -0.893124f * dy);
                discOffsets[8] = new Vector2(0.507431f * dx, 0.064425f * dy);
                discOffsets[9] = new Vector2(0.89642f * dx, 0.412458f * dy);
                discOffsets[10] = new Vector2(-0.32194f * dx, -0.932615f * dy);
                discOffsets[11] = new Vector2(-0.791559f * dx, -0.59771f * dy);

                // Set array of offsets
                PostProcessManager.DOFEffect.Parameters["g_vFilterTaps"].SetValue(discOffsets);

                PostProcessManager.DOFEffect.CurrentTechnique = PostProcessManager.DOFEffect.Techniques["DOFDiscBlur"];

                PostProcessManager.DOFEffect.Parameters["g_fFocalDistance"].SetValue(focalDistance);
                PostProcessManager.DOFEffect.Parameters["g_fFocalWidth"].SetValue(focalWidth / 2.0f);
                PostProcessManager.DOFEffect.Parameters["g_fFarClip"].SetValue(World.camera.FarPlane);
                PostProcessManager.DOFEffect.Parameters["g_fAttenuation"].SetValue(attenuation);

                RenderTarget2D[] sources = doubleSourceArray;
                sources[0] = source;
                sources[1] = depthTexture;

                PostProcess(sources, result, PostProcessManager.DOFEffect);
            }
            else
            {
                PresentationParameters pp = PoolGame.device.PresentationParameters;
                // Downscale to 1/16th size and blur
                TextureInUse downscaleTexture = PostProcessManager.GetIntermediateTexture(source.Width / 4, source.Height / 4, SurfaceFormat.Color, pp.MultiSampleType, pp.MultiSampleQuality);
                GenerateDownscaleTargetSW(source, downscaleTexture.renderTarget);

                // For the "dumb" DOF type just do a blur, otherwise use a special blur
                // that takes depth into account
                if (dofType == DOFType.BlurBuffer)
                    Blur(downscaleTexture.renderTarget, downscaleTexture.renderTarget);
                else if (dofType == DOFType.BlurBufferDepthCorrection)
                    DepthBlur(downscaleTexture.renderTarget, downscaleTexture.renderTarget, depthTexture);


                PostProcessManager.DOFEffect.CurrentTechnique = PostProcessManager.DOFEffect.Techniques["DOFBlurBuffer"];

                PostProcessManager.DOFEffect.Parameters["g_fFocalDistance"].SetValue(focalDistance);
                PostProcessManager.DOFEffect.Parameters["g_fFocalWidth"].SetValue(focalWidth / 2.0f);
                PostProcessManager.DOFEffect.Parameters["g_fFarClip"].SetValue(World.camera.FarPlane);
                PostProcessManager.DOFEffect.Parameters["g_fAttenuation"].SetValue(attenuation);

                RenderTarget2D[] sources = tripleSourceArray;
                sources[0] = source;
                sources[1] = downscaleTexture.renderTarget;
                sources[2] = depthTexture;

                PostProcess(sources, result, PostProcessManager.DOFEffect);

                downscaleTexture.DontUse();
            }

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

            // Begin effect
            effect.Begin(SaveStateMode.None);
            effect.CurrentTechnique.Passes[0].Begin();

            // Draw the quad
            PostProcessManager.quad.Draw(PoolGame.device);

            // We're done
            effect.CurrentTechnique.Passes[0].End();
            effect.End();
        }
    }
}
