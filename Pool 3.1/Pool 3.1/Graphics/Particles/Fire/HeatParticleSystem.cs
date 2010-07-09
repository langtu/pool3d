﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace XNA_PoolGame.Graphics.Particles.Fire
{
    /// <summary>
    /// Custom particle system for creating a heat haze effect.
    /// </summary>
    public class HeatParticleSystem : ParticleSystem
    {
        public HeatParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "fire";

            settings.MaxParticles = 50;

            settings.Duration = TimeSpan.FromSeconds(2.0f);

            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 15;

            settings.MinVerticalVelocity = -10;
            settings.MaxVerticalVelocity = 20;

            // Set gravity upside down, so the flames will 'fall' upward.
            settings.Gravity = new Vector3(0, 15, 0);

            settings.MinColor = new Color(255, 240, 240, 25);
            settings.MaxColor = new Color(255, 240, 240, 40);

            settings.MinStartSize = 5 * 2;
            settings.MaxStartSize = 10 * 2;

            settings.MinEndSize = 10 * 2;
            settings.MaxEndSize = 40 * 2;

            // Use additive blending.
            settings.SourceBlend = Blend.SourceAlpha;
            settings.DestinationBlend = Blend.One;
        }
    }
}