using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Extreme_Pool.Screens
{
    class SplashScreen : Screen
    {
        private Texture2D texSplashBack;
        private GamePadState gpad;
        private bool success = false;

        public SplashScreen(ExPool _game)
            : base(_game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();
        }
        protected override void LoadContent()
        {
            texSplashBack = ExPool.content.Load<Texture2D>("Textures\\SplashScreen\\SplashScreen");
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (!success)
            {
                KeyboardState kb = Keyboard.GetState();

                if (kb.IsKeyDown(Keys.Space))
                {
                    success = true;
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        gpad = GamePad.GetState((PlayerIndex)i);
                        if (gpad.Buttons.Start == ButtonState.Pressed)
                        {
                            success = true;
                            break;
                        }
                    }
                }
                if (success)
                {
                    ExPool.game.nextMenuState = MenuState.MainMenuMode;
                }
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            ExPool.batch.Begin();
            ExPool.batch.Draw(texSplashBack, new Rectangle(0, 0, ExPool.Width, ExPool.Height), Color.White);
            ExPool.batch.End();
            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            texSplashBack.Dispose();
            base.Dispose(disposing);
        }
    }
}
