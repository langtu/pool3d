﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNA_PoolGame.Threading;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Graphics.Particles;
using XNA_PoolGame.Helpers;

namespace XNA_PoolGame.Scenarios
{
    /// <summary>
    /// 
    /// </summary>
    public class ParticlesCore : ThreadComponent
    {
        private Scenario scenario = null;
        private List<ParticleSystem> particles;

        public Scenario Scenario
        {
            set { scenario = value; scenario.Enabled = false; }
        }
        public ParticlesCore(Game _game)
            : base(_game)
        {
            particles = new List<ParticleSystem>();
            UseThread = true;
        }

        public void AddParticlesFromMultiMap(MultiMap<int, ParticleSystem> map)
        {
            
            foreach (ParticleSystem particle in map)
            {
                particle.Enabled = false;
                particles.Add(particle);
            }
        }
        public override void Update(GameTime gameTime)
        {
            scenario.UpdateParticles(gameTime);
            for (int i = 0; i < particles.Count; ++i)
                particles[i].Update(gameTime);
            
            base.Update(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            if (particles != null) particles.Clear();
            particles = null;
            scenario = null;
            base.Dispose(disposing);
        }
    }
}
