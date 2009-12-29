using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Models;

namespace XNA_PoolGame.Scenarios
{
    public abstract class Scenario : GameComponent
    {
        public List<BasicModel> scene;
        public Scenario(Game game)
            : base(game)
        {
            scene = new List<BasicModel>();
            LoadLights();
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
            
        }

        public abstract void LoadLights();

        public abstract void SetParticleSettings();

        protected override void Dispose(bool disposing)
        {
            World.scenario = null;
            if (scene != null) scene.Clear();
            scene = null;
            PoolGame.game.Components.Remove(this);
            base.Dispose(disposing);
        }
    }
}
