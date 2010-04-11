using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using XNA_PoolGame.Graphics.Bloom;
using XNA_PoolGame.Graphics;

namespace XNA_PoolGame.Screens
{
    public class BackgroundScreen : Screen
    {
        #region Fields

        //ContentManager content;
        string textureAsset;
        Texture2D backgroundTexture;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public BackgroundScreen(string _textureAsset)
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
            this.textureAsset = _textureAsset;
        }
        public BackgroundScreen() 
            : this("Textures\\MainMenu\\background")
        {
            
        }

        /// <summary>
        /// Loads graphics content for this screen. The background texture is quite
        /// big, so we use our own local ContentManager to load it. This allows us
        /// to unload before going from the menus into the game itself, wheras if we
        /// used the shared ContentManager provided by the Game class, the content
        /// would remain loaded forever.
        /// </summary>
        public override void LoadContent()
        {
            //if (content == null)
            //    content = new ContentManager(ScreenManager.Game.Services, "Content");

            backgroundTexture = PoolGame.content.Load<Texture2D>(textureAsset);
        }


        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void UnloadContent()
        {

        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the background screen. Unlike most screens, this should not
        /// transition off even if it has been covered by another screen: it is
        /// supposed to be covered, after all! This overload forces the
        /// coveredByOtherScreen parameter to false in order to stop the base
        /// Update method wanting to transition off.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);
        }


        /// <summary>
        /// Draws the background screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);
            byte fade = TransitionAlpha;

            //Texture2D result = PostProcessManager.gaussianBlur.PerformGaussianBlur(backgroundTexture, PostProcessManager.renderTargets[3],
            //    PostProcessManager.renderTargets[4], spriteBatch, fullscreen);

            Texture2D result = backgroundTexture;

            spriteBatch.Begin(SpriteBlendMode.None);
            spriteBatch.Draw(result, fullscreen, new Color(fade, fade, fade));
            spriteBatch.End();



        }


        #endregion
    }
}
