using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNA_PoolGame.Threading;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Graphics.Particles;
using XNA_PoolGame.Helpers;

namespace XNA_PoolGame.Scenarios
{
    public class ParticlesCore : ThreadComponent
    {
        private Scenario scenario = null;
        private List<ParticleSystem> particles;

        public Scenario Scenario
        {
            set { scenario = value; }
        }
        public ParticlesCore(Game _game)
            : base(_game)
        {
            particles = new List<ParticleSystem>();
            UseThread = true;
        }

        public void AddParticlesFromMultiMap(MultiMap<int, ParticleSystem> map)
        {
            World.scenario.Enabled = false;
            foreach (ParticleSystem particle in map)
            {
                particle.Enabled = false;
                particles.Add(particle);
            }
        }
        public override void Update(GameTime gameTime)
        {
            scenario.Update(gameTime);
            for (int i = 0; i < particles.Count; ++i)
            {
                particles[i].Update(gameTime);
            }
            base.Update(gameTime);
        }
    }
}
