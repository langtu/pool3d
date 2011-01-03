#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Sticks;
using XNA_PoolGame.PoolTables;
using XNA_PoolGame.GameControllers;
using XNA_PoolGame.Match;
using XNA_PoolGame.Screens;
using XNA_PoolGame.PoolTables.Racks;
#endregion

namespace XNA_PoolGame
{
    /// <summary>
    /// The player.
    /// </summary>
    public class Player : GameComponent
    {
        /// <summary>
        /// Player Name.
        /// </summary>
        public string playerName = "";

        /// <summary>
        /// Player's stick.
        /// </summary>
        public Stick stick = null;
        public GameController controller;
        public PoolTable table = null;
        public Team team;
        public TeamNumber teamNumber = TeamNumber.One;

        /// <summary>
        /// This is the world's player index.
        /// </summary>
        public int playerIndex = -1;

        /// <summary>
        /// How many faults the player has commit?.
        /// </summary>
        public int faults = 0;

        private float repeater = 1.0f;
        public List<Ball> ballsPotted;

        public bool waitingforOther;
        public bool aimLagShot;
        
        /// <summary>
        /// Raised when the player is ready to excecuted his
        /// lagging shot.
        /// </summary>
        public event EventHandler LaggingShotReady;

        #region Constructors
        public Player(Game game, int playerIndex, GameController controller)
            : base(game)
        {
            this.controller = controller;
            this.table = World.poolTable;
            this.playerIndex = playerIndex;
            ballsPotted = new List<Ball>();
        }
        public Player(Game game, int playerIndex, GameController controller, TeamNumber team, Stick stick, PoolTable poolTable)
            : base(game)
        {
            this.playerIndex = playerIndex;
            this.controller = controller;
            this.stick = stick;
            this.teamNumber = team;
            this.table = poolTable;
            ballsPotted = new List<Ball>();
        }
        public Player(Game game, string playerName, int playerIndex, GameController controller, TeamNumber teamNumber, PoolTable poolTable)
            : base(game)
        {
            this.playerIndex = playerIndex;
            this.controller = controller;
            this.table = poolTable;
            this.teamNumber = teamNumber;
            this.playerName = playerName;
            ballsPotted = new List<Ball>();
        }
        #endregion

        /// <summary>
        /// Create a standard stick for this player.
        /// If it has been created, just add it to components list.
        /// </summary>
        public void CreateStick()
        {
            if (stick == null) stick = new Stick(PoolGame.game, null, playerIndex);
            stick.DrawOrder = 3;
            stick.PreRotation = Matrix.CreateRotationZ(MathHelper.ToRadians(90.0f));
            stick.AngleY = 0.0f;
            stick.charging = false;

            PoolGame.game.Components.Add(stick);

            World.scenario.Objects.Add(stick);
        }

        public override void Initialize()
        {
            CreateStick();
            waitingforOther = false;
            aimLagShot = true;
            this.UpdateOrder = 5; //this.DrawOrder = 5;
            base.Initialize();

        }

        #region Update
        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (World.playerInTurnIndex == -1 || (playerIndex != World.playerInTurnIndex && table.phase == MatchPhase.Playing)) { base.Update(gameTime); return; }
            
            GameController prevcontroller = (GameController)controller.Clone();
            controller.Update();

            if (controller.isYPressed && !prevcontroller.isYPressed) table.Reset();
            if (!controller.isRightShoulderPressed && prevcontroller.isRightShoulderPressed)
                World.cursor.Visible = false;
            else if (controller.isRightShoulderPressed && !prevcontroller.isRightShoulderPressed)
                World.cursor.Visible = true;

            switch (table.phase)
            {
                #region Lagging Shot
                case MatchPhase.LaggingShot:
                    if (!stick.charging && aimLagShot)
                    {
                        if (prevcontroller.LeftStick.X > 0.0f && controller.LeftStick.X > 0.0f) repeater = MathHelper.Clamp(repeater + 0.04f, 1.0f, 10.0f);
                        else if (prevcontroller.LeftStick.X < 0.0f && controller.LeftStick.X < 0.0f) repeater = MathHelper.Clamp(repeater + 0.04f, 1.0f, 10.0f);

                        if (prevcontroller.LeftStick.X != 0.0f && controller.LeftStick.X == 0.0f) repeater = 1.0f;

                        stick.AngleY -= controller.LeftStick.X * repeater * dt * 20.0f;

                        if (!prevcontroller.isAPressed && controller.isAPressed) stick.charging = true;
                    }
                    else
                    {
                        if (stick.Power >= stick.MAX_POWER || prevcontroller.isAPressed && !controller.isAPressed)
                        {
                            if (stick.Power > 0)
                            {                                
                                aimLagShot = false;
                                waitingforOther = true;
                                if (LaggingShotReady != null)
                                    LaggingShotReady(this, EventArgs.Empty);
                            }
                            stick.charging = false;
                        }
                        else if (prevcontroller.isAPressed && controller.isAPressed)
                            stick.Power = MathHelper.Clamp(stick.Power + (controller.RightTrigger - controller.LeftStick.Y * 10.0f) * dt * 40.0f, 0.0f, stick.MAX_POWER);

                    }
                #endregion
                    break;
                #region Playing
                case MatchPhase.Playing:

                    if (controller.RightStick != Vector2.Zero) 
                        World.camera.MovePicthYaw(controller.RightStick * 200.0f * dt);
                if (!table.ballsMoving)
                {

                    if (!stick.charging)
                    {
                        if (!stick.Visible) stick.Visible = true;

                        if (!controller.isRightShoulderPressed)
                        {
                            if (controller.isXPressed && table.roundInfo.cueBallInHand)
                            {
                                if (controller.LeftStick.Y != 0.0f)
                                {
                                    Vector3 newPosition = stick.ballTarget.Position + controller.LeftStick.Y * stick.Direction * dt * 200.0f;
                                    //if (table.roundInfo.cueBallBehindHeadString) newPosition = Vector3.Max(table.headDelimiters[0], Vector3.Min(newPosition, table.headDelimiters[1]));
                                    //else newPosition = Vector3.Max(table.surfaceDelimiters[0], Vector3.Min(newPosition, table.surfaceDelimiters[1]));

                                    stick.ballTarget.Position = newPosition;
                                }
                                if (controller.LeftStick.X != 0.0f)
                                {
                                    Vector3 newPosition = stick.ballTarget.Position - controller.LeftStick.X * stick.AxisOfRotation * dt * 200.0f;
                                    //if (table.roundInfo.cueBallBehindHeadString) newPosition = Vector3.Max(table.headDelimiters[0], Vector3.Min(newPosition, table.headDelimiters[1]));
                                    //else newPosition = Vector3.Max(table.surfaceDelimiters[0], Vector3.Min(newPosition, table.surfaceDelimiters[1]));

                                    stick.ballTarget.Position = newPosition;
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
                                    if (prevcontroller.LeftStick.X > 0.0f && controller.LeftStick.X > 0.0f) repeater = MathHelper.Clamp(repeater + 50f * dt, 1.0f, 12.0f);
                                    else if (prevcontroller.LeftStick.X < 0.0f && controller.LeftStick.X < 0.0f) repeater = MathHelper.Clamp(repeater + 50f * dt, 1.0f, 12.0f);

                                    else if (prevcontroller.LeftStick.X != 0.0f && controller.LeftStick.X == 0.0f)
                                        repeater = 1.0f;


                                    stick.AngleY -= controller.LeftStick.X * repeater * dt * 20.0f;
                                    Console.WriteLine(repeater);
                                }
                            }

                            if (!prevcontroller.isAPressed && controller.isAPressed) stick.charging = true;
                        }
                        else
                        {
                            //Vector2 newPosition = World.gameplayscreen.CursorPosition;
                            //newPosition.Y -= (controller.LeftStick.Y) * dt * 1.0f;
                            //newPosition.X += (controller.LeftStick.X) * dt * 1.0f;

                            //newPosition = Vector2.Max(Vector2.Zero, Vector2.Min(Vector2.One, newPosition));
                            
                            //World.gameplayscreen.CursorPosition = newPosition;

                            if (controller.isAPressed && !prevcontroller.isAPressed)
                            {
                                Ball ball = table.IntersectsABall();
                                if (ball != null && table.roundInfo.enabledCalledBall)
                                    table.roundInfo.calledBall = ball;
                                else if (table.roundInfo.enabledCalledPocket)
                                {
                                    Pocket pocket = table.IntersectsAPocket();
                                    if (!(table.roundInfo.calledPocket != null && pocket == null))
                                    {
                                        table.roundInfo.calledPocket = pocket;
                                    }
                                }
                            }
                        }

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
                                TakeShot();
                            }
                            stick.charging = false;
                        }
                        else if (prevcontroller.isAPressed && controller.isAPressed)
                            stick.Power = MathHelper.Clamp(stick.Power + (controller.RightTrigger - controller.LeftStick.Y * 20.0f) * dt * 400.0f, 0.0f, stick.MAX_POWER);

                    }
                }
                #endregion
                    break;
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
                stick.Visible = false;
                table.roundInfo.ballStates.Clear();

                for (int i = 0; i < table.TotalBalls; i++)
                {
                    table.roundInfo.ballStates.Add(new PoolBallState(table.poolBalls[i].Position, table.poolBalls[i].PreRotation * table.poolBalls[i].Rotation, 
                        table.poolBalls[i].pocketWhereAt, table.poolBalls[i].currentTrajectory));
                }

                table.roundInfo.cueballState = table.roundInfo.ballStates[0];

                table.roundInfo.cueballPotted = false;
                table.roundInfo.cueBallInHand = false;

                Vector3 force = new Vector3(stick.Direction.X, 0.0f, stick.Direction.Z) * stick.Power * 2.5f;
                //stick.ballTarget.acceleration = force / stick.ballTarget.Mass;

                //stick.ballTarget.angularVelocity = force / 10.0f;
                stick.ballTarget.direction = stick.Direction;
                stick.ballTarget.ApplyImpulse(force);

                table.roundInfo.stickRotation = stick.AngleY;



            }
            stick.Power = 0.0f;
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                PoolGame.game.Components.Remove(this);
                if (stick != null) stick.Dispose();
                stick = null;
                table = null;
                controller = null;
                team = null;
                World.players[this.playerIndex] = null;
                //GC.SuppressFinalize(this);
            }
            base.Dispose(disposing);
        }
        //~Player()
        //{
        //    this.Dispose();
        //}
        #endregion
    }
}
