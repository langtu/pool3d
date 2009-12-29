using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNA_PoolGame.Models;
using Microsoft.Xna.Framework;
using System.Threading;
using System.Diagnostics;

namespace XNA_PoolGame
{
    /// <summary>
    /// The Ball
    /// </summary>
    public class Ball : BasicModel
    {
        public int ballNumber;
        private float radius;
        

        public Vector3 last_position = Vector3.Zero;
        public Vector3 direction = Vector3.Zero;

        public Vector3 velocity = Vector3.Zero;
        public Vector3 acceleration = Vector3.Zero;
        public Vector2 angularvelocity = Vector2.Zero;

        private float mass = 0.22f;
        private float dt = 0.0f;

        public bool isInPlay = true;
        public int pocketWhereAt = -1;

        public const float MIN_SPEED = 2.73333f;//0.02999f;//0.10f;
        public const float MIN_SPEED_Y = 0.73333f;//0.02999f;//0.10f;
        public int previousHitRail = -1, previousInsideHitRail = -1;
        private float MIN_SPEED_SQUARED;
        public float min_Altitute = World.ballRadius + World.poolTable.SURFACEPOS_Y;
        public Trajectory currentTrajectory = Trajectory.Motion;

        #region Properties
        public float Radius
        {
            get { return radius; }
        }

        public float Mass
        {
            get { return mass; }
            set { mass = value; }
        }

        public Vector3 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        public Vector3 Center
        {
            get { return new Vector3(Position.X, Position.Y, Position.Z); }
        }

        public float Diameter
        {
            get { return radius * 2; }
        }

        public bool IsMoving()
        {
            if (velocity.X == 0 && velocity.Y == 0 && velocity.Z == 0)
                return false;
            else
                return true;
        }


        public void SetCenter(float x, float y, float z)
        {
            this.Position = new Vector3(x, y, z);
        }
        public void SetCenter(Vector3 v_center)
        {
            this.Position = v_center;
        }

        #endregion

        #region Constructors

        public Ball(Game game, int ballNumber, String ballModel, float radius)
            : base(game, ballModel)
        {
            this.ballNumber = ballNumber;
            this.radius = radius;
            previousHitRail = -1; previousInsideHitRail = -1; pocketWhereAt = -1;
            isInPlay = true;
        }

        public Ball(Game game, int ballNumber, String ballModel, String ballTexture, float radius)
            : this(game, ballNumber, ballModel, radius)
        {
            this.textureAsset = ballTexture;

        }

        public override void Initialize()
        {
            //this.UpdateOrder = 4 + ballNumber; this.DrawOrder = 4 + ballNumber;
            this.UpdateOrder = 4; //this.DrawOrder = 4;
            

            MIN_SPEED_SQUARED = MIN_SPEED * MIN_SPEED;
            base.Initialize();
        }
        #endregion

        public void probarfree()
        {
            velocity = (World.players[World.playerInTurn].stick.Direction + Vector3.Up) * 15f;

            currentTrajectory = Trajectory.Free;


        }

        #region Update
        public override bool UpdateLogic(GameTime gameTime)
        {
            return false;
            
        }
        
        public override void Update(GameTime gameTime)
        {
            dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float t;
            switch (currentTrajectory)
            {
                #region Motion
                case Trajectory.Motion:
                    if (!IsMoving()) return;

                    //friction deceleration
                    acceleration -= velocity * 0.5f;

                    //recompute new velocity and position
                    velocity += acceleration * dt;

                    if (velocity.LengthSquared() < MIN_SPEED_SQUARED)
                    {
                        velocity = Vector3.Zero;
                        acceleration = Vector3.Zero;
                    }

                    float remainingTime = dt;

                    if (this.pocketWhereAt == -1 && World.poolTable.cueBall != null)
                    {
                        if (World.poolTable.cueBall != this)
                            CollideSphereWithSphere(this, World.poolTable.cueBall, remainingTime, out remainingTime);

                        foreach (Ball sd in World.poolTable.poolBalls)
                        {
                            if (sd != this && sd.pocketWhereAt == -1)
                                CollideSphereWithSphere(this, sd, remainingTime, out remainingTime);
                        }
                    }

                    if (World.poolTable.cueBall != null)
                        CollideSphereWithWall(this, remainingTime);

                    Vector3 movementDelta = velocity * remainingTime;
                    this.Position += movementDelta;

                    float distanceMoved = movementDelta.Length();

                    Vector3 frontVector = velocity;
                    Vector3 upvector = Vector3.Up;
                    Vector3 rightVector = Vector3.Cross(frontVector, upvector);
                    if (rightVector.Length() > 0.0f)
                        rightVector.Normalize();

                    Rotation *= Matrix.CreateFromAxisAngle(rightVector, -distanceMoved * World.ballRadius * 0.0230f);
                    //Rotation *= Matrix.CreateFromAxisAngle(rightVector, -distanceMoved);
                    acceleration = Vector3.Zero;
                    break;
                #endregion
                #region Free
                case Trajectory.Free:
                    t = dt * World.timeFactor;

                    if (IsMoving())
                    {
                        //Vector3 tmp = velocity - new Vector3(0, velocity.Y, 0);
                        Vector3 tmp = velocity;
                        Vector3 axis = Vector3.Normalize(Vector3.Cross(Vector3.Up, velocity));
                        float angle = tmp.Length() * World.ballRadius * 0.0229f * t;
                        //float angle = velocity.Length() * dt * MathHelper.TwoPi / World.ballRadious;

                        //Quaternion rotationThisFrame = Quaternion.CreateFromAxisAngle(axis, angle);
                        //localRotation *= rotationThisFrame;

                        //localRotation.Normalize();

                        //this.Rotation = Matrix.CreateFromQuaternion(localRotation);
                    }

                    PositionX += velocity.X * t;
                    PositionZ += velocity.Z * t;

                    if (PositionY > min_Altitute)
                        velocity.Y += World.gravity * t;

                    if (PositionY < min_Altitute - 0.05f)
                    {
                        PositionY = min_Altitute;
                        velocity.Y *= -0.08f;
                        velocity.X *= 0.7f;
                        velocity.Z *= 0.7f;
                    }
                    else
                    {
                        PositionY += velocity.Y * t + 0.5f * World.gravity * t * t;
                    }
                    /*if (Math.Abs(velocity.X) < MIN_SPEED)
                    {
                        velocity.X = 0;
                    }*/
                    /*if (Math.Abs(velocity.Z) < MIN_SPEED)
                    {
                        velocity.Z = 0;
                    }*/

                    //Console.WriteLine(velocity.Y);
                    if (Math.Abs(velocity.X) < MIN_SPEED && Math.Abs(velocity.Z) < MIN_SPEED && Math.Abs(velocity.Y) < MIN_SPEED_Y
                        && pocketWhereAt != -1 && PositionY >= min_Altitute && PositionY <= min_Altitute + 2.0f)
                    {
                        Stop();
                    }

                    break;
                #endregion
                #region Potting
                case Trajectory.Potting:
                    //float x, y, z;
                    t = dt * World.timeFactor;

                    if (IsMoving())
                    {
                        Vector3 tmp = velocity - new Vector3(0, velocity.Y, 0);
                        Vector3 axis = Vector3.Normalize(Vector3.Cross(Vector3.Up, velocity));
                        float angle = tmp.Length() * World.ballRadius * 0.0229f * t;
                        //float angle = velocity.Length() * dt * MathHelper.TwoPi / World.ballRadious;

                        //Quaternion rotationThisFrame = Quaternion.CreateFromAxisAngle(axis, angle);
                        //localRotation *= rotationThisFrame;

                        //localRotation.Normalize();

                        //this.Rotation = Matrix.CreateFromQuaternion(localRotation);
                    }


                    PositionX += velocity.X * t;
                    PositionZ += velocity.Z * t;



                    //if (PositionY > min_Altitute) PositionY += velocity.Y * t + 0.5f * World.gravity * t * t;

                    PositionY += velocity.Y * t + 0.5f * World.gravity * t * t;
                    velocity.Y += World.gravity * t;

                    //if (PositionY < min_Altitute + 3.0f)
                    if (PositionY < min_Altitute)
                    {
                        //PositionY = min_Altitute + 3.0f;
                        PositionY = min_Altitute;

                        velocity.Y = (Math.Abs(velocity.Y) * 0.05f);// *(1 / World.timeFactor);
                    }
                    //else velocity.Y += World.gravity * t;


                    if (PositionY == min_Altitute)
                    {
                        velocity.X *= World.poolTable.FRICTION_SURFACE;
                        velocity.Z *= World.poolTable.FRICTION_SURFACE;
                    }

                    if (IsMoving())
                    {
                        //if (Math.Abs(velocity.X) < MIN_SPEED && Math.Abs(velocity.Z) < MIN_SPEED && Math.Abs(velocity.Y) < MIN_SPEED)
                        if (Math.Abs(velocity.X) < MIN_SPEED && Math.Abs(velocity.Z) < MIN_SPEED)
                        {
                            Stop();
                        }
                    }
                    break;
                #endregion
            }
            base.Update(gameTime);
        }
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
        

        private void CollideSphereWithWall(Ball ball, float remainingTime)
        {
            
            for (int i = 0; i < World.poolTable.rails.Length; i++)
            {
                if (!ball.isInPlay) continue;
                if (ball.ThisBound().Intersects(World.poolTable.rails[i]) && i != ball.previousHitRail)
                {
                    ball.InitialRotation *= ball.Rotation;
                    ball.Rotation = Matrix.Identity;
                    ball.previousHitRail = i;


                    ball.velocity = Vector3.Reflect(ball.velocity, World.poolTable.railsNormals[i]);
                    ball.acceleration = Vector3.Reflect(ball.acceleration, World.poolTable.railsNormals[i]);
                    ball.direction = Vector3.Reflect(ball.direction, World.poolTable.railsNormals[i]);
                    return;
                }
            }
        }
        #endregion

        #region Draw
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
        #endregion

        #region Bounding spheres of current position
        public BoundingSphere ThisBound()
        {
            return new BoundingSphere(Center, Radius);
        }
        
        #endregion

        #region Apply force and Bounce ball
        
        public void ApplyImpulse(Vector3 impulse)
        {
            this.InitialRotation *= this.Rotation;
            this.Rotation = Matrix.Identity;
            velocity += impulse;
        }

        public void Bounce(Ball other)
        {
            Vector3 impact = other.Velocity - this.Velocity;
            Vector3 impulse = Vector3.Normalize(other.Position - this.Position);

            float impactSpeed = Vector3.Dot(impact, impulse);

            impulse.X *= impactSpeed;
            impulse.Y *= impactSpeed;
            impulse.Z *= impactSpeed;

            other.ApplyImpulse(-impulse);
            this.ApplyImpulse(impulse);
        }
        #endregion

        #region Stop the ball
        public void Stop()
        {
            //InitialRotation = Rotation;
            //Rotation = Matrix.Identity;
            acceleration = Vector3.Zero;
            velocity = Vector3.Zero;
            previousHitRail = -1; previousInsideHitRail = -1;
            currentTrajectory = Trajectory.Motion;
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
        #endregion

        
    }
}
