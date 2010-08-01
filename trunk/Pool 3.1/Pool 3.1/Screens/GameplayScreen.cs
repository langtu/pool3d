#region Using statement
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
using TextureInUse = XNA_PoolGame.Graphics.PostProcessManager.TextureInUse;
using XNA_PoolGame.Graphics.Shading;
#endregion

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
            if (World.emptycamera == null) World.emptycamera = new EmptyCamera(PoolGame.game);

            // SET SHADING TECHNIQUE
            if (PostProcessManager.shading != null) PostProcessManager.shading.Dispose();
            switch (World.shadingTech)
            {
                case ShadingTechnnique.Foward:
                    PostProcessManager.shading = new FowardShading();
                    break;
                case ShadingTechnnique.Deferred:
                    PostProcessManager.shading = new DeferredShading();
                    break;
            }
            // SET SHADOW TECHNIQUE
            switch (World.shadowTechnique)
            {
                case ShadowTechnnique.PSMShadowMapping:
                    PostProcessManager.shading.shadows = new PSMShadowMapping();
                    break;
                case ShadowTechnnique.ScreenSpaceShadowMapping:
                    PostProcessManager.shading.shadows = new ScreenSpaceShadowMapping();
                    break;
                case ShadowTechnnique.VarianceShadowMapping:
                    PostProcessManager.shading.shadows = new VarianceShadowMapping();
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
            LightManager.sphereModel.belongsToScenario = false;
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
            //GC.WaitForPendingFinalizers();

            
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
            TextureInUse resultTIU = null;

            List<TextureInUse> useless = new List<TextureInUse>();
            TextureInUse distortionTIU = null;

            #region SHADOW MAPPING

            if (World.displayShadows) PostProcessManager.shading.Draw(gameTime);
            else PostProcessManager.shading.DrawTextured(gameTime);

            #endregion

            resultTIU = PostProcessManager.shading.resultTIU;
            PoolGame.device.SetRenderTarget(0, resultTIU.renderTarget);
            

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
            if (World.drawParticles)
            {
                PostProcessManager.ChangeRenderMode(RenderMode.ParticleSystem);
                World.scenario.SetParticlesSettings();
                World.scenario.Draw(gameTime);
            }
            #endregion
            
            #region DISTORTION PARTICLES
            if (World.doDistortion)
            {
                PostProcessManager.ChangeRenderMode(RenderMode.DistortionParticleSystem);
                PostProcessManager.DistorionParticles();
                World.scenario.SetDistortionParticleSettings();
                World.scenario.Draw(gameTime);
            }
            #endregion
            
            PoolGame.device.SetRenderTarget(0, null);
            if (World.motionblurType != MotionBlurType.None || World.dofType != DOFType.None)
            {
                PoolGame.device.SetRenderTarget(1, null);
                PoolGame.device.SetRenderTarget(2, null);
            }
            
            #region DISTORTION COMBINE

            if (World.doDistortion)
            {
                distortionTIU = PostProcessManager.GetIntermediateTexture();
                PostProcessManager.DistortionParticlesCombine(resultTIU.renderTarget, distortionTIU.renderTarget);
                resultTIU.DontUse();
                resultTIU = distortionTIU;
                useless.Add(distortionTIU);

                PostProcessManager.distortionsample.DontUse();
            }

            #endregion
            
            if (World.doSSAO)
            {
                PostProcessManager.ssao.Draw(resultTIU);
                resultTIU.DontUse();
                resultTIU = PostProcessManager.ssao.resultTIU;
            }

            if (World.volumetricLights)
            {
                PostProcessManager.scattering.Draw(resultTIU);
                resultTIU.DontUse();
                resultTIU = PostProcessManager.scattering.resultTIU;
            }

            #region MOTION BLUR AND DEPTH OF FIELD

            if (World.motionblurType != MotionBlurType.None)
            {
                if (World.dofType != DOFType.None)
                {
                    TextureInUse texturetemp1 = PostProcessManager.GetIntermediateTexture();
                    PostProcessManager.ChangeRenderMode(RenderMode.MotionBlur);
                    PostProcessManager.RenderMotionBlur(resultTIU.renderTarget, texturetemp1.renderTarget);
                    resultTIU.DontUse();

                    TextureInUse texturetemp2 = PostProcessManager.GetIntermediateTexture();
                    PostProcessManager.ChangeRenderMode(RenderMode.DoF);
                    PostProcessManager.DOF(texturetemp1.renderTarget, texturetemp2.renderTarget);

                    texturetemp1.DontUse();
                    useless.Add(texturetemp2);
                    resultTIU = texturetemp2;
                }
                else
                {
                    TextureInUse texturetemp = PostProcessManager.GetIntermediateTexture();
                    PostProcessManager.ChangeRenderMode(RenderMode.MotionBlur);
                    PostProcessManager.RenderMotionBlur(resultTIU.renderTarget, texturetemp.renderTarget);

                    useless.Add(texturetemp);
                    resultTIU = texturetemp;
                }
            }
            else
            {
                if (World.dofType != DOFType.None)
                {
                    TextureInUse texturetemp = PostProcessManager.GetIntermediateTexture();
                    PostProcessManager.ChangeRenderMode(RenderMode.DoF);
                    PostProcessManager.DOF(resultTIU.renderTarget, texturetemp.renderTarget);
                    resultTIU.DontUse();
                    useless.Add(texturetemp);
                    resultTIU = texturetemp;
                }
            }

            #endregion

            #region BLOOM
            PoolGame.game.PrepareRenderStates();
            if (World.BloomPostProcessing)
            {
                PostProcessManager.ChangeRenderMode(RenderMode.Bloom);
                PostProcessManager.DrawBloomPostProcessing(resultTIU.renderTarget, null, gameTime);
            }
            else
            {
                PoolGame.device.SetRenderTarget(0, null);

                PostProcessManager.spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);
                PostProcessManager.spriteBatch.Draw(resultTIU.renderTarget.GetTexture(), Vector2.Zero, Color.White);
                PostProcessManager.spriteBatch.End();
            }

            #endregion


            for (int k = 0; k < useless.Count; ++k) useless[k].DontUse();
            useless.Clear(); useless = null;
            resultTIU.DontUse();
            PostProcessManager.ssao.FreeStuff();
            PostProcessManager.shading.FreeStuff();

            PostProcessManager.halfVertTIU.DontUse(); PostProcessManager.halfHorTIU.DontUse();
            PostProcessManager.depthTIU.DontUse();
            if (World.motionblurType == MotionBlurType.None) { PostProcessManager.velocityTIU.DontUse(); PostProcessManager.velocityLastFrameTIU.DontUse(); }
            if (PostProcessManager.distortionsample != null) PostProcessManager.distortionsample.DontUse();

            if (World.motionblurType != MotionBlurType.None)
            {
                foreach (DrawableComponent e in World.scenario.objects)
                {
                    if (e is Entity) ((Entity)e).SetPreviousWorld();
                }

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
