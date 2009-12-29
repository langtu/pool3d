#define DRAW_DEBUGTEXT

#region Using Statements
using XNA_PoolGame;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using XNA_PoolGame.Screens;
using System.Diagnostics;
using XNA_PoolGame.Models;
using XNA_PoolGame.Models.Bloom;
using XNA_PoolGame.Screens.Screen_Manager;
using XNA_PoolGame.Helpers;
using XNA_PoolGame.Graphics;
using XNA_PoolGame.Controllers;
using XNA_PoolGame.PoolTables;
using XNA_PoolGame.Cameras;
using XNA_PoolGame.Scenarios;
#endregion

namespace XNA_PoolGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PoolGame : Game
    {
        public static GraphicsDeviceManager graphics;
        public static ContentManager content;
        public static GraphicsDevice device;
        public static SpriteFont spriteFont = null;

        public static FPSCounter FramesPerSecond = null;
        public static ScreenManager screenManager = null;
        private float saturation = 1.0f;

        #region Constants

        public const float FieldOfView = MathHelper.PiOver4;
        public const float nearPlane = 1f;
        public const float farPlane = 100000.0f;

        #endregion

        #region Variables
        public static int bloomSettingsIndex = 0;

        private static int width, height;
        public static PoolGame game;
        public static Random random;

        public static BloomComponent bloom;

        protected KeyboardState lastkb, kb;


        #endregion

        #region Properties

        public static int Width 
        { 
            get { return width; } 
        }

        public static int Height
        {
            get { return height; }
        }
        #endregion

        #region Constructor
        public PoolGame()
        {
            content = new ContentManager(Services, "Content");
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            game = this;

            graphics.PreferredBackBufferWidth = 1024;//1024;
            graphics.PreferredBackBufferHeight = 576; // 768;
            graphics.SynchronizeWithVerticalRetrace = false;
            //graphics.IsFullScreen = true;
            //graphics.MinimumPixelShaderProfile = ShaderProfile.PS_3_0;
            //graphics.MinimumVertexShaderProfile = ShaderProfile.VS_3_0;

            //graphics.PreferMultiSampling = true;
            graphics.ApplyChanges();
            //graphics.GraphicsDevice.Reset();

            this.IsFixedTimeStep = false;
            //this.TargetElapsedTime = TimeSpan.FromMilliseconds(16);

            random = new Random();

            


            
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            device = graphics.GraphicsDevice;


            //GraphicsDeviceCapabilities Caps = GraphicsAdapter.DefaultAdapter.GetCapabilities(DeviceType.Hardware);
            //if (Caps.TextureCapabilities.RequiresSquareOnly)
            //{
            //    bool b = false;
            //}
            width = graphics.GraphicsDevice.Viewport.Width;
            height = graphics.GraphicsDevice.Viewport.Height;

            PostProcessManager.Load();
            ModelManager.Load();

            // Create the bloom component to create some effects.
            bloom = new BloomComponent(this);
            bloom.Settings = BloomSettings.PresetSettings[0];
            Components.Add(bloom);

            // Add FPS component to show frame per second rate.
            FramesPerSecond = new FPSCounter(this);
            Components.Add(FramesPerSecond);

            // Create the screen manager component.
            screenManager = new ScreenManager(this);

            Components.Add(screenManager);

            // Activate the first screens.
            //screenManager.AddScreen(new BackgroundScreen(), null);
            //screenManager.AddScreen(new MainMenuScreen(), null);
            screenManager.AddScreen(new GameplayScreen(), PlayerIndex.One);

            kb = Keyboard.GetState();

            base.Initialize();
        }
        #endregion

        #region Load and Unload Content
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            
            spriteFont = content.Load<SpriteFont>("Fonts\\Arial");
            PostProcessManager.InitRenderTargets();

            

            base.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            PostProcessManager.UnloadContent();
            content.Unload();
            ModelManager.UnloadContent();
            base.UnloadContent();
        }
        #endregion

        #region Update
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            #region Keyboard Updates
            lastkb = kb;
            kb = Keyboard.GetState();

            if (kb.IsKeyDown(Keys.Q) && lastkb.IsKeyUp(Keys.Q) && World.poolTable != null)
            {
                World.poolTable.cueBall.Stop();
            }

            // SHADOWS
            if (kb.IsKeyDown(Keys.Y) && lastkb.IsKeyUp(Keys.Y))
            {
                World.displayShadows = !World.displayShadows;
            }

            if (kb.IsKeyDown(Keys.F6) && lastkb.IsKeyUp(Keys.F6) && World.camera != null)
            {
                Vector3 position = World.camera.CameraPosition;
                
                Matrix viewMatrix = World.camera.View;

                if (World.camera is ChaseCamera)
                {
                    Vector3 pointOfView = ((ChaseCamera)(World.camera)).LookAt;
                    ((ChaseCamera)(World.camera)).Reset();

                    Vector3 angle;
                    
                    angle.X = (float)LightManager.AngleBetweenVectors(Vector3.Up, viewMatrix.Up);
                    angle.Y = -(float)LightManager.AngleBetweenVectors(Vector3.Right, viewMatrix.Right);
                    angle.Z = 0.0f;

                    World.camera.Dispose();
                    World.camera = new FreeCamera(this, angle);
                    World.camera.CameraPosition = position;
                    World.camera.SetMouseCentered();
                }
                else
                {
                    World.camera.Dispose();
                    World.camera = new ChaseCamera(this);
                }
                Components.Add(World.camera);
            }

            if (kb.IsKeyDown(Keys.R) && lastkb.IsKeyUp(Keys.R) && World.poolTable != null)
                World.poolTable.Reset();
            

            
            if (kb.IsKeyDown(Keys.L) && lastkb.IsKeyUp(Keys.L))
            {
                World.displaySceneFromLightSource = !World.displaySceneFromLightSource;
            }
            ////////////////////////////////////////// BLOOM

            if (kb.IsKeyDown(Keys.V) && lastkb.IsKeyUp(Keys.V))
            {
                bloom.Visible = true;
                bloom.ShowBuffer++;

                if (bloom.ShowBuffer > BloomComponent.IntermediateBuffer.FinalResult)
                    bloom.ShowBuffer = 0;
            }
            if (kb.IsKeyDown(Keys.B) && lastkb.IsKeyUp(Keys.B))
            {
                bloomSettingsIndex = (bloomSettingsIndex + 1) %
                                     BloomSettings.PresetSettings.Length;

                bloom.Settings = BloomSettings.PresetSettings[bloomSettingsIndex];
                bloom.Visible = true;
            }

            if (kb.IsKeyDown(Keys.N) && lastkb.IsKeyUp(Keys.N)) bloom.Visible = !bloom.Visible;

            ////////////////////////////////////////// DEPTH BIAS
            if (kb.IsKeyDown(Keys.G)) PostProcessManager.depthBias += 0.015f * dt;
            if (kb.IsKeyDown(Keys.H)) PostProcessManager.depthBias -= 0.015f * dt;

            if (kb.IsKeyDown(Keys.J) && lastkb.IsKeyUp(Keys.J)) PostProcessManager.ShadowTechnique = (Shadow)((((int)PostProcessManager.ShadowTechnique) + 1) % 2);

            if (kb.IsKeyDown(Keys.U)) saturation += 0.15f * dt;
            if (kb.IsKeyDown(Keys.I)) saturation -= 0.15f * dt;
            saturation = MathHelper.Clamp(saturation, 0.0f, 1.0f);


            ////////////////////////////////////////// FRUSTUM
            if (kb.IsKeyDown(Keys.P) && lastkb.IsKeyUp(Keys.P)) World.camera.EnableFrustumCulling = !World.camera.EnableFrustumCulling;

            ////////////////////////////////////////// LIGHTS

            if (LightManager.sphereModel != null)
            {
                GamePadState state = GamePad.GetState(PlayerIndex.One);
                Vector3 pos = new Vector3(LightManager.lights.Position.X, LightManager.lights.Position.Y, LightManager.lights.Position.Z);
                pos.X += state.ThumbSticks.Left.X;
                pos.Y += state.ThumbSticks.Left.Y;
                pos.Z += state.ThumbSticks.Right.X;
                LightManager.lights.Position = pos;
                LightManager.sphereModel.Position = pos;
                LightManager.sphereModel.Update(gameTime);
            }

            #endregion
            
            base.Update(gameTime);
            
        }
        #endregion

        /// <summary>
        /// Reset the basic states of the Graphics Device needed to draw the scene
        /// </summary>
        public void PrepareRenderStates()
        {
            PoolGame.device.RenderState.DepthBufferEnable = true;
            PoolGame.device.RenderState.DepthBufferWriteEnable = true;
            PoolGame.device.RenderState.AlphaBlendEnable = false;
            PoolGame.device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            //PoolGame.device.RenderState.FillMode = FillMode.Solid;
        }

        #region Draw
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            //PoolGame.device.RenderState.DepthBias = 0.0003f;

            PrepareRenderStates();
            if (World.displayShadows)
            {

                PostProcessManager.ChangeRenderMode(RenderMode.ShadowMapRender);
                PostProcessManager.RenderShadowMap();
                base.Draw(gameTime);

                //PrepareRenderStates();

                PostProcessManager.ChangeRenderMode(RenderMode.PCFShadowMapRender);
                PostProcessManager.RenderPCFShadowMap();
                base.Draw(gameTime);

                //PrepareRenderStates();

                PostProcessManager.ChangeRenderMode(RenderMode.ScreenSpaceSoftShadowRender);
                PostProcessManager.RenderSSSoftShadow();
                base.Draw(gameTime);

                // Reset the CullMode and AlphaBlendEnable that the previous effect changed
                //GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
                //GraphicsDevice.RenderState.AlphaBlendEnable = true;
            }
            else
            {
                PostProcessManager.ChangeRenderMode(RenderMode.BasicRender);
                base.Draw(gameTime);
            }

            //PrepareRenderStates();

            //PostProcessManager.ChangeRenderMode(RenderMode.Render);
            //graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            // Render our scene
            //device.RenderState.DepthBufferEnable = false;
            //device.RenderState.DepthBufferWriteEnable = false;
            //PoolGame.device.RenderState.AlphaBlendEnable = true;
            
            PostProcessManager.ChangeRenderMode(RenderMode.Bloom);
            base.Draw(gameTime);

            PostProcessManager.ChangeRenderMode(RenderMode.BasicRender);
            if (LightManager.sphereModel != null) LightManager.sphereModel.Draw(gameTime);


            if (saturation != 1.0f) PostProcessManager.DrawSaturation(saturation);


            PostProcessManager.ChangeRenderMode(RenderMode.Menu);
            base.Draw(gameTime);

            //PoolGame.device.RenderState.BlendFactor = Color.Black;



            // Show Shadow Map Texture (depth) 
            if (World.displayShadows && World.displayShadowsTextures)
            {
                SpriteBatch batch = PostProcessManager.spriteBatch;
                Texture2D endTexture = PostProcessManager.ShadowMapRT.GetTexture();
                Rectangle rect = new Rectangle(0, 0, 128, 128);

                batch.Begin(SpriteBlendMode.None);
                //batch.Begin();
                batch.Draw(endTexture, rect, Color.White);


                endTexture = PostProcessManager.ShadowRT.GetTexture();
                rect = new Rectangle(0, 128, 128, 128);
                batch.Draw(endTexture, rect, Color.White);

                
                //endTexture = PostProcessManager.GBlurVRT.GetTexture();
                //rect = new Rectangle(0, 128*2, 128, 128);
                //batch.Draw(endTexture, rect, Color.White);

                batch.End();
            }

#if DRAW_DEBUGTEXT
            DrawText(gameTime);
#endif
        }

        #region Draw Text for Debugging
        public void DrawText(GameTime gameTime)
        {
            SpriteBatch batch = PostProcessManager.spriteBatch;
            batch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred,
                SaveStateMode.SaveState);
            string text = "";


            //text += "Screen Manager: " + screenManager.GetScreens().Length;
            text += "\nComponents: " + Components.Count;

            /*if (World.playerInTurn != -1 && World.players[World.playerInTurn].stick != null)
            {


                text += "\nStick Angle Y: " + World.players[World.playerInTurn].stick.AngleY;
                text += "\nStick Power: " + World.players[World.playerInTurn].stick.Power;

                Vector3 aa = World.poolTable.cueBall.Position;// World.players[World.playerInTurn].stick.direction;

                text += "\nCue Ball Pos\nX: " + aa.X;
                text += "\nY: " + aa.Y;
                text += "\nZ: " + aa.Z;

                aa = World.poolTable.cueBall.velocity;
                text += "\nCB Velocity\nX: " + aa.X;
                text += "\nY: " + aa.Y;
                text += "\nZ: " + aa.Z;
                text += "\nTrajectory: " + World.poolTable.cueBall.currentTrajectory.ToString();

                //text += "\nX: " + World.players[World.playerInTurn].stick.direction.X;
                //text += "\nY: " + World.players[World.playerInTurn].stick.direction.Y;
                //text += "\nZ: " + World.players[World.playerInTurn].stick.direction.Z;
            }*/
            text += "\nElapsedGameTime.TotalSeconds: " + gameTime.ElapsedGameTime.TotalSeconds;

            if (World.camera != null) text += "\nFrustum Culling: " + World.camera.EnableFrustumCulling.ToString() + " (KEY: P)";
            //text += "\nDirection: \nX: " + forward.X + "\nY: " + forward.Y + "\nZ: " + forward.Z + "\n";

            text += "\n\nBloom Name: (" + bloom.Settings.Name + ") (KEY: B)" + "\nShow buffer (" + bloom.ShowBuffer.ToString() + ")" + " (KEY: V)";

            text += "\n\nDepthBias: " + PostProcessManager.depthBias + " (KEYS: G, H)";

            text += "\n\nShow Shadows: " + World.displayShadows.ToString() + " (KEY: Y)";
            text += "\nShadowTechnique: " + PostProcessManager.ShadowTechnique.ToString() + " (KEY: J)";
            text += "\nSaturation: " + saturation + " (KEYS: U, I)";

            batch.DrawString(spriteFont, text, new Vector2(18, height - 276), Color.Black);
            batch.DrawString(spriteFont, text, new Vector2(17, height - 277), Color.Tomato);

            batch.End();
        }
        #endregion

        #endregion

        public void DefreezeComponentUpdates(bool enable)
        {
            foreach (GameComponent component in Components)
            {
                // Disable/Enable the components when PauseMenuScreen is on/off
                // Do not disable the screen manager, fps, bloom, they always are working continously
                if (!(component is ScreenManager || component is FPSCounter || component is BloomComponent)) 
                    component.Enabled = enable;
            }
        }
    }

    #region Entry Point

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
        static void Main()
        {

            using (Game game = new PoolGame())
            {
                game.Run();
            }
        }
    }

    #endregion

}
