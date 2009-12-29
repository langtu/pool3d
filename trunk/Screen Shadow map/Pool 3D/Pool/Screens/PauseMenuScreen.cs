using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Models.Bloom;
using Microsoft.Xna.Framework.Graphics;
using XNA_PoolGame.Graphics;

namespace XNA_PoolGame.Screens
{
    /// <summary>
    /// The pause menu comes up over the top of the game,
    /// giving the player options to resume or quit.
    /// </summary>
    public class PauseMenuScreen : MenuScreen
    {
        #region Fields
        
        BloomSettings pauseMenuBloomSettings = BloomSettings.PresetSettings[4];
        
        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public PauseMenuScreen()
            : base("Paused")
        {
            // Flag that there is no need for the game to transition
            // off when the pause menu is on top of it.
            IsPopup = true;

            // Create our menu entries.
            MenuEntry resumeGameMenuEntry = new MenuEntry("Resume Game");
            MenuEntry quitGameMenuEntry = new MenuEntry("Quit Game");

            // Hook up menu event handlers.
            resumeGameMenuEntry.Selected += ResumeGameSelected;
            quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;

            // Add entries to the menu.
            MenuEntries.Add(resumeGameMenuEntry);
            MenuEntries.Add(quitGameMenuEntry);
        }


        #endregion

        #region Handle Input
        
        /// <summary>
        /// Event handler for when the Quit Game menu entry is selected.
        /// </summary>
        void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(), new MainMenuScreen());

            World.camera.SetMouseCentered();
            PoolGame.game.DefreezeComponentUpdates(true);
        }

        protected override void OnCancel(PlayerIndex playerIndex)
        {
            World.camera.SetMouseCentered();
            PoolGame.game.DefreezeComponentUpdates(true);
            base.OnCancel(playerIndex);
        }

        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to quit" message box. This uses the loading screen to
        /// transition from the game back to the main menu screen.
        /// </summary>
        void ResumeGameSelected(object sender, PlayerIndexEventArgs e)
        {

            OnCancel(e.PlayerIndex);
        }


        #endregion

        #region Draw


        /// <summary>
        /// Draws the pause menu screen. This darkens down the gameplay screen
        /// that is underneath us, and then chains to the base MenuScreen.Draw.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

            if (ScreenState == ScreenState.Active || ScreenState == ScreenState.TransitionOn) 
            {
                SpriteBatch sprite = PostProcessManager.spriteBatch;
                GaussianBlur gauss = PostProcessManager.gaussianBlur;

                PoolGame.device.ResolveBackBuffer(PostProcessManager.resolveTarget);
                Texture2D backtex = PostProcessManager.resolveTarget;

                Texture2D tex = gauss.PerformGaussianBlur(backtex, PostProcessManager.halfRTHor, PostProcessManager.halfRTVert, sprite);
                sprite.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);
                sprite.Draw(tex, new Rectangle(0, 0, PoolGame.Width, PoolGame.Height), Color.White);
                sprite.End();
            }

            base.Draw(gameTime);
        }


        #endregion
    }
}
