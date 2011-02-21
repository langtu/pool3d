using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using XNA_PoolGame.Graphics;


namespace XNA_PoolGame.Helpers
{
    public class FPSCounter : DrawableGameComponent
    {
        private float elapsedFPS;
        private int totalFrames;
        private int rateFrames;
        public FPSCounter(Game game)
            : base(game)
        {
            elapsedFPS = 0.0f;
            totalFrames = 0;
            rateFrames = 0;

            this.UpdateOrder = 999; this.DrawOrder = 9000;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            elapsedFPS += dt;
            
            if (elapsedFPS >= 1.0f)
            {
                elapsedFPS -= 1.0f;
                
                rateFrames = totalFrames;
                totalFrames = 0;
                
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {

            SpriteBatch batch = PostProcessManager.spriteBatch;

            SpriteFont font = PoolGame.spriteFont;
            batch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred,
                SaveStateMode.SaveState);
            string text = "FPS: " + rateFrames;

            Vector2 offset = font.MeasureString(text); offset.X += 0.0175f * PoolGame.Width; offset.Y = -0.0175f * PoolGame.Height;
            totalFrames++;

            batch.DrawString(font, text, new Vector2(PoolGame.Width, 4) - offset, Color.Black);
            batch.DrawString(font, text, new Vector2(PoolGame.Width, 5) - offset, Color.Tomato);

            batch.End();
            base.Draw(gameTime);
        }
    }
}