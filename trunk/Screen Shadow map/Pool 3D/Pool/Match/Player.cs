using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Sticks;
using XNA_PoolGame.PoolTables;
using XNA_PoolGame.Controllers;
using XNA_PoolGame.Match;

namespace XNA_PoolGame
{
    /// <summary>
    /// The player.
    /// </summary>
    public class Player : GameComponent
    {
        public string playerName = "";
        public Stick stick = null;
        public Controller controller;
        public PoolTable poolTable = null;

        /// <summary>
        /// This is the world's player index.
        /// </summary>
        public int playerIndex = -1;

        /// <summary>
        /// How many faults the player has commit?.
        /// </summary>
        public int faults = 0;

        private float repeater = 1.0f;
        public List<Ball> ballsPotted = new List<Ball>();
        public TeamNumber teamNumber = TeamNumber.One;

        #region Constructors
        public Player(Game game, int playerIndex, Controller controller)
            : base(game)
        {
            this.controller = controller;
            this.poolTable = World.poolTable;
            this.playerIndex = playerIndex;
        }
        public Player(Game game, int playerIndex, Controller controller, TeamNumber team, Stick stick, PoolTable poolTable)
            : base(game)
        {
            this.playerIndex = playerIndex;
            this.controller = controller;
            this.stick = stick;
            this.teamNumber = team;
            this.poolTable = poolTable;
        }
        public Player(Game game, int playerIndex, Controller controller, TeamNumber team, PoolTable poolTable)
            : base(game)
        {
            this.playerIndex = playerIndex;
            this.controller = controller;
            this.poolTable = poolTable;
            this.teamNumber = team;
        }
        #endregion

        /// <summary>
        /// Create a standard stick for this player.
        /// If it has been created, just add it to component's list.
        /// </summary>
        public void CreateStick()
        {
            if (stick == null) stick = new Stick(PoolGame.game, poolTable.cueBall, playerIndex);
            stick.InitialRotation = Matrix.CreateRotationZ(MathHelper.ToRadians(90.0f));

            stick.AngleY = 0.0f;
            stick.charging = false;

            PoolGame.game.Components.Add(stick);

            World.scenario.scene.Add(stick);
        }

        public override void Initialize()
        {
            CreateStick();

            this.UpdateOrder = 5; //this.DrawOrder = 5;
            base.Initialize();

            

        }

        #region Update
        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (World.playerInTurn == -1 || playerIndex != World.playerInTurn) { base.Update(gameTime); return; }
            
            Controller prevcontroller = (Controller)controller.Clone();
            controller.Update();

            if (controller.isYPressed && !prevcontroller.isYPressed) poolTable.Reset();

            if (!poolTable.ballsMoving)
            {
                if (controller.RightStick != Vector2.Zero) 
                    World.camera.MovePicthYaw(controller.RightStick * 200.0f * dt);

                if (!stick.charging)
                {
                    if (!stick.Visible) stick.Visible = true;

                    if (controller.isXPressed && poolTable.roundInfo.cueBallInHand)
                    {
                        Vector3 a;
                        if (controller.LeftStick.Y != 0.0f)//stick.balltarget.Position.X + p.X >= poolTable.MIN_HEAD_X && stick.balltarget.Position.X + p.X >= poolTable.MAX_HEAD_X)
                        {
                            a = stick.ballTarget.Position + controller.LeftStick.Y * stick.Direction;
                            stick.ballTarget.Position = a;
                        }
                        if (controller.LeftStick.X != 0.0f)//stick.balltarget.Position.X + p.X >= poolTable.MIN_HEAD_X && stick.balltarget.Position.X + p.X >= poolTable.MAX_HEAD_X)
                        {
                            Vector3 b = Vector3.Cross(Vector3.Up, stick.Direction);
                            a = stick.ballTarget.Position - controller.LeftStick.X * b;
                            stick.ballTarget.Position = a;
                        }

                    }
                    else
                    {
                        if (controller.isLeftShoulderPressed)
                        {
                            if (controller.LeftStick.X > 0.2f) stick.AngleY = 90.0f;
                            else if (controller.LeftStick.X < -0.2f) stick.AngleY = 270.0f;
                            if (controller.LeftStick.Y > 0.2f) stick.AngleY = 0.0f;
                            else if (controller.LeftStick.Y < -0.2f) stick.AngleY = 180.0f;
                        }
                        else
                        {
                            if (prevcontroller.LeftStick.X > 0.0f && controller.LeftStick.X > 0.0f) repeater = MathHelper.Clamp(repeater + 0.04f, 1.0f, 6.0f);
                            else if (prevcontroller.LeftStick.X < 0.0f && controller.LeftStick.X < 0.0f) repeater = MathHelper.Clamp(repeater + 0.04f, 1.0f, 6.0f);

                            if (prevcontroller.LeftStick.X != 0.0f && controller.LeftStick.X == 0.0f) repeater = 1.0f;


                            stick.AngleY -= controller.LeftStick.X * repeater * dt * 10.0f;
                        }
                    }

                    if (!prevcontroller.isAPressed && controller.isAPressed) stick.charging = true;
                }
                else
                {
                    

                    if (prevcontroller.isAPressed && !controller.isAPressed && controller.RightTrigger > 0.0f)
                    {
                        //CancelShot();
                    }

                    if (stick.Power >= stick.MAX_POWER || prevcontroller.isAPressed && !controller.isAPressed)
                    {
                        if (stick.Power > 0)
                        {
                            stick.Visible = false;
                            TakeShot();
                        }
                        stick.charging = false;
                    }
                    else if (prevcontroller.isAPressed && controller.isAPressed)
                        stick.Power = MathHelper.Clamp(stick.Power + (controller.RightTrigger - controller.LeftStick.Y * 10.0f) * dt * 40.0f, 0.0f, stick.MAX_POWER);

                }
            }
            else
            {

            }
            

            base.Update(gameTime);
        }
        #endregion

        #region Shooting
        private void CancelShot()
        {
            stick.charging = false;
            stick.Power = 0.0f;
        }

        public void TakeShot()
        {
            if (stick.Power >= stick.MIN_POWER)
            {
                poolTable.roundInfo.cueballState = new PoolBallState(poolTable.cueBall.Position,
                    poolTable.cueBall.isInPlay, poolTable.cueBall.InitialRotation * poolTable.cueBall.Rotation, poolTable.cueBall.pocketWhereAt,
                    poolTable.cueBall.currentTrajectory);

                poolTable.roundInfo.ballsState = new List<PoolBallState>();

                for (int i = 0; i < poolTable.TotalBalls; i++)
                {
                    poolTable.roundInfo.ballsState.Add(new PoolBallState(poolTable.poolBalls[i].Position,
                        poolTable.poolBalls[i].isInPlay, poolTable.poolBalls[i].InitialRotation * poolTable.poolBalls[i].Rotation, 
                        poolTable.poolBalls[i].pocketWhereAt, poolTable.poolBalls[i].currentTrajectory));
                }

                poolTable.roundInfo.cueballPotted = false;
                poolTable.roundInfo.cueBallInHand = false;

                Vector3 force = new Vector3(stick.Direction.X, 0, stick.Direction.Z) * stick.Power * 2.5f;
                stick.ballTarget.acceleration = force / stick.ballTarget.Mass;


                stick.ballTarget.direction = stick.Direction;
                stick.ballTarget.ApplyImpulse(force);

                poolTable.roundInfo.stickRotation = stick.AngleY;



            }
            stick.Power = 0.0f;
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            stick.Dispose();
            World.players[this.playerIndex] = null;
            PoolGame.game.Components.Remove(this);
            base.Dispose(disposing);
        }
        #endregion
    }
}
