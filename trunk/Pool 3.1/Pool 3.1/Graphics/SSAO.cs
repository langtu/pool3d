using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using TextureInUse = XNA_PoolGame.Graphics.PostProcessManager.TextureInUse;
using Microsoft.Xna.Framework;

namespace XNA_PoolGame.Graphics
{
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
        public SSAO()
        {
            noiseTexture = PoolGame.content.Load<Texture2D>("Textures\\noise");
            effect = PoolGame.content.Load<Effect>("Effects\\SSAO");
            combinefinal = PoolGame.content.Load<Effect>("Effects\\Multiply");
            intensity = 5.0f;
            scale = 1.0f;
            randomsize = (float)(noiseTexture.Width * noiseTexture.Height);
            screensize = (float)(PoolGame.Width * PoolGame.Height);
            radius = 18.85f;
            bias = 0.001f;
            halfPixel.X = 0.5f / (float)PoolGame.Width;
            halfPixel.Y = 0.5f / (float)PoolGame.Height;

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
            ssaoTIU = PostProcessManager.GetIntermediateTexture(PoolGame.Width, PoolGame.Height, SurfaceFormat.HalfVector4, pp.MultiSampleType, pp.MultiSampleQuality);
            PoolGame.device.SetRenderTarget(0, ssaoTIU.renderTarget);

            effect.Parameters["NormalMap"].SetValue(normalTIU.renderTarget.GetTexture());
            effect.Parameters["PositionMap"].SetValue(viewTIU.renderTarget.GetTexture());


            effect.Begin();
            effect.Techniques[0].Passes[0].Begin();
            PostProcessManager.quad.Draw(PoolGame.device);
            effect.Techniques[0].Passes[0].End();
            effect.End();

            binaryTIU = PostProcessManager.GetIntermediateTexture();

            PoolGame.device.SetRenderTarget(0, binaryTIU.renderTarget);
            combinefinal.Parameters["sceneMap"].SetValue(source.renderTarget.GetTexture());
            combinefinal.Parameters["SSAOMap"].SetValue(ssaoTIU.renderTarget.GetTexture());

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
