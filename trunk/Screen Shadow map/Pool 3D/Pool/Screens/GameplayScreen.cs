using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNA_PoolGame.Scenarios;
using XNA_PoolGame.Cameras;
using XNA_PoolGame.PoolTables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNA_PoolGame.Controllers;
using XNA_PoolGame.Models.Bloom;
using System.Threading;
using XNA_PoolGame.Screens.Screen_Manager;
using XNA_PoolGame.Models;
using XNA_PoolGame.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace XNA_PoolGame.Screens
{
    public class GameplayScreen : Screen
    {
        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }
        
        public override void LoadContent()
        {
            // CAMERA
            World.camera = new ChaseCamera(PoolGame.game);

            // SCENARIO
            switch (World.scenarioType)
            {
                case ScenarioType.Cribs:
                    World.scenario = new CribsBasement(PoolGame.game);
                    break;

                case ScenarioType.Garage:
                    break;
            }

            // MAIN POOLTABLE
            World.poolTable = new Classic(PoolGame.game);
            World.poolTable.InitialRotation = Matrix.CreateRotationY(MathHelper.Pi);

            LightManager.sphereModel = new BasicModel(PoolGame.game, "Models\\Balls\\newball");
            LightManager.sphereModel.isObjectAtScenario = false;
            LightManager.sphereModel.Position = LightManager.lights.Position;
            LightManager.sphereModel.LoadContent();

            /////// PLAYERS
            World.playerCount = 1;
            World.playerInTurn = 0;
            World.players[0] = new Player(PoolGame.game, (int)PlayerIndex.One, new KBoard(PlayerIndex.One), TeamNumber.One, World.poolTable);

            PoolGame.game.Components.Add(World.camera);
            PoolGame.game.Components.Add(World.scenario);
            PoolGame.game.Components.Add(World.poolTable);

            

            for (int i = 0; i < 4; i++)
                if (World.players[i] != null) PoolGame.game.Components.Add(World.players[i]);

            World.scenario.scene.Add(World.poolTable);

            // A real game would probably have more content than this sample, so
            // it would take longer to load. We simulate that by delaying for a
            // while, giving you a chance to admire the beautiful loading screen.
            Thread.Sleep(1000);

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }

        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            World.playerInTurn = -1;
            World.playerCount = 0;

            for (int i = 0; i < 4; i++)
                if (World.players[i] != null) World.players[i].Dispose();

            World.scenario.Dispose();
            World.poolTable.Dispose();
            World.camera.Dispose();
        }
        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            //if (IsActive)
            //{
            //}
        }

        public static BloomSettings previousSettings;
        public static IntermediateBuffer previousshowBuffer;
        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleMenuController(MenuController input)
        {
            if (input == null)
                throw new ArgumentNullException("input");


            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];




            if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
            {
                PoolGame.game.DefreezeComponentUpdates(false);

                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            }
            else
            {
                
            }
        }
        private void DrawScene(GameTime gameTime)
        {
            for (int i = 0; i < World.scenario.scene.Count; ++i)
                if (World.scenario.scene[i].Visible) World.scenario.scene[i].Draw(gameTime);
            
        }

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            PoolGame.game.PrepareRenderStates();

            #region DOF

            PostProcessManager.ChangeRenderMode(RenderMode.DoF);
            PostProcessManager.CreateDOFMap();
            DrawScene(gameTime);

            #endregion

            #region SHADOW MAPPING
            if (World.displayShadows)
            {
                
                PostProcessManager.ChangeRenderMode(RenderMode.ShadowMapRender);
                PostProcessManager.RenderShadowMap();
                                
                DrawScene(gameTime);

                PostProcessManager.ChangeRenderMode(RenderMode.PCFShadowMapRender);
                PostProcessManager.RenderPCFShadowMap();

                DrawScene(gameTime);

                PostProcessManager.ChangeRenderMode(RenderMode.ScreenSpaceSoftShadowRender);
                PostProcessManager.RenderSSSoftShadow();

                DrawScene(gameTime);

            }
            else
            {
                PostProcessManager.ChangeRenderMode(RenderMode.BasicRender);
                PostProcessManager.RenderTextured();

                DrawScene(gameTime);
            }
            #endregion
                                    
            #region LIGHT'S POINT
            //PoolGame.game.PrepareRenderStates();
            //PostProcessManager.ChangeRenderMode(RenderMode.BasicRender);
            //if (LightManager.sphereModel != null) LightManager.sphereModel.Draw(gameTime);
            #endregion

            #region PARTICLES
            World.scenario.SetParticleSettings();
            World.scenario.Draw(gameTime);

            //PoolGame.device.RenderState.AlphaTestEnable = false;
            #endregion

            #region BLOOM
            PoolGame.game.PrepareRenderStates();
            if (PoolGame.game.DrawBloom)
            {
                PostProcessManager.ChangeRenderMode(RenderMode.Bloom);
                PostProcessManager.DrawBloomPostProcessing(gameTime);
            }
            else
            {
                PoolGame.device.SetRenderTarget(0, null);

                PostProcessManager.spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);
                PostProcessManager.spriteBatch.Draw(PostProcessManager.mainRT.GetTexture(), Vector2.Zero, Color.White);
                PostProcessManager.spriteBatch.End();
            }
            #endregion

            #region DOF COMBINE
            PostProcessManager.ChangeRenderMode(RenderMode.DOFCombine);
            PostProcessManager.CombineDOF();
            #endregion

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(255 - TransitionAlpha);
        }


        #endregion
    }
}
