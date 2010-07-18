using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace XNA_PoolGame.Graphics.Particles
{
    /// <summary>
    /// Custom particle system for creating a giant plume of long lasting smoke.
    /// </summary>
    class SmokePlumeParticleSystem : ParticleSystem
    {
        public SmokePlumeParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoke";

            settings.MaxParticles = 1000;
            //settings.MaxParticles = 40;

            //settings.Duration = TimeSpan.FromSeconds(10);
            settings.Duration = TimeSpan.FromSeconds(4.0f);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 15;

            settings.MinVerticalVelocity = 10;
            settings.MaxVerticalVelocity = 20;

            // Create a wind effect by tilting the gravity vector sideways.
            settings.Gravity = new Vector3(10, 6, 0);

            settings.MinColor = new Color(160, 160, 160, 200);
            settings.MaxColor = new Color(255, 255, 255, 255);

            settings.EndVelocity = 0.75f;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 15;
            settings.MaxStartSize = 30;

            settings.MinEndSize = 50;
            settings.MaxEndSize = 80;

            settings.SourceBlend = Blend.SourceAlpha;
            settings.DestinationBlend = Blend.InverseSourceAlpha;
        }
    }
}
