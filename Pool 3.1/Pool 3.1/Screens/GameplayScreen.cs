using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNA_PoolGame.Scenarios;
using XNA_PoolGame.Cameras;
using XNA_PoolGame.PoolTables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNA_PoolGame.GameControllers;
using XNA_PoolGame.Graphics.Bloom;
using System.Threading;
using XNA_PoolGame.Screens.Screen_Manager;
using XNA_PoolGame.Graphics;
using Microsoft.Xna.Framework.Graphics;
using XNA_PoolGame.Graphics.Models;
using XNA_PoolGame.Graphics.Shadows;

namespace XNA_PoolGame.Screens
{
    /// <summary>
    /// Match screen
    /// </summary>
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
            World.camera.CameraPosition = Vector3.Zero;
            World.camera.FarPlane = 5500.0f;

            // SET SHADOW TECHNIQUE
            if (PostProcessManager.shadows != null) PostProcessManager.shadows.Dispose();
            switch (World.shadowTechnique)
            {
                case ShadowTechnnique.PSMShadowMapping:
                    PostProcessManager.shadows = new PSMShadowMapping();
                    break;
                case ShadowTechnnique.ScreenSpaceShadowMapping:
                    PostProcessManager.shadows = new ScreenSpaceShadowMapping();
                    break;
            }

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
            World.poolTable.PreRotation = Matrix.CreateRotationY(MathHelper.Pi);
            World.poolTable.DrawOrder = 1;

            //World.poolTable.UseThread = true;

            LightManager.sphereModel = new Entity(PoolGame.game, "Models\\Balls\\newball", VolumeType.BoundingSpheres);
            LightManager.sphereModel.isObjectAtScenario = false;
            LightManager.sphereModel.Position = LightManager.lights[0].Position;
            LightManager.sphereModel.LoadContent();

            /////// PLAYERS
            World.playerCount = 1;
            World.playerInTurn = 0;
            World.players[0] = new Player(PoolGame.game, (int)PlayerIndex.One, new KeyBoard(PlayerIndex.One), TeamNumber.One, World.poolTable);

            PoolGame.game.Components.Add(World.camera);
            PoolGame.game.Components.Add(World.scenario);
            PoolGame.game.Components.Add(World.poolTable);

            World.scenario.Objects.Add(World.poolTable);
           

            for (int i = 0; i < 4; i++)
            {
                if (World.players[i] != null) 
                    PoolGame.game.Components.Add(World.players[i]);
            }


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
            ModelManager.AbortAllThreads();
            World.playerInTurn = -1;
            World.playerCount = 0;

            for (int i = 0; i < 4; i++)
            {
                if (World.players[i] != null)
                {
                    World.players[i].Dispose();
                    World.players[i] = null;
                }
            }

            World.scenario.Dispose();
            World.poolTable.Dispose();
            World.camera.Dispose();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            //GC.Collect();
            //GC.GetTotalMemory(false);
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
            bool gamePadDisconnected = !gamePadState.IsConnected && input.GamePadWasConnected[playerIndex];
            
            //
            if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
            {
                PoolGame.game.DefreezeComponentUpdates(false);

                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            }
            else
            {
                
            }
        }
        

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            PoolGame.game.PrepareRenderStates();
            RenderTarget2D result = null;

            #region SHADOW MAPPING

            if (World.displayShadows)
            {
                PostProcessManager.shadows.Draw(gameTime);
            }
            else
            {
                World.camera.ItemsDrawn = 0;
                PostProcessManager.ChangeRenderMode(RenderMode.BasicRender);
                PostProcessManager.RenderTextured();

                World.scenario.DrawScene(gameTime);
            }

            #endregion

            #region LIGHT'S POINT
            //PoolGame.game.PrepareRenderStates();
            /*PostProcessManager.ChangeRenderMode(RenderMode.BasicRender);
            if (LightManager.sphereModel != null)
            {
                for (int i = 0; i < LightManager.totalLights; ++i)
                {

                    LightManager.sphereModel.Position = new Vector3(LightManager.lights[i].Position.X, LightManager.lights[i].Position.Y, LightManager.lights[i].Position.Z);
                    LightManager.sphereModel.Draw(gameTime);
                    
                }
            }*/
            #endregion

            #region PARTICLES

            World.scenario.SetParticleSettings();
            World.scenario.Draw(gameTime);

            PoolGame.device.SetRenderTarget(0, null);
            if (!(World.motionblurType == MotionBlurType.None && World.dofType == DOFType.None))
            {
                PoolGame.device.SetRenderTarget(1, null);
                PoolGame.device.SetRenderTarget(2, null);
            }
            

            #endregion

            #region MOTION BLUR & DOF

            if (World.motionblurType != MotionBlurType.None)
            {
                if (World.dofType != DOFType.None)
                {
                    PostProcessManager.ChangeRenderMode(RenderMode.MotionBlur);
                    PostProcessManager.RenderMotionBlur(PostProcessManager.mainRT, PostProcessManager.GBlurVRT);

                    PostProcessManager.ChangeRenderMode(RenderMode.DoF);
                    PostProcessManager.DOF(PostProcessManager.GBlurVRT, PostProcessManager.GBlurHRT);
                }
                else
                {
                    PostProcessManager.ChangeRenderMode(RenderMode.MotionBlur);
                    PostProcessManager.RenderMotionBlur(PostProcessManager.mainRT, PostProcessManager.GBlurHRT);
                }
                result = PostProcessManager.GBlurHRT;
            }
            else
            {
                if (World.dofType != DOFType.None)
                {
                    PostProcessManager.ChangeRenderMode(RenderMode.DoF);
                    PostProcessManager.DOF(PostProcessManager.mainRT, PostProcessManager.GBlurHRT);
                    result = PostProcessManager.GBlurHRT;
                }
                else
                {
                    result = PostProcessManager.mainRT;
                }
            }

            #endregion

            #region BLOOM
            PoolGame.game.PrepareRenderStates();
            if (World.BloomPostProcessing)
            {
                PostProcessManager.ChangeRenderMode(RenderMode.Bloom);
                PostProcessManager.DrawBloomPostProcessing(result, null, gameTime);
            }
            else
            {
                
                PoolGame.device.SetRenderTarget(0, null);

                PostProcessManager.spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);
                PostProcessManager.spriteBatch.Draw(result.GetTexture(), Vector2.Zero, Color.White);
                PostProcessManager.spriteBatch.End();
                
            }

            #endregion

            if (World.motionblurType != MotionBlurType.None)
            {
                foreach (Entity e in World.scenario.objects)
                    e.SetPreviousWorld();

                World.camera.PrevViewProjection = World.camera.ViewProjection;
                World.camera.PrevView = World.camera.View;
            }

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(255 - TransitionAlpha);
        }


        #endregion
    }
}
