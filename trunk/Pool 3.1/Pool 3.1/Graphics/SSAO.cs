using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using TextureInUse = XNA_PoolGame.Graphics.PostProcessManager.TextureInUse;
using Microsoft.Xna.Framework;

namespace XNA_PoolGame.Graphics
{
    /// <summary>
    /// Screen Space Ambient Occlussion
    /// </summary>
    public class SSAO : IDisposable
    {
        public float intensity;
        public float scale;
        public float randomsize;
        public float radius;
        public float bias;
        public float screensize;
        public Texture2D noiseTexture;
        public Effect effect;
        public Effect combinefinal;
        public TextureInUse ssaoTIU, viewTIU, normalTIU, binaryTIU, resultTIU;
        public Vector2 halfPixel;
        private PresentationParameters pp;
        public bool blurIt;
        /// <summary>
        /// Create a new instance of SSAO.
        /// </summary>
        public SSAO()
        {
            noiseTexture = PoolGame.content.Load<Texture2D>("Textures\\noise");
            effect = PoolGame.content.Load<Effect>("Effects\\SSAO");
            combinefinal = PoolGame.content.Load<Effect>("Effects\\Multiply");
            intensity = 8.0f;//4.0f;//2.0f;
            scale = 1.5f;//1.0f;
            randomsize = (float)(noiseTexture.Width * noiseTexture.Height);
            screensize = (float)(PoolGame.Width * PoolGame.Height);
            radius = 15.5f;//radius = 15.5f;
            bias = 0.01f;
            halfPixel.X = 0.5f / (float)PoolGame.Width;
            halfPixel.Y = 0.5f / (float)PoolGame.Height;
            blurIt = true;

            effect.Parameters["halfPixel"].SetValue(halfPixel);
            effect.Parameters["g_screen_size"].SetValue(screensize);
            effect.Parameters["random_size"].SetValue(randomsize);


            effect.Parameters["g_scale"].SetValue(scale);
            effect.Parameters["g_intensity"].SetValue(intensity);
            effect.Parameters["g_bias"].SetValue(bias);
            effect.Parameters["g_sample_rad"].SetValue(radius);

            effect.Parameters["RandomMap"].SetValue(noiseTexture);
            pp = PoolGame.device.PresentationParameters;
        }

        public void Draw(TextureInUse source)
        {
            ssaoTIU = PostProcessManager.GetIntermediateTexture();
            PoolGame.device.SetRenderTarget(0, ssaoTIU.renderTarget);
            PoolGame.device.SetRenderTarget(1, null);
            PoolGame.device.SetRenderTarget(2, null);

            PoolGame.device.Clear(Color.Black);

            effect.Parameters["NormalMap"].SetValue(normalTIU.renderTarget.GetTexture());
            if (World.shadingTech == ShadingTechnnique.Deferred)
            {
                effect.Parameters["PositionMap"].SetValue(PostProcessManager.depthTIU.renderTarget.GetTexture());
                effect.Parameters["InvertViewProjection"].SetValue(World.camera.InvViewProjection);
            }
            else
                effect.Parameters["PositionMap"].SetValue(viewTIU.renderTarget.GetTexture());

            effect.Parameters["calculatePosition"].SetValue(World.shadingTech == ShadingTechnnique.Deferred);

            effect.CommitChanges();

            effect.Begin();
            effect.Techniques[0].Passes[0].Begin();
            PostProcessManager.quad.Draw(PoolGame.device);
            effect.Techniques[0].Passes[0].End();
            effect.End();

            resultTIU = ssaoTIU;
            if (blurIt)
            {
                PostProcessManager.SetBlurEffectParameters(0.5f / PoolGame.device.Viewport.Width, 0.0f, PostProcessManager.GBlurHEffect);
                PostProcessManager.SetBlurEffectParameters(0.0f, 0.5f / PoolGame.device.Viewport.Height, PostProcessManager.GBlurVEffect);

                TextureInUse tmp1 = PostProcessManager.GetIntermediateTexture();
                //Gaussian Blur H
                PoolGame.device.SetRenderTarget(0, tmp1.renderTarget);
                PostProcessManager.DrawQuad(resultTIU.renderTarget.GetTexture(), PostProcessManager.GBlurHEffect);

                resultTIU.DontUse();
                //Guassian Blur V
                PoolGame.device.SetRenderTarget(0, ssaoTIU.renderTarget);
                PostProcessManager.DrawQuad(tmp1.renderTarget.GetTexture(), PostProcessManager.GBlurVEffect);

                PostProcessManager.SetBlurEffectParameters(0.5f / PoolGame.device.Viewport.Width, 0.0f, PostProcessManager.GBlurHEffect);
                PostProcessManager.SetBlurEffectParameters(0.0f, 0.5f / PoolGame.device.Viewport.Height, PostProcessManager.GBlurVEffect);
                tmp1.DontUse();
                
                ssaoTIU.Use();
                resultTIU = ssaoTIU;
            }
            
            binaryTIU = PostProcessManager.GetIntermediateTexture();

            PoolGame.device.SetRenderTarget(0, binaryTIU.renderTarget);
            PoolGame.device.Clear(Color.Black);
            
            combinefinal.Parameters["sceneMap"].SetValue(source.renderTarget.GetTexture());
            combinefinal.Parameters["SSAOMap"].SetValue(resultTIU.renderTarget.GetTexture());

            combinefinal.Begin();
            combinefinal.Techniques[0].Passes[0].Begin();
            PostProcessManager.quad.Draw(PoolGame.device);
            combinefinal.Techniques[0].Passes[0].End();
            combinefinal.End();

            resultTIU = binaryTIU;
            ssaoTIU.DontUse();
        }

        #region Miembros de IDisposable

        public void Dispose()
        {
            if (noiseTexture != null) noiseTexture.Dispose();
            noiseTexture = null;

            if (effect != null) effect.Dispose();
            effect = null;
        }

        #endregion

        public void FreeStuff()
        {
            if (viewTIU != null) viewTIU.DontUse();
            if (normalTIU != null) normalTIU.DontUse();
            if (ssaoTIU != null) ssaoTIU.DontUse();
            if (binaryTIU != null) binaryTIU.DontUse();
        }
    }
}
