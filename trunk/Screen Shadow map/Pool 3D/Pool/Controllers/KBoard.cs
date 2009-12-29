﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace XNA_PoolGame.Controllers
{
    public class KBoard : Controller
    {
        public KBoard(PlayerIndex index)
            : base(index)
        {

        }

        private KBoard(PlayerIndex index, KBoard other)
            : base(index)
        {
            this.leftStick = other.leftStick;
            this.rightStick = other.rightStick;
            this.APressed = other.APressed;
            this.BPressed = other.BPressed;
            this.XPressed = other.XPressed;
            this.YPressed = other.YPressed;
        }

        public override object Clone()
        {
            return new KBoard(this.playerIndex, this);
        }

        public override void Update()
        {
            KeyboardState kbs = Keyboard.GetState();
            this.leftStick = Vector2.Zero;

            if (kbs.IsKeyDown(Keys.Up)) this.leftStick.Y = 1.0f;
            else if (kbs.IsKeyDown(Keys.Down)) this.leftStick.Y = -1.0f;
            else this.leftStick.Y = 0.0f;
            
            if (kbs.IsKeyDown(Keys.Left)) this.leftStick.X = -1.0f;
            else if (kbs.IsKeyDown(Keys.Right)) this.leftStick.X = 1.0f;
            else this.leftStick.X = 0.0f;

            if (kbs.IsKeyDown(Keys.Space)) this.rightTrigger = 1.0f;
            if (kbs.IsKeyUp(Keys.Space)) this.rightTrigger = 0.0f;

            if (kbs.IsKeyDown(Keys.F)) this.APressed = true;
            if (kbs.IsKeyUp(Keys.F)) this.APressed = false;

            if (kbs.IsKeyDown(Keys.LeftControl)) this.leftShoulderPressed = true;
            else this.leftShoulderPressed = false;
            

            if (kbs.IsKeyDown(Keys.Back)) this.startPressed = true;
            if (kbs.IsKeyUp(Keys.Back)) this.startPressed = false;

            if (kbs.IsKeyDown(Keys.L)) this.BPressed = true;
            if (kbs.IsKeyUp(Keys.L)) this.BPressed = false;

            if (kbs.IsKeyDown(Keys.M)) this.XPressed = true;
            if (kbs.IsKeyUp(Keys.M)) this.XPressed = false;

            
        }
    }
}
