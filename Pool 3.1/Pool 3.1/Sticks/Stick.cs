using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Graphics;
using XNA_PoolGame.Cameras;
using XNA_PoolGame.Graphics.Models;

namespace XNA_PoolGame.Sticks
{
    public class Stick : Entity
    {
        public bool charging = false;
        public Vector3 Direction = new Vector3(-1, 0, 0);
        public Vector3 AxisOfRotation = Vector3.UnitX;
        public Vector2 cueTargetPos = Vector2.Zero;
        //public Player player = null;

        private float power = 0.0f;
        public float MAX_POWER = 1500.0f;
        public float MIN_POWER = 0.2f;
        private float angleY = 0.0f;
        protected float width = 483.291f + 7.0f;

        public Ball ballTarget;
        public float angle = 0.0f;

        public int playerIndex;

        public float Power
        {
            get { return power; }
            set { power = value; }
        }
        public float AngleY
        {
            get
            {
                return angleY;
            }
            set
            {
                if (angleY == value) return;
                angleY = MathHelper.Clamp(value, 0, 360);

                if (this.angleY == 0) angleY = 360;
                else if (this.angleY == 360) angleY = 0;
                
            }
        }

        public Stick(Game game, Ball ballTarget, int playerIndex)
            : base(game, "Models\\Sticks\\stick_universal")
        {
            this.ballTarget = ballTarget;
            this.playerIndex = playerIndex;
        }

        public Stick(Game game, Ball balltarget, string stickName, string textureStick, int playerIndex)
            : base(game, stickName, textureStick)
        {
            this.ballTarget = balltarget;
            this.playerIndex = playerIndex;
        }

        public override void Initialize()
        {
            this.UpdateOrder = 6; //this.DrawOrder = 6;
            base.Initialize();
        }

        #region Update
        public override void Update(GameTime gameTime)
        {
            if (ballTarget == null || World.camera == null) { base.Update(gameTime); return; }
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            angle = MathHelper.ToRadians(10.0f);

            if (!World.poolTable.ballsMoving)
            {

                //Direction = new Vector3((float)-Math.Cos(MathHelper.ToRadians(angleY)), (float)-Math.Sin(MathHelper.ToRadians(angle)), (float)Math.Sin(MathHelper.ToRadians(angleY)));
                Direction = new Vector3((float)-Math.Cos(MathHelper.ToRadians(angleY)), 0.0f, (float)Math.Sin(MathHelper.ToRadians(angleY)));
                Direction.Normalize();
                if (World.camera is ChaseCamera && World.playerInTurn == this.playerIndex && this.playerIndex != -1)
                {
                    ((ChaseCamera)World.camera).ChasePosition = ballTarget.Position;
                    ((ChaseCamera)World.camera).ChaseDirection = new Vector3(Direction.X, 0, Direction.Z);
                }

                Vector3 offset = -Direction * (width + power * 0.09f);
                float cateto_opuesto = (float)Math.Tan(angle);
                float cateto_adyacente = new Vector2(-Direction.X * (width + power * 0.09f), -Direction.Z * (width + power * 0.09f)).Length();

                offset.Y = (cateto_adyacente * cateto_opuesto);

                this.Position = ballTarget.Position + offset;
            }
            Vector3 v = new Vector3(Direction.X, 0.0f, Direction.Z);
            v.Normalize();
            AxisOfRotation = Vector3.Cross(Vector3.Up, v);
            
            this.Rotation = Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(angleY))
                * Matrix.CreateFromAxisAngle(AxisOfRotation, angle);

            base.Update(gameTime);
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            ballTarget = null;
            PoolGame.game.Components.Remove(this);
            base.Dispose(disposing);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
        #endregion
    }
}
