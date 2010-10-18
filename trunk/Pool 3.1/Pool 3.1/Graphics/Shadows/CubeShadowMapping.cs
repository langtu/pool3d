#region Using Statments
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TextureInUse = XNA_PoolGame.Graphics.PostProcessManager.TextureInUse;
#endregion

namespace XNA_PoolGame.Graphics.Shadows
{
    /// <summary>
    /// Cube Shadow Mapping.
    /// </summary>
    public class CubeShadowMapping : Shadow
    {
        public RenderTargetCube renderCube;
        PresentationParameters pp;
        int size = 512;

        Matrix[] views = new Matrix[6];
        BoundingFrustum[] frustums = new BoundingFrustum[6];
        Matrix projection;
        bool firstTime;
        int currentFace;
        Color[] colors;

        private SurfaceFormat format;

        public Texture2D frontTexture;

        public BoundingFrustum CurrentFrustum
        {
            get { return frustums[currentFace]; }
        }

        public CubeShadowMapping()
        {
            pp = PoolGame.device.PresentationParameters;

            //frontTexture = new Texture2D(PoolGame.device, size, size, 1, TextureUsage.None, SurfaceFormat.Color);

            format = SurfaceFormat.Single;
            renderCube = new RenderTargetCube(PoolGame.device, size, 1, format, pp.MultiSampleType, pp.MultiSampleQuality, RenderTargetUsage.DiscardContents);
            stencilBuffer = new DepthStencilBuffer(PoolGame.device, size, size, PoolGame.device.PresentationParameters.AutoDepthStencilFormat);
            ShadowRT = new RenderTarget2D(PoolGame.device, pp.BackBufferWidth, pp.BackBufferHeight, 1, PoolGame.device.DisplayMode.Format,
                pp.MultiSampleType, pp.MultiSampleQuality);

            shadowTIU = new TextureInUse(ShadowRT, false);
            PostProcessManager.renderTargets.Add(shadowTIU);

            colors = new Color[size * size];
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1.0f, 1.0f, 2800.0f);
            firstTime = true;
        }

        public override void Draw(GameTime gameTime)
        {
            if (firstTime) { UpdateFacesView(); /*firstTime = false*/; }

            ///////////////// PASS 1 - Cube Depth Map ////////
            PostProcessManager.ChangeRenderMode(RenderPassMode.CubeShadowMapPass);

            BoundingFrustum cameraFrustum = World.camera.Frustum;

            oldBuffer = PoolGame.device.DepthStencilBuffer;
            PoolGame.device.DepthStencilBuffer = stencilBuffer;
            lightpass = 0;

            PoolGame.device.RenderState.ColorWriteChannels = ColorWriteChannels.Red;

            for (int i = 0; i < LightManager.totalLights; ++i)
            {
                SetDepthMapParameters(LightManager.lights[i]);
                for (int iFace = 0; iFace < 6; iFace++)
                {
                    //if (cameraFrustum.Contains(frustums[iFace]) == ContainmentType.Disjoint)
                    //    continue;

                    currentFace = iFace;
                    SetupShadowMap(i, iFace);

                    World.scenario.DrawScene(gameTime);
                }
                ++lightpass;
            }
            PoolGame.device.SetRenderTarget(0, null);
            
            PoolGame.device.RenderState.ColorWriteChannels = ColorWriteChannels.All;
            ///////////////// PASS 2 - PCF //////////////
            PostProcessManager.ChangeRenderMode(RenderPassMode.PCFShadowMapRender);
            SetupPCFShadowMap();
            SetPCFParameters(ref LightManager.lights);
            shadowTIU.Use();

            World.scenario.DrawScene(gameTime);
            shadowOcclussionTIU = shadowTIU;

            //renderCube.GetTexture().GetData<Color>(CubeMapFace.PositiveZ, colors);
            ////frontTexture.SetData<Color>(colors);
            ////frontTexture.SetData
            ///////////////// PASS 3 - SSSM /////////////
            World.camera.ItemsDrawn = 0;
            PostProcessManager.ChangeRenderMode(RenderPassMode.ScreenSpaceSoftShadowRender);
            RenderSoftShadow();

            World.scenario.DrawScene(gameTime);

        }
        private void RenderSoftShadow()
        {
            // Soft
            if (PostProcessManager.shadowBlurTech == ShadowBlurTechnnique.SoftShadow)
            {
                TextureInUse tmp = PostProcessManager.GetIntermediateTexture();
                PostProcessManager.SetBlurEffectParameters(0.5f / PoolGame.device.Viewport.Width, 0.0f, PostProcessManager.GBlurHEffect);
                PostProcessManager.SetBlurEffectParameters(0.0f, 0.5f / PoolGame.device.Viewport.Height, PostProcessManager.GBlurVEffect);

                //Gaussian Blur H
                PoolGame.device.SetRenderTarget(0, tmp.renderTarget);
                PostProcessManager.DrawQuad(shadowOcclussionTIU.renderTarget.GetTexture(), PostProcessManager.GBlurHEffect);

                //Guassian Blur V
                PoolGame.device.SetRenderTarget(0, shadowOcclussionTIU.renderTarget);
                PostProcessManager.DrawQuad(tmp.renderTarget.GetTexture(), PostProcessManager.GBlurVEffect);

                //PostProcessManager.SetBlurEffectParameters(0.5f / PoolGame.device.Viewport.Width, 0.0f, PostProcessManager.GBlurHEffect);
                //PostProcessManager.SetBlurEffectParameters(0.0f, 0.5f / PoolGame.device.Viewport.Height, PostProcessManager.GBlurVEffect);

                tmp.DontUse();
            }

            //Screen Space Shadow
            PostProcessManager.mainTIU.Use();
            PoolGame.device.SetRenderTarget(0, PostProcessManager.mainRT);

            if (World.motionblurType != MotionBlurType.None || World.dofType != DOFType.None)
            {
                PostProcessManager.depthTIU.Use(); PostProcessManager.velocityTIU.Use(); PostProcessManager.velocityLastFrameTIU.Use();
                PoolGame.device.SetRenderTarget(1, PostProcessManager.depthRT);
                PoolGame.device.SetRenderTarget(2, PostProcessManager.velocityRT);


                PostProcessManager.clearGBufferEffect.CurrentTechnique = PostProcessManager.clearGBufferEffect.Techniques["ClearGBufferTechnnique"];
                PostProcessManager.DrawQuad(PostProcessManager.whiteTexture, PostProcessManager.clearGBufferEffect);

                //PoolGame.device.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1.0f, 0);
                PoolGame.device.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            }
            else
            {
                PostProcessManager.depthTIU.DontUse(); PostProcessManager.velocityTIU.DontUse(); PostProcessManager.velocityLastFrameTIU.DontUse();

                {
                    PoolGame.device.SetRenderTarget(1, null);
                    PoolGame.device.SetRenderTarget(2, null);

                    //PoolGame.device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target | ClearOptions.Stencil, Color.CornflowerBlue, 1.0f, 0);
                    PoolGame.device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, Color.CornflowerBlue, 1.0f, 0);
                }
            }

            PoolGame.device.RenderState.DepthBufferEnable = true;
            PoolGame.device.RenderState.DepthBufferWriteEnable = true;
            PoolGame.device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            resultTIU = PostProcessManager.mainTIU;
        }
        private void SetupPCFShadowMap()
        {
            PoolGame.device.RenderState.DepthBufferEnable = true;
            PoolGame.device.RenderState.DepthBufferWriteEnable = true;
            //Render PCF Shadow Map
            PoolGame.device.DepthStencilBuffer = oldBuffer;
            PoolGame.device.SetRenderTarget(0, ShadowRT);
            PoolGame.device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, Color.White, 1.0f, 0);

            PoolGame.device.RenderState.CullMode = CullMode.None;
        }

        private void SetupShadowMap(int light, int iFace)
        {
            PoolGame.device.RenderState.DepthBufferEnable = true;
            PoolGame.device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            // Render the scene to all cubemap faces.
            CubeMapFace cubeMapFace = (CubeMapFace)iFace;

            PoolGame.device.SetRenderTarget(0, renderCube, cubeMapFace);
            PoolGame.device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, Color.Black, 1.0f, 0);
        }

        public void UpdateFacesView()
        {
            for (int i = 0; i < 6; i++)
            {
                CubeMapFace cubeMapFace = (CubeMapFace)i;

                #region Cube Map Faces
                switch (cubeMapFace)
                {
                    case CubeMapFace.NegativeX:
                        {
                            views[i] = Matrix.CreateLookAt(LightManager.lights[0].Position, LightManager.lights[0].Position + Vector3.Left, Vector3.Up);
                            break;
                        }
                    case CubeMapFace.NegativeY:
                        {
                            views[i] = Matrix.CreateLookAt(LightManager.lights[0].Position, LightManager.lights[0].Position + Vector3.Down, Vector3.Forward);
                            break;
                        }
                    case CubeMapFace.NegativeZ:
                        {
                            views[i] = Matrix.CreateLookAt(LightManager.lights[0].Position, LightManager.lights[0].Position + Vector3.Backward, Vector3.Up);
                            break;
                        }
                    case CubeMapFace.PositiveX:
                        {
                            views[i] = Matrix.CreateLookAt(LightManager.lights[0].Position, LightManager.lights[0].Position + Vector3.Right, Vector3.Up);
                            break;
                        }
                    case CubeMapFace.PositiveY:
                        {
                            views[i] = Matrix.CreateLookAt(LightManager.lights[0].Position, LightManager.lights[0].Position + Vector3.Up, Vector3.Backward);
                            break;
                        }
                    case CubeMapFace.PositiveZ:
                        {
                            views[i] = Matrix.CreateLookAt(LightManager.lights[0].Position, LightManager.lights[0].Position + Vector3.Forward, Vector3.Up);
                            break;
                        }
                }
                #endregion
                
                frustums[i] = new BoundingFrustum(views[i] * projection);
            }
        }


        public override void PostDraw()
        {
            /////////////////////////////////////////////
            PoolGame.device.RenderState.StencilEnable = false;

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
            return "CubeDepthMap";
        }

        public override void SetPCFParameters(ref List<Light> lights)
        {
            PostProcessManager.PCFShadowMap.Parameters["cubeShadowMap"].SetValue(this.renderCube.GetTexture());
            PostProcessManager.PCFShadowMap.Parameters["eyePosition"].SetValue(World.camera.CameraPosition);
            PostProcessManager.PCFShadowMap.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);
            PostProcessManager.PCFShadowMap.Parameters["LightPosition"].SetValue(lights[0].Position);
        }

        public override void SetDepthMapParameters(Light light)
        {
            PostProcessManager.DepthEffect.Parameters["LightPosition"].SetValue(light.Position);
            PostProcessManager.DepthEffect.Parameters["ViewProj"].SetValue(frustums[currentFace].Matrix);
        }
    }
}
