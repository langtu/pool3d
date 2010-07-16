using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA_PoolGame.Scenarios
{
    public class Garage : Scenario
    {

        public Garage(Game _game)
            : base(_game)
        {

        }

        public override void Initialize()
        {

            base.Initialize();
        }
        

        public override void LoadLights()
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {

            base.Dispose(disposing);
        }

        public override void SetParticlesSettings()
        {
            throw new NotImplementedException();
        }

        public override void UpdateParticles(GameTime gameTime)
        {
            throw new NotImplementedException();
        }
    }
}
