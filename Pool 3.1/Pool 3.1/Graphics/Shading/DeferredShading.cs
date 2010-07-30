#region Using Statments
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Graphics.Shadows;
using Microsoft.Xna.Framework.Graphics;
using TextureInUse = XNA_PoolGame.Graphics.PostProcessManager.TextureInUse;
using XNA_PoolGame.Scenarios;
#endregion

namespace XNA_PoolGame.Graphics.Shading
{
    /// <summary>
    /// Deferred shading for deferred lighting. Also can work with SSAO implementation.
    /// </summary>
    public class DeferredShading : BaseShading
    {
        private PresentationParameters pp;
        public TextureInUse diffuseColorTIU, normalTIU, lightTIU, depthTIU, combineTIU;
        private SurfaceFormat format = SurfaceFormat.HalfVector4;
        public Texture2D normalTexture;
        private Model sphereModel;

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        public DeferredShading()
        {
            halfPixel.X = 0.5f / (float)PoolGame.device.PresentationParameters.BackBufferWidth;
            halfPixel.Y = 0.5f / (float)PoolGame.device.PresentationParameters.BackBufferHeight;

            pp = PoolGame.device.PresentationParameters;
            stencilBuffer = new DepthStencilBuffer(PoolGame.device, PoolGame.Width, PoolGame.Height, PoolGame.device.DepthStencilBuffer.Format, pp.MultiSampleType, pp.MultiSampleQuality);

            // Point Light
            sphereModel = PoolGame.content.Load<Model>("Models\\pointlight");

        }

        public override void Draw(GameTime gameTime)
        {
            SetGBuffer();
            ClearGBuffer();

            PostProcessManager.ChangeRenderMode(RenderMode.RenderGBuffer);
            World.scenario.DrawScene(gameTime);
            
            ResolveGBuffer();

            shadows.Draw(gameTime);

            shadows.PostDraw();
            
            DepthStencilBuffer oldBuffer = PoolGame.device.DepthStencilBuffer;
            PoolGame.device.DepthStencilBuffer = stencilBuffer;
            
            DrawLights(gameTime);
            CombineFinal(shadows.ShadowRT.GetTexture());
            PoolGame.device.DepthStencilBuffer = oldBuffer;

            normalTexture = normalTIU.renderTarget.GetTexture();
            if (World.doSSAO)
            {
                PostProcessManager.ssao.normalTIU = normalTIU;
            }
        }

        private void DrawLights(GameTime gameTime)
        {
            lightTIU = PostProcessManager.GetIntermediateTexture(PoolGame.Width, PoolGame.Height, format, pp.MultiSampleType, pp.MultiSampleQuality);

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

            for (int i = 0; i < LightManager.totalLights; ++i)
            {
                DrawDirectionalLight(LightManager.lights[i].Position, LightManager.lights[i].LightColor, "DirectionalPositionTechnique");
            }
            DrawPointLight(((CribsBasement)World.scenario).smokestack.AditionalLights[0].Position, 
                ((CribsBasement)World.scenario).smokestack.AditionalLights[0].LightColor, 
                ((CribsBasement)World.scenario).smokestack.AditionalLights[0].Radius, 5.0f);
        }

        private void DrawDirectionalLight(Vector3 lightDirection, Color color, string technique)
        {
            //set all parameters
            PostProcessManager.directionalLightEffect.CurrentTechnique = PostProcessManager.directionalLightEffect.Techniques[technique];
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

        private void DrawPointLight(Vector3 lightPosition, Color color, float lightRadius, float lightIntensity)
        {
            PoolGame.device.RenderState.CullMode = CullMode.None;
            
            //set the G-Buffer parameters
            PostProcessManager.pointLightEffect.Parameters["colorMap"].SetValue(diffuseColorTIU.renderTarget.GetTexture());
            PostProcessManager.pointLightEffect.Parameters["normalMap"].SetValue(normalTIU.renderTarget.GetTexture());
            PostProcessManager.pointLightEffect.Parameters["depthMap"].SetValue(PostProcessManager.depthTIU.renderTarget.GetTexture());

            //compute the light world matrix
            //scale according to light radius, and translate it to light position
            Matrix sphereWorldMatrix = Matrix.CreateScale(lightRadius) * Matrix.CreateTranslation(lightPosition);
            PostProcessManager.pointLightEffect.Parameters["World"].SetValue(sphereWorldMatrix);
            PostProcessManager.pointLightEffect.Parameters["View"].SetValue(World.camera.View);
            PostProcessManager.pointLightEffect.Parameters["Projection"].SetValue(World.camera.Projection);
            //light position
            PostProcessManager.pointLightEffect.Parameters["lightPosition"].SetValue(lightPosition);

            //set the color, radius and Intensity
            PostProcessManager.pointLightEffect.Parameters["Color"].SetValue(color.ToVector3());
            PostProcessManager.pointLightEffect.Parameters["lightRadius"].SetValue(lightRadius);
            PostProcessManager.pointLightEffect.Parameters["lightIntensity"].SetValue(lightIntensity);

            //parameters for specular computations
            PostProcessManager.pointLightEffect.Parameters["cameraPosition"].SetValue(World.camera.CameraPosition);
            PostProcessManager.pointLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(World.camera.ViewProjection));
            //size of a halfpixel, for texture coordinates alignment
            PostProcessManager.pointLightEffect.Parameters["halfPixel"].SetValue(halfPixel);
            //calculate the distance between the camera and light center
            float cameraToCenter = Vector3.Distance(World.camera.CameraPosition, lightPosition);
            //if we are inside the light volume, draw the sphere's inside face
            if (cameraToCenter < lightRadius)
                PoolGame.device.RenderState.CullMode = CullMode.CullClockwiseFace;
            else
                PoolGame.device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            PoolGame.device.RenderState.DepthBufferEnable = false;
            //PoolGame.device.RenderState.CullMode = CullMode.None;

            PostProcessManager.pointLightEffect.Begin();
            PostProcessManager.pointLightEffect.Techniques[0].Passes[0].Begin();

            foreach (ModelMesh mesh in sphereModel.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    PoolGame.device.VertexDeclaration = meshPart.VertexDeclaration;
                    PoolGame.device.Vertices[0].SetSource(mesh.VertexBuffer, meshPart.StreamOffset, meshPart.VertexStride);
                    PoolGame.device.Indices = mesh.IndexBuffer;
                    PoolGame.device.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshPart.BaseVertex, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                }
            }

            PostProcessManager.pointLightEffect.Techniques[0].Passes[0].End();
            PostProcessManager.pointLightEffect.End();

            PoolGame.device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            PoolGame.device.RenderState.DepthBufferWriteEnable = true;
        }


        private void SetGBuffer()
        {
            diffuseColorTIU = PostProcessManager.GetIntermediateTexture(PoolGame.Width, PoolGame.Height, format, pp.MultiSampleType, pp.MultiSampleQuality);
            normalTIU = PostProcessManager.GetIntermediateTexture(PoolGame.Width, PoolGame.Height, format, pp.MultiSampleType, pp.MultiSampleQuality);
            PostProcessManager.depthTIU.Use();
            PoolGame.device.SetRenderTarget(0, diffuseColorTIU.renderTarget);
            PoolGame.device.SetRenderTarget(1, normalTIU.renderTarget);
            PoolGame.device.SetRenderTarget(2, PostProcessManager.depthRT);
        }

        private void ClearGBuffer()
        {
            
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

        private void CombineFinal(Texture2D shadowOcclussion)
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
            PostProcessManager.combineFinal_DefEffect.Parameters["shadowOcclusion"].SetValue(shadowOcclussion);

            PostProcessManager.combineFinal_DefEffect.Begin();
            PostProcessManager.combineFinal_DefEffect.Techniques[0].Passes[0].Begin();

            //render a full-screen quad
            PostProcessManager.quad.Draw(PoolGame.device);

            PostProcessManager.combineFinal_DefEffect.Techniques[0].Passes[0].End();
            PostProcessManager.combineFinal_DefEffect.End();

            if (!World.doSSAO && !World.doNormalPositionPass) normalTIU.DontUse();
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

        /// <summary>
        /// Draw the scene without shadows.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void DrawTextured(GameTime gameTime)
        {
            SetGBuffer();
            ClearGBuffer();

            PostProcessManager.ChangeRenderMode(RenderMode.RenderGBuffer);
            World.scenario.DrawScene(gameTime);

            ResolveGBuffer();

            DepthStencilBuffer oldBuffer = PoolGame.device.DepthStencilBuffer;
            PoolGame.device.DepthStencilBuffer = stencilBuffer;

            DrawLights(gameTime);
            CombineFinal(PostProcessManager.whiteTexture);
            PoolGame.device.DepthStencilBuffer = oldBuffer;

            normalTexture = normalTIU.renderTarget.GetTexture();
            if (World.doSSAO)
            {
                PostProcessManager.ssao.normalTIU = normalTIU;
            }
        }

        public override string GetBasicRenderTechnique()
        {
            if (format == SurfaceFormat.HalfVector4) return "RenderHalf4";
            return "RenderColor";
        }

        public override void FreeStuff()
        {
            normalTIU.DontUse();
            combineTIU.DontUse();
            diffuseColorTIU.DontUse();
            lightTIU.DontUse();
        }
    }
}
