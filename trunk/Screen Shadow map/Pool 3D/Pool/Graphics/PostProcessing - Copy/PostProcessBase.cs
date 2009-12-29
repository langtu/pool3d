using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Extreme_Pool.Models.PostProcessing
{
    class PostProcessBase
    {
        public ContentManager content;
        public GraphicsDevice graphics;
        public Effect effect;
        public SpriteBatch spriteBatch;
        public Texture2D backBuffer;
        public Texture2D inputTexture;
        protected Texture2D depthBuffer;
        public RenderTarget2D outputRT;
        public bool IsFinal;
        public bool IsEnabled;

        public SpriteBlendMode blendMode;
        public SpriteSortMode sortMode;
        public SaveStateMode saveMode;

        public PostProcessBase()
        {
            //default to !final effect in the chain
            IsFinal = false;

            //default to enabled
            IsEnabled = true;

            blendMode = SpriteBlendMode.None;
            sortMode = SpriteSortMode.Immediate;
            saveMode = SaveStateMode.None;
        }
        /// <summary>
        /// Draws a full screen quad to the screen using the provided RenterTarget. Automatically
        /// decides whether to draw to the render target or to the backbuffer based on the component's
        /// index in the effects chain
        /// </summary>
        /// <param name="texture">Texture used to draw the sprite batch</param>
        /// <param name="renderTarget">Render Target to draw to</param>
        /// <param name="effect">The effect to manipulate the full screen quad</param>
        protected bool DrawFullscreenQuad(Texture2D texture, RenderTarget2D renderTarget, Effect effect)
        {
            //set the render target. render directly to the backbuffer if this effect is the last in the chain
            if (!IsFinal)
            {
                graphics.SetRenderTarget(0, renderTarget);

                DrawFullscreenQuad(texture,
                                   renderTarget.Width, renderTarget.Height,
                                   effect);

                graphics.SetRenderTarget(0, null);

                return true;
            }

            DrawFullscreenQuad(texture, backBuffer.Width, backBuffer.Height, effect);

            return false; //didn't resolve the render target
        }

        /// <summary>
        /// Draws a full screen quad to the screen using the provided RenterTarget. Automatically
        /// decides whether to draw to the render target or to the backbuffer based on the component's
        /// index in the effects chain
        /// </summary>
        /// <param name="texture">Texture used to draw the sprite batch</param>
        /// <param name="renderTarget">Render Target to draw to</param>
        /// <param name="effect">The effect to manipulate the full screen quad</param>
        /// <param name="color">The color channel used modulation to use</param>
        protected bool DrawFullscreenQuad(Texture2D texture, RenderTarget2D renderTarget, Effect effect, Color color)
        {
            //set the render target. render directly to the backbuffer if this effect is the last in the chain
            if (!IsFinal)
            {
                graphics.SetRenderTarget(0, renderTarget);

                DrawFullscreenQuad(texture,
                                   renderTarget.Width, renderTarget.Height,
                                   effect);

                graphics.SetRenderTarget(0, null);

                return true;
            }

            DrawFullscreenQuad(texture, backBuffer.Width, backBuffer.Height, effect);

            return false; //didn't resolve the render target
        }

        /// <summary>
        /// Draws a full screen quad to the backbuffer or the current RenterTarget
        /// </summary>
        /// <param name="texture">Texture used to draw the sprite batch</param>
        /// <param name="width">Width of the Render Target or Backbuffer</param>
        /// <param name="height">Height of the Render Target or Backbuffer</param>
        /// <param name="effect">The effect to manipulate the full screen quad</param>
        protected void DrawFullscreenQuad(Texture2D texture, int width, int height, Effect effect)
        {
            spriteBatch.Begin(blendMode, sortMode, saveMode);

            //begin the shader effect, only need 1 pass
            if (effect != null)
            {
                effect.Begin();
                effect.CurrentTechnique.Passes[0].Begin();
            }

            spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
            spriteBatch.End();

            if (effect != null)
            {
                effect.CurrentTechnique.Passes[0].End();
                effect.End();
            }
        }

        /// <summary>
        /// Draws a full screen quad to the backbuffer or the current RenterTarget
        /// </summary>
        /// <param name="texture">Texture used to draw the sprite batch</param>
        /// <param name="width">Width of the Render Target or Backbuffer</param>
        /// <param name="height">Height of the Render Target or Backbuffer</param>
        /// <param name="effect">The effect to manipulate the full screen quad</param>
        /// <param name="color">The color channel used modulation to use</param>
        protected void DrawFullscreenQuad(Texture2D texture, int width, int height, Effect effect, Color color)
        {
            spriteBatch.Begin(blendMode, sortMode, saveMode);

            //begin the shader effect, only need 1 pass
            if (effect != null)
            {
                effect.Begin();
                effect.CurrentTechnique.Passes[0].Begin();
            }

            spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), color);
            spriteBatch.End();

            if (effect != null)
            {
                effect.CurrentTechnique.Passes[0].End();
                effect.End();
            }
        }

        /// <summary>
        /// Draws a full screen quad to the screen using the provided RenterTarget. Ensures that the
        /// render target is used.
        /// </summary>
        /// <param name="texture">Texture used to draw the sprite batch</param>
        /// <param name="renderTarget">Render Target to draw to</param>
        /// <param name="effect">The effect to manipulate the full screen quad</param>
        protected void DrawFullscreenQuadUseRT(Texture2D texture, RenderTarget2D renderTarget, Effect effect)
        {
            //set the render target. 
            graphics.SetRenderTarget(0, renderTarget);

            DrawFullscreenQuad(texture,
                               renderTarget.Width, renderTarget.Height,
                               effect);

            graphics.SetRenderTarget(0, null);
        }

        /// <summary>
        /// Draws a full screen quad to the screen using the provided RenterTarget. Ensures that the
        /// render target is used.
        /// </summary>
        /// <param name="texture">Texture used to draw the sprite batch</param>
        /// <param name="renderTarget">Render Target to draw to</param>
        /// <param name="effect">The effect to manipulate the full screen quad</param>
        /// <param name="color">The color channel used modulation to use</param>
        protected void DrawFullscreenQuadUseRT(Texture2D texture, RenderTarget2D renderTarget, Effect effect, Color color)
        {
            //set the render target. 
            graphics.SetRenderTarget(0, renderTarget);

            DrawFullscreenQuad(texture,
                               renderTarget.Width, renderTarget.Height,
                               effect, color);

            graphics.SetRenderTarget(0, null);
        }
        public virtual void Draw(GameTime gameTime) { }

        public virtual void Update(GameTime gameTime) { }

        public virtual void LoadContent()
        {
            spriteBatch = new SpriteBatch(graphics);

            PresentationParameters pp = graphics.PresentationParameters;

            int width = pp.BackBufferWidth;
            int height = pp.BackBufferHeight;

            SurfaceFormat format = pp.BackBufferFormat;

            backBuffer = new Texture2D(graphics, width, height, 1, TextureUsage.None, format);
            //sceneTexture = new Texture2D(graphics, width, height, 1, TextureUsage.None, format);
        }

        public virtual void UnLoadContent()
        {
            spriteBatch.Dispose();
            backBuffer.Dispose();
        }
    }
}
