using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNA_PoolGame.Graphics.Models
{
    public class AlphaEntity : Entity
    {
        public AlphaEntity(Game _game, string _modelName)
            : base(_game, _modelName)
        {

        }

        public override void Draw(GameTime gameTime)
        {
            if (World.camera == null) return;
            RenderMode renderMode = PostProcessManager.currentRenderMode;

            updateLocalWorld();

            switch (renderMode)
            {
                case RenderMode.RenderGBuffer:
                case RenderMode.ScreenSpaceSoftShadowRender:
                    {
                        frustum = World.camera.FrustumCulling;
                        string basicTechnique = PostProcessManager.shading.GetBasicRenderTechnique();
                        //if (World.motionblurType == MotionBlurType.None && World.dofType == DOFType.None) basicTechnique = "NoMRT" + basicTechnique;

                        PoolGame.device.RenderState.AlphaBlendEnable = true;
                        PoolGame.device.RenderState.AlphaBlendOperation = BlendFunction.Add;
                        /*PoolGame.device.RenderState.SourceBlend = Blend.SourceAlpha;
                        PoolGame.device.RenderState.DestinationBlend = Blend.One;*/
                        PoolGame.device.RenderState.SourceBlend = Blend.One;
                        PoolGame.device.RenderState.DestinationBlend = Blend.One;
                        PoolGame.device.RenderState.AlphaTestEnable = true;
                        PoolGame.device.RenderState.AlphaFunction = CompareFunction.Greater;
                        PoolGame.device.RenderState.ReferenceAlpha = 0;
                        PoolGame.device.RenderState.SeparateAlphaBlendEnabled = false;

                        PoolGame.device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

                        bool depthbufferWrite_old = PoolGame.device.RenderState.DepthBufferWriteEnable;
                        PoolGame.device.RenderState.DepthBufferEnable = true;
                        PoolGame.device.RenderState.DepthBufferWriteEnable = false;

                        DrawModel(true, PostProcessManager.alphaEffect, basicTechnique, delegate { SetParametersAlphaModel(); });
                        PoolGame.device.RenderState.AlphaTestEnable = false;
                        PoolGame.device.RenderState.AlphaBlendEnable = false;
                        PoolGame.device.RenderState.DepthBufferWriteEnable = depthbufferWrite_old;
                        PoolGame.device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
                        PoolGame.device.RenderState.DepthBufferEnable = true;
                    }
                    break;
            }
        }

        public void SetParametersAlphaModel()
        {
            PostProcessManager.alphaEffect.Parameters["World"].SetValue(localWorld);
            PostProcessManager.alphaEffect.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);
            PostProcessManager.alphaEffect.Parameters["LightPosition"].SetValue(this.Position - Vector3.Up * 35.0f);
        }
    }
}
