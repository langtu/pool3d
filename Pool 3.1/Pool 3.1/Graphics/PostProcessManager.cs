

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using XNA_PoolGame.Graphics;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Graphics.Shadows;
using XNA_PoolGame.Graphics.Bloom;

namespace XNA_PoolGame.Graphics
{
    public static class PostProcessManager
    {
        public static SpriteBatch spriteBatch;

        // EFFECTS
        public static Effect bloomExtractEffect;
        public static Effect bloomCombineEffect;
        public static Effect gaussianBlurEffect;
        public static Effect saturation;
        public static Effect modelEffect;
        public static Effect blurEffect;
        public static Effect DOFEffect;
        public static Effect scalingEffect;
        //public static Effect gaussianBlurEffect2;
        //public static Effect shadowMapEffect; // FOR VARIANCE SHADOW MAPPING
        //public static Effect depthEffect; // FOR VARIANCE SHADOW MAPPING
        public static Effect SSSoftShadow;
        public static Effect SSSoftShadow_MRT;
        public static Effect clearGBufferEffect;
        public static Effect PCFShadowMap;
        public static Effect motionblurEffect;
        public static Effect GBlurH;
        public static Effect GBlurV;
        public static Effect Depth;
        public static Effect distortionCombineEffect;

        public static BasicEffect basicEffect;

        // EFFECTS PARAMETERS
        public static EffectParameter depthViewParam;
        public static EffectParameter depthFarClipParam;
        public static EffectParameter depthProjectionParam;

        // SHADER
        public static RenderMode currentRenderMode = RenderMode.BasicRender;

        // SHADOW MAPPING
        public static Shadow shadows;
        public static Texture2D depthBlurred;
        
        public static float[] weights;
        public static Vector2[] offsets;
        
        public static ShadowBlurTechnnique shadowBlurTech;
        

        // RENDER TARGETS
        
        public static RenderTarget2D halfRTVert;
        public static RenderTarget2D halfRTHor;
        public static RenderTarget2D GBlurHRT;
        public static RenderTarget2D GBlurVRT;
        public static RenderTarget2D mainRT;
        public static RenderTarget2D depthRT;
        public static RenderTarget2D velocityRT, velocityRTLastFrame;
        public static List<TextureInUse> renderTargets;

        public static TextureInUse velocityTIU, velocityLastFrameTIU, depthTIU, halfVertTIU, halfHorTIU, mainTIU;
        public static TextureInUse distortionsample;
        //
        public static Texture2D whiteTexture;

        //
        public static ResolveTexture2D resolveTarget;

        // BLOOM COMPONENT
        public static BloomSettings bloomSettings;
        public static IntermediateBuffer showBuffer = IntermediateBuffer.FinalResult;

        // BLUR
        public static GaussianBlur gaussianBlur;
        private const int BLUR_RADIUS = 7;
        private const float BLUR_AMOUNT = 2.0f;

        // MOTION BLUR
        public static MotionBlur motionBlur;

        // DEPTH OF FIELD
        public static DepthOfField depthOfField;
        public static Texture2D dofMapTex;

        //
        public static FullScreenQuad quad;

        public static void Load()
        {
            spriteBatch = new SpriteBatch(PoolGame.device);

            
            shadowBlurTech = ShadowBlurTechnnique.Normal;



            renderTargets = new List<TextureInUse>();

            bloomExtractEffect = PoolGame.content.Load<Effect>("Effects\\BloomExtract");
            bloomCombineEffect = PoolGame.content.Load<Effect>("Effects\\BloomCombine");
            gaussianBlurEffect = PoolGame.content.Load<Effect>("Effects\\GaussianBlur");
            saturation = PoolGame.content.Load<Effect>("Effects\\Saturate");
            modelEffect = PoolGame.content.Load<Effect>("Effects\\ModelEffect_MRT");
            distortionCombineEffect = PoolGame.content.Load<Effect>("Effects\\DistortionCombine");
            Depth = PoolGame.content.Load<Effect>("Effects\\Depth");
            PCFShadowMap = PoolGame.content.Load<Effect>("Effects\\PCFSM");
            GBlurH = PoolGame.content.Load<Effect>("Effects\\GBlurH");
            GBlurV = PoolGame.content.Load<Effect>("Effects\\GBlurV");
            SSSoftShadow = PoolGame.content.Load<Effect>("Effects\\ScreenSpaceSoftShadow");
            SSSoftShadow_MRT = PoolGame.content.Load<Effect>("Effects\\ScreenSpaceSoftShadow_MRT");
            motionblurEffect = PoolGame.content.Load<Effect>("Effects\\MotionBlur");
            clearGBufferEffect = PoolGame.content.Load<Effect>("Effects\\ClearGBuffer");

            blurEffect = PoolGame.content.Load<Effect>("Effects\\Blur");
            DOFEffect = PoolGame.content.Load<Effect>("Effects\\DOF");
            scalingEffect = PoolGame.content.Load<Effect>("Effects\\Scale");
            whiteTexture = PoolGame.content.Load<Texture2D>("Textures\\white");

            distortionCombineEffect.Parameters["texelx"].SetValue(1.0f / (float)PoolGame.device.PresentationParameters.BackBufferWidth);
            distortionCombineEffect.Parameters["texely"].SetValue(1.0f / (float)PoolGame.device.PresentationParameters.BackBufferHeight);

            basicEffect = new BasicEffect(PoolGame.device, null);

            SetBlurEffectParameters(1.5f / PoolGame.device.Viewport.Width, 0, GBlurH);
            SetBlurEffectParameters(0, 1.5f / PoolGame.device.Viewport.Height, GBlurV);

            gaussianBlur = new GaussianBlur(PoolGame.game);
            gaussianBlur.ComputeKernel(BLUR_RADIUS, BLUR_AMOUNT);

            depthOfField = new DepthOfField();
            depthOfField.focalWidth = 1750.0f;
            depthOfField.focalDistance = 550.0f;

            quad = new FullScreenQuad(PoolGame.device);
        }

        #region Gaussian Helper
        public static void SetBlurEffectParameters(float dx, float dy, Effect effect)
        {
            // Look up the sample weight and offset effect parameters.
            EffectParameter weightsParameter, offsetsParameter;

            weightsParameter = effect.Parameters["SampleWeights"];
            offsetsParameter = effect.Parameters["SampleOffsets"];

            // Look up how many samples our gaussian blur effect supports.
            int sampleCount = weightsParameter.Elements.Count;

            // Create temporary arrays for computing our filter settings.
            float[] sampleWeights = new float[sampleCount];
            Vector2[] sampleOffsets = new Vector2[sampleCount];

            // The first sample always has a zero offset.
            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = new Vector2(0);

            // Maintain a sum of all the weighting values.
            float totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < sampleCount / 2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1);

                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(dx, dy) * sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= totalWeights;
            }

            // Tell the effect about our new filter settings.
            weightsParameter.SetValue(sampleWeights);
            offsetsParameter.SetValue(sampleOffsets);
        }

        private static float ComputeGaussian(float n)
        {
            float BlurAmount = 4;
            float theta = BlurAmount;

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                           Math.Exp(-(n * n) / (2 * theta * theta)));
        }
        #endregion

        #region Shadow Mapping
        
        #endregion

        #region No Shadows
        public static void RenderTextured()
        {
            PoolGame.device.SetRenderTarget(0, mainRT);
            if (World.dofType == DOFType.None && World.motionblurType == MotionBlurType.None)
            {
                PoolGame.device.SetRenderTarget(1, null);
                PoolGame.device.SetRenderTarget(2, null);
                PoolGame.device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, Color.CornflowerBlue, 1.0f, 0);
            }
            else
            {
                depthTIU.Use();
                PoolGame.device.SetRenderTarget(1, depthRT);
                PoolGame.device.SetRenderTarget(2, velocityRT);

                //PoolGame.device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1.0f, 0);
                PoolGame.device.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1.0f, 0);
                PostProcessManager.DrawQuad(PostProcessManager.whiteTexture, PostProcessManager.clearGBufferEffect);
            }

            

        }
        #endregion
        
        #region DOF
        public static void CreateDOFMap()
        {
            PoolGame.device.SetRenderTarget(0, depthRT);
            PoolGame.device.Clear(Color.White);

            Depth.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);
            Depth.Parameters["MaxDepth"].SetValue(World.camera.FarPlane);
        }
        public static void DOF(RenderTarget2D source, RenderTarget2D result)
        {
            
            PoolGame.device.RenderState.StencilEnable = false;
            depthOfField.DOF(source, result, depthRT, World.dofType);

        }
        #endregion
        
        #region Initialize Render Targets
        public static void InitRenderTargets()
        {
            int renderTargetWidth = PoolGame.device.Viewport.Width / 2;
            int renderTargetHeight = PoolGame.device.Viewport.Height / 2;

            PresentationParameters pp = PoolGame.device.PresentationParameters;
            
            int width = pp.BackBufferWidth;
            int height = pp.BackBufferHeight;

            SurfaceFormat format = pp.BackBufferFormat;
            
            halfRTHor = new RenderTarget2D(PoolGame.device,
                renderTargetWidth, renderTargetHeight, 1,
                format, pp.MultiSampleType,
                pp.MultiSampleQuality);

            halfHorTIU = new TextureInUse(halfRTHor, false);
            renderTargets.Add(halfHorTIU);

            halfRTVert = new RenderTarget2D(PoolGame.device,
                renderTargetWidth, renderTargetHeight, 1,
                format, pp.MultiSampleType, pp.MultiSampleQuality);


            halfVertTIU = new TextureInUse(halfRTVert, false);
            renderTargets.Add(halfVertTIU);
            

            

            GBlurHRT = new RenderTarget2D(PoolGame.device, pp.BackBufferWidth, pp.BackBufferHeight, 1, pp.BackBufferFormat, pp.MultiSampleType, pp.MultiSampleQuality);
            renderTargets.Add(new TextureInUse(GBlurHRT, false));

            GBlurVRT = new RenderTarget2D(PoolGame.device, pp.BackBufferWidth, pp.BackBufferHeight, 1, pp.BackBufferFormat, pp.MultiSampleType, pp.MultiSampleQuality);
            renderTargets.Add(new TextureInUse(GBlurVRT, false));

            depthRT = new RenderTarget2D(PoolGame.device, pp.BackBufferWidth, pp.BackBufferHeight, 1, SurfaceFormat.Single, pp.MultiSampleType,
                pp.MultiSampleQuality, RenderTargetUsage.DiscardContents);

            depthTIU = new TextureInUse(depthRT, false);
            renderTargets.Add(depthTIU);

            velocityRT = new RenderTarget2D(PoolGame.device,
                                                pp.BackBufferWidth,
                                                pp.BackBufferHeight,
                                                1,
                                                SurfaceFormat.Vector2,
                                                pp.MultiSampleType,
                                                pp.MultiSampleQuality,
                                                RenderTargetUsage.DiscardContents);

            velocityTIU = new TextureInUse(velocityRT, false);
            renderTargets.Add(velocityTIU);

            velocityRTLastFrame = new RenderTarget2D(PoolGame.device,
                                                pp.BackBufferWidth,
                                                pp.BackBufferHeight,
                                                1,
                                                SurfaceFormat.Vector2,
                                                pp.MultiSampleType,
                                                pp.MultiSampleQuality,
                                                RenderTargetUsage.DiscardContents);


            velocityLastFrameTIU = new TextureInUse(velocityRTLastFrame, false);
            renderTargets.Add(velocityLastFrameTIU);

            mainRT = new RenderTarget2D(PoolGame.device, pp.BackBufferWidth, pp.BackBufferHeight, 1, format, pp.MultiSampleType,
                pp.MultiSampleQuality, RenderTargetUsage.DiscardContents);

            mainTIU = new TextureInUse(mainRT, false);
            renderTargets.Add(mainTIU);

            
            resolveTarget = new ResolveTexture2D(PoolGame.device, width, height, 1, format);

            motionBlur = new MotionBlur();

            gaussianBlur.ComputeOffsets(renderTargetWidth, renderTargetHeight);

            //shadow = new Shadow(shadowMapEffect);

        }
        #endregion

        #region Depth Stencil Helper
        public static DepthStencilBuffer CreateDepthStencil(RenderTarget2D target)
        {
            return new DepthStencilBuffer(target.GraphicsDevice, target.Width,
                target.Height, target.GraphicsDevice.DepthStencilBuffer.Format,
                target.MultiSampleType, target.MultiSampleQuality);
        }

        public static DepthStencilBuffer CreateDepthStencil(RenderTarget2D target, DepthFormat depth)
        {
            if (GraphicsAdapter.DefaultAdapter.CheckDepthStencilMatch(DeviceType.Hardware,
               GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Format, target.Format,
                depth))
            {
                return new DepthStencilBuffer(target.GraphicsDevice, target.Width,
                    target.Height, depth, target.MultiSampleType, target.MultiSampleQuality);
            }
            else
                return CreateDepthStencil(target);
        }
        #endregion

        #region Render Target Helper
        public static RenderTarget2D CreateRenderTarget(GraphicsDevice device, int numberLevels,
            SurfaceFormat surface)
        {
            MultiSampleType type = device.PresentationParameters.MultiSampleType;

            // If the card can't use the surface format
            if (!GraphicsAdapter.DefaultAdapter.CheckDeviceFormat(DeviceType.Hardware,
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Format, TextureUsage.None,
                QueryUsages.None, ResourceType.RenderTarget, surface))
            {
                // Fall back to current display format
                surface = device.DisplayMode.Format;
            }
            // Or it can't accept that surface format with the current AA settings
            else if (!GraphicsAdapter.DefaultAdapter.CheckDeviceMultiSampleType(DeviceType.Hardware,
                surface, device.PresentationParameters.IsFullScreen, type))
            {
                // Fall back to no antialiasing
                type = MultiSampleType.None;
            }

            int width, height;

            // See if we can use our buffer size as our texture
            CheckTextureSize(device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight,
                out width, out height);

            // Create our render target
            return new RenderTarget2D(device,
                width, height, numberLevels, surface,
                type, 0);

        }
        public static bool CheckTextureSize(int width, int height, out int newwidth, out int newheight)
        {
            bool retval = false;

            GraphicsDeviceCapabilities Caps;
            Caps = GraphicsAdapter.DefaultAdapter.GetCapabilities(DeviceType.Hardware);

            if (Caps.TextureCapabilities.RequiresPower2)
            {
                retval = true;  // Return true to indicate the numbers changed

                // Find the nearest base two log of the current width, and go up to the next integer                
                double exp = Math.Ceiling(Math.Log(width) / Math.Log(2));
                // and use that as the exponent of the new width
                width = (int)Math.Pow(2, exp);
                // Repeat the process for height
                exp = Math.Ceiling(Math.Log(height) / Math.Log(2));
                height = (int)Math.Pow(2, exp);
            }
            if (Caps.TextureCapabilities.RequiresSquareOnly)
            {
                retval = true;  // Return true to indicate numbers changed
                width = Math.Max(width, height);
                height = width;
            }

            newwidth = Math.Min(Caps.MaxTextureWidth, width);
            newheight = Math.Min(Caps.MaxTextureHeight, height);
            return retval;
        }
        #endregion

        #region Quad
        public static void DrawQuad(Texture2D texture, Effect effect)
        {
            Viewport viewport = PoolGame.device.Viewport;
            
            effect.Begin();
            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                spriteBatch.Draw(texture, viewport.TitleSafeArea, Color.White);
                pass.End();
            }
            spriteBatch.End();
            effect.End();
        }

        public static void DrawQuad(Texture2D texture, Effect effect, IntermediateBuffer currentBuffer)
        {

            Viewport viewport = PoolGame.device.Viewport;
            
            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);

            if (showBuffer >= currentBuffer)
            {
                effect.Begin();
                effect.CurrentTechnique.Passes[0].Begin();
            }
            spriteBatch.Draw(texture, viewport.TitleSafeArea, Color.White);
            spriteBatch.End();

            if (showBuffer >= currentBuffer)
            {
                effect.CurrentTechnique.Passes[0].End();
                effect.End();
            }
        }
        #endregion

        #region Unload
        public static void UnloadContent()
        {
            spriteBatch.Dispose();

            // EFFECT'S DISPOSING
            distortionCombineEffect.Dispose();
            motionblurEffect.Dispose();
            bloomExtractEffect.Dispose();
            bloomCombineEffect.Dispose();
            gaussianBlurEffect.Dispose();
            SSSoftShadow.Dispose();
            PCFShadowMap.Dispose();
            GBlurH.Dispose();
            GBlurV.Dispose();
            Depth.Dispose();
            SSSoftShadow_MRT.Dispose();
            blurEffect.Dispose();
            scalingEffect.Dispose();
            DOFEffect.Dispose();

            // DISPOSE RENDER TARGETS
            foreach (TextureInUse t in renderTargets)
                t.renderTarget.Dispose();

            renderTargets.Clear();

            resolveTarget.Dispose();
        }
        #endregion

        public static void ChangeRenderMode(RenderMode thisMode)
        {
            currentRenderMode = thisMode;
            
        }

        public static void DrawSaturation(float sat)
        {
            PoolGame.device.ResolveBackBuffer(resolveTarget);
            PoolGame.device.Clear(Color.White);

            saturation.Parameters["Saturation"].SetValue(sat);
            saturation.Parameters["Base"].SetValue(new Vector4(0.9f, 0.7f, 0.3f, 1.0f));

            DrawQuad(resolveTarget, saturation);
        }

        #region Bloom
        public static void DrawBloomPostProcessing(RenderTarget2D input, RenderTarget2D result, GameTime gameTime)
        {
            bloomExtractEffect.Parameters["BloomThreshold"].SetValue(bloomSettings.BloomThreshold);
            //PoolGame.device.ResolveBackBuffer(PostProcessManager.resolveTarget);

            PostProcessManager.halfHorTIU.Use(); PostProcessManager.halfVertTIU.Use();

            PoolGame.device.SetRenderTarget(0, halfRTHor);
            DrawQuad(input.GetTexture(), bloomExtractEffect, IntermediateBuffer.PreBloom);

            SetBlurEffectParameters(1.0f / (float)halfRTHor.Width, 0, gaussianBlurEffect);


            PoolGame.device.SetRenderTarget(0, halfRTVert);
            DrawQuad(halfRTHor.GetTexture(), gaussianBlurEffect, IntermediateBuffer.BlurredHorizontally);


            SetBlurEffectParameters(0, 1.0f / (float)halfRTHor.Height, gaussianBlurEffect);


            PoolGame.device.SetRenderTarget(0, halfRTHor);
            DrawQuad(halfRTVert.GetTexture(), PostProcessManager.gaussianBlurEffect, IntermediateBuffer.BlurredBothWays);

            Texture2D tex = input.GetTexture();
            PoolGame.device.SetRenderTarget(0, result);

            EffectParameterCollection parameters = bloomCombineEffect.Parameters;

            parameters["BloomIntensity"].SetValue(bloomSettings.BloomIntensity);
            parameters["BaseIntensity"].SetValue(bloomSettings.BaseIntensity);
            parameters["BloomSaturation"].SetValue(bloomSettings.BloomSaturation);
            parameters["BaseSaturation"].SetValue(bloomSettings.BaseSaturation);

            PoolGame.device.Textures[1] = tex;

            DrawQuad(halfRTHor.GetTexture(), PostProcessManager.bloomCombineEffect, IntermediateBuffer.FinalResult);
            PoolGame.device.Textures[1] = null;
        }

        #endregion

        public static void DistorionParticles()
        {
            PoolGame.device.RenderState.DepthBufferEnable = true;
            PoolGame.device.RenderState.DepthBufferWriteEnable = false;
            distortionsample = GetIntermediateTexture();
            PoolGame.device.SetRenderTarget(0, distortionsample.renderTarget);
            PoolGame.device.Clear(Color.Black);
        }

        public static void RenderMotionBlur(RenderTarget2D source, RenderTarget2D result)
        {

            velocityTIU.Use(); velocityLastFrameTIU.Use();
            motionBlur.DoMotionBlur(source, result, depthRT,
                    velocityRT, velocityRTLastFrame, World.camera, World.camera.PrevView * World.camera.Projection,
                    World.motionblurType);

            // Swap the velocity buffers
            RenderTarget2D temp = velocityRTLastFrame;
            velocityRTLastFrame = velocityRT;
            velocityRT = temp;
        }

        public static TextureInUse GetIntermediateTexture()
        {
            PresentationParameters pp = PoolGame.device.PresentationParameters;
            return GetIntermediateTexture(pp.BackBufferWidth, pp.BackBufferHeight, pp.BackBufferFormat, pp.MultiSampleType, pp.MultiSampleQuality);
        }

        public static TextureInUse GetIntermediateTexture(int width, int height, SurfaceFormat format)
        {
            return GetIntermediateTexture(width, height, format, MultiSampleType.None, 0);
        }

        public static TextureInUse GetIntermediateTexture(int width, int height, SurfaceFormat format, MultiSampleType msType, int msQuality)
        {
            for (int i = 0; i < renderTargets.Count; i++)
            {
                if (renderTargets[i].inUse == false
                    && height == renderTargets[i].renderTarget.Height
                    && format == renderTargets[i].renderTarget.Format
                    && width == renderTargets[i].renderTarget.Width
                    && msType == renderTargets[i].renderTarget.MultiSampleType
                    && msQuality == renderTargets[i].renderTarget.MultiSampleQuality)
                {
                    renderTargets[i].inUse = true;
                    return renderTargets[i];
                }
            }

            TextureInUse newTexture = new TextureInUse();
            newTexture.renderTarget = new RenderTarget2D(PoolGame.device,
                                                            width,
                                                            height,
                                                            1,
                                                            format,
                                                            msType,
                                                            msQuality,
                                                            RenderTargetUsage.DiscardContents);
            renderTargets.Add(newTexture);
            newTexture.inUse = true;
            return newTexture;
        }
        

        public class TextureInUse
        {
            public RenderTarget2D renderTarget;
            public bool inUse;
            public TextureInUse() { }
            public TextureInUse(RenderTarget2D rt2D, bool use)
            {
                this.renderTarget = rt2D;
                this.inUse = use;
            }
            public void Use() 
            {
                inUse = true;
            }
            public void DontUse()
            {
                inUse = false;
            }
        }



        public static void DistortionParticlesCombine(RenderTarget2D result)
        {
            PoolGame.device.SetRenderTarget(0, result);
            distortionCombineEffect.Parameters["DistortionMap"].SetValue(distortionsample.renderTarget.GetTexture());

            DrawQuad(mainRT.GetTexture(), distortionCombineEffect);
        }
    }
}
