using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace XNA_PoolGame.Graphics.Particles.Fire
{

    /// <summary>
    /// Custom particle system for creating a flame effect.
    /// </summary>
    public class SmokeStackFireParticleSystem : ParticleSystem
    {
        public SmokeStackFireParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "fire2";

            settings.MaxParticles = 1500;
            /////////settings.MaxParticles = 500;
            //////settings.MaxParticles = 1500;

            settings.Duration = TimeSpan.FromSeconds(0.85f);
            /////////settings.Duration = TimeSpan.FromSeconds(1.5f);
            /////settings.Duration = TimeSpan.FromSeconds(1.0);

            settings.DurationRandomness = 0.5f;

            settings.MinHorizontalVelocity = -10;
            settings.MaxHorizontalVelocity = 30;

            settings.MinVerticalVelocity = -20;
            settings.MaxVerticalVelocity = 30;

            // Set gravity upside down, so the flames will 'fall' upward.
            settings.Gravity = new Vector3(0, 15, 0);

            //ray
            //settings.MinColor = new Color(180, 20, 30, 40);
            //settings.MaxColor = new Color(255, 70, 50, 150);

            // fire
            //settings.MinColor = new Color(180, 100, 150, 6);
            //settings.MaxColor = new Color(255, 170, 170, 40);

            settings.MinColor = new Color(200, 100, 150, 15);
            settings.MaxColor = new Color(255, 170, 170, 40);

            //settings.MinColor = new Color(255, 255, 255, 25);
            //settings.MaxColor = new Color(255, 255, 255, 40);

            settings.MinStartSize = 1 * 5;
            settings.MaxStartSize = 10 * 5;

            settings.MinEndSize = 10 * 5;
            settings.MaxEndSize = 50 * 5;

            //////

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            // Use additive blending.
            settings.SourceBlend = Blend.SourceAlpha;
            settings.DestinationBlend = Blend.One;
        }
    }
}
