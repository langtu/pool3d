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

namespace XNA_PoolGame.Scenarios
{
    /// <summary>
    /// Scenario.
    /// </summary>
    public abstract class Scenario : GameComponent
    {
        /// <summary>
        /// Object's scene.
        /// </summary>
        public MultiMap<int, DrawableComponent> objects;

        private Vector4 ambientColor;

        /// <summary>
        /// Lights from the scene
        /// </summary>
        public List<Light> lights;

        /// <summary>
        /// Particles to be rendered. Use Multimap for the drawing order of particles.
        /// </summary>
        public MultiMap<int, ParticleSystem> particles;

        /// <summary>
        /// Distortion particles to be rendered. Use Multimap for the drawing order of particles.
        /// </summary>
        public MultiMap<int, ParticleSystem> distortionparticles;

        /// <summary>
        /// DEM's scene
        /// </summary>
        public MultiMap<int, Entity> dems_basicrender;

        /// <summary>
        /// DEM's Objects scene
        /// </summary>
        public MultiMap<int, Entity> dems;
        public Vector4 AmbientColor
        {
            get { return ambientColor; }
            set { ambientColor = value; }
        }

        public object syncobject = new object();
        public Scenario(Game game)
            : base(game)
        {
            objects = new MultiMap<int, DrawableComponent>();
            particles = new MultiMap<int, ParticleSystem>();
            distortionparticles = new MultiMap<int, ParticleSystem>();
            dems_basicrender = new MultiMap<int, Entity>();
            dems = new MultiMap<int, Entity>();
            lights = new List<Light>();

            this.ambientColor = new Vector4(0, 0, 0, 1);
            
            LoadLights();
        }


        public MultiMap<int, DrawableComponent> Objects
        {
            get { return objects; }
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
            if (PostProcessManager.currentRenderMode == RenderMode.ParticleSystem)
            {
                foreach (ParticleSystem pa in particles)
                    pa.Draw(gameTime);
            } else if (PostProcessManager.currentRenderMode == RenderMode.DistortionParticleSystem)
            {
                foreach (ParticleSystem pa in distortionparticles)
                    pa.Draw(gameTime);
            }
        }

        /// <summary>
        /// Draw the entire scene.
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
        /// Initialize lights from the scene.
        /// </summary>
        public abstract void LoadLights();

        /// <summary>
        /// Set particles settings before drawing them.
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
        /// Set distortion particles settings before drawing them.
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

        protected override void Dispose(bool disposing)
        {
            World.scenario = null;

            if (objects != null) objects.Clear();
            objects = null;

            if (particles != null) particles.Clear();
            particles = null;

            if (distortionparticles != null) distortionparticles.Clear();
            distortionparticles = null;
            syncobject = null;

            if (dems_basicrender != null) dems_basicrender.Clear();
            dems_basicrender = null;

            PoolGame.game.Components.Remove(this);
            base.Dispose(disposing);
        }

        
    }
}
