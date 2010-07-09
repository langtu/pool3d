﻿using System;
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
        /// Object's scene
        /// </summary>
        public MultiMap<int, Entity> objects;

        /// <summary>
        /// Lights from the scene
        /// </summary>
        public List<Light> lights;

        /// <summary>
        /// Particles to be rendered. Use Multimap for the drawing order of particles
        /// </summary>
        public MultiMap<int, ParticleSystem> particles;

        /// <summary>
        /// Distortion particles to be rendered. Use Multimap for the drawing order of particles
        /// </summary>
        public MultiMap<int, ParticleSystem> distortionparticles;

        public Scenario(Game game)
            : base(game)
        {
            objects = new MultiMap<int, Entity>();
            particles = new MultiMap<int, ParticleSystem>();
            distortionparticles = new MultiMap<int, ParticleSystem>();
            lights = new List<Light>();
            
            
            LoadLights();
        }

        
        public MultiMap<int, Entity> Objects
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
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        public void DrawScene(GameTime gameTime)
        {
            foreach (Entity bm in this.Objects)
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
        public virtual void SetParticleSettings()
        {
            foreach (ParticleSystem particle in particles)
                particle.SetCamera(World.camera.View, World.camera.Projection);
        }

        /// <summary>
        /// Set distortion particles settings before drawing them.
        /// </summary>
        public virtual void SetDistortionParticleSettings()
        {
            foreach (ParticleSystem particle in distortionparticles)
                particle.SetCamera(World.camera.View, World.camera.Projection);
        }

        protected override void Dispose(bool disposing)
        {
            World.scenario = null;

            if (objects != null) objects.Clear();
            objects = null;

            if (particles != null) particles.Clear();
            particles = null;

            if (distortionparticles != null) distortionparticles.Clear();
            distortionparticles = null;

            PoolGame.game.Components.Remove(this);
            base.Dispose(disposing);
        }

        
    }
}
