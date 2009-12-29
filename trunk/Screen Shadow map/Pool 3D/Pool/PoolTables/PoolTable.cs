//#define DRAW_BOUNDINGBOX

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNA_PoolGame.Models;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Match;
using XNA_PoolGame.Helpers;
using XNA_PoolGame.Screens;
using Microsoft.Xna.Framework.Graphics;
using XNA_PoolGame.Graphics;
using XNA_PoolGame.Scenarios;
using System.Threading;

namespace XNA_PoolGame.PoolTables
{
    public class PoolTable : BasicModel
    {
        public float FRICTION_SURFACE = 0.99f;
        public float BORDER_FRITCION = 0.95f;

        public Ball cueBall = null;
        public Ball[] poolBalls;
        public int TotalBalls;
        public RoundInformation roundInfo = null;

        public bool ballsMoving = false;

        public Vector3[] pockets_pos;

        public BoundingSphere[] pockets_bounds;
        public BoundingBox[] rails;
        public Vector3[] railsNormals;
        public OrientedBoundingBox[] insidebands_pockets;
        public VectorRenderComponent vectorRenderer;

        public Vector3[] inside_normals;

        public float pocket_radius;

        //public Vector3 surface1, surface2;
        public Vector3 MIN_HEAD, MAX_HEAD;
        public float MIN_HEAD_X = 0.0f;
        public float MIN_HEAD_Z = 0.0f;
        public float MAX_HEAD_X = 0.0f;
        public float MAX_HEAD_Z = 0.0f;

        public float SURFACEPOS_Y = 192.6f;
        public float MAXSURFACEPOS_Y = 204.0f;

        public float MAX_X = 285.0f;
        public float MIN_X = -267.0f;

        public float MAX_Z = 156.0f;
        public float MIN_Z = -136.3f;

        protected const float poolballscaleFactor = 1.0f; //1.85f;
        protected bool loaded = false;

        #region Constructor
        public PoolTable(Game game, String modelName)
            : base(game, modelName)
        {
            roundInfo = new RoundInformation();

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
            roundInfo.StartSet();

            cueBall = new Ball(PoolGame.game, 0, "Models\\Balls\\newball", "Textures\\Balls\\ball 3", World.ballRadius);
            if (!World.DebugMatch)
                cueBall.SetCenter(MIN_HEAD_X, SURFACEPOS_Y + World.ballRadius, 0);
            else
                cueBall.SetCenter(new Vector3(MIN_X + World.ballRadius * 3.5f, SURFACEPOS_Y + World.ballRadius, MIN_Z / 2 - World.ballRadius));

            cueBall.Scale = new Vector3(poolballscaleFactor);



            BuildBallsTriangle(new Vector3(MIN_X / 3, SURFACEPOS_Y + World.ballRadius, -World.ballRadius), World.gameMode, World.ballRadius);

            //((CribsBasement)(World.scenario)).eightRackTriangle.Position = poolBalls[4].Position + new Vector3(-World.ballRadius, -World.ballRadius, 0);

            roundInfo.cueballState = new PoolBallState(cueBall.Position, cueBall.isInPlay, cueBall.InitialRotation,
                cueBall.pocketWhereAt, cueBall.currentTrajectory);

            PoolGame.game.Components.Add(cueBall);


            for (int i = 0; i < TotalBalls; i++)
            {
                roundInfo.ballsState.Add(new PoolBallState(poolBalls[i].Position,
                    poolBalls[i].isInPlay, poolBalls[i].InitialRotation, poolBalls[i].pocketWhereAt,
                    poolBalls[i].currentTrajectory));

                PoolGame.game.Components.Add(poolBalls[i]);
            }

            World.scenario.scene.Add(cueBall);
            for (int i = 0; i < TotalBalls; i++)
                World.scenario.scene.Add(poolBalls[i]);

            loaded = true;
        }

        public void Reset()
        {
            loaded = false;

            for (int i = 0; i < TotalBalls; i++)
            {
                poolBalls[i].Stop();
                poolBalls[i].Position = roundInfo.ballsState[i].Position;
                poolBalls[i].isInPlay = roundInfo.ballsState[i].isInplay;
                poolBalls[i].InitialRotation = roundInfo.ballsState[i].RotationInitial;
                poolBalls[i].Rotation = Matrix.Identity;//Matrix.CreateFromQuaternion(roundInfo.ballsState[i].LocalQuat);
                poolBalls[i].currentTrajectory = roundInfo.ballsState[i].currentTrajectory;
                poolBalls[i].pocketWhereAt = roundInfo.ballsState[i].PocketWhereAt;
                poolBalls[i].previousHitRail = -1; poolBalls[i].previousInsideHitRail = -1;
            }
            cueBall.Stop();
            cueBall.Position = roundInfo.cueballState.Position;
            cueBall.isInPlay = roundInfo.cueballState.isInplay;
            cueBall.InitialRotation = roundInfo.cueballState.RotationInitial;
            cueBall.Rotation = Matrix.Identity;
            cueBall.currentTrajectory = roundInfo.cueballState.currentTrajectory;
            cueBall.pocketWhereAt = roundInfo.cueballState.PocketWhereAt;
            cueBall.previousHitRail = -1; cueBall.previousInsideHitRail = -1;
            pp = Vector3.Zero;
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

        public void BuildBallsTriangle(Vector3 triangleOrigin, GameMode gameMode, float ballRadius)
        {
            bool[] ballsReady;
            TotalBalls = 0;
            switch (gameMode)
            {
                #region Black Mode
                case GameMode.Black:
                    float ballDiameter = 2.0f * ballRadius;
                    poolBalls = new Ball[15];
                    
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
                        xPos = triangleOrigin.X - (row * ballDiameter) + 2.7f * row;

                        zOffset = triangleOrigin.Z - (row * ballRadius);

                        for (int col = 1; col <= row; col++)
                        {
                            zPos = zOffset + (col * ballRadius * 2);

                            if (row == BLACK_ROW && col == BLACK_COL)
                            {
                                ballnumber = 7;
                                poolBalls[TotalBalls] = new Ball(PoolGame.game, 8, "Models\\Balls\\newball", "Textures\\Balls\\ball 8", ballRadius);
                            }
                            else if (row == 5 && (col == 1 || col == 5))
                            {
                                ballnumber = col == 1 ? solidballnumber : stripeballnumber;

                                poolBalls[TotalBalls] = new Ball(PoolGame.game, ballnumber + 1, "Models\\Balls\\newball", "Textures\\Balls\\ball " + (ballnumber + 1), ballRadius);
                                
                            }
                            else
                            {
                                ballnumber = findRandomBall(ballsReady);
                                poolBalls[TotalBalls] = new Ball(PoolGame.game, ballnumber + 1, "Models\\Balls\\newball", "Textures\\Balls\\ball " + (ballnumber + 1), ballRadius);
                            }
                            ballsReady[ballnumber] = true;

                            poolBalls[TotalBalls].Scale = new Vector3(poolballscaleFactor);
                            poolBalls[TotalBalls].SetCenter(xPos, triangleOrigin.Y, zPos);
                            TotalBalls++;
                        }

                    }

                    break;
                #endregion
                #region Nine balls Mode
                case GameMode.NineBalls:
                    TotalBalls = 1;
                    poolBalls = new Ball[1];
                    poolBalls[0] = new Ball(PoolGame.game, 1, "Models\\Balls\\newball", "Textures\\Balls\\Ball 1", World.ballRadius);
                    poolBalls[0].SetCenter(triangleOrigin);
                    break;
                #endregion
            }
        }

        public virtual void BuildPockets()
        {
            Console.WriteLine("virtual void de pool table...");
        }
        #endregion

        Vector3 pp = Vector3.Zero;
        public bool CheckPocketsBoundaries(Ball p)
        {
            //if (pp == Vector3.Zero) return false;
            //p.pocketWhereAt = 0;
            //Vector3 tmp = Vector3.Zero;
            Vector3 tmp = p.velocity;

            if (tmp.Length() > 0)
                tmp.Normalize();

            tmp *= p.Radius;

            float x2 = (pockets_bounds[p.pocketWhereAt].Center.X - p.Position.X - tmp.X) * (pockets_bounds[p.pocketWhereAt].Center.X - p.Position.X - tmp.X);
            float y2 = (pockets_bounds[p.pocketWhereAt].Center.Z - p.Position.Z - tmp.Z) * (pockets_bounds[p.pocketWhereAt].Center.Z - p.Position.Z - tmp.Z);
            float r2 = pockets_bounds[p.pocketWhereAt].Radius * pockets_bounds[p.pocketWhereAt].Radius;

            if (x2 + y2 - r2 >= 2 && (p.Position + tmp) != pp)
            {

                Vector3 normal = new Vector3(-(p.Position.X + tmp.X) + pockets_bounds[p.pocketWhereAt].Center.X, 0, -(p.Position.Z + tmp.Z) + pockets_bounds[p.pocketWhereAt].Center.Z);
                normal.Normalize();


                float rapidez = p.velocity.Length();

                pp = p.Position + tmp;
                //normal *= rapidez;
                tmp /= p.Radius;

                p.InitialRotation *= p.Rotation;
                p.Rotation = Matrix.Identity;

                //if ((normal - tmp) == Vector3.Zero) 
                //    tmp = Vector3.Zero;
                tmp = normal - tmp;
                tmp.Normalize();
                p.velocity = tmp * rapidez * 0.02f;
                p.velocity.Y += World.gravity;
                //p.velocity.Y = World.gravity * 40f;


                //Console.WriteLine(p.velocity);
                //p.velocity.X = 0;
                //p.velocity.Z = 0;
                //p.velocity.Y = 0;

                return true;
            }

            return false;
        }

        #region Update
        public override void Update(GameTime gameTime)
        {
            if (!loaded || cueBall == null) return;
            if (ballsMoving)
            {
                /*if (CheckBallIsPotted(cueBall))
                {
                    //roundInfo.cueballPotted = true;
                }
                foreach (PoolBall p in poolBalls)
                {
                    if (CheckBallIsPotted(p))
                    {
                        // check rules!!
                        p.isInPlay = false;
                    }
                }*/

                //CheckBallCollisions(gameTime);
                CheckSideCollisions(gameTime);
            }

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

        public void CheckSideCollisions(GameTime gameTime)
        {
            if (!CheckInsidePocketCollisionsWithBall(cueBall))
            {
                //if (CheckSideCollisionsWithBall(cueBall))
                {
                    // cueball hit one side

                }
            }
            if (cueBall.pocketWhereAt != -1)
            {
                CheckPocketsBoundaries(cueBall);
            }

            for (int i = 0; i < TotalBalls; i++)
            {
                if (!CheckInsidePocketCollisionsWithBall(poolBalls[i]))
                {
                    //if (CheckSideCollisionsWithBall(poolBalls[i]))
                    {
                        // poolBalls [i] hit one side

                    }
                }
                if (poolBalls[i].pocketWhereAt != -1)
                {
                    CheckPocketsBoundaries(poolBalls[i]);
                }
            }
        }

        OrientedBoundingBox localobb;
        public bool CheckInsidePocketCollisionsWithBall(Ball p)
        {
            for (int i = 0; i < insidebands_pockets.Length; i++)
            {
                if (insidebands_pockets[i] == null) continue;
                localobb = new OrientedBoundingBox(p.Position,
                    Vector3.One * p.Radius * 0.699f, Matrix.CreateRotationY(MathHelper.ToRadians(World.players[World.playerInTurn].stick.AngleY)));
                //Vector3 aa = p.velocity;
                //if (aa != Vector3.Zero) aa.Normalize();

                //localobb = new OrientedBoundingBox(p.Position + aa * p.Radius,
                //    Vector3.One, Matrix.CreateRotationY(MathHelper.ToRadians( World.players[World.playerInTurn].stick.AngleY)));


                if (localobb.Intersects(insidebands_pockets[i]) && p.previousInsideHitRail != i)
                {
                    p.InitialRotation *= p.Rotation;
                    p.Rotation = Matrix.Identity;

                    p.previousInsideHitRail = i;


                    float rapidez = p.velocity.Length();

                    Vector3 normal = inside_normals[i];

                    p.velocity = Vector3.Reflect(p.velocity, normal) * 0.8f;

                    return true;
                }
            }
            return false;
        }
        
        #endregion

        #region Check for balls collisions with others balls

        private bool AreSpheresColliding(Ball s1, Ball s2, float elapsedSeconds, out float timeAfterCollision)
        {
            timeAfterCollision = elapsedSeconds;
            Vector3 relativeVelocity = s1.velocity - s2.velocity;
            float radius = World.ballRadius;
            Vector3 relativePosition = s1.Position - s2.Position;

            // If the relative movement of two spheres show that they are moving away, no collision.
            float relativeMovement = Vector3.Dot(relativePosition, relativeVelocity);
            if (relativeMovement >= 0)
            {
                return false;
            }

            // Checks if two spheres are already colliding.
            if (relativePosition.LengthSquared() - (radius * radius) <= 0.0f)
            {
                return true;
            }


            //is this still required?
            float relativeDistance = relativePosition.Length() - radius;
            if (relativeDistance <= radius)
            {
                return true;
            }

            // does collision happen this frame
            // how much time remains after collision?
            if (relativeDistance < relativeVelocity.Length() * elapsedSeconds)
            {
                float timeFraction = relativeDistance / relativeVelocity.Length();

                s1.Position = s1.Position + s1.velocity * timeFraction;
                s2.Position = s2.Position + s2.velocity * timeFraction;

                timeAfterCollision = elapsedSeconds * (1.0f - timeFraction);

                relativePosition = s1.Position - s2.Position;

                if ((relativePosition.LengthSquared() - (radius * radius)) <= 0)
                {
                    return true;
                }
            }

            return false;
        }
        private void CollideSphereWithSphere(Ball s1, Ball s2, float elapsedSeconds, out float remainingTime)
        {
            if (AreSpheresColliding(s1, s2, elapsedSeconds, out remainingTime))
            {
                //we have collision, compute new velocities;

                Vector3 relativePosition = s1.Position - s2.Position;
                Vector3 relativeUnit = Vector3.Normalize(relativePosition);

                float s1VelDotUnit = Vector3.Dot(s1.velocity, relativeUnit);
                float s2VelDotUnit = Vector3.Dot(s2.velocity, relativeUnit);

                float momentumDifference = (2.0f * (s1VelDotUnit - s2VelDotUnit)) / (2.0f);

                // Compute the final velocity of the two spheres.
                s1.velocity = s1.velocity - momentumDifference * relativeUnit;
                s2.velocity = s2.velocity + momentumDifference * relativeUnit;

                s1.previousHitRail = -1; s1.previousInsideHitRail = -1;
                s2.previousHitRail = -1; s2.previousInsideHitRail = -1;
            }
            else
            {
                remainingTime = elapsedSeconds;
            }
        }

        private void CheckBallCollisions(GameTime gameTime)
        {
            for (int i = 0; i < TotalBalls; ++i)
            {
                float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
                CollideSphereWithSphere(poolBalls[i], cueBall, dt, out dt);

                for (int j = i + 1; j < TotalBalls; ++j)
                {
                    dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
                    CollideSphereWithSphere(poolBalls[i], poolBalls[j], dt, out dt);
                }
            }
                
            /*foreach (Ball ball in poolBalls)
            {
                if (!ball.isInPlay) continue;
                if (ball.NextBound().Intersects(cueBall.NextBound()))
                {
                    if (roundInfo.ballHitFirstThisRound == null) roundInfo.ballHitFirstThisRound = ball;
                    ball.previousHitRail = -1; cueBall.previousHitRail = -1;
                    ball.previousInsideHitRail = -1; cueBall.previousInsideHitRail = -1;
                    ball.Bounce(cueBall);
                }
                foreach (Ball other in poolBalls)
                {
                    if (other.Equals(ball)) continue;
                    if (!other.isInPlay) continue;

                    if (other.NextBound().Intersects(ball.NextBound()))
                    {
                        ball.previousHitRail = -1; other.previousHitRail = -1;
                        ball.previousInsideHitRail = -1; other.previousInsideHitRail = -1;
                        other.Bounce(ball);
                    }
                }
            }*/


        }

        public bool CheckBallIsPotted(Ball p)
        {

            for (int i = 0; i < pockets_bounds.Length; i++)
            {
                float x2 = (pockets_bounds[i].Center.X - p.PositionX) * (pockets_bounds[i].Center.X - p.PositionX);
                float y2 = (pockets_bounds[i].Center.Z - p.PositionZ) * (pockets_bounds[i].Center.Z - p.PositionZ);
                float r2 = pockets_bounds[i].Radius * pockets_bounds[i].Radius;

                if (x2 + y2 - r2 <= -4.0f)
                {
                    p.min_Altitute = 173.44f;
                    p.pocketWhereAt = i;
                    p.currentTrajectory = Trajectory.Free;
                    p.velocity.Y = -p.velocity.Length();


                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Draw model and draw bounding box
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            //base.DrawBoundingBox();

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
            vectorRenderer.SetColor(Color.Yellow);
            for (int i = 0; i < insidebands_pockets.Length; i++)
            {
                if (insidebands_pockets[i] == null) continue;
                vectorRenderer.SetWorldMatrix(insidebands_pockets[i].WorldTransform);
                vectorRenderer.DrawBoundingBox(insidebands_pockets[i].LocalBoundingBox);
            }

            //vectorRenderer.SetWorldMatrix(localobb.WorldTransform);
            //vectorRenderer.DrawBoundingBox(localobb.LocalBoundingBox);

            vectorRenderer.SetWorldMatrix(Matrix.Identity);
            vectorRenderer.SetColor(Color.Aqua);
            for (int i = 0; i < pockets_bounds.Length; i++)
            {
                vectorRenderer.DrawBoundingSphere(pockets_bounds[i]);
            }

            PoolGame.device.RenderState.DepthBufferEnable = true;
            PoolGame.device.RenderState.DepthBufferWriteEnable = true;
#endif

        }
        #endregion

        #region Check if any ball is moving
        private bool CheckBallMovement()
        {
            ballsMoving = false;
            if (cueBall.IsMoving()) ballsMoving = true;

            for (int i = 0; i < TotalBalls && !ballsMoving; i++)
            {
                if (poolBalls[i].IsMoving())
                {
                    ballsMoving = true;
                    break;
                }
            }
            return ballsMoving;
        }
        #endregion

        #region Get out the cue ball if were potted
        public void UnpottedcueBall()
        {
            roundInfo.cueballPotted = false;
            cueBall.pocketWhereAt = -1;
            cueBall.Visible = true;
            cueBall.SetCenter(MAX_X - Math.Abs(MAX_X - MIN_X) / 5, SURFACEPOS_Y + World.ballRadius, 0);
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            
            if (vectorRenderer != null) vectorRenderer.Dispose();
            base.Dispose(disposing);

            if (cueBall != null)
            {
                cueBall.Dispose();
                cueBall = null;
            }
            for (int i = 0; i < TotalBalls; i++)
            {
                poolBalls[i].Dispose();
                poolBalls[i] = null;
            }
            TotalBalls = 0;

            poolBalls = null;
        }
        #endregion

        
        

        
    }
}
