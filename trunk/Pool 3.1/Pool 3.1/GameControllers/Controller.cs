using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA_PoolGame.GameControllers
{
    public abstract class GameController : ICloneable, IEquatable<GameController>
    {
        protected PlayerIndex playerIndex;

        protected Vector2 leftStick = Vector2.Zero;
        protected Vector2 rightStick = Vector2.Zero;

        protected bool startPressed;

        protected float rightTrigger, leftTrigger;

        protected bool APressed;
        protected bool XPressed;
        protected bool YPressed;
        protected bool BPressed;
        protected bool leftShoulderPressed;

        #region Properties

        public Vector2 LeftStick
        {
            get { return leftStick; }
        }

        public Vector2 RightStick
        {
            get { return rightStick; }
        }

        public bool isStartPressed
        {
            get { return startPressed; }
        }

        public float RightTrigger
        {
            get { return rightTrigger; }
        }

        public float LeftTrigger
        {
            get { return leftTrigger; }
        }

        public bool isAPressed
        {
            get { return APressed; }
        }

        public bool isXPressed
        {
            get { return XPressed; }
            set { XPressed = value; }
        }

        public bool isYPressed
        {
            get { return YPressed; }
        }

        public bool isBPressed
        {
            get { return BPressed; }
        }
        public bool isLeftShoulderPressed
        {
            get { return leftShoulderPressed; }
        }
        

        #endregion

        public GameController(PlayerIndex index)
        {
            this.playerIndex = index;
        }

        //
        public abstract void Update();

        public void ResetButtons()
        {
            rightTrigger = 0.0f;
            leftTrigger = 0.0f;
            leftStick = Vector2.Zero;
            rightStick = Vector2.Zero;
            APressed = false;
            XPressed = false;
            YPressed = false;
            BPressed = false;
            leftShoulderPressed = false;
        }

        public bool isIdle()
        {
            return (leftStick == Vector2.Zero && rightStick == Vector2.Zero && !APressed
                && leftTrigger == 0.0f && rightTrigger == 0.0f && !leftShoulderPressed && !XPressed && !YPressed);
        }


        

        #region Cloneable

        public abstract object Clone();

        #endregion

        #region Miembros de IEquatable<Controller>

        public bool Equals(GameController other)
        {
            return (other.APressed.Equals(this.APressed) &&
                other.leftStick.Equals(this.leftStick) &&
                other.playerIndex.Equals(this.playerIndex));
        }

        #endregion

        #region Miembros de IEquatable<Controller>

        bool IEquatable<GameController>.Equals(GameController other)
        {
            return (other.APressed.Equals(this.APressed) &&
                other.leftStick.Equals(this.leftStick) &&
                other.playerIndex.Equals(this.playerIndex));
        }

        #endregion
    }
}
