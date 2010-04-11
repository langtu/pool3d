using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace XNA_PoolGame.GameControllers
{
    public class XboxPad : GameController
    {
        public XboxPad(PlayerIndex index)
            : base(index)
        {

        }

        private XboxPad(PlayerIndex index, XboxPad other)
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
            return new XboxPad(this.playerIndex, this);
        }

        public override void Update()
        {
            GamePadState gps = GamePad.GetState(playerIndex);

            this.leftStick = gps.ThumbSticks.Left;
            this.rightStick = gps.ThumbSticks.Right;

            startPressed = gps.Buttons.Start == ButtonState.Pressed;
            if (gps.Buttons.A == ButtonState.Pressed) this.APressed = true;
            else this.APressed = false;

            if (gps.Buttons.X == ButtonState.Pressed) this.XPressed = true;
            else this.XPressed = false;
            

            if (gps.Buttons.Y == ButtonState.Pressed) this.YPressed = true;
            else this.YPressed = false;

            if (gps.Buttons.LeftShoulder == ButtonState.Pressed) this.leftShoulderPressed = true;
            else this.leftShoulderPressed = false;

            rightTrigger = gps.Triggers.Right;
            leftTrigger = gps.Triggers.Left;
        }


    }
}
