//#define DRAW_DEBUGTEXT

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
using XNA_PoolGame.Graphics;
using XNA_PoolGame.Graphics.Bloom;
using XNA_PoolGame.Screens.Screen_Manager;
using XNA_PoolGame.Helpers;
using XNA_PoolGame.GameControllers;
using XNA_PoolGame.PoolTables;
using XNA_PoolGame.Cameras;
using XNA_PoolGame.Scenarios;
using XNA_PoolGame.Graphics.Particles;
using XNA_PoolGame.Graphics.Models;
using XNA_PoolGame.Threading;
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

        public static FPSCounter framesPerSecond = null;
        public static ScreenManager screenManager = null;
        private float saturation = 1.0f;

        public int currentlight = 0;

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

        //public static BloomComponent bloom;

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

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.SynchronizeWithVerticalRetrace = false;
            //graphics.PreferMultiSampling = true;
            
            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            //graphics.IsFullScreen = true;

            graphics.MinimumVertexShaderProfile = ShaderProfile.VS_3_0;
            graphics.MinimumPixelShaderProfile = ShaderProfile.PS_3_0;

            graphics.PreparingDeviceSettings +=
               new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);

            //graphics.MinimumPixelShaderProfile = ShaderProfile.PS_2_0;

            //graphics.MinimumPixelShaderProfile = ShaderProfile.PS_3_0;
            //graphics.MinimumVertexShaderProfile = ShaderProfile.VS_3_0;

            //graphics.PreferMultiSampling = true;
            graphics.ApplyChanges();
            //graphics.GraphicsDevice.Reset();

            this.IsFixedTimeStep = false;
            //this.TargetElapsedTime = TimeSpan.FromMilliseconds(16);

            random = new Random();

            


            
        }

        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            int quality = 0;
            GraphicsAdapter adapter = e.GraphicsDeviceInformation.Adapter;
            SurfaceFormat format = adapter.CurrentDisplayMode.Format;
            DisplayMode currentmode = adapter.CurrentDisplayMode;
            
            //e.GraphicsDeviceInformation.PresentationParameters.MultiSampleQuality = 0;
            //e.GraphicsDeviceInformation.PresentationParameters.MultiSampleType =
            //    MultiSampleType.FourSamples;
#if XBOX
            e.GraphicsDeviceInformation.PresentationParameters.MultiSampleQuality = 0;
            e.GraphicsDeviceInformation.PresentationParameters.MultiSampleType =
                MultiSampleType.FourSamples;
            e.GraphicsDeviceInformation.PresentationParameters.BackBufferWidth = 1280;
            e.GraphicsDeviceInformation.PresentationParameters.BackBufferHeight = 720;
            e.GraphicsDeviceInformation.PresentationParameters.BackBufferFormat = SurfaceFormat.Bgr32;
            e.GraphicsDeviceInformation.PresentationParameters.AutoDepthStencilFormat = DepthFormat.Depth24Stencil8Single;
            e.GraphicsDeviceInformation.PresentationParameters.EnableAutoDepthStencil = true;
            return;
#endif

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
            //bloom = new BloomComponent(this);
            //bloom.Settings = BloomSettings.PresetSettings[0];
            PostProcessManager.bloomSettings = BloomSettings.PresetSettings[0];
            World.BloomPostProcessing = false;

            // Add FPS component to show frame per second rate.
            framesPerSecond = new FPSCounter(this);
            Components.Add(framesPerSecond);

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

            foreach (IThreadable bm in ModelManager.allthreads)
                bm.BeginThread(gameTime);

            
            #region Keyboard Updates
            lastkb = kb;
            kb = Keyboard.GetState();

            if (kb.IsKeyDown(Keys.Q) && lastkb.IsKeyUp(Keys.Q) && World.poolTable != null)
            {
                World.poolTable.StopAllBalls();
            }

            // SHADOWS
            if (kb.IsKeyDown(Keys.F10) && lastkb.IsKeyUp(Keys.F10))
            {
                World.displacementType = (DisplacementType)(((int)(World.displacementType + 1) % 2));
            }

            if (kb.IsKeyDown(Keys.Y) && lastkb.IsKeyUp(Keys.Y))
            {
                World.displayShadows = !World.displayShadows;
            }
            if (kb.IsKeyDown(Keys.Z) )
            {
                LightManager.lights[currentlight].LightFarPlane = LightManager.lights[currentlight].LightFarPlane + 1.1f;
            }
            if (kb.IsKeyDown(Keys.X) )
            {
                LightManager.lights[currentlight].LightFarPlane = LightManager.lights[currentlight].LightFarPlane - 1.1f;
            }
            if (kb.IsKeyDown(Keys.Add))
            {
                LightManager.lights[currentlight].LightFOV = LightManager.lights[currentlight].LightFOV + 0.001f;
            }
            if (kb.IsKeyDown(Keys.Subtract))
            {
                LightManager.lights[currentlight].LightFOV = LightManager.lights[currentlight].LightFOV - 0.001f;
            }
            LightManager.lights[currentlight].LightFOV = MathHelper.Clamp(LightManager.lights[currentlight].LightFOV, MathHelper.ToRadians(0.1f), MathHelper.ToRadians(179.999f));

            if (kb.IsKeyDown(Keys.Q))
                World.poolTable.roundInfo.cueBallInHand = true;

            if (kb.IsKeyDown(Keys.D1) /*&& lastkb.IsKeyUp(Keys.D1)*/)
            {
                PostProcessManager.depthOfField.focalWidth -= dt * 100.0f;
                PostProcessManager.depthOfField.focalWidth = MathHelper.Max(PostProcessManager.depthOfField.focalWidth, 0);
            }
            if (kb.IsKeyDown(Keys.D2) /*&& lastkb.IsKeyUp(Keys.D2)*/)
            {
                PostProcessManager.depthOfField.focalWidth += dt * 100.0f;
                PostProcessManager.depthOfField.focalWidth = MathHelper.Max(PostProcessManager.depthOfField.focalWidth, 0);
            }
            if (kb.IsKeyDown(Keys.D3) /*&& lastkb.IsKeyUp(Keys.D3)*/)
            {
                PostProcessManager.depthOfField.focalDistance -= dt * 100.0f;
                PostProcessManager.depthOfField.focalDistance = MathHelper.Clamp(PostProcessManager.depthOfField.focalDistance,
                    World.camera.NearPlane, World.camera.FarPlane);
            }
            if (kb.IsKeyDown(Keys.D4) /*&& lastkb.IsKeyUp(Keys.D4)*/)
            {
                PostProcessManager.depthOfField.focalDistance += dt * 100.0f;
                PostProcessManager.depthOfField.focalDistance  = MathHelper.Clamp(PostProcessManager.depthOfField.focalDistance,
                    World.camera.NearPlane, World.camera.FarPlane);
            }

            if (kb.IsKeyDown(Keys.K) && lastkb.IsKeyUp(Keys.K))
            {
                World.dofType = (DOFType)(((int)World.dofType + 1) % 4);
            }

            if (kb.IsKeyDown(Keys.L) && lastkb.IsKeyUp(Keys.L) && World.playerInTurn != -1)
            {

                World.motionblurType = (MotionBlurType)(((int)World.motionblurType + 1) % 10);

            }

            if (kb.IsKeyDown(Keys.F2) && lastkb.IsKeyUp(Keys.F2) && World.camera is FreeCamera)
            {
                Vector3 position = World.camera.CameraPosition;
                Matrix viewMatrix = World.camera.View;

                Vector3 angle;

                angle.X = MathHelper.PiOver2;
                angle.Y = 0.0f;
                angle.Z = 0.0f;

                World.camera.Dispose();
                World.camera = new FreeCamera(this, angle);
                World.camera.CameraPosition = position;
                World.camera.SetMouseCentered();
                Components.Add(World.camera);
            }

            if (kb.IsKeyDown(Keys.F6) && lastkb.IsKeyUp(Keys.F6) && World.camera != null)
            {
                Vector3 position = World.camera.CameraPosition;
                
                Matrix viewMatrix = World.camera.View;
                float farPlane = World.camera.FarPlane;
                if (World.camera is ChaseCamera)
                {
                    Vector3 pointOfView = ((ChaseCamera)(World.camera)).LookAt;
                    ((ChaseCamera)(World.camera)).Reset();

                    Vector3 angle;

                    angle.X = (float)Maths.AngleBetweenVectors(Vector3.Up, viewMatrix.Up);
                    angle.Y = Math.Min((float)Maths.AngleBetweenVectors(Vector3.Right, viewMatrix.Right), -(float)Maths.AngleBetweenVectors(Vector3.Left, viewMatrix.Right));
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
                World.camera.FarPlane = farPlane;
                Components.Add(World.camera);
            }

            if (kb.IsKeyDown(Keys.R) && lastkb.IsKeyUp(Keys.R) && World.poolTable != null)
                World.poolTable.Reset();
            

            ////////////////////////////////////////// BLOOM

            if (kb.IsKeyDown(Keys.V) && lastkb.IsKeyUp(Keys.V))
            {
                World.BloomPostProcessing = true;
                PostProcessManager.showBuffer++;

                if (PostProcessManager.showBuffer > IntermediateBuffer.FinalResult)
                    PostProcessManager.showBuffer = 0;
            }
            if (kb.IsKeyDown(Keys.B) && lastkb.IsKeyUp(Keys.B))
            {
                bloomSettingsIndex = (bloomSettingsIndex + 1) %
                                     BloomSettings.PresetSettings.Length;

                PostProcessManager.bloomSettings = BloomSettings.PresetSettings[bloomSettingsIndex];
                World.BloomPostProcessing = true;
            }

            if (kb.IsKeyDown(Keys.N) && lastkb.IsKeyUp(Keys.N)) World.BloomPostProcessing = !World.BloomPostProcessing;

            ////////////////////////////////////////// DEPTH BIAS
            if (kb.IsKeyDown(Keys.G)) { LightManager.lights[currentlight].DepthBias += 0.015f * dt; LightManager.UpdateLights(); }
            if (kb.IsKeyDown(Keys.H)) { LightManager.lights[currentlight].DepthBias -= 0.015f * dt; LightManager.UpdateLights(); }

            if (kb.IsKeyDown(Keys.J) && lastkb.IsKeyUp(Keys.J)) PostProcessManager.shadowBlurTech = (ShadowBlurTechnnique)((((int)PostProcessManager.shadowBlurTech) + 1) % 2);

            if (kb.IsKeyDown(Keys.U)) saturation += 0.15f * dt;
            if (kb.IsKeyDown(Keys.I)) saturation -= 0.15f * dt;
            saturation = MathHelper.Clamp(saturation, 0.0f, 1.0f);


            ////////////////////////////////////////// FRUSTUM
            if (kb.IsKeyDown(Keys.P) && lastkb.IsKeyUp(Keys.P)) World.camera.EnableFrustumCulling = !World.camera.EnableFrustumCulling;

            ////////////////////////////////////////// LIGHTS

            if (LightManager.sphereModel != null)
            {
                
                if (kb.IsKeyDown(Keys.C) && lastkb.IsKeyUp(Keys.C)) currentlight = (currentlight + 1) % LightManager.totalLights;
                GamePadState state = GamePad.GetState(PlayerIndex.One);

                if (state.ThumbSticks.Left.X != 0.0f || state.ThumbSticks.Left.Y != 0.0f || state.ThumbSticks.Right.X != 0.0f)
                {

                    Vector3 pos = new Vector3(LightManager.lights[currentlight].Position.X, LightManager.lights[currentlight].Position.Y, LightManager.lights[currentlight].Position.Z);
                    pos.X += state.ThumbSticks.Left.X;
                    pos.Y += state.ThumbSticks.Left.Y;
                    pos.Z += state.ThumbSticks.Right.X;

                    LightManager.lights[currentlight].Position = pos;

                    LightManager.sphereModel.Position = pos;
                    LightManager.UpdateLights();
                }
                LightManager.sphereModel.Update(gameTime);
            }

            #endregion

            //while (World.poolTable.ballsready > 0)
            {
                
            }
            base.Update(gameTime);
            
        }
        #endregion

        /// <summary>
        /// Reset the basic states of the Graphics Device needed to draw the scene
        /// </summary>
        public void PrepareRenderStates()
        {

            PoolGame.device.RenderState.SourceBlend = Blend.One;
            PoolGame.device.RenderState.DestinationBlend = Blend.Zero;

            PoolGame.device.RenderState.DepthBufferEnable = true;
            PoolGame.device.RenderState.DepthBufferWriteEnable = true;
            PoolGame.device.RenderState.DepthBufferFunction = CompareFunction.LessEqual;

            
            
        }

        /// <summary>
        /// Reset the states after applying any particle system
        /// </summary>
        public void ResetParticlesRenderStates()
        {
            PoolGame.device.RenderState.AlphaBlendOperation = BlendFunction.Add;
            PoolGame.device.RenderState.AlphaFunction = CompareFunction.Always;
            PoolGame.device.RenderState.AlphaTestEnable = false;
            PoolGame.device.RenderState.AlphaBlendEnable = false;
            PoolGame.device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            PoolGame.device.RenderState.PointSpriteEnable = false;
        }

        #region Draw
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);       
            SpriteBatch batch = PostProcessManager.spriteBatch;
            
            //PoolGame.device.RenderState.DepthBias = PostProcessManager.depthBias;
            //PoolGame.device.RenderState.DepthBias = 0.000001f;
            
            PrepareRenderStates();
            ResetParticlesRenderStates();

            base.Draw(gameTime);

            if (saturation != 1.0f) PostProcessManager.DrawSaturation(saturation);

            // Show Shadow Map Texture (depth) 
            if (World.displayShadows && World.displayShadowsTextures)
            {
                Texture2D endTexture = PostProcessManager.shadows.ShadowMapRT[0].GetTexture();
                //Texture2D endTexture = PostProcessManager.depthRT.GetTexture();
                //Texture2D endTexture = PostProcessManager.motionBlur.RT.GetTexture();
                Rectangle rect = new Rectangle(0, 0, 128, 128);

                batch.Begin(SpriteBlendMode.None);
                //batch.Begin();
                batch.Draw(endTexture, rect, Color.White);


                endTexture = PostProcessManager.shadows.ShadowMapRT[1].GetTexture();
                //endTexture = PostProcessManager.shadows.ShadowRT.GetTexture();
                rect = new Rectangle(0, 128, 128, 128);
                //rect = new Rectangle(0, 0, Width, Height);
                batch.Draw(endTexture, rect, Color.White);

                
                //endTexture = PostProcessManager.GBlurVRT.GetTexture();
                //rect = new Rectangle(0, 128*2, 128, 128);
                //batch.Draw(endTexture, rect, Color.White);

                batch.End();
            }

#if DRAW_DEBUGTEXT
            DrawText(gameTime);
#endif
            //GraphicsDevice.Present();
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
            text += "\nNormalMapping: " + World.displacementType.ToString();
            text += "\nMotionBlur: " + World.motionblurType.ToString();
            text += "\nDOF: " + World.dofType.ToString();
            text += "\nFocal Width and Distance: " + PostProcessManager.depthOfField.focalWidth.ToString() + "; " + PostProcessManager.depthOfField.focalDistance.ToString();
            //if (World.poolTable != null && World.poolTable.cueBall != null) text += "\nCue Ball Velocity\nX: " + World.poolTable.cueBall.velocity;
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

            if (World.camera != null)
            {
                text += "\nFrustum Culling: " + World.camera.EnableFrustumCulling.ToString() + " (" + World.camera.ItemsDrawn + "/" + World.scenario.objects.Count + ")" + " (KEY: P)";
                
            }
            //text += "\nDirection: \nX: " + forward.X + "\nY: " + forward.Y + "\nZ: " + forward.Z + "\n";

            text += "\n\nBloom Name: (" + PostProcessManager.bloomSettings.Name + ") (KEY: B)" + "\nShow buffer (" + PostProcessManager.showBuffer.ToString() + ")" + " (KEY: V)";


            text += "\n\nShow Shadows: " + World.displayShadows.ToString() + " (KEY: Y)";
            text += "\nShadowTechnique: " + PostProcessManager.shadowBlurTech.ToString() + " (KEY: J)";
            text += "\nSaturation: " + saturation + " (KEYS: U, I)";

            text += "\n\nDepthBias: " + LightManager.lights[currentlight].DepthBias + " (KEYS: G, H)";
            text += "\nFarPlane: " + LightManager.lights[currentlight].LightFarPlane + " (KEYS: Z, X)";
            text += "\nFOV: " + LightManager.lights[currentlight].LightFOV + " (KEYS: +, -)";
            text += "\nPosition: " + LightManager.lights[currentlight].Position;

            batch.DrawString(spriteFont, text, new Vector2(18, height - 306), Color.Black);
            batch.DrawString(spriteFont, text, new Vector2(17, height - 307), Color.Tomato);

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
                if (!(component is ScreenManager || component is FPSCounter))
                {
                    // Threads
                    if (component is IThreadable && ((IThreadable)component).UseThread)
                    {
                        IThreadable thread = ((IThreadable)component);
                        if (thread.Running) thread.StopThread();
                        else thread.ResumeThread();
                    }
                    else
                        component.Enabled = enable;
                }
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
