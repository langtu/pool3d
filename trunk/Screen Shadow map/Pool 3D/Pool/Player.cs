using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Extreme_Pool.Sticks;
using Extreme_Pool.PoolTables;
using Extreme_Pool.Controllers;
using Extreme_Pool.Match;
using Extreme_Pool.Cameras;

namespace Extreme_Pool
{
    public class Player : DrawableGameComponent
    {
        public string playerName = "";
        public Stick stick = null;
        public Controller controller;
        public PoolTable poolTable = null;
        public int playerIndex = 0;
        public int faults = 0;

        public List<PoolBall> ballsPotted = new List<PoolBall>();
        public TeamNumber teamNumber = TeamNumber.One;

        #region Constructors
        public Player(ExPool game, Controller controller)
            : base(game)
        {
            this.controller = controller;
            this.poolTable = World.poolTable;
        }
        public Player(ExPool game, int playerIndex, Controller controller, TeamNumber team, Stick stick, PoolTable poolTable)
            : base(game)
        {
            this.playerIndex = playerIndex;
            this.controller = controller;
            this.stick = stick;
            this.teamNumber = team;
            this.poolTable = poolTable;
        }
        public Player(ExPool game, int playerIndex, Controller controller, TeamNumber team, PoolTable poolTable)
            : base(game)
        {
            this.playerIndex = playerIndex;
            this.controller = controller;
            this.poolTable = poolTable;
            this.teamNumber = team;
        }
        #endregion

        public override void Initialize()
        {
            if (stick == null) stick = new Stick(ExPool.game, poolTable.cueBall, playerIndex);

            stick.Scale = new Vector3(2.2f);
            stick.RotationInitial = Matrix.CreateRotationY(MathHelper.ToRadians(-90.0f));

            //if (World.useThreads)
            //    ExPool.AddModelToList(stick);
            //else

            ExPool.game.Components.Add(stick);

            this.UpdateOrder = 4; this.DrawOrder = 4;
            base.Initialize();
        }

        #region Update
        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (World.playerInTurn == -1) { base.Update(gameTime); return; }

            if (playerIndex == World.playerInTurn)
            {
                Controller prevcontroller = (Controller)controller.Clone();
                controller.Refresh();

                

                if (controller.isStartPressed) poolTable.Reset();

                if (!poolTable.ballsMoving)
                {
                    //if (controller.rightStick != Vector2.Zero)
                    //    World.camera.MovePicthYaw(controller.rightStick * 2.0f);

                    if (!stick.charging)
                    {
                        if (!stick.Visible) stick.Visible = true;

                        if (controller.isXPressed && poolTable.roundInfo.cueballMoveable)
                        {
                            Vector3 a;
                            if (controller.leftStick.Y != 0.0f)//stick.balltarget.Position.X + p.X >= poolTable.MIN_HEAD_X && stick.balltarget.Position.X + p.X >= poolTable.MAX_HEAD_X)
                            {
                                a = stick.balltarget.Position + controller.leftStick.Y * stick.direction;
                                stick.balltarget.Position = a;
                            }
                            if (controller.leftStick.X != 0.0f)//stick.balltarget.Position.X + p.X >= poolTable.MIN_HEAD_X && stick.balltarget.Position.X + p.X >= poolTable.MAX_HEAD_X)
                            {
                                Vector3 b = Vector3.Cross(Vector3.Up, stick.direction);
                                a = stick.balltarget.Position - controller.leftStick.X * b;
                                stick.balltarget.Position = a;
                            }

                        }
                        else
                        {
                            if (controller.isLeftShoulderPressed)
                            {
                                if (controller.leftStick.X > 0.2f)
                                {
                                    stick.AngleY = 90.0f;
                                }
                                else if (controller.leftStick.X < -0.2f)
                                {
                                    stick.AngleY = 270.0f;
                                }

                                if (controller.leftStick.Y > 0.2f)
                                {
                                    stick.AngleY = 0.0f;
                                }
                                else if (controller.leftStick.Y < -0.2f)
                                {
                                    stick.AngleY = 180.0f;
                                }
                            }
                            else
                            {
                                stick.AngleY -= controller.leftStick.X * 0.5f;
                            }
                            
                        }

                        if (!prevcontroller.isAPressed && controller.isAPressed)
                        {
                            stick.charging = true;
                        }
                    }
                    else
                    {
                        if (prevcontroller.isAPressed && !controller.isAPressed && controller.rightTrigger > 0.0f)
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
                        }
                        else if (prevcontroller.isAPressed && controller.isAPressed)
                        {
                            stick.Power += (controller.rightTrigger - controller.leftStick.Y * 10.0f) * dt * 40.0f;
                        }

                    }
                }
                else
                {

                }
            }

            base.Update(gameTime);
        }
        #endregion

        private void CancelShot()
        {
            stick.charging = false;
            stick.Power = 0.0f;
        }
        public void TakeShot()
        {
            stick.charging = false;

            if (stick.Power >= stick.MIN_POWER)
            {
                poolTable.roundInfo.cueballState = new PoolBallState(poolTable.cueBall.Position,
                    poolTable.cueBall.isInPlay, poolTable.cueBall.RotationInitial, poolTable.cueBall.pocketWhereAt,
                    poolTable.cueBall.currentTrajectory);

                poolTable.roundInfo.ballsState = new List<PoolBallState>();

                for (int i = 0; i < poolTable.poolBalls.Count; i++)
                {
                    poolTable.roundInfo.ballsState.Add(new PoolBallState(poolTable.poolBalls[i].Position,
                        poolTable.poolBalls[i].isInPlay, poolTable.poolBalls[i].RotationInitial, 
                        poolTable.poolBalls[i].pocketWhereAt, poolTable.poolBalls[i].currentTrajectory));
                }

                poolTable.roundInfo.cueballPotted = false;
                poolTable.roundInfo.cueballMoveable = false;

                Vector3 force = new Vector3(stick.direction.X,0,stick.direction.Z) * stick.Power;
                stick.balltarget.acceleration = force / stick.balltarget.Mass;


                stick.balltarget.direction = stick.direction;
                stick.balltarget.ApplyImpulse(force);

                poolTable.roundInfo.stick_rotation = stick.AngleY;



            }
            stick.Power = 0.0f;
        }
        protected override void Dispose(bool disposing)
        {
            stick.Dispose();
            World.players[this.playerIndex] = null;
            base.Dispose(disposing);
        }
    }
}
