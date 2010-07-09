//#define DRAW_BOUNDINGBOX

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Match;
using XNA_PoolGame.Helpers;
using XNA_PoolGame.Screens;
using Microsoft.Xna.Framework.Graphics;
using XNA_PoolGame.Graphics;
using XNA_PoolGame.Scenarios;
using System.Threading;
using XNA_PoolGame.Graphics.Models;
#endregion

namespace XNA_PoolGame.PoolTables
{
    /// <summary>
    /// Pool table
    /// </summary>
    public class PoolTable : Entity
    {
        public float FRICTION_SURFACE = 0.99f;
        public float BORDER_FRITCION = 0.95f;

        /// <summary>
        /// Start position of the cueball for a set.
        /// </summary>
        public Vector3 cueBallStartPosition;

        /// <summary>
        /// The cueball
        /// </summary>
        public Ball cueBall = null;

        /// <summary>
        /// All balls, including cueball.
        /// </summary>
        public Ball[] poolBalls;

        /// <summary>
        /// Number of balls.
        /// </summary>
        public int TotalBalls;
        public volatile int ballsready = 0;
        public object syncballsready = new object();

        /// <summary>
        /// Round information.
        /// </summary>
        public RoundInformation roundInfo = null;

        /// <summary>
        /// Determines if the balls are in motion
        /// </summary>
        public bool ballsMoving = false;

        /// <summary>
        /// Collection of pockets for this pool table
        /// </summary>
        public Pocket[] pockets;

        public BoundingBox[] rails;
        public Plane[] planes;
        public Vector3[] railsNormals;
        public VectorRenderComponent vectorRenderer;

        /// <summary>
        /// 
        /// </summary>
        public float pocket_radius;

        /// <summary>
        /// 
        /// </summary>
        public Vector3[] headDelimiters;
        public Vector3 MIN_HEAD, MAX_HEAD;
        public float MIN_HEAD_X = 0.0f;
        public float MIN_HEAD_Z = 0.0f;
        public float MAX_HEAD_X = 0.0f;
        public float MAX_HEAD_Z = 0.0f;

        public float SURFACE_POSITION_Y = 192.6f;
        public float MAXSURFACEPOS_Y = 204.0f;

        public float MAX_X = 285.0f;
        public float MIN_X = -267.0f;

        public float MAX_Z = 156.0f;
        public float MIN_Z = -136.3f;

        protected const float poolballscaleFactor = 1.0f; //1.85f;
        protected bool loaded = false;

        /// <summary>
        /// Maximum number of balls that supports the pocket.
        /// </summary>
        internal int maximumBallsInPocket;

        /// <summary>
        /// Position of a ball after maximum number of balls supported is over.
        /// </summary>
        internal Vector3 ballstuckposition;

        #region Constructor
        public PoolTable(Game game, String modelName)
            : base(game, modelName)
        {

            loaded = false;
        }
        #endregion

        #region Initialize functions
        public override void Initialize()
        {
            this.UpdateOrder = 2; //this.DrawOrder = 2;

            vectorRenderer = new VectorRenderComponent(PoolGame.game,
                                                       PoolGame.content,
                                                       "Effects\\VectorLineEffect");

            PoolGame.game.Components.Add(vectorRenderer);

            CommonInitialization();

            base.Initialize();
        }


        public void CommonInitialization()
        {
            this.SpecularColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            

            cueBall = new Ball(PoolGame.game, 0, "Models\\Balls\\newball", "Textures\\Balls\\ball 3", this, World.ballRadius);
            cueBall.DrawOrder = 2;
            if (!World.Debug)
                cueBall.SetCenter(cueBallStartPosition);
            else
                cueBall.SetCenter(new Vector3(MIN_X + World.ballRadius * 3.5f, SURFACE_POSITION_Y + World.ballRadius, MIN_Z / 2 - World.ballRadius));

            cueBall.Scale = new Vector3(poolballscaleFactor);



            BuildBallsTriangle(new Vector3(MIN_X / 3, SURFACE_POSITION_Y + World.ballRadius, -World.ballRadius), World.gameMode, World.ballRadius);

            //TotalBalls = 4;
            poolBalls[0] = cueBall;

            for (int i = 0; i < TotalBalls; i++)
            {
                poolBalls[i].UseThread = World.UseThreads;
                PoolGame.game.Components.Add(poolBalls[i]);
                World.scenario.Objects.Add(poolBalls[i]);
            }

            roundInfo = new RoundInformation();
            roundInfo.table = this;

            roundInfo.StartSet();
            loaded = true;
        }

        public void Reset()
        {
            loaded = false;

            for (int i = 0; i < TotalBalls; i++)
            {
                while (poolBalls[i].thinkingFlag) { }
                poolBalls[i].Stop();

                poolBalls[i].Position = roundInfo.ballsState[i].Position;
                poolBalls[i].PreRotation = roundInfo.ballsState[i].RotationInitial;
                poolBalls[i].Rotation = Matrix.Identity;
                poolBalls[i].currentTrajectory = roundInfo.ballsState[i].currentTrajectory;

                poolBalls[i].rotQ = Quaternion.Identity;
                poolBalls[i].invWorldInertia = Matrix.Identity;
                poolBalls[i].angularMomentum = Vector3.Zero;

                if (poolBalls[i].pocketWhereAt != -1 && roundInfo.ballsState[i].PocketWhereAt == -1)
                {
                    lock (pockets[poolBalls[i].pocketWhereAt].balls)
                    {
                        pockets[poolBalls[i].pocketWhereAt].balls.Remove(poolBalls[i]);
                    }

                    if (roundInfo.ballsState[i].PocketWhereAt != -1)
                    {
                        lock (pockets[roundInfo.ballsState[i].PocketWhereAt].balls)
                        {
                            pockets[roundInfo.ballsState[i].PocketWhereAt].balls.Add(poolBalls[i]);
                        }
                    }
                }

                poolBalls[i].pocketWhereAt = roundInfo.ballsState[i].PocketWhereAt;
                
                poolBalls[i].previousHitRail = -1; poolBalls[i].previousInsideHitRail = -1;
                
            }
            
            loaded = true;

        }

        /// <summary>
        /// Pick randomly a ball.
        /// </summary>
        /// <param name="ballReady"></param>
        /// <returns></returns>
        public int findRandomBall(bool[] ballReady)
        {
            bool IsDone = true;
            int num, lower_limit = 0;
            for (int i = 0; i < ballReady.Length; i++)
            {
                if (!ballReady[i])
                {
                    lower_limit = i; IsDone = false;
                    break;
                }
            }
            if (IsDone) return -1;

            while (ballReady[num = PoolGame.random.Next(lower_limit, 15)]) { }
            return num;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="triangleOrigin"></param>
        /// <param name="gameMode"></param>
        /// <param name="ballRadius"></param>
        public void BuildBallsTriangle(Vector3 triangleOrigin, GameMode gameMode, float ballRadius)
        {
            bool[] ballsReady;
            TotalBalls = 1;
            switch (gameMode)
            {
                #region Black Mode
                case GameMode.Black:
                    float ballDiameter = 2.0f * (ballRadius+0.1f);
                    poolBalls = new Ball[15 + 1];
                    
                    ballsReady = new bool[15];
                    int ballnumber = 0, solidballnumber, stripeballnumber;
                    const int ROWS = 5; // Numbers of rows in the triangle 
                    const int BLACK_ROW = 3, BLACK_COL = 2; // Position i,j of eight ball (black ball)
                    float xPos = 0, zPos = 0, zOffset = 0;
                    
                    for (int k = 0; k < ballsReady.Length; k++) ballsReady[k] = false;

                    solidballnumber = PoolGame.random.Next(0, 7);
                    stripeballnumber = PoolGame.random.Next(8, 15);

                    ballsReady[solidballnumber] = true; ballsReady[stripeballnumber] = true;
                    ballsReady[7] = true;

                    for (int row = 1; row <= ROWS; row++)
                    {
                        xPos = triangleOrigin.X - (row * ballDiameter) + 2.8f * row;

                        zOffset = triangleOrigin.Z - (row * ballRadius);

                        for (int col = 1; col <= row; col++)
                        {
                            zPos = zOffset + (col * (ballRadius+0.1f) * 2.0f);

                            if (row == BLACK_ROW && col == BLACK_COL)
                            {
                                ballnumber = 7;
                                poolBalls[TotalBalls] = new Ball(PoolGame.game, 8, "Models\\Balls\\newball", "Textures\\Balls\\ball 8", this, ballRadius);
                            }
                            else if (row == 5 && (col == 1 || col == 5))
                            {
                                ballnumber = col == 1 ? solidballnumber : stripeballnumber;

                                poolBalls[TotalBalls] = new Ball(PoolGame.game, ballnumber + 1, "Models\\Balls\\newball", "Textures\\Balls\\ball " + (ballnumber + 1), this, ballRadius);
                            }
                            else
                            {
                                ballnumber = findRandomBall(ballsReady);
                                poolBalls[TotalBalls] = new Ball(PoolGame.game, ballnumber + 1, "Models\\Balls\\newball", "Textures\\Balls\\ball " + (ballnumber + 1), this, ballRadius);
                            }
                            ballsReady[ballnumber] = true;

                            //add some entropy
                            //Vector3 entropy = new Vector3(((float)PoolGame.random.NextDouble() - 0.5f) * 1.5f, 0.0f, ((float)PoolGame.random.NextDouble() - 0.5f) * 1.5f);

                            Vector3 entropy = Vector3.Zero;
                            poolBalls[TotalBalls].Scale = new Vector3(poolballscaleFactor);
                            poolBalls[TotalBalls].SetCenter(xPos + entropy.X, triangleOrigin.Y, zPos + entropy.Z);
                            poolBalls[TotalBalls].DrawOrder = 2;
                            ++TotalBalls;
                        }

                    }

                    break;
                #endregion
                #region Nine balls Mode
                case GameMode.NineBalls:
                    poolBalls = new Ball[1 + 1];
                    poolBalls[1] = new Ball(PoolGame.game, 1, "Models\\Balls\\newball", "Textures\\Balls\\Ball 1", this, World.ballRadius);
                    poolBalls[1].SetCenter(triangleOrigin);
                    ++TotalBalls;
                    break;
                default:
                    poolBalls = new Ball[1];
                    break;
                #endregion
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void BuildPockets()
        {

        }
        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (!loaded || cueBall == null || pockets == null) return;

            // Gets if there is a new state of the balls, including cueball
            bool ballMovingState = ballsMoving;
            if (!CheckBallMovement() && ballMovingState)
            {
                roundInfo.EndRound();
                if (roundInfo.cueballPotted)
                {
                    roundInfo.cueBallInHand = true;
                    UnpottedcueBall();
                }
            }
            base.Update(gameTime);
            
        }
        #endregion

        #region Check for side collisions

                
        #endregion

        #region Check for balls collisions with others balls

        
        #endregion

        #region Draw model and draw bounding box
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

#if DRAW_BOUNDINGBOX
            if (PostProcessManager.currentRenderMode != RenderMode.ScreenSpaceSoftShadowRender &&
                PostProcessManager.currentRenderMode != RenderMode.BasicRender) return;

            PoolGame.device.RenderState.DepthBufferEnable = false;
            PoolGame.device.RenderState.DepthBufferWriteEnable = false;

            vectorRenderer.SetViewProjMatrix(World.camera.ViewProjection);
            vectorRenderer.SetWorldMatrix(Matrix.Identity);
            for (int i = 0; i < rails.Length; i++)
            {
                
                vectorRenderer.SetColor(Color.Red);
                vectorRenderer.DrawBoundingBox(rails[i]);
            }

            for (int i = 0; i < pockets.Length; i++)
            {
                vectorRenderer.SetColor(Color.Yellow);
                if (pockets[i].insideBands[0] == null) continue;
                vectorRenderer.SetWorldMatrix(pockets[i].insideBands[0].WorldTransform);
                vectorRenderer.DrawBoundingBox(pockets[i].insideBands[0].LocalBoundingBox);

                vectorRenderer.SetColor(Color.YellowGreen);
                if (pockets[i].insideBands[1] == null) continue;
                vectorRenderer.SetWorldMatrix(pockets[i].insideBands[1].WorldTransform);
                vectorRenderer.DrawBoundingBox(pockets[i].insideBands[1].LocalBoundingBox);

                vectorRenderer.SetColor(Color.Red);
                Vector3 point = (pockets[i].insideBands[0].LocalBoundingBox.Max + pockets[i].insideBands[0].LocalBoundingBox.Min) / 2.0f;
                point = Vector3.Transform(point, pockets[i].insideBands[0].WorldTransform);
                
                vectorRenderer.SetWorldMatrix(Matrix.Identity);
                vectorRenderer.DrawLine(point, point + pockets[i].insideNormal[0] * 15.0f);

                point = (pockets[i].insideBands[1].LocalBoundingBox.Max + pockets[i].insideBands[1].LocalBoundingBox.Min) / 2.0f;
                point = Vector3.Transform(point, pockets[i].insideBands[1].WorldTransform);
                
                vectorRenderer.DrawLine(point, point + pockets[i].insideNormal[1] * 15.0f);
            }
            if (cueBall.obb != null)
            {
                vectorRenderer.SetColor(Color.White);
                vectorRenderer.SetWorldMatrix(cueBall.obb.WorldTransform);
                vectorRenderer.DrawBoundingBox(cueBall.obb.LocalBoundingBox);

                vectorRenderer.SetWorldMatrix(Matrix.Identity);
                Vector3 point = (cueBall.obb.LocalBoundingBox.Max + cueBall.obb.LocalBoundingBox.Min) / 2.0f;
                point = Vector3.Transform(point, cueBall.obb.WorldTransform);
                vectorRenderer.DrawLine(point, point + cueBall.direction * 25.0f);
            }
            
            vectorRenderer.SetWorldMatrix(Matrix.Identity);
            vectorRenderer.SetColor(Color.Aqua);
            for (int i = 0; i < pockets.Length; i++)
            {
                vectorRenderer.DrawBoundingSphere(pockets[i].bounds);
            }

            PoolGame.device.RenderState.DepthBufferEnable = true;
            PoolGame.device.RenderState.DepthBufferWriteEnable = true;
#endif
        }
        #endregion

        #region Check if any ball is moving
        public bool CheckBallMovement()
        {
            for (int i = 0; i < TotalBalls; i++)
            {
                if (poolBalls[i].IsMoving())
                    return ballsMoving = true;
                
            }
            return ballsMoving = false;
        }
        #endregion

        #region Get out the cue ball of any pocket
        /// <summary>
        /// Restore cueball position. Check if cueBallStartPosition is a valid position.
        /// </summary>
        public void UnpottedcueBall()
        {
            roundInfo.cueballPotted = false;
            if (cueBall.pocketWhereAt != -1) pockets[cueBall.pocketWhereAt].balls.Remove(cueBall);

            cueBall.pocketWhereAt = -1;
            cueBall.currentTrajectory = Trajectory.Motion;
            cueBall.previousHitRail = cueBall.previousInsideHitRail = -1;
            cueBall.Visible = true;
            cueBall.Rotation = Matrix.Identity;
            cueBall.PreRotation = Matrix.Identity;
            

            // Check.
            cueBall.SetCenter(cueBallStartPosition);
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (vectorRenderer != null) vectorRenderer.Dispose();

            if (pockets != null)
            {
                for (int i = 0; i < pockets.Length; i++)
                {
                    pockets[i].Dispose();
                    pockets[i] = null;
                }
            }
            pockets = null;
            rails = null;

            for (int i = 0; i < TotalBalls; i++)
            {
                poolBalls[i].Dispose();
                poolBalls[i] = null;
            }
            TotalBalls = 0;
            
            poolBalls = null;
            cueBall = null;
            if (World.ballcollider != null) World.ballcollider.Dispose();
            World.ballcollider = null;
            base.Dispose(disposing);
        }
        #endregion



        public void StopAllBalls()
        {
            for (int i = 0; i < TotalBalls; ++i)
                poolBalls[i].Stop();
        }
    }
}
