using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace XNA_PoolGame.GameControllers
{
    /// <summary>
    /// Keyboard controller. Uses the mouse to control the leftstick movement.
    /// </summary>
    public class KeyBoard : GameController
    {
        /// <summary>
        /// Mouse state.
        /// </summary>
        MouseState mouseState;

        /// <summary>
        /// Creates a new instance of KeyBoard class.
        /// </summary>
        /// <param name="index"></param>
        public KeyBoard(PlayerIndex index)
            : base(index)
        {

        }

        private KeyBoard(PlayerIndex index, KeyBoard other)
            : base(index)
        {
            this.leftStick = other.leftStick;
            this.rightStick = other.rightStick;
            this.APressed = other.APressed;
            this.BPressed = other.BPressed;
            this.XPressed = other.XPressed;
            this.YPressed = other.YPressed;
            this.rightShoulderPressed = other.rightShoulderPressed;
            this.leftShoulderPressed = other.leftShoulderPressed;
            this.startPressed = other.startPressed;
        }

        public override GameController Clone()
        {
            return new KeyBoard(this.playerIndex, this);
        }

        public MouseState MouseState()
        {
            return mouseState;
        }

        /// <summary>
        /// Update method.
        /// </summary>
        public override void Update()
        {
            KeyboardState kbs = Keyboard.GetState();
            this.leftStick = Vector2.Zero;

            mouseState = Mouse.GetState();

            int centerX = PoolGame.game.Window.ClientBounds.Width / 2;
            int centerY = PoolGame.game.Window.ClientBounds.Height / 2;
            if (PoolGame.game.IsActive) Mouse.SetPosition(centerX, centerY);

            Vector2 dt = Vector2.Zero;
            if (PoolGame.game.IsActive)
            {
                Vector2 center = new Vector2((float)centerX, (float)centerY);
                Vector2 mouse = new Vector2((float)mouseState.X, (float)mouseState.Y);
                dt = (mouse - center) / center;

                dt.Y = -dt.Y;
                dt = Vector2.Clamp(dt, -Vector2.One, Vector2.One);
            }

            //Console.WriteLine(dt);
            this.leftStick = dt;

            if (kbs.IsKeyDown(Keys.Up)) this.rightStick.Y = 1.0f;
            else if (kbs.IsKeyDown(Keys.Down)) this.rightStick.Y = -1.0f;
            else this.rightStick.Y = 0.0f;

            if (kbs.IsKeyDown(Keys.Left)) this.rightStick.X = -1.0f;
            else if (kbs.IsKeyDown(Keys.Right)) this.rightStick.X = 1.0f;
            else this.rightStick.X = 0.0f;

            if (kbs.IsKeyDown(Keys.Space)) this.rightTrigger = 1.0f;
            if (kbs.IsKeyUp(Keys.Space)) this.rightTrigger = 0.0f;

            if (mouseState.LeftButton == ButtonState.Pressed) this.APressed = true;
            else this.APressed = false;

            //if (kbs.IsKeyDown(Keys.F)) this.APressed = true;
            //if (kbs.IsKeyUp(Keys.F)) this.APressed = false;

            if (kbs.IsKeyDown(Keys.LeftControl)) this.leftShoulderPressed = true;
            else this.leftShoulderPressed = false;

            if (kbs.IsKeyDown(Keys.LeftShift)) this.rightShoulderPressed = true;
            else this.rightShoulderPressed = false;

            if (kbs.IsKeyDown(Keys.Back)) this.startPressed = true;
            if (kbs.IsKeyUp(Keys.Back)) this.startPressed = false;

            if (kbs.IsKeyDown(Keys.L)) this.BPressed = true;
            if (kbs.IsKeyUp(Keys.L)) this.BPressed = false;

            if (kbs.IsKeyDown(Keys.M)) this.XPressed = true;
            if (kbs.IsKeyUp(Keys.M)) this.XPressed = false;

            
        }

    }
}
