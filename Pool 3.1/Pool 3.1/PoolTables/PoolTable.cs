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
using XNA_PoolGame.PoolTables.Racks;
using XNA_PoolGame.Scene;
#endregion

namespace XNA_PoolGame.PoolTables
{
    public enum MatchPhase
    {
        None,
        LaggingShot,
        Playing
    }
    
    public enum GameMode
    {
        EightBalls,
        NineBalls,
        ESPN
    }

    /// <summary>
    /// Abstract pool table.
    /// </summary>
    public abstract class PoolTable : Entity
    {
        public float FRICTION_SURFACE = 0.99f;
        public float BORDER_FRITCION = 0.95f;

        /// <summary>
        /// Raised when the balls are stopped after
        /// a player shot.
        /// </summary>
        public event EventHandler BallsStopped;

        /// <summary>
        /// Start position of the cueball for a game set.
        /// </summary>
        public Vector3 cueBallStartPosition;

        /// <summary>
        /// Cue ball start position for the team 1 in the lagging shot.
        /// </summary>
        public Vector3 cueBallStartLagPositionTeam1;

        /// <summary>
        /// Cue ball start position for the team 2 in the lagging shot.
        /// </summary>
        public Vector3 cueBallStartLagPositionTeam2;

        /// <summary>
        /// The cue ball.
        /// </summary>
        public Ball cueBall = null;

        /// <summary>
        /// All balls, including cue ball.
        /// </summary>
        public List<Ball> poolBalls;

        /// <summary>
        /// Number of balls.
        /// </summary>
        public int TotalBalls;
        public volatile int ballsready = 0;
        public object syncballsready = new object();

        /// <summary>
        /// Table rack.
        /// </summary>
        public Rack rack;

        /// <summary>
        /// Round information of the match on this table.
        /// </summary>
        public RoundInformation roundInfo = null;

        /// <summary>
        /// Determines whether the balls are in motion.
        /// </summary>
        public bool ballsMoving = false;
        public bool previousBallsMoving = false;

        /// <summary>
        /// Collection of pockets for this pool table.
        /// </summary>
        public Pocket[] pockets;

        public BoundingBox[] rails;
        public Plane[] planes;
        public Vector3[] railsNormals;
        public VectorRenderComponent vectorRenderer;

        /// <summary>
        /// Pocket radius.
        /// </summary>
        public float pocket_radius;

        /// <summary>
        /// Head string delimiters.
        /// </summary>
        public Vector3[] headDelimiters;
        public Vector3 MIN_HEAD, MAX_HEAD;
        public float MIN_HEAD_STRING_X = 0.0f;
        public float MIN_HEAD_STRING_Z = 0.0f;
        public float MAX_HEAD_STRING_X = 0.0f;
        public float MAX_HEAD_STRING_Z = 0.0f;

        public Vector3[] surfaceDelimiters;

        public float SURFACE_POSITION_Y = 192.6f;
        public float MAXSURFACEPOS_Y = 204.0f;

        public float MAX_X = 285.0f;
        public float MIN_X = -267.0f;

        public float MAX_Z = 156.0f;
        public float MIN_Z = -136.3f;

        protected const float poolballscaleFactor = 1.0f; //1.85f;
        protected bool loaded = false;

        public Vector3 minLongString;
        public Vector3 maxLongString;

        public Plane[] longStringPlanes;

        public Vector3 footSpotPosition;

        /// <summary>
        /// Far rail index in lagging shot.
        /// </summary>
        public int footCushionIndex;
        /// <summary>
        /// Near rail index in lagging shot.
        /// </summary>
        public int headCushionIndex;

        public List<Ball> laggedBalls;
        private List<Ball> tempPoolBalls;

        public Referee referee;

        public bool openTable;
        /// <summary>
        /// Maximum number of balls that supports the pocket.
        /// </summary>
        public int maximumBallsInPocket;

        /// <summary>
        /// Position of a ball after maximum number of balls supported is over.
        /// </summary>
        public Vector3 ballstuckposition;

        /// <summary>
        /// State of the match.
        /// </summary>
        public MatchPhase phase;

        #region Properties

        public float PoolBallScaleFactor
        { get { return poolballscaleFactor; } }

        #endregion

        #region Constructor
        public PoolTable(Game game, string modelName)
            : base(game, modelName, true)
        {
            //this.DEM = true;
            loaded = false;
            //this.drawboundingvolume = true;

            /////
            this.TEXTURE_ADDRESS_MODE = TextureAddressMode.Wrap;
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

            laggedBalls = new List<Ball>();
            
            CommonInitialization();

            base.Initialize();
        }

        /// <summary>
        /// Common initialization.
        /// </summary>
        public void CommonInitialization()
        {
            this.SpecularColor = Vector4.Zero;

            openTable = false;
            roundInfo = new RoundInformation();
            roundInfo.table = this;

            phase = MatchPhase.None;
            loaded = true;
        }

        public void CreatePoolBalls()
        {
            cueBall = new Ball(PoolGame.game, 0, "Models\\Balls\\newball", "Textures\\Balls\\white", this, World.ballRadius);            
            if (!World.Debug)
                cueBall.SetCenter(cueBallStartPosition);
            else
                cueBall.SetCenter(new Vector3(MIN_X + World.ballRadius * 3.5f, SURFACE_POSITION_Y + World.ballRadius, MIN_Z / 2 - World.ballRadius));

            rack = World.rackfactories[World.gameMode].CreateRack(this);
            rack.BuildBallsRack();

            //TotalBalls = 0;
            for (int i = 0; i < TotalBalls; i++)
            {
                poolBalls[i].Scale = new Vector3(poolballscaleFactor);
                poolBalls[i].DrawOrder = 2;
                poolBalls[i].EMType = EnvironmentType.DualParaboloid;
                poolBalls[i].UseThread = World.UseThreads;

                PoolGame.game.Components.Add(poolBalls[i]);
                World.scenario.Objects.Add(poolBalls[i]);
            }
        }

        /// <summary>
        /// Prepares everything to start the lagging shot.
        /// </summary>
        public void LagForBreak()
        {
            for (int i = 0; i < TotalBalls; ++i)
            {
                poolBalls[i].Enabled = false;
                poolBalls[i].Visible = false;
            }

            // Prepare lag balls for determinate who play first and add
            // them to the laggedBalls list.
            laggedBalls.Clear();

            Ball ball1;
            ball1 = new Ball(PoolGame.game, 0, "Models\\Balls\\newball", "Textures\\Balls\\white", this, World.ballRadius);
            ball1.IsLagging = true;
            ball1.DrawOrder = 24;
            ball1.Scale = new Vector3(poolballscaleFactor);
            ball1.SetCenter(cueBallStartLagPositionTeam1);
            laggedBalls.Add(ball1);

            Ball ball2 = new Ball(PoolGame.game, 0, "Models\\Balls\\newball", "Textures\\Balls\\white", this, World.ballRadius);
            ball2.IsLagging = true;
            ball2.DrawOrder = 25;
            ball2.Scale = new Vector3(poolballscaleFactor);
            ball2.SetCenter(cueBallStartLagPositionTeam2);
            laggedBalls.Add(ball2);

            PoolGame.game.Components.Add(laggedBalls[0]);
            PoolGame.game.Components.Add(laggedBalls[1]);

            World.scenario.Objects.Add(laggedBalls[0]);
            World.scenario.Objects.Add(laggedBalls[1]);

            // Change the current poolballs list to the laggedballs list.
            tempPoolBalls = poolBalls;
            poolBalls = laggedBalls;
            TotalBalls = 2;

            phase = MatchPhase.LaggingShot;
        }

        /// <summary>
        /// Sets visible the pool balls for the match.
        /// </summary>
        public void InitializeMatch()
        {
            if (tempPoolBalls != null)
            {
                poolBalls = tempPoolBalls;
                TotalBalls = poolBalls.Count;
                World.scenario.Objects.Remove(laggedBalls[0]);
                World.scenario.Objects.Remove(laggedBalls[1]);

                PoolGame.game.Components.Remove(laggedBalls[0]);
                PoolGame.game.Components.Remove(laggedBalls[1]);
                laggedBalls[0] = laggedBalls[1] = null;
            }

            for (int i = 0; i < TotalBalls; ++i)
            {
                poolBalls[i].Enabled = true;
                poolBalls[i].Visible = true;
            }

            for (int i = 0; i < 4; ++i)
            {
                if (World.players[i] != null)
                    World.players[i].stick.ballTarget = cueBall;
            }

            this.phase = MatchPhase.Playing;
            this.openTable = true;
            roundInfo.StartSet();
        }

        /// <summary>
        /// Sets the rack balls for the game set.
        /// </summary>
        public void InitializeGameSet()
        {
            cueBall.SetCenter(cueBallStartPosition);
            rack.BuildBallsRack();

            roundInfo.StartSet();
            rack.StartGame();
        }

        public void Reset()
        {
            loaded = false;

            for (int i = 0; i < TotalBalls; i++)
            {
                while (poolBalls[i].thinkingFlag) { }
                poolBalls[i].Stop();

                poolBalls[i].Position = roundInfo.ballStates[i].Position;
                poolBalls[i].PreRotation = roundInfo.ballStates[i].RotationInitial;
                poolBalls[i].Rotation = Matrix.Identity;
                poolBalls[i].currentTrajectory = roundInfo.ballStates[i].currentTrajectory;

                poolBalls[i].rotQ = Quaternion.Identity;
                poolBalls[i].invWorldInertia = Matrix.Identity;
                poolBalls[i].angularMomentum = Vector3.Zero;

                if (poolBalls[i].pocketWhereAt != -1 && roundInfo.ballStates[i].PocketWhereAt == -1)
                {
                    lock (pockets[poolBalls[i].pocketWhereAt].balls)
                    {
                        pockets[poolBalls[i].pocketWhereAt].balls.Remove(poolBalls[i]);
                    }

                    if (roundInfo.ballStates[i].PocketWhereAt != -1)
                    {
                        lock (pockets[roundInfo.ballStates[i].PocketWhereAt].balls)
                        {
                            pockets[roundInfo.ballStates[i].PocketWhereAt].balls.Add(poolBalls[i]);
                        }
                    }
                }

                poolBalls[i].pocketWhereAt = roundInfo.ballStates[i].PocketWhereAt;
                
                poolBalls[i].previousHitRail = -1; poolBalls[i].previousInsideHitRail = -1;
                
            }
            
            loaded = true;

        }

        /// <summary>
        /// 
        /// </summary>
        public abstract void BuildPockets();

        /// <summary>
        /// 
        /// </summary>
        public abstract void BuildRails();
        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (!loaded || cueBall == null || pockets == null) return;

            // Gets if there is a new state of the balls, including cueball.
            previousBallsMoving = ballsMoving;
            CheckBallMovement();
            if (previousBallsMoving && !ballsMoving)
            {
                if (BallsStopped != null)
                    BallsStopped(this, EventArgs.Empty);
            }

            //// PRUEBA!
            //List<Vector3> points = new List<Vector3>();
            //List<int> triangles = new List<int>();
            //BoundingSphere bs = new BoundingSphere(cueBall.Position, cueBall.Radius);
            //((OctreePartitioner)World.scenario.sceneManager).collider.CheckCollisions(bs, ref triangles, ref points);
            //PoolGame.cueBallCollisionPoints = points.Count;
            base.Update(gameTime);
            
        }
        #endregion

        /// <summary>
        /// Determinates whether a ball is selected with the cursor
        /// from screen space to world space.
        /// </summary>
        /// <returns>The picked ball.</returns>
        public Ball IntersectsABall()
        {
            Ray cursorRay = World.cursor.CalculateCursorRay();
            Ball ball = null;
            float distance = float.MaxValue;
            for (int i = 1; i < TotalBalls; ++i)
            {
                if (poolBalls[i].pocketWhereAt != -1) continue;
                float? t = poolBalls[i].BoundingSphere.Intersects(cursorRay);
                if (t != null)
                {
                    if (distance > t)
                    {
                        ball = poolBalls[i];
                        t = distance;
                    }
                }
            }
            return ball;
        }

        /// <summary>
        /// Determinates whether a pocket is selected with the cursor
        /// from screen space to world space.
        /// </summary>
        /// <returns>The picked pocket.</returns>
        public Pocket IntersectsAPocket()
        {
            Ray cursorRay = World.cursor.CalculateCursorRay();
            foreach (Pocket pocket in pockets)
            {
                if (pocket.bounds.Intersects(cursorRay) != null)
                {
                    return pocket;
                }
            }
            return null;
        }

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
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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

        #region Get out the cue ball from pocket
        /// <summary>
        /// Restores cueball position after a scratch. 
        /// Checks if cueBallStartPosition is a valid position.
        /// </summary>
        public void UnpocketCueBall()
        {
            if (cueBall.pocketWhereAt != -1)
            {
                lock (pockets[cueBall.pocketWhereAt].balls)
                {
                    pockets[cueBall.pocketWhereAt].balls.Remove(cueBall);
                }
                cueBall.pocketWhereAt = -1;
            }

            cueBall.currentTrajectory = Trajectory.Motion;
            cueBall.previousHitRail = cueBall.previousInsideHitRail = -1;
            cueBall.Visible = true;
            cueBall.Rotation = Matrix.Identity;
            cueBall.PreRotation = Matrix.Identity;
            

            // Check.
            cueBall.SetCenter(cueBallStartPosition);
        }

        /// <summary>
        /// Restores cue ball behind the head string.
        /// </summary>
        public void RestoreCueBall()
        {
            cueBall.currentTrajectory = Trajectory.Motion;
            cueBall.previousHitRail = cueBall.previousInsideHitRail = -1;
            cueBall.Visible = true;
            cueBall.Rotation = Matrix.Identity;
            cueBall.PreRotation = Matrix.Identity;


            // Check.
            cueBall.SetCenter(cueBallStartPosition);
        }

        /// <summary>
        /// Restores a ball behind the foot spot.
        /// </summary>
        public void RestoreBallToFootSpot(Ball ball)
        {
            if (ball.pocketWhereAt != -1)
            {
                lock (pockets[ball.pocketWhereAt].balls)
                {
                    pockets[ball.pocketWhereAt].balls.Remove(ball);
                }
                ball.pocketWhereAt = -1;
            }

            ball.currentTrajectory = Trajectory.Motion;
            ball.previousHitRail = ball.previousInsideHitRail = -1;
            ball.Visible = true;
            ball.Rotation = Matrix.Identity;
            ball.PreRotation = Matrix.Identity;

            // Place the ball on the foot spot. If there's not a
            // valid position try to put it around foot spot.

            // Check.
            //ball.SetCenter(cueBallStartPosition);
        }
        
        #endregion

        /// <summary>
        /// Returns whether a ball is on the head surface.
        /// </summary>
        /// <param name="ball">Position of the ball (only matters X and Z coordinates).</param>
        /// <returns>Return true or false.</returns>
        public bool BallBehindHeadString(Vector3 position)
        {
            if (position.X >= MIN_HEAD_STRING_X && position.X <= MAX_HEAD_STRING_X &&
                position.Z >= MIN_HEAD_STRING_Z && position.Z <= MAX_HEAD_STRING_Z)
                return true;

            return false;
        }

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

            if (phase == MatchPhase.LaggingShot && laggedBalls != null)
            {
                if (laggedBalls.Count > 0)
                {
                    laggedBalls[0].Dispose();
                    laggedBalls[1].Dispose();
                    laggedBalls[0] = null;
                    laggedBalls[1] = null;
                }
                
                laggedBalls.Clear();
                laggedBalls = null;

                poolBalls = tempPoolBalls;
                TotalBalls = poolBalls.Count;
            }
            for (int i = 0; i < TotalBalls; i++)
            {
                poolBalls[i].Dispose();
                poolBalls[i] = null;
            }
            TotalBalls = 0;
            
            poolBalls = null;
            cueBall = null;
            tempPoolBalls = null;

            if (World.ballcollider != null) World.ballcollider.Dispose();
            World.ballcollider = null;

            longStringPlanes = null;

            if (roundInfo != null)
                roundInfo.Dispose();

            if (rack != null)
                rack.Dispose();
            rack = null;

            roundInfo = null;
            referee = null;
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
