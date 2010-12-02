#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Graphics;
using XNA_PoolGame.Graphics.Shadows;
using XNA_PoolGame.Helpers;
using XNA_PoolGame.Graphics.Models;
using XNA_PoolGame.Graphics.Particles;
using Microsoft.Xna.Framework.Graphics;
using XNA_PoolGame.Cameras;
#endregion

namespace XNA_PoolGame.Scenarios
{
    public enum ScenarioType
    {
        Cribs,
        Garage,
        Bar,
        CristalHotel
    }

    /// <summary>
    /// Scenario.
    /// </summary>
    public abstract class Scenario : GameComponent
    {
        #region Settings

        /// <summary>
        /// Object's scene.
        /// </summary>
        public MultiMap<int, DrawableComponent> objects;

        /// <summary>
        /// Ambient color.
        /// </summary>
        private Vector4 ambientColor;

        /// <summary>
        /// Lights from the scene.
        /// </summary>
        protected List<Light> lights;

        /// <summary>
        /// Particles to be rendered. Use Multimap for the drawing order of particles.
        /// </summary>
        protected MultiMap<int, ParticleSystem> particles;

        /// <summary>
        /// Distortion particles to be rendered. Use Multimap for the drawing order of particles.
        /// </summary>
        protected MultiMap<int, ParticleSystem> distortionparticles;

        /// <summary>
        /// DEM's scene.
        /// </summary>
        public MultiMap<int, Entity> dems_basicrender;

        /// <summary>
        /// DEM's Objects scene.
        /// </summary>
        public MultiMap<int, Entity> dems;


        public MultiMap<int, VolumetricLightEntity> volumetriclights;

        public RenderTargetCube refCubeMap = null;
        public TextureCube environmentMap = null;

        protected bool DEMable;

        public bool texcubeGenerated = false;
        #endregion
        
        #region Properties
        /// <summary>
        /// Object's scene.
        /// </summary>
        public MultiMap<int, DrawableComponent> Objects
        {
            get { return objects; }
        }
        
        /// <summary>
        /// Ambient color.
        /// </summary>
        public Vector4 AmbientColor
        {
            get { return ambientColor; }
            set { ambientColor = value; }
        }

        #endregion

        public object syncobject = new object();

        protected Scenario(Game game)
            : base(game)
        {
            objects = new MultiMap<int, DrawableComponent>();
            particles = new MultiMap<int, ParticleSystem>();
            distortionparticles = new MultiMap<int, ParticleSystem>();
            dems_basicrender = new MultiMap<int, Entity>();
            dems = new MultiMap<int, Entity>();
            lights = new List<Light>();
            volumetriclights = new MultiMap<int, VolumetricLightEntity>();

            this.ambientColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);

            //if (World.EM == EnvironmentType.Dynamic || World.EM == EnvironmentType.Static)
            //    refCubeMap = new RenderTargetCube(PoolGame.device, World.EMSize, 1, PoolGame.device.PresentationParameters.BackBufferFormat);
            CreateLights();
        }

        public virtual void LoadContent()
        {
        }

        public override void Initialize()
        {
            base.Initialize();
        }
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public virtual void Draw(GameTime gameTime) 
        {
            if (PostProcessManager.currentRenderMode == RenderPassMode.ParticleSystemPass && World.drawParticles)
            {
                foreach (ParticleSystem pa in particles)
                    pa.Draw(gameTime);
            }
            else if (PostProcessManager.currentRenderMode == RenderPassMode.DistortionParticleSystemPass && World.doDistortion)
            {
                foreach (ParticleSystem pa in distortionparticles)
                    pa.Draw(gameTime);
            }
        }

        /// <summary>
        /// Draw the entire scene, calling Draw() method for each object 
        /// that belongs to the scene.
        /// </summary>
        /// <param name="gameTime"></param>
        public void DrawScene(GameTime gameTime)
        {
            foreach (DrawableComponent bm in this.Objects)
            {
                if (bm.Visible) bm.Draw(gameTime);
            }
        }

        public void DrawDEMBasicRenderObjects(GameTime gameTime)
        {
            foreach (Entity bm in this.dems_basicrender)
            {
                if (bm.Visible) bm.Draw(gameTime);
            }
        }

        public void DrawDEMObjects(GameTime gameTime)
        {
            foreach (Entity bm in this.dems)
            {
                if (bm.Visible) bm.Draw(gameTime);
            }
        }

        /// <summary>
        /// Initialize lights of the scene.
        /// </summary>
        public abstract void CreateLights();

        /// <summary>
        /// Sets particles settings before drawing them.
        /// </summary>
        public virtual void SetParticlesSettings()
        {
            foreach (ParticleSystem particle in particles)
            {
                if (World.motionblurType != MotionBlurType.None)
                    particle.SetCamera(World.camera.View, World.camera.Projection, World.camera.PrevViewProjection);
                else
                    particle.SetCamera(World.camera.View, World.camera.Projection);
            }
        }

        /// <summary>
        /// Sets distortion particles settings before drawing them.
        /// </summary>
        public virtual void SetDistortionParticleSettings()
        {
            foreach (ParticleSystem particle in distortionparticles)
            {
                if (World.motionblurType != MotionBlurType.None)
                    particle.SetCamera(World.camera.View, World.camera.Projection, World.camera.PrevViewProjection);
                else
                    particle.SetCamera(World.camera.View, World.camera.Projection);
            }
        }

        public abstract void PrefetchData();

        /// <summary>
        /// 
        /// </summary>
        public void SetParticleEffectTechnique()
        {
            foreach (ParticleSystem particle in particles)
                particle.ChooseTechnique();

            foreach (ParticleSystem particle in distortionparticles)
                particle.ChooseTechnique();
        }

        public abstract void UpdateParticles(GameTime gameTime);


        public void DrawEnvironmetMappingScene(GameTime gameTime)
        {
            PostProcessManager.ChangeRenderMode(RenderPassMode.DEMBasicRender);
            Camera oldCamera = World.camera;

            World.camera = World.emptycamera;

            Vector3 position = new Vector3(0.0f, World.poolTable.SURFACE_POSITION_Y + World.ballRadius, 0.0f);
            World.emptycamera.CameraPosition = position;
            World.emptycamera.Projection = oldCamera.Projection;

            DepthStencilBuffer oldBuffer = PoolGame.device.DepthStencilBuffer;
            // Render our cube map, once for each cube face (6 times).
            for (int i = 0; i < 6; i++)
            {
                // render the scene to all cubemap faces
                CubeMapFace cubeMapFace = (CubeMapFace)i;

                switch (cubeMapFace)
                {
                    case CubeMapFace.NegativeX:
                        {
                            World.emptycamera.View = Matrix.CreateLookAt(position, position + Vector3.Left, Vector3.Up);
                            break;
                        }
                    case CubeMapFace.NegativeY:
                        {
                            World.emptycamera.View = Matrix.CreateLookAt(position, position + Vector3.Down, Vector3.Forward);
                            break;
                        }
                    case CubeMapFace.NegativeZ:
                        {
                            World.emptycamera.View = Matrix.CreateLookAt(position, position + Vector3.Backward, Vector3.Up);
                            break;
                        }
                    case CubeMapFace.PositiveX:
                        {
                            World.emptycamera.View = Matrix.CreateLookAt(position, position + Vector3.Right, Vector3.Up);
                            break;
                        }
                    case CubeMapFace.PositiveY:
                        {
                            World.emptycamera.View = Matrix.CreateLookAt(position, position + Vector3.Up, Vector3.Backward);
                            break;
                        }
                    case CubeMapFace.PositiveZ:
                        {
                            World.emptycamera.View = Matrix.CreateLookAt(position, position + Vector3.Forward, Vector3.Up);
                            break;
                        }
                }

                PoolGame.device.SetRenderTarget(0, refCubeMap, cubeMapFace);
                PoolGame.device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, Color.White, 1.0f, 0);
                World.emptycamera.ViewProjection = World.emptycamera.View * World.emptycamera.Projection;
                World.scenario.DrawScene(gameTime);
            }
            PoolGame.device.DepthStencilBuffer = oldBuffer;
            PoolGame.device.SetRenderTarget(0, null);
            environmentMap = refCubeMap.GetTexture();

            World.camera = oldCamera;

            World.poolTable.cueBall.environmentMap = this.environmentMap;
            for (int i = 1; i < World.poolTable.TotalBalls; ++i) World.poolTable.poolBalls[i].environmentMap = this.environmentMap;
        }

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            World.scenario = null;

            if (refCubeMap != null) refCubeMap.Dispose();
            refCubeMap = null;

            if (objects != null) objects.Clear();
            objects = null;

            if (particles != null) particles.Clear();
            particles = null;

            if (distortionparticles != null) distortionparticles.Clear();
            distortionparticles = null;
            syncobject = null;

            if (dems_basicrender != null) dems_basicrender.Clear();
            dems_basicrender = null;

            if (volumetriclights != null) volumetriclights.Clear();
            volumetriclights = null;

            PoolGame.game.Components.Remove(this);
            base.Dispose(disposing);
        }
        #endregion
    }
}
