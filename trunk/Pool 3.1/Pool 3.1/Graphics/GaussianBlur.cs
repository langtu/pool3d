using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace XNA_PoolGame.Graphics
{
    public class GaussianBlur
    {
        private Game game;
        private Effect effect;
        private int radius;
        private float amount;
        private float sigma;
        private float[] kernel;
        private Vector2[] offsetsHoriz;
        private Vector2[] offsetsVert;

        /// <summary>
        /// Returns the radius of the Gaussian blur filter kernel in pixels.
        /// </summary>
        public int Radius
        {
            get { return radius; }
        }

        /// <summary>
        /// Returns the blur amount. This value is used to calculate the
        /// Gaussian blur filter kernel's sigma value. Good values for this
        /// property are 2 and 3. 2 will give a more blurred result whilst 3
        /// will give a less blurred result with sharper details.
        /// </summary>
        public float Amount
        {
            get { return amount; }
        }

        /// <summary>
        /// Returns the Gaussian blur filter's standard deviation.
        /// </summary>
        public float Sigma
        {
            get { return sigma; }
            set { sigma = value; }
        }

        /// <summary>
        /// Returns the Gaussian blur filter kernel matrix. Note that the
        /// kernel returned is for a 1D Gaussian blur filter kernel matrix
        /// intended to be used in a two pass Gaussian blur operation.
        /// </summary>
        public float[] Kernel
        {
            get { return kernel; }
        }

        /// <summary>
        /// Returns the texture offsets used for the horizontal Gaussian blur
        /// pass.
        /// </summary>
        public Vector2[] TextureOffsetsX
        {
            get { return offsetsHoriz; }
        }

        /// <summary>
        /// Returns the texture offsets used for the vertical Gaussian blur
        /// pass.
        /// </summary>
        public Vector2[] TextureOffsetsY
        {
            get { return offsetsVert; }
        }

        /// <summary>
        /// Default constructor for the GaussianBlur class. This constructor
        /// should be called if you don't want the GaussianBlur class to use
        /// its GaussianBlur.fx effect file to perform the two pass Gaussian
        /// blur operation.
        /// </summary>
        public GaussianBlur()
        {
        }

        /// <summary>
        /// This overloaded constructor instructs the GaussianBlur class to
        /// load and use its GaussianBlur.fx effect file that implements the
        /// two pass Gaussian blur operation on the GPU. The effect file must
        /// be already bound to the asset name: 'Effects\GaussianBlur' or
        /// 'GaussianBlur'.
        /// </summary>
        public GaussianBlur(Game game)
        {
            this.game = game;

            effect = PostProcessManager.gaussianBlurEffect;
        }

        /// <summary>
        /// Calculates the Gaussian blur filter kernel. This implementation is
        /// ported from the original Java code appearing in chapter 16 of
        /// "Filthy Rich Clients: Developing Animated and Graphical Effects for
        /// Desktop Java".
        /// </summary>
        /// <param name="blurRadius">The blur radius in pixels.</param>
        /// <param name="blurAmount">Used to calculate sigma.</param>
        public void ComputeKernel(int blurRadius, float blurAmount)
        {
            radius = blurRadius;
            amount = blurAmount;

            kernel = null;
            kernel = new float[radius * 2 + 1];
            sigma = radius / amount;

            float twoSigmaSquare = 2.0f * sigma * sigma;
            float sigmaRoot = (float)Math.Sqrt(twoSigmaSquare * Math.PI);
            float total = 0.0f;
            float distance = 0.0f;
            int index = 0;

            for (int i = -radius; i <= radius; ++i)
            {
                distance = i * i;
                index = i + radius;
                kernel[index] = (float)Math.Exp(-distance / twoSigmaSquare) / sigmaRoot;
                total += kernel[index];
            }

            for (int i = 0; i < kernel.Length; ++i)
                kernel[i] /= total;
        }

        /// <summary>
        /// Calculates the texture coordinate offsets corresponding to the
        /// calculated Gaussian blur filter kernel. Each of these offset values
        /// are added to the current pixel's texture coordinates in order to
        /// obtain the neighboring texture coordinates that are affected by the
        /// Gaussian blur filter kernel. This implementation has been adapted
        /// from chapter 17 of "Filthy Rich Clients: Developing Animated and
        /// Graphical Effects for Desktop Java".
        /// </summary>
        /// <param name="textureWidth">The texture width in pixels.</param>
        /// <param name="textureHeight">The texture height in pixels.</param>
        public void ComputeOffsets(float textureWidth, float textureHeight)
        {
            offsetsHoriz = null;
            offsetsHoriz = new Vector2[radius * 2 + 1];

            offsetsVert = null;
            offsetsVert = new Vector2[radius * 2 + 1];

            int index = 0;
            float xOffset = 1.0f / textureWidth;
            float yOffset = 1.0f / textureHeight;

            for (int i = -radius; i <= radius; ++i)
            {
                index = i + radius;
                offsetsHoriz[index] = new Vector2(i * xOffset, 0.0f);
                offsetsVert[index] = new Vector2(0.0f, i * yOffset);
            }
        }

        /// <summary>
        /// Performs the Gaussian blur operation on the source texture image.
        /// The Gaussian blur is performed in two passes: a horizontal blur
        /// pass followed by a vertical blur pass. The output from the first
        /// pass is rendered to renderTarget1. The output from the second pass
        /// is rendered to renderTarget2. The dimensions of the blurred texture
        /// is therefore equal to the dimensions of renderTarget2.
        /// </summary>
        /// <param name="srcTexture">The source image to blur.</param>
        /// <param name="renderTarget1">Stores the output from the horizontal blur pass.</param>
        /// <param name="renderTarget2">Stores the output from the vertical blur pass.</param>
        /// <param name="spriteBatch">Used to draw quads for the blur passes.</param>
        /// <returns>The resulting Gaussian blurred image.</returns>
        public Texture2D PerformGaussianBlur(Texture2D srcTexture,
                                             RenderTarget2D renderTarget1,
                                             RenderTarget2D renderTarget2,
                                             SpriteBatch spriteBatch)
        {
            if (effect == null)
                throw new InvalidOperationException("GaussianBlur.fx effect not loaded.");

            Texture2D outputTexture = null;
            Rectangle srcRect = new Rectangle(0, 0, srcTexture.Width, srcTexture.Height);
            Rectangle destRect1 = new Rectangle(0, 0, renderTarget1.Width, renderTarget1.Height);
            Rectangle destRect2 = new Rectangle(0, 0, renderTarget2.Width, renderTarget2.Height);

            effect.CurrentTechnique = effect.Techniques["GaussianBlur"];
            effect.Parameters["SampleWeights"].SetValue(kernel);

            // Perform horizontal Gaussian blur.

            game.GraphicsDevice.SetRenderTarget(0, renderTarget1);

            effect.Parameters["colorMapTexture"].SetValue(srcTexture);
            effect.Parameters["SampleOffsets"].SetValue(offsetsHoriz);

            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);
            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();

            spriteBatch.Draw(srcTexture, destRect1, Color.White);
            spriteBatch.End();

            effect.CurrentTechnique.Passes[0].End();
            effect.End();

            // Perform vertical Gaussian blur.

            game.GraphicsDevice.SetRenderTarget(0, renderTarget2);
            outputTexture = renderTarget1.GetTexture();

            effect.Parameters["colorMapTexture"].SetValue(outputTexture);
            effect.Parameters["SampleOffsets"].SetValue(offsetsVert);

            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);
            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();

            spriteBatch.Draw(outputTexture, destRect2, Color.White);
            spriteBatch.End();

            effect.CurrentTechnique.Passes[0].End();
            effect.End();

            // Return the Gaussian blurred texture.

            game.GraphicsDevice.SetRenderTarget(0, null);
            outputTexture = renderTarget2.GetTexture();

            return outputTexture;
        }
    }
}
