#define DRAWBALL_BV
#define DRAWBALL_BV_AFTERISPOTTED

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNA_PoolGame.Graphics;
using Microsoft.Xna.Framework;
using System.Threading;
using System.Diagnostics;
using XNA_PoolGame.Helpers;
using XNA_PoolGame.PoolTables;
using XNA_PoolGame.Graphics.Models;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace XNA_PoolGame
{
    /// <summary>
    /// Define a ball.
    /// </summary>
    public class Ball : Entity
    {
        public int ballNumber;
        private float radius;

        public volatile bool collisionFlag = false;
        public volatile bool thinkingFlag = false;

        public Vector3 last_position = Vector3.Zero;
        public Vector3 direction = Vector3.Zero;

        public Vector3 initialvelocity = Vector3.Zero;
        public Vector3 velocity = Vector3.Zero;
        public Vector3 previousvelocity = Vector3.One * 550.0f; // any number
        public Vector3 prepreviousvelocity = Vector3.Up * 750.0f; // any number

        public Vector3 acceleration = Vector3.Zero;
        public Vector2 angularVelocity = Vector2.Zero;
        public Vector3 rightVector = Vector3.Zero;
        public float angleRotation = 0.0f;
        public OrientedBoundingBox obb;

        public Matrix invWorldInertia = Matrix.Identity;
        public Quaternion rotQ = Quaternion.Identity;
        public Vector3 angularMomentum = Vector3.Zero;
        public Matrix invBodyInertia = Matrix.Identity;

        private const int numSteps = 6;

        public float mMass = 1.0f;

        public float totaltime = 0.0f;

        /// <summary>
        /// Index of the pocket where the ball is.
        /// </summary>
        public volatile int pocketWhereAt = -1;

        /// <summary>
        /// Index of the previous rail hit.
        /// </summary>
        public int previousHitRail = -1;

        /// <summary>
        /// Index of the previous inside rail hit.
        /// </summary>
        public int previousInsideHitRail = -1;

        public const float MIN_SPEED = 2.73333f;//0.02999f;//0.10f;
        public const float MIN_SPEED_Y = 9.73333f;//0.02999f;//0.10f;
        private float MIN_SPEED_SQUARED;

        /// <summary>
        /// The Pooltable where this ball belongs to.
        /// </summary>
        public PoolTable table = null;
        public Trajectory currentTrajectory = Trajectory.Motion;
        public float min_Altitute = World.ballRadius + World.poolTable.SURFACE_POSITION_Y;

        /// <summary>
        /// Determinate if this ball is in lagging phase.
        /// </summary>
        private bool isLagging = false;
        public List<int> ballRailHitsIndexes;

        #region Properties
        public float Radius
        {
            get { return radius; }
        }

        public float Mass
        {
            get { return mMass; }
            set { mMass = value; }
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
            get { return radius * 2.0f; }
        }

        public bool IsLagging
        {
            get { return isLagging; }
            set { isLagging = value; }
        }

        public bool IsMoving()
        {
            //while (this.thinkingFlag) { }

            if (currentTrajectory == Trajectory.Motion)
            {
                if (velocity.LengthSquared() < MIN_SPEED_SQUARED)
                    return false;
                else
                    return true;
            }
            else if (currentTrajectory == Trajectory.Free)
            {
                //if (velocity.LengthSquared() < MIN_SPEED_SQUARED /*&& (this.PositionY == min_Altitute)*/)
                if (prepreviousvelocity == previousvelocity && previousvelocity == velocity)
                    return false;
                else
                    return true;
            }
            return false;
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

        public Ball(Game game, int ballNumber, string ballModel, PoolTable table, float radius)
            : base(game, ballModel, true)
        {
            this.table = table;
            this.ballNumber = ballNumber;
            this.radius = radius;
            previousHitRail = -1; previousInsideHitRail = -1;
            pocketWhereAt = -1;
            volume = VolumeType.BoundingSpheres;
            useModelPartBB = false;
            rightVector = Vector3.Zero;
            ballRailHitsIndexes = new List<int>();
#if DRAWBALL_BV
            drawboundingvolume = true;
#endif
        }

        public Ball(Game game, int ballNumber, string ballModel, string ballTexture, PoolTable table, float radius)
            : this(game, ballNumber, ballModel, table, radius)
        {
            this.textureAsset = ballTexture;

        }

        public override void Initialize()
        {
            this.UpdateOrder = 4; //this.DrawOrder = 4;

            MIN_SPEED_SQUARED = MIN_SPEED * MIN_SPEED;

            if (this.ballNumber >= 1 && this.ballNumber <= 7)
                this.bvColor = Color.Aqua;
            else if (this.ballNumber >= 9 && this.ballNumber <= 15)
                this.bvColor = Color.Plum;
            else
                this.bvColor = Color.Red;

            base.Initialize();
        }
        #endregion

        /// <summary>
        /// Set Ball's Oriented Bounding Box for check inside pocket-ball collision.
        /// This must be called everytime the direction vector changes.
        /// </summary>
        private void SetOBB()
        {
            float directionAngle;
            if (direction.Z == 0.0f) directionAngle = (float)Math.Atan(direction.X);
            else directionAngle = (float)Math.Atan(direction.X / direction.Z);

            Matrix rotation = Matrix.CreateRotationY(directionAngle);


            obb = new OrientedBoundingBox(this.Position,
                        Vector3.One * this.Radius * 0.6845f,
                        rotation);
        }

        #region HandleMotion
        public void HandleMotion(float _dt)
        {
            //lock (this.syncObject)

            for (int step = 0; step < numSteps; ++step)
            {
                float dt = _dt / (float)numSteps;
                thinkingFlag = true;

                //while (this.collisionFlag) {
                //Monitor.Wait(syncObject);
                //}
                //while (this.collisionFlag) { }

                if (direction == Vector3.Zero)
                {
                    direction = velocity;
                    direction.Normalize();
                }

                SetOBB();

                // compute acceleration
                acceleration -= velocity * 0.5f;

                //recompute new velocity and position
                Vector3 velocityOnThisFrame = acceleration * dt;
                velocity += velocityOnThisFrame;

                if (velocity.LengthSquared() < MIN_SPEED_SQUARED && !this.collisionFlag)
                {
                    velocity = Vector3.Zero;
                    initialvelocity = Vector3.Zero;
                    acceleration = Vector3.Zero;
                    previousHitRail = -1;
                    previousInsideHitRail = -1;
                    direction = Vector3.Zero;
                    totaltime = 0.0f;
                    //this.InitialRotation = this.Rotation;
                }

                float remainingTime = dt;

                if (this.pocketWhereAt == -1 && table.cueBall != null)
                {
                    #region BFS
                    int collisionResult = 0;
                    Dictionary<Ball, bool> mark = new Dictionary<Ball, bool>();

                    for (int i = 0; i < table.TotalBalls; ++i) mark[table.poolBalls[i]] = this == table.poolBalls[i];

                    Queue<Ball> queue = new Queue<Ball>();
                    queue.Enqueue(this);

                    while (queue.Count > 0)
                    {
                        Ball T = queue.Dequeue();

                        for (int i = 0; i < table.TotalBalls; ++i)
                        {
                            if (table.poolBalls[i].Visible && !mark[table.poolBalls[i]] && Vector3.Distance(this.Position, table.poolBalls[i].Position) <= 2.0f * this.radius)
                            {
                                collisionResult = CheckBallWithBallCollision(this, table.poolBalls[i], remainingTime, out remainingTime);
                                if (collisionResult != 0)
                                {
                                    if (table.poolBalls[i] == table.cueBall && table.roundInfo.BallHitFirstThisRound == null)
                                        table.roundInfo.BallHitFirstThisRound = this;

                                    if (this == table.cueBall && table.roundInfo.BallHitFirstThisRound == null)
                                        table.roundInfo.BallHitFirstThisRound = table.poolBalls[i];
                                }
                                queue.Enqueue(table.poolBalls[i]);
                                mark[table.poolBalls[i]] = true;

                            }
                        }

                    }
                    #endregion
                }

                //angularVelocity
                int railIndex;
                if ((railIndex = CheckBallWithRailCollision(remainingTime)) != -1)
                {
                    if (this == table.cueBall)
                    {
                        // cue ball hit a side
                        table.roundInfo.cueBallHitRail(railIndex);
                    }
                    else if (IsLagging)
                    {
                        ballRailHitsIndexes.Add(railIndex);

                    }
                }
                else
                {
                    if (CheckInsidePocketCollision())
                    {
                    }
                }

                

                // (x - x0) = vo * dt + 0.5 * a * dt * dt
                Vector3 movementDelta = velocity * remainingTime + 0.5f * acceleration * remainingTime * remainingTime;


                this.Position += movementDelta;

                if (Position.Y < this.table.SURFACE_POSITION_Y + this.radius)
                {
                    PositionY = this.table.SURFACE_POSITION_Y + this.radius;
                    velocity.Y = -velocity.Y * 0.65f;

                    if (Math.Abs(velocity.Y) < MIN_SPEED_Y) velocity.Y = 0.0f;
                }
                else if (Position.Y > this.table.SURFACE_POSITION_Y + this.radius)
                {
                    velocity.Y += World.gravity * dt;
                }

                float angleOnThisFrame = (-movementDelta.Length() / this.radius);

                angleRotation += angleOnThisFrame;
                this.Rotation = Matrix.CreateFromAxisAngle(rightVector, angleRotation);

                if (this.pocketWhereAt == -1 && CheckBallIsPotted())
                {
                    table.roundInfo.BallsPottedThisRound.Add(this);
                    if (this == table.cueBall)
                        table.roundInfo.cueballPotted = true;
                }

                acceleration = Vector3.Zero;

                // add time
                totaltime += remainingTime;


                thinkingFlag = false;
                if (this.pocketWhereAt != -1) break;

            }
        }
        #endregion
        #region HandleFree
        public void HandleFree(float _dt)
        {
            for (int step = 0; step < numSteps && this.currentTrajectory == Trajectory.Free && this.pocketWhereAt != -1; ++step)
            {
                prepreviousvelocity = previousvelocity;
                previousvelocity = velocity;

                this.thinkingFlag = true;
                float dt = _dt / (float)numSteps;
                acceleration -= velocity;

                float x2 = (table.pockets[this.pocketWhereAt].bounds.Center.X - this.PositionX) * (table.pockets[this.pocketWhereAt].bounds.Center.X - this.PositionX);
                float y2 = (table.pockets[this.pocketWhereAt].bounds.Center.Z - this.PositionZ) * (table.pockets[this.pocketWhereAt].bounds.Center.Z - this.PositionZ);
                float r = table.pockets[this.pocketWhereAt].bounds.Radius + 0.6f;
                float thisr2 = this.radius * this.radius;
                float r2 = r * r;

                float t = x2 + y2 - r2;
                //Console.WriteLine(t);
                float remainingTime = dt;
                bool hascollided = false;
                float constant = 1.0f;
                if (t <= -60.0f && this.PositionY > min_Altitute)
                {
                    if (t > -thisr2 && this.PositionY <= table.SURFACE_POSITION_Y + this.radius &&
                        this.PositionY >= table.SURFACE_POSITION_Y - 5.0f)
                    {
                        Vector3 normal = table.pockets[this.pocketWhereAt].bounds.Center - this.Position;
                        normal.Normalize();

                        acceleration.X += normal.X * 850.0f;
                        acceleration.Z += normal.Z * 850.0f;

                        constant = 0.6f;
                    }
                    
                }
                Dictionary<Ball, bool> pelotasVisitadas = new Dictionary<Ball, bool>();

                for (int i = 0; i < table.TotalBalls; ++i) pelotasVisitadas[table.poolBalls[i]] = this == table.poolBalls[i];

                Queue<Ball> cola = new Queue<Ball>();
                cola.Enqueue(this);

                while (cola.Count > 0)
                {
                    Ball elemento = cola.Dequeue();

                    for (int i = 0; i < table.TotalBalls; ++i)
                    {
                        if (!pelotasVisitadas[table.poolBalls[i]] && Vector3.Distance(this.Position, table.poolBalls[i].Position) <= 2.0f * this.radius)
                        {
                            CheckBallWithBallCollision(this, table.poolBalls[i], remainingTime, out remainingTime);
                            hascollided = true;
                            cola.Enqueue(table.poolBalls[i]);
                            pelotasVisitadas[table.poolBalls[i]] = true;

                        }
                    }

                }
                if (!hascollided) acceleration.Y += World.gravity * constant;
                //Console.WriteLine("acceleration = " + acceleration);


                if (this.pocketWhereAt != -1 && CheckPocketsBoundaries(remainingTime))
                {

                }
                Vector3 velocityOnThisFrame = acceleration * dt;
                velocity += velocityOnThisFrame;

                if (velocity.LengthSquared() < 0.0064f && hascollided)
                {
                    velocity = Vector3.Zero;
                    initialvelocity = Vector3.Zero;
                    acceleration = Vector3.Zero;
                    previousHitRail = -1; previousInsideHitRail = -1;
                    direction = Vector3.Zero;
                    totaltime = 0.0f;
                }

                direction = velocity;
                if (direction != Vector3.Zero) direction.Normalize();

                Vector3 movementDelta = velocity * remainingTime + 0.5f * acceleration * remainingTime * remainingTime;
                Vector3 newPosition = this.Position + movementDelta;

                if (this.pocketWhereAt != -1 && newPosition.Y <= this.min_Altitute)
                {
                    newPosition.Y = this.min_Altitute;
                    velocity.Y = -velocity.Y * 0.65f;

                    if (Math.Abs(velocity.Y) < MIN_SPEED_Y) velocity.Y = 0.0f;

                    velocity.X = velocity.X * 0.97f;
                    velocity.Z = velocity.Z * 0.97f;
                    float lenghtsquared = velocity.LengthSquared();
                    if (lenghtsquared < 0.25f && lenghtsquared > 0.0f)
                    {
                        velocity = Vector3.Zero;
                        initialvelocity = Vector3.Zero;
                        acceleration = Vector3.Zero;
                        previousHitRail = -1; previousInsideHitRail = -1;
                        direction = Vector3.Zero;
                        totaltime = 0.0f;
                    }
                }
                

                this.Position = newPosition;




                Vector3 movement = movementDelta;
                movement.Y = 0.0f;

                Vector3 frontVector = velocity;
                Vector3 upvector = Vector3.Up;
                Vector3 rightVector = Vector3.Cross(frontVector, upvector);
                if (rightVector != Vector3.Zero)
                {
                    rightVector.Normalize();

                    float angleOnThisFrame = (-movement.Length() / this.radius);
                    angleRotation += angleOnThisFrame;

                    this.Rotation = Matrix.CreateFromAxisAngle(rightVector, angleRotation);
                }
                acceleration = Vector3.Zero;
                // End
                this.thinkingFlag = false;
            }
        }
        #endregion

        #region Update

        #region Old HandleMotion
        public void HandleMotion2(float _dt)
        {
            /*if (velocity.LengthSquared() < MIN_SPEED_SQUARED)
            {
                velocity = Vector3.Zero;
                initialvelocity = Vector3.Zero;
                
                previousHitRail = -1;
                previousInsideHitRail = -1;
                direction = Vector3.Zero;
                angleRot = 0.0f;
                totalt = 0.0f;
                //this.PositionY = table.SURFACE_POSITION_Y;
                //this.InitialRotation = this.Rotation;
            }*/

            float dt2 = _dt / (float)(numSteps);

            for (int step = 0; step < 3; ++step)
            {
                // compute forces
                Vector3 forces = -0.15f * velocity;
                forces += World.mGravity;

                // compute acceleration
                Vector3 acce = forces / mMass;


                // integrate (keep it simple: just use euler)
                this.velocity += acce * dt2;
                this.Position += this.velocity * dt2;

                // compute new angular momentum (damping)
                this.angularMomentum *= 0.995f;

                // integrate angular momentum
                Matrix rot = Matrix.CreateFromQuaternion(this.rotQ);
                this.invWorldInertia = rot * this.invBodyInertia * Matrix.Transpose(rot);

                // compute angular velocity
                Vector3 omega = Vector3.Transform(this.angularMomentum, this.invWorldInertia);

                // integrate rotational part into quaternion
                Quaternion quatRotDot = new Quaternion(omega, 0.0f) * this.rotQ;
                quatRotDot *= -0.5f;
                quatRotDot *= dt2;
                //quatRotDot *= 0.01f;

                quatRotDot += this.rotQ;
                //quatRotDot *= 0.02f;
                quatRotDot.Normalize();

                this.rotQ = quatRotDot;
                this.Rotation = Matrix.CreateFromQuaternion(rotQ);

                Vector3 avgFaceNormal = Vector3.Up;
                Vector3 avgColPoint = this.Position - Vector3.Up * this.radius;
                avgColPoint.Y = Math.Max(avgColPoint.Y, table.SURFACE_POSITION_Y);

                //if (avgColPoint.Y - table.SURFACE_POSITION_Y <= 0.0001f)
                if (avgColPoint.Y - table.SURFACE_POSITION_Y == 0.0f)
                    resolveCollisions(avgColPoint, avgFaceNormal);

                handleSphereSphereCollisions();

                StopBall();
            }
        }
        private void resolveSphereSphereCollision(Ball cur, ref Ball other)
        {
            // the coefficient of restitution (1 for perfect elastic pulse)
            float e = 0.5f;
            float j = 0.0f;

            // compute penetration
            float penetration = 2 * World.ballRadius - Vector3.Distance(cur.Position, other.Position);

            // resolve player collision based on their impulses
            Vector3 rOne = Vector3.Normalize(other.Position - cur.Position);
            Vector3 rTwo = -rOne;
            Vector3 colNormal = rOne;

            rOne *= cur.radius;
            rTwo *= other.radius;

            // project positions out of collisions
            cur.Position += rTwo * penetration / 2.0f;
            other.Position += rOne * penetration / 2.0f;

            // relativ velocity
            float relVelocity = Vector3.Dot(colNormal, cur.velocity - other.velocity);

            // intermediate computations
            Vector3 tmpOne = Vector3.Cross(rOne, colNormal);
            tmpOne = Vector3.Transform(tmpOne, cur.invBodyInertia);
            tmpOne = Vector3.Cross(tmpOne, rOne);

            Vector3 tmpTwo = Vector3.Cross(rTwo, colNormal);
            tmpTwo = Vector3.Transform(tmpTwo, other.invBodyInertia);
            tmpTwo = Vector3.Cross(tmpTwo, rTwo);

            // compute j
            j = (-(1.0f + e) * relVelocity) / ((1.0f / cur.mMass) + (1.0f / other.mMass) + Vector3.Dot(colNormal, tmpOne) + Vector3.Dot(colNormal, tmpTwo));

            // compute J
            Vector3 J = j * colNormal;

            // apply the impulse to the linear velocities
            cur.velocity += J / cur.mMass;
            other.velocity -= J / other.mMass;

            // compute impulse torque
            Vector3 torqueImpulsTwo = Vector3.Cross(rTwo, -J);
            Vector3 torqueImpulsOne = Vector3.Cross(rOne, J);

            // apply the torque to the angular momentum
            cur.angularMomentum += torqueImpulsOne;
            other.angularMomentum -= torqueImpulsTwo;
        }

        private void handleSphereSphereCollisions()
        {
            // now handle sphere-sphere collisions
            //LinkedListNode<SphereProperties> curSimulatedSphere = mSimulatedSpheres.First;
            //LinkedListNode<SphereProperties> nextSimulatedSphere = mSimulatedSpheresNextStep.First;
            int curSimulatedSphere = 0;
            //int nextSimulatedSphere = 1;

            //float radiusSquared = World.ballRadius * World.ballRadius;

            // for each (unordered) sphere-sphere pair: 
            while (curSimulatedSphere < table.TotalBalls)
            {
                Ball curSphere = table.poolBalls[curSimulatedSphere];


                // spheres collide? call resolveSphereSphereCollision()
                if (curSphere != this && Vector3.Distance(this.Position, curSphere.Position) < 2.0f * this.radius)
                {

                    resolveSphereSphereCollision(this, ref curSphere);
                    // break;
                    // update the other sphere
                    //otherSphere.Value = other;
                }





                // update current sphere
                //nextSimulatedSphere.Value = curSphere;

                curSimulatedSphere = curSimulatedSphere + 1;
                // advance to next sphere
                /*curSimulatedSphere = curSimulatedSphere.Next;
                nextSimulatedSphere = nextSimulatedSphere.Next;*/
            }

            // swap lists (update all spheres at once)
            /*LinkedList<SphereProperties> tmp = mSimulatedSpheres;
            mSimulatedSpheres = mSimulatedSpheresNextStep;
            mSimulatedSpheresNextStep = tmp;*/
        }
        private void StopBall()
        {
            if (velocity.LengthSquared() < MIN_SPEED_SQUARED * MIN_SPEED_SQUARED)
            {
                velocity = Vector3.Zero;
                angularMomentum = Vector3.Zero;
                invWorldInertia = Matrix.Identity;
                rotQ = Quaternion.Identity;
                //this.PositionY = table.SURFACE_POSITION_Y + this.radius;
            }
        }

        private void resolveCollisions(Vector3 avgColPoint, Vector3 avgFaceNormal)
        {
            // average inverse collision direction
            Vector3 avgColVec = Position - avgColPoint;

            // collision direction
            Vector3 r = Vector3.Normalize(-avgColVec);
            r *= this.radius;

            // move sphere out of colliding state
            this.Position += (-r - avgColVec);

            // the coefficient of restitution (1 for perfect elastic pulse)
            float e = 0.4f;

            // the relative velocity
            float relativeVelocity = Vector3.Dot(velocity, avgFaceNormal);

            // intermediate computations
            Vector3 tmp = Vector3.Cross(r, avgFaceNormal);
            tmp = Vector3.Transform(tmp, this.invWorldInertia);
            tmp = Vector3.Cross(tmp, r);

            // the impulse's length
            float j = (-(1 + e) * relativeVelocity) / ((1 / mMass) + Vector3.Dot(avgFaceNormal, tmp));

            // the actual impulse
            Vector3 J = avgFaceNormal * j;

            // apply the impulse to the linear velocity
            this.velocity += (J / mMass);

            // the impulse torque
            Vector3 torqueImpuls = Vector3.Cross(r, J);

            // apply the torque to the angular velocity
            this.angularMomentum += torqueImpuls;

            // initiate rotation by changing angular momentum based on velocity of particle in contact with arena
            Vector3 omega = Vector3.Transform(this.angularMomentum, this.invBodyInertia);
            Vector3 collisionPointVelocity = Vector3.Cross(omega, r);

            // velocity difference of negative particle velocity and center of mass velocity
            Vector3 velocityDifference = collisionPointVelocity + this.velocity;

            // adjust angular momentum such that the velocity of the colliding particle equals the velocity of the center of mass
            this.angularMomentum = -Vector3.Cross(r, collisionPointVelocity - velocityDifference);
        }
        #endregion


        public override void Update(GameTime gameTime)
        {
            Thread.MemoryBarrier();
            obb = null;

#if DRAWBALL_BV_AFTERISPOTTED && DRAWBALL_BV
            drawboundingvolume = this.pocketWhereAt >= 0;
#endif
            if (!IsMoving() || table == null) { base.Update(gameTime); return; }
            lock (table.syncballsready)
                ++table.ballsready;

            switch (currentTrajectory)
            {
                #region Motion
                case Trajectory.Motion:
                    HandleMotion((float)gameTime.ElapsedGameTime.TotalSeconds);
                    break;
                #endregion
                #region Free
                case Trajectory.Free:
                    HandleFree((float)gameTime.ElapsedGameTime.TotalSeconds);

                    #region Old
                    //t = dt * World.timeFactor;

                    //if (IsMoving())
                    //{
                    //    //Vector3 tmp = velocity - new Vector3(0, velocity.Y, 0);
                    //    Vector3 tmp = velocity;
                    //    Vector3 axis = Vector3.Normalize(Vector3.Cross(Vector3.Up, velocity));
                    //    float angle = tmp.Length() * World.ballRadius * 0.0229f * t;
                    //    //float angle = velocity.Length() * dt * MathHelper.TwoPi / World.ballRadious;

                    //    //Quaternion rotationThisFrame = Quaternion.CreateFromAxisAngle(axis, angle);
                    //    //localRotation *= rotationThisFrame;

                    //    //localRotation.Normalize();

                    //    //this.Rotation = Matrix.CreateFromQuaternion(localRotation);
                    //}

                    //PositionX += velocity.X * t;
                    //PositionZ += velocity.Z * t;

                    //if (PositionY > min_Altitute)
                    //    velocity.Y += World.gravity * t;

                    //if (PositionY < min_Altitute - 0.05f)
                    //{
                    //    PositionY = min_Altitute;
                    //    velocity.Y *= -0.08f;
                    //    velocity.X *= 0.7f;
                    //    velocity.Z *= 0.7f;
                    //}
                    //else
                    //{
                    //    PositionY += velocity.Y * t + 0.5f * World.gravity * t * t;
                    //}
                    ///*if (Math.Abs(velocity.X) < MIN_SPEED)
                    //{
                    //    velocity.X = 0;
                    //}*/
                    ///*if (Math.Abs(velocity.Z) < MIN_SPEED)
                    //{
                    //    velocity.Z = 0;
                    //}*/

                    ////Console.WriteLine(velocity.Y);
                    //if (Math.Abs(velocity.X) < MIN_SPEED && Math.Abs(velocity.Z) < MIN_SPEED && Math.Abs(velocity.Y) < MIN_SPEED_Y
                    //    && pocketWhereAt != -1 && PositionY >= min_Altitute && PositionY <= min_Altitute + 2.0f)
                    //{
                    //    Stop();
                    //}
                    #endregion

                    break;
                #endregion
            }

            if (!this.collisionFlag && !IsMoving()) Stop();

            lock (table.syncballsready)
                --table.ballsready;
            base.Update(gameTime);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="elapsedSeconds"></param>
        /// <param name="timeAfterCollision"></param>
        /// <returns>0 no colliding, 1 true and 2 colliding in this frame</returns>
        private int AreSpheresColliding(Ball ball1, Ball ball2, float elapsedSeconds, out float timeAfterCollision, Vector3 currentPositionB1, Vector3 currentPositionB2, out Vector3 s1pos, out Vector3 s2pos)
        {
            timeAfterCollision = elapsedSeconds;
            s1pos = currentPositionB1;
            s2pos = currentPositionB2;

            Vector3 relativeVelocity = ball1.velocity - ball2.velocity;
            Vector3 relativePosition = ball1.Position - ball2.Position;

            // If the relative movement of two spheres show that they are moving away, no collision.
            float relativeMovement = Vector3.Dot(relativePosition, relativeVelocity);
            if (relativeMovement >= 0.0f)
            {
                return 0;
            }

            // Checks if two spheres are already colliding.
            if (relativePosition.LengthSquared() - (radius * radius) <= 0.0f)
            {
                return 1;
            }


            //is this still required?
            float relativeDistance = relativePosition.Length() - radius;
            if (relativeDistance <= radius)
            {
                return 1;
            }

            // does collision happen this frame
            // how much time remains after collision?
            if (relativeDistance < relativeVelocity.Length() * elapsedSeconds)
            {
                float timeFraction = relativeDistance / relativeVelocity.Length();

                Vector3 oldPosition1 = ball1.Position;
                s1pos = ball1.Position + ball1.velocity * timeFraction;

                float distanceMoved1 = (s1pos - oldPosition1).Length();
                ball1.angleRotation += (-distanceMoved1 / ball1.radius);
                ball1.Rotation = Matrix.CreateFromAxisAngle(rightVector, angleRotation);


                Vector3 oldPosition2 = ball2.Position;

                s2pos = ball2.Position + ball2.velocity * timeFraction;

                float distanceMoved2 = (s2pos - oldPosition1).Length();
                ball2.angleRotation += (-distanceMoved2 / ball2.radius);
                ball2.Rotation = Matrix.CreateFromAxisAngle(rightVector, angleRotation);

                if (ball1.pocketWhereAt != -1) s1pos.Y = Math.Max(s1pos.Y, min_Altitute);
                if (ball2.pocketWhereAt != -1) s2pos.Y = Math.Max(s2pos.Y, min_Altitute);
                //ball1.Position = s1pos;
                //ball2.Position = s2pos;
                timeAfterCollision = elapsedSeconds * (1.0f - timeFraction);

                relativePosition = s1pos - s2pos;

                if ((relativePosition.LengthSquared() - (radius * radius)) <= 0.0f)
                {
                    return 2;
                }
            }

            return 0;
        }
        private int CheckBallWithBallCollision(Ball thisball, Ball ball2, float elapsedSeconds, out float remainingTime)
        {
            Vector3 s1p = thisball.Position, s2p = ball2.Position;

            //while (thisball.collisionFlag || ball2.collisionFlag) 
            {
                //Monitor.Wait(s1.syncObject);
            }

            int collisionResult = AreSpheresColliding(thisball, ball2, elapsedSeconds, out remainingTime, thisball.Position, ball2.Position, out s1p, out s2p);
            if (collisionResult != 0)
            {
                DoCollision(thisball, ball2);
            }
            else
            {
                //ball1.collisionFlag = false;
                //ball2.collisionFlag = false;
                remainingTime = elapsedSeconds;
            }
            return collisionResult;
        }

        private void DoCollision(Ball thisball, Ball ball2)
        {
            //while (thisball.collisionFlag || ball2.collisionFlag) 
            {
                //Monitor.Wait(s1.syncObject);
            }
            //we have collision, compute new velocities;



            //lock (thisball.syncObject)
            /*{
                lock (ball2.syncObject)
                {
                    ball2.collisionFlag = true;
                    thisball.collisionFlag = true;
                    World.ballcollider.AddToQueue(thisball, ball2);
                    World.ballcollider.BeginThread();
                    //ball2.mutex.WaitOne();
                    mutex.WaitOne();
                }
            }*/
            //thisball.StopThread();
            //ball2.StopThread();
            //thisball.Position = s1p; ball2.Position = s2p;

            object tmp = ball2.syncObject;
            ball2.syncObject = thisball.syncObject;
            // Compute the final velocity of the two spheres.
            lock (thisball.syncObject)
            {
                Vector3 tmpvelocity;
                thisball.collisionFlag = true;
                ball2.collisionFlag = true;
                //if (collisionResult == 2) s1.Position = s1p;
                //if (collisionResult == 2) s2.Position = s2p;
                Vector3 relativePosition = thisball.Position - ball2.Position;
                Vector3 relativeUnit = Vector3.Normalize(relativePosition);

                float s1VelDotUnit = Vector3.Dot(thisball.velocity, relativeUnit);
                float s2VelDotUnit = Vector3.Dot(ball2.velocity, relativeUnit);

                float momentumDifference = (2.0f * (s1VelDotUnit - s2VelDotUnit)) / (2.0f);

                tmpvelocity = thisball.velocity - momentumDifference * relativeUnit;
                if (Math.Abs(tmpvelocity.Y) < MIN_SPEED_Y)
                {
                    tmpvelocity.Y = 0.0f;
                    ball2.acceleration.Y = 0.0f;
                }
                thisball.SetVelocity(tmpvelocity);

                //Console.WriteLine("this = " + thisball.velocity);
                thisball.previousHitRail = -1; thisball.previousInsideHitRail = -1;
                thisball.initialvelocity = thisball.velocity;

                thisball.PreRotation *= thisball.Rotation;
                thisball.Rotation = Matrix.Identity;
                thisball.angleRotation = 0.0f;
                thisball.totaltime = 0.0f;

                //s1.InitialRotation = s1.Rotation;
                //s1.Rotation = Matrix.Identity;

                //Monitor.PulseAll(s1.syncObject);
                //Monitor.PulseAll(s1.syncObject);

                //lock (s2.syncObject)
                {
                    tmpvelocity = ball2.velocity + momentumDifference * relativeUnit;
                    if (Math.Abs(tmpvelocity.Y) < MIN_SPEED_Y)
                    {
                        tmpvelocity.Y = 0.0f;
                        ball2.acceleration.Y = 0.0f;
                    }
                    ball2.SetVelocity(tmpvelocity);

                    //Console.WriteLine("ball2 = " + ball2.velocity);

                    ball2.previousHitRail = -1; ball2.previousInsideHitRail = -1;
                    ball2.initialvelocity = ball2.velocity;
                    ball2.totaltime = 0.0f;

                    ball2.PreRotation *= ball2.Rotation;
                    ball2.Rotation = Matrix.Identity;
                    ball2.angleRotation = 0.0f;

                    //s2.InitialRotation = s2.Rotation;
                    //s2.Rotation = Matrix.Identity;
                    //Monitor.PulseAll(s2.syncObject);
                }

                //Monitor.PulseAll(s1.syncObject);
            }


            ball2.syncObject = tmp;

            thisball.collisionFlag = false;
            ball2.collisionFlag = false;
        }

        public bool CheckInsidePocketCollision()
        {
            if (World.playerInTurnIndex == -1) return false;

            for (int i = 0; i < table.pockets.Length; i++)
            {
                for (int j = 0; j < 2; ++j)
                {
                    if (table.pockets[i].insideBands[j] == null) continue;


                    float angle = (float)Maths.AngleBetweenVectors(table.pockets[i].insideNormal[j], this.direction);

                    if (obb != null && obb.Intersects(table.pockets[i].insideBands[j]) && this.previousHitRail != table.pockets.Length + (i * 2 + j)
                        && angle > MathHelper.PiOver2 && angle <= MathHelper.Pi)
                    {
                        lock (this.syncObject)
                        {
                            this.PreRotation *= this.Rotation;
                            this.Rotation = Matrix.Identity;

                            this.angleRotation = 0.0f;
                            //this.previousInsideHitRail = i * 2 + j;
                            this.previousHitRail = table.pockets.Length + (i * 2 + j);
                            this.angleRotation = 0.0f;

                            Vector3 normal = table.pockets[i].insideNormal[j];



                            this.SetVelocity(Vector3.Reflect(this.velocity, normal) * 0.9f);
                            this.initialvelocity = this.velocity;
                            totaltime = 0.0f;

                            table.roundInfo.ballsRailsHit[this] = true;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private int CheckBallWithRailCollision(float remainingTime)
        {
            float vLength2 = this.velocity.Length() * remainingTime;
            //Ray ray = new Ray(this.Position + this.direction * this.radius, this.direction);
            Ray ray = new Ray(this.Position, this.direction);

            float? intersectPos;
            for (int i = 0; i < table.rails.Length; i++)
            {
                intersectPos = ray.Intersects(table.planes[i]);
                //intersectPos = ray.Intersects(table.rails[i]);
                if (intersectPos != null)
                {
                    float intersectValue = (float)intersectPos;

                    if ((intersectValue >= 0.0f) && (intersectValue < vLength2))
                    {
                        float angle = (float)Maths.AngleBetweenVectors(table.planes[i].Normal, this.direction);
                        Vector3 tmp = this.Position + intersectValue * this.direction;
                        if (((tmp.X >= table.rails[i].Min.X - 5.0f && tmp.X <= table.rails[i].Max.X + 5.0f)
                           || (tmp.Z >= table.rails[i].Min.Z - 5.0f && tmp.Z <= table.rails[i].Max.Z + 5.0f)) && 
                            angle > MathHelper.PiOver2 && angle <= MathHelper.Pi && this.previousHitRail != i)

                        
                        //BoundingSphere bs = new BoundingSphere(tmp, Radius);
                        //float plane_distance1 = table.planes[i].D + Vector3.Dot(table.planes[i].Normal, this.Position + direction * this.radius);
                        //float plane_distance2 = table.planes[i].D + Vector3.Dot(table.planes[i].Normal, this.Position - direction * this.radius);
                        //if (bs.Intersects(table.rails[i]) && plane_distance2 > 0.00001f && plane_distance1 <= 0.00001f)
                        {
                            lock (this.syncObject)
                            {
                                this.PreRotation *= this.Rotation;
                                this.Rotation = Matrix.Identity;
                                this.previousHitRail = i;
                                this.previousInsideHitRail = -1;
                                this.angleRotation = 0.0f;


                                SetVelocity(Vector3.Reflect(this.velocity, table.planes[i].Normal));
                                this.initialvelocity = this.velocity;
                                totaltime = 0.0f;

                                table.roundInfo.ballsRailsHit[this] = true;
                            }
                            return i;
                        }
                    }
                }
            }

            //for (int i = 0; i < table.rails.Length; i++)
            //{
            //    if (!this.isInPlay) continue;
            //    float plane_distance1 = table.planes[i].D + Vector3.Dot(table.planes[i].Normal, this.Position + direction * this.radius);
            //    float plane_distance2 = table.planes[i].D + Vector3.Dot(table.planes[i].Normal, this.Position /*- direction * this.radius*/);
            //    if (bs.Intersects(table.rails[i]) /*&& plane_distance2 > 0.00001f && plane_distance1 <= 0.00001f*/ && i != this.previousHitRail)
            //    {
            //        this.InitialRotation *= this.Rotation;
            //        this.Rotation = Matrix.Identity;
            //        this.previousHitRail = i;

            //        //this.Position = Vector3.Clamp(this.Position, new Vector3(table.MIN_X + World.ballRadius, this.Position.Y, table.MIN_Z + World.ballRadius)
            //        //    , new Vector3(table.MAX_X - World.ballRadius, this.Position.Y, table.MAX_Z - World.ballRadius));

            //        this.velocity = Vector3.Reflect(this.velocity, table.railsNormals[i]);
            //        this.initialvelocity = this.velocity;
            //        //this.acceleration = Vector3.Reflect(this.acceleration, table.railsNormals[i]);
            //        this.direction = Vector3.Reflect(this.direction, table.railsNormals[i]);
            //        return true;
            //    }
            //}
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool CheckPocketsBoundaries(float remainingTime)
        {
            bool coll = false;
            Vector3 point = this.direction;
            point *= this.Radius;

            float x2 = (table.pockets[this.pocketWhereAt].bounds.Center.X - this.Position.X - point.X) * (table.pockets[this.pocketWhereAt].bounds.Center.X - this.Position.X - point.X);
            float y2 = (table.pockets[this.pocketWhereAt].bounds.Center.Z - this.Position.Z - point.Z) * (table.pockets[this.pocketWhereAt].bounds.Center.Z - this.Position.Z - point.Z);
            float r2 = table.pockets[this.pocketWhereAt].bounds.Radius * table.pockets[this.pocketWhereAt].bounds.Radius;

            if (x2 + y2 - r2 >= -1.5f)
            {
                lock (this.syncObject)
                {
                    Vector3 normal = new Vector3(-(this.Position.X + point.X) + table.pockets[this.pocketWhereAt].bounds.Center.X, 0.0f, -(this.Position.Z + point.Z) + table.pockets[this.pocketWhereAt].bounds.Center.Z);

                    //normal.X += (float)Maths.random.NextDouble() * 50.0f;
                    //normal.Z += (float)Maths.random.NextDouble() * 50.0f;
                    if (normal.LengthSquared() > 0.0f)
                    {
                        normal.Normalize();


                        this.PreRotation *= this.Rotation;
                        this.Rotation = Matrix.Identity;
                        this.angleRotation = 0.0f;

                        this.SetVelocity(Vector3.Reflect(this.velocity, normal) * 0.45f);

                        //if (PositionY > this.min_Altitute)
                        //{
                        //    this.acceleration.Y += World.gravity;
                        //    this.velocity.Y += acceleration.Y * remainingTime;
                        //}


                        //Vector3 movementDelta = velocity * remainingTime + 0.5f * acceleration * remainingTime * remainingTime;
                        //this.Position += movementDelta;
                        coll = true;
                    }
                }
            }

            return coll;
        }

        public bool CheckBallIsPotted()
        {

            for (int i = 0; i < table.pockets.Length; i++)
            {
                float x2 = (table.pockets[i].bounds.Center.X - this.PositionX) * (table.pockets[i].bounds.Center.X - this.PositionX);
                float y2 = (table.pockets[i].bounds.Center.Z - this.PositionZ) * (table.pockets[i].bounds.Center.Z - this.PositionZ);
                float r = table.pockets[i].bounds.Radius + 0.6f /*1.105f*/;
                float r2 = r * r;

                if (x2 + y2 - r2 <= -60.0f)
                {
                    this.min_Altitute = 138.44f;
                    this.pocketWhereAt = i;
                    this.acceleration.Y = 0.0f;
                    this.velocity.Y = 0.0f;

                    lock (table.pockets[i].balls)
                    {
                        if (table.pockets[i].balls.Count >= table.maximumBallsInPocket)
                        {
                            table.pockets[i].balls[0].Position = table.ballstuckposition;
                            for (int k = 1; k < table.pockets[i].balls.Count; ++k)
                            {
                                table.pockets[i].balls[k].previousvelocity = Vector3.One * 550.0f; // any number
                                table.pockets[i].balls[k].prepreviousvelocity = Vector3.Up * 750.0f; // any number
                            }

                            table.pockets[i].firstballsstuck.Add(table.pockets[i].balls[0]);
                            table.pockets[i].balls.RemoveAt(0);
                        }

                        table.pockets[i].balls.Add(this);
                    }
                    this.PreRotation *= Rotation;
                    this.Rotation = Matrix.Identity;
                    this.angleRotation = 0.0f;
                    this.currentTrajectory = Trajectory.Free;
                    return true;
                }
            }
            return false;
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
            this.PreRotation *= this.Rotation;
            this.Rotation = Matrix.Identity;
            this.angleRotation = 0.0f;

            SetVelocity(velocity + impulse);
            this.initialvelocity = this.velocity;

            totaltime = 0.0f;
            //collision.Set();
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

        public void SetVelocity(Vector3 newVelocity)
        {
            velocity = newVelocity;

            Vector3 frontVector = velocity;
            Vector3 upvector = Vector3.Up;
            rightVector = Vector3.Cross(frontVector, upvector);
            if (rightVector.LengthSquared() > 0.0f)
            {
                rightVector.Normalize();

                direction = velocity;
                direction.Normalize();
            }

        }

        #region Stop the ball
        /// <summary>
        /// Stop the ball.
        /// </summary>
        public void Stop()
        {
            //StopThread();

            lock (this.syncObject)
            {

                acceleration = Vector3.Zero;
                initialvelocity = Vector3.Zero;
                direction = Vector3.Zero;
                velocity = Vector3.Zero;
                previousHitRail = -1; previousInsideHitRail = -1;
                //currentTrajectory = Trajectory.Motion;
                totaltime = 0.0f;


                angularMomentum = Vector3.Zero;
                invWorldInertia = Matrix.Identity;
                rotQ = Quaternion.Identity;
            }
            //ResumeThread();
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public override void Run()
        {
#if XBOX
            Thread.CurrentThread.SetProcessorAffinity(cpu);
#endif

            while (true)
            {
                mutex.WaitOne();
                if (!running) stopped.WaitOne();
                this.Update(gameTime);
            }
        }

        #region Dispose
        protected override void Dispose(bool disposing)
        {

            base.Dispose(disposing);
        }
        #endregion



        public void LanzarBola()
        {
            for (int i = 0; i < table.TotalBalls; ++i)
            {
                table.poolBalls[i].Stop();
            }
            this.Position = new Vector3(0, 500.0f, 0);
        }
    }
}

