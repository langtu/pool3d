

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using XNA_PoolGame.Models;
using XNA_PoolGame.Graphics;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Graphics.Shadows;
using XNA_PoolGame.Models.Bloom;

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
        //public static Effect gaussianBlurEffect2;
        //public static Effect shadowMapEffect; // FOR VARIANCE SHADOW MAPPING
        //public static Effect depthEffect; // FOR VARIANCE SHADOW MAPPING
        public static Effect SSSoftShadow;
        public static Effect PCFShadowMap;
        public static Effect GBlurH;
        public static Effect GBlurV;
        public static Effect Depth;
        public static Effect DoFCombine;
        public static BasicEffect basicEffect;

        // EFFECTS PARAMETERS
        public static EffectParameter depthViewParam;
        public static EffectParameter depthFarClipParam;
        public static EffectParameter depthProjectionParam;

        // SHADER
        public static RenderMode currentRenderMode = RenderMode.BasicRender;

        // SHADOW MAPPING
        public static Texture2D depthBlurred;        
        //public static DepthStencilBuffer shadowDepthBuffer;
        public static DepthStencilBuffer stencilBuffer;
        public static DepthStencilBuffer oldBuffer;
        public static int shadowMapSize;
        public static float[] weights;
        public static Vector2[] offsets;
        public static float depthBias;
        public static Shadow ShadowTechnique;
        public static Vector2[] pcfSamples = new Vector2[9];

        // RENDER TARGETS
        public static RenderTarget2D ShadowMapRT;
        public static RenderTarget2D ShadowRT;
        public static RenderTarget2D halfRTVert;
        public static RenderTarget2D halfRTHor;
        public static RenderTarget2D GBlurHRT;
        public static RenderTarget2D GBlurVRT;
        public static RenderTarget2D mainRT;
        public static RenderTarget2D DepthOfFieldRT;
        
        //
        public static ResolveTexture2D resolveTarget;

        // BLOOM COMPONENT
        public static BloomSettings bloomSettings;
        public static IntermediateBuffer showBuffer = IntermediateBuffer.FinalResult;

        // BLUR
        public static GaussianBlur gaussianBlur;
        private const int BLUR_RADIUS = 7;
        private const float BLUR_AMOUNT = 2.0f;

        public static void Load()
        {
            spriteBatch = new SpriteBatch(PoolGame.device);

            shadowMapSize = 1024;
            depthBias = 0.0042f; // 0.0035f;
            ShadowTechnique = Shadow.Normal;

            float texelSize = 0.75f / (float)shadowMapSize;

            pcfSamples[0] = new Vector2(0.0f, 0.0f);
            pcfSamples[1] = new Vector2(-texelSize, 0.0f);
            pcfSamples[2] = new Vector2(texelSize, 0.0f);
            pcfSamples[3] = new Vector2(0.0f, -texelSize);
            pcfSamples[4] = new Vector2(-texelSize, -texelSize);
            pcfSamples[5] = new Vector2(texelSize, -texelSize);
            pcfSamples[6] = new Vector2(0.0f, texelSize);
            pcfSamples[7] = new Vector2(-texelSize, texelSize);
            pcfSamples[8] = new Vector2(texelSize, texelSize);

            bloomExtractEffect = PoolGame.content.Load<Effect>("Effects\\BloomExtract");
            bloomCombineEffect = PoolGame.content.Load<Effect>("Effects\\BloomCombine");
            gaussianBlurEffect = PoolGame.content.Load<Effect>("Effects\\GaussianBlur");
            saturation = PoolGame.content.Load<Effect>("Effects\\Saturate");
            modelEffect = PoolGame.content.Load<Effect>("Effects\\ModelEffect");
            DoFCombine = PoolGame.content.Load<Effect>("Effects\\DofCombine");
            Depth = PoolGame.content.Load<Effect>("Effects\\Depth");
            PCFShadowMap = PoolGame.content.Load<Effect>("Effects\\PCFSM");
            GBlurH = PoolGame.content.Load<Effect>("Effects\\GBlurH");
            GBlurV = PoolGame.content.Load<Effect>("Effects\\GBlurV");
            SSSoftShadow = PoolGame.content.Load<Effect>("Effects\\ScreenSpaceSoftShadow");

            basicEffect = new BasicEffect(PoolGame.device, null);

            SetBlurEffectParameters(1.5f / PoolGame.device.Viewport.Width, 0, GBlurH);
            SetBlurEffectParameters(0, 1.5f / PoolGame.device.Viewport.Height, GBlurV);

            gaussianBlur = new GaussianBlur(PoolGame.game);
            gaussianBlur.ComputeKernel(BLUR_RADIUS, BLUR_AMOUNT);
        }

        #region Gaussian Helper
        private static void SetBlurEffectParameters(float dx, float dy, Effect effect)
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
        public static void RenderShadowMap()
        {
            PoolGame.device.RenderState.DepthBufferEnable = true;
            PoolGame.device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            //Render Shadow Map
            oldBuffer = PoolGame.device.DepthStencilBuffer;
            PoolGame.device.DepthStencilBuffer = stencilBuffer;
            PoolGame.device.SetRenderTarget(0, ShadowMapRT);
            PoolGame.device.Clear(Color.White);
        }

        public static void RenderPCFShadowMap()
        {
            //Render PCF Shadow Map
            PoolGame.device.DepthStencilBuffer = oldBuffer;
            PoolGame.device.SetRenderTarget(0, ShadowRT);
            PoolGame.device.Clear(Color.White);

            PoolGame.device.RenderState.CullMode = CullMode.None;
            //PoolGame.device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, Color.White, 1.0f, 0);
        }

        public static void RenderSSSoftShadow()
        {
            if (ShadowTechnique == Shadow.SoftShadow)
            {
                SetBlurEffectParameters(0.5f / PoolGame.device.Viewport.Width, 0, GBlurH);
                SetBlurEffectParameters(0, 0.5f / PoolGame.device.Viewport.Height, GBlurV);

                //Gaussian Blur H
                PoolGame.device.SetRenderTarget(0, GBlurHRT);
                DrawQuad(ShadowRT.GetTexture(), GBlurH);

                //Guassian Blur V
                PoolGame.device.SetRenderTarget(0, GBlurVRT);
                DrawQuad(GBlurHRT.GetTexture(), GBlurV);
                
                //PoolGame.device.SetRenderTarget(0, null);
                //prueba = gaussianBlur.PerformGaussianBlur(ShadowRT.GetTexture(), PostProcessManager.halfRTHor, PostProcessManager.halfRTVert, spriteBatch);

                SetBlurEffectParameters(1.5f / PoolGame.device.Viewport.Width, 0, GBlurH);
                SetBlurEffectParameters(0, 1.5f / PoolGame.device.Viewport.Height, GBlurV);
            }

            //Screen Space SoftShadow
            //PoolGame.device.SetRenderTarget(0, null);
            PoolGame.device.SetRenderTarget(0, mainRT);
            PoolGame.device.Clear(Color.CornflowerBlue);

            PoolGame.device.RenderState.DepthBufferEnable = true;
            PoolGame.device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
        }
        #endregion
        
        #region DOF
        //public static Texture2D dofMapTex;
        public static void CreateDOFMap()
        {
            Effect depth = Depth;

            PoolGame.device.SetRenderTarget(0, DepthOfFieldRT);
            PoolGame.device.Clear(Color.White);

            depth.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);
            depth.Parameters["MaxDepth"].SetValue(World.camera.FarPlane);
        }

        public static void CombineDOF()
        {
            PoolGame.device.SetRenderTarget(0, null);

            PoolGame.device.ResolveBackBuffer(PostProcessManager.resolveTarget);

            //Gaussian Blur H
            PoolGame.device.SetRenderTarget(0, GBlurHRT);
            DrawQuad(resolveTarget, GBlurH);

            //Guassian Blur V
            PoolGame.device.SetRenderTarget(0, GBlurVRT);
            DrawQuad(GBlurHRT.GetTexture(), GBlurV);

            

            /*Texture2D endTexture = PostProcessManager.mainRT.GetTexture();
            Rectangle rect = new Rectangle(0, 0, 128, 128);

            spriteBatch.Begin(SpriteBlendMode.None);
            
            spriteBatch.Draw(endTexture, rect, Color.White);
            spriteBatch.End();*/

            PoolGame.device.SetRenderTarget(0, null);
            PoolGame.device.Textures[0] = resolveTarget;
            PoolGame.device.Textures[1] = GBlurVRT.GetTexture();
            PoolGame.device.Textures[2] = DepthOfFieldRT.GetTexture();
            DrawQuad(resolveTarget, DoFCombine);
        }
        #endregion

        public static void RenderTextured()
        {
            PoolGame.device.SetRenderTarget(0, mainRT);
            PoolGame.device.Clear(Color.CornflowerBlue);
        }

        #region Initialize Render Targets
        public static void InitRenderTargets()
        {
            int renderTargetWidth = PoolGame.device.Viewport.Width / 2;
            int renderTargetHeight = PoolGame.device.Viewport.Height / 2;

            PresentationParameters pp = PoolGame.device.PresentationParameters;
            
            int width = pp.BackBufferWidth;
            int height = pp.BackBufferHeight;

            SurfaceFormat format = pp.BackBufferFormat;

            //depthBlurH = new RenderTarget2D(PoolGame.device, shadowMapSize, shadowMapSize, 1, SurfaceFormat.Rg32);
            //depthBlurV = new RenderTarget2D(PoolGame.device, shadowMapSize, shadowMapSize, 1, SurfaceFormat.Rg32);

            halfRTHor = new RenderTarget2D(PoolGame.device,
                renderTargetWidth, renderTargetHeight, 1,
                format, pp.MultiSampleType,
                pp.MultiSampleQuality);

            halfRTVert = new RenderTarget2D(PoolGame.device,
                renderTargetWidth, renderTargetHeight, 1,
                format, pp.MultiSampleType, pp.MultiSampleQuality);

            ShadowMapRT = new RenderTarget2D(PoolGame.device, shadowMapSize, shadowMapSize, 1, pp.BackBufferFormat);
            ShadowRT = new RenderTarget2D(PoolGame.device, pp.BackBufferWidth, pp.BackBufferHeight, 1, PoolGame.device.DisplayMode.Format);
            GBlurHRT = new RenderTarget2D(PoolGame.device, pp.BackBufferWidth, pp.BackBufferHeight, 1, pp.BackBufferFormat);
            GBlurVRT = new RenderTarget2D(PoolGame.device, pp.BackBufferWidth, pp.BackBufferHeight, 1, pp.BackBufferFormat);
            DepthOfFieldRT = new RenderTarget2D(PoolGame.device, pp.BackBufferWidth, pp.BackBufferHeight, 1, pp.BackBufferFormat);
            stencilBuffer = new DepthStencilBuffer(PoolGame.device, shadowMapSize, shadowMapSize, PoolGame.device.DepthStencilBuffer.Format);

            mainRT = new RenderTarget2D(PoolGame.device, pp.BackBufferWidth, pp.BackBufferHeight, 1, format);


            resolveTarget = new ResolveTexture2D(PoolGame.device, width, height, 1, format);
            
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

        #region Unload
        public static void UnloadContent()
        {
            spriteBatch.Dispose();

            // EFFECT'S DISPOSING
            bloomExtractEffect.Dispose();
            bloomCombineEffect.Dispose();
            gaussianBlurEffect.Dispose();
            SSSoftShadow.Dispose();
            PCFShadowMap.Dispose();
            GBlurH.Dispose();
            GBlurV.Dispose();
            Depth.Dispose();

            // DISPOSE RENDER TARGETS
            ShadowMapRT.Dispose();
            ShadowRT.Dispose();
            GBlurHRT.Dispose();
            GBlurVRT.Dispose();
            halfRTHor.Dispose();
            halfRTVert.Dispose();
            
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

            saturation.Parameters["saturation"].SetValue(sat);

            DrawQuad(resolveTarget, saturation);
        }

        #region Bloom
        public static void DrawBloomPostProcessing(GameTime gameTime)
        {
            bloomExtractEffect.Parameters["BloomThreshold"].SetValue(bloomSettings.BloomThreshold);

            PoolGame.device.SetRenderTarget(0, halfRTHor);
            DrawQuad(mainRT.GetTexture(), bloomExtractEffect, IntermediateBuffer.PreBloom);

            SetBlurEffectParameters(1.0f / (float)halfRTHor.Width, 0, gaussianBlurEffect);


            PoolGame.device.SetRenderTarget(0, halfRTVert);
            DrawQuad(halfRTHor.GetTexture(), gaussianBlurEffect, IntermediateBuffer.BlurredHorizontally);


            SetBlurEffectParameters(0, 1.0f / (float)halfRTHor.Height, gaussianBlurEffect);


            PoolGame.device.SetRenderTarget(0, halfRTHor);
            DrawQuad(halfRTVert.GetTexture(), PostProcessManager.gaussianBlurEffect, IntermediateBuffer.BlurredBothWays);

            PoolGame.device.SetRenderTarget(0, null);

            EffectParameterCollection parameters = bloomCombineEffect.Parameters;

            parameters["BloomIntensity"].SetValue(bloomSettings.BloomIntensity);
            parameters["BaseIntensity"].SetValue(bloomSettings.BaseIntensity);
            parameters["BloomSaturation"].SetValue(bloomSettings.BloomSaturation);
            parameters["BaseSaturation"].SetValue(bloomSettings.BaseSaturation);

            PoolGame.device.Textures[1] = mainRT.GetTexture();

            DrawQuad(halfRTHor.GetTexture(), PostProcessManager.bloomCombineEffect, IntermediateBuffer.FinalResult);

        }

        #endregion


        
    }
}
