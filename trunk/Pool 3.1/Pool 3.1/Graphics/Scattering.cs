#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Graphics.Shading;
using TextureInUse = XNA_PoolGame.Graphics.PostProcessManager.TextureInUse;
#endregion

namespace XNA_PoolGame.Graphics
{
    /// <summary>
    /// Light Shafts.
    /// </summary>
    public class Scattering : IDisposable
    {
        Effect scatterEffect;
        Vector3 position;
        Vector3 direction;
        private float Density;
        private float Weight;
        private float Decay;
        private float Exposition;

        int samples;

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// Resulting RenderTarget.
        /// </summary>
        public TextureInUse resultTIU;


        /// <summary>
        /// Create a new instance of Scattering.
        /// </summary>
        public Scattering()
        {
            scatterEffect = PoolGame.content.Load<Effect>("Effects\\Scatter");

            direction = Vector3.Up;

            Density = 0.286f;
            Weight = 0.832f;
            Decay = 1.0261f;
            Exposition = 0.0104f;

            samples = 32;
            resultTIU = null;

            SetParameters();
        }

        private void SetParameters()
        {

            scatterEffect.Parameters["Decay"].SetValue(this.Decay);
            scatterEffect.Parameters["Density"].SetValue(this.Density);
            scatterEffect.Parameters["Exposition"].SetValue(this.Exposition);
            scatterEffect.Parameters["Weight"].SetValue(this.Weight);

            scatterEffect.Parameters["LightDirection"].SetValue(direction);
            scatterEffect.Parameters["numSamples"].SetValue(samples);

            scatterEffect.CommitChanges();
        }

        public void Draw(TextureInUse source)
        {
            resultTIU = PostProcessManager.GetIntermediateTexture();
            PoolGame.device.SetRenderTarget(0, resultTIU.renderTarget);
            PoolGame.device.RenderState.DepthBufferEnable = true;
            PoolGame.device.RenderState.DepthBufferWriteEnable = false;
            PoolGame.device.RenderState.AlphaBlendEnable = false;
            PoolGame.device.RenderState.AlphaTestEnable = false;

            if (PostProcessManager.shading.Format == SurfaceFormat.Color) scatterEffect.CurrentTechnique = scatterEffect.Techniques["ScatterColor"];
            else scatterEffect.CurrentTechnique = scatterEffect.Techniques["ScatterHalf4"];

            scatterEffect.Parameters["frameTex"].SetValue(source.renderTarget.GetTexture());
            scatterEffect.Parameters["blackTex"].SetValue(((DeferredShading)PostProcessManager.shading).scatterTIU.renderTarget.GetTexture());
            scatterEffect.Parameters["View"].SetValue(World.camera.View);
            scatterEffect.Parameters["WorldViewProjection"].SetValue(
                Matrix.CreateTranslation(position) *
                World.camera.ViewProjection);

            scatterEffect.Parameters["LightPosition"].SetValue(position);

            scatterEffect.CommitChanges();
            scatterEffect.Begin();
            scatterEffect.Techniques[0].Passes[0].Begin();
            //draw a full-screen quad
            PostProcessManager.quad.Draw(PoolGame.device);
            scatterEffect.Techniques[0].Passes[0].End();
            scatterEffect.End();
        }

        #region Miembros de IDisposable

        public void Dispose()
        {
            if (scatterEffect != null) scatterEffect.Dispose();
            scatterEffect = null;
            resultTIU = null;
        }

        #endregion
    }
}
