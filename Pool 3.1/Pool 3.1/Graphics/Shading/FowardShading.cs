using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA_PoolGame.Graphics.Shading
{
    public class FowardShading : BaseShading
    {
        public override void Draw(GameTime gameTime)
        {
            shadows.Draw(gameTime);

            shadows.Pass3(gameTime);
            shadows.Pass4(gameTime);

            shadows.PostDraw();
            if (World.motionblurType != MotionBlurType.None || World.dofType != DOFType.None)
            {
                PoolGame.device.SetRenderTarget(1, null);
                PoolGame.device.SetRenderTarget(2, null);
            }

            resultTIU = PostProcessManager.mainTIU;
        }

        public override void DrawTextured(GameTime gameTime)
        {
            shadows.DrawTextured(gameTime);

            resultTIU = PostProcessManager.mainTIU;
        }

        public override string GetBasicRenderTechnique()
        {
            return "ModelTechnique";
        }
    }
}
