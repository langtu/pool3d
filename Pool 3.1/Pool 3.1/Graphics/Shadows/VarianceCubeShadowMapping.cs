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
    /// Variance Cube Shadow Mapping.
    /// </summary>
    public class VarianceCubeShadowMapping : CubeShadowMapping
    {
        public VarianceCubeShadowMapping()
        {
            pp = PoolGame.device.PresentationParameters;

            format = SurfaceFormat.Rg32;
            renderCube = new RenderTargetCube(PoolGame.device, size, 1, format, pp.MultiSampleType, pp.MultiSampleQuality, RenderTargetUsage.DiscardContents);
            stencilBuffer = new DepthStencilBuffer(PoolGame.device, size, size, PoolGame.device.PresentationParameters.AutoDepthStencilFormat);
            ShadowRT = new RenderTarget2D(PoolGame.device, pp.BackBufferWidth, pp.BackBufferHeight, 1, PoolGame.device.DisplayMode.Format,
                pp.MultiSampleType, pp.MultiSampleQuality);

            shadowTIU = new TextureInUse(ShadowRT, false);
            PostProcessManager.renderTargets.Add(shadowTIU);

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1.0f, 1.0f, 2750.0f);
            firstTime = true;
        }

        public override void Draw(GameTime gameTime)
        {
            throw new NotImplementedException();
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

        public override string GetDepthMapTechnique()
        {
            return "";
        }

        public override void SetPCFParameters(ref List<Light> lights)
        {
            throw new NotImplementedException();
        }

        public override void SetDepthMapParameters(Light light)
        {
            throw new NotImplementedException();
        }
    }
}
