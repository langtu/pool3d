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

namespace XNA_PoolGame
{
    /// <summary>
    /// The Ball
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
        public Vector3 acceleration = Vector3.Zero;
        public Vector2 angularVelocity = Vector2.Zero;
        public Vector3 rightVector = Vector3.Zero;
        public float angleRotation = 0.0f;
        public OrientedBoundingBox obb;

        public Matrix invWorldInertia = Matrix.Identity;
        public Quaternion rotQ = Quaternion.Identity;
        public Vector3 angularMomentum = Vector3.Zero;
        public Matrix invBodyInertia = Matrix.Identity;

        private const int numSteps = 5;//5;

        public float mMass = 1.0f;

        public float totaltime = 0.0f;
        public volatile int pocketWhereAt = -1;
        public int previousHitRail = -1, previousInsideHitRail = -1;

        public const float MIN_SPEED = 2.73333f;//0.02999f;//0.10f;
        public const float MIN_SPEED_Y = 9.73333f;//0.02999f;//0.10f;
        private float MIN_SPEED_SQUARED;

        public PoolTable table = null;
        public Trajectory currentTrajectory = Trajectory.Motion;
        public float min_Altitute = World.ballRadius + World.poolTable.SURFACE_POSITION_Y;

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
            get { return radius * 2; }
        }

        public bool IsMoving()
        {
            while (this.thinkingFlag) { };

            //if (velocity.X == 0.0f && velocity.Y == 0.0f && velocity.Z == 0.0f)
            if (velocity.LengthSquared() < MIN_SPEED_SQUARED)
            {
                //StopBall();
                return false;
            }
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

        public Ball(Game game, int ballNumber, String ballModel, PoolTable table, float radius)
            : base(game, ballModel)
        {
            this.table = table;
            this.ballNumber = ballNumber;
            this.radius = radius;
            previousHitRail = -1; previousInsideHitRail = -1;
            pocketWhereAt = -1;
            volume = VolumeType.BoundingSpheres;
            rightVector = Vector3.Zero;
        }

        public Ball(Game game, int ballNumber, String ballModel, String ballTexture, PoolTable table, float radius)
            : this(game, ballNumber, ballModel, table, radius)
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

        /// <summary>
        /// Set Ball's Oriented Bounding Box for check inside pockect-ball collision.
        /// This must be called everytime the direction vector changes.
        /// </summary>
        private void SetOBB()
        {
            float directionAngle;
            if (direction.X == 0.0f) directionAngle = (float)Math.Atan(direction.Z);
            else directionAngle = (float)Math.Atan(direction.Z / direction.X);

            Matrix rotation = Matrix.CreateRotationY(-directionAngle);


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
                if (this.collisionFlag) { mutex.WaitOne(); }

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

                if (velocity.LengthSquared() < MIN_SPEED_SQUARED)
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
                    int collisionResult = 0;
                    for (int i = 0; i < table.TotalBalls; ++i)
                    {
                        if (table.poolBalls[i] != this && table.poolBalls[i].pocketWhereAt == -1)
                        {
                            if (this.collisionFlag) { mutex.WaitOne(); }
                            //lock (this)
                            //lock (this.syncObject)
                            {
                                collisionResult = CheckBallWithBallCollision(this, table.poolBalls[i], remainingTime, out remainingTime);
                            }
                        }
                    }
                }

                //angularVelocity
                int rail;
                if ((rail = CheckBallWithRailCollision(remainingTime)) != -1)
                {
                    if (this == table.cueBall)
                    {
                        // cue ball hit a side
                        table.roundInfo.cueBallHitRail(rail);
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

                float angleOnThisFrame = (-movementDelta.Length() / this.radius);

                //if (this == table.cueBall && angleOnThisFrame >= -0.018f)
                //    angleOnThisFrame = -0.018f;

                angleRotation += angleOnThisFrame;
                this.Rotation = Matrix.CreateFromAxisAngle(rightVector, angleRotation);

                if (this.pocketWhereAt == -1 && CheckBallIsPotted() && this == table.cueBall)
                {
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
                this.thinkingFlag = true;
                float dt = _dt / (float)numSteps;
                acceleration -= velocity;

                float x2 = (table.pockets[this.pocketWhereAt].bounds.Center.X - this.PositionX) * (table.pockets[this.pocketWhereAt].bounds.Center.X - this.PositionX);
                float y2 = (table.pockets[this.pocketWhereAt].bounds.Center.Z - this.PositionZ) * (table.pockets[this.pocketWhereAt].bounds.Center.Z - this.PositionZ);
                float r = table.pockets[this.pocketWhereAt].bounds.Radius + 0.6f;
                float thisr2 = this.radius * this.radius;
                float r2 = r * r;

                float t = x2 + y2 - r2;
                float remainingTime = dt;
                //Console.WriteLine(t);
                if (t <= -60.0f && this.PositionY > min_Altitute)
                {
                    if (t > -thisr2 /*&& t <= -60.0f*/ && this.PositionY <= table.SURFACE_POSITION_Y + this.radius &&
                        this.PositionY >= table.SURFACE_POSITION_Y - 5.0f)
                    {
                        acceleration.Y += World.gravity * 0.75f;
                        //acceleration.Y += World.gravity;

                        Vector3 normal = table.pockets[this.pocketWhereAt].bounds.Center - this.Position;
                        normal.Normalize();
                        //Vector3 speedVector = acceleration;
                        //speedVector.Y = 0.0f;
                        //float speed = 1.0f / (speedVector.Length());

                        acceleration.X += normal.X * 950.0f;
                        acceleration.Z += normal.Z * 950.0f;

                        for (int i = 0; i < table.TotalBalls; ++i)
                        {
                            if (table.poolBalls[i] != this)
                            {
                                Vector3 s1p = this.Position, s2p = table.poolBalls[i].Position;
                                int collisionResult = AreSpheresColliding(this, table.poolBalls[i], dt, out remainingTime, this.Position, table.poolBalls[i].Position, out s1p, out s2p);
                                if (collisionResult != 0)
                                {
                                    acceleration.Y -= World.gravity * 0.75f;
                                }
                            }
                        }
                    }
                    else
                    {
                        acceleration.Y += World.gravity;
                    }
                }

                Vector3 velocityOnThisFrame = acceleration * dt;
                velocity += velocityOnThisFrame;

                direction = velocity;
                direction.Normalize();

                //SetOBB();

                if (velocity.LengthSquared() < MIN_SPEED_SQUARED)
                {
                    velocity = Vector3.Zero;
                    initialvelocity = Vector3.Zero;
                    acceleration = Vector3.Zero;
                    previousHitRail = -1; previousInsideHitRail = -1;
                    direction = Vector3.Zero;
                    totaltime = 0.0f;
                }



                if (table.cueBall != null)
                {
                    int collisionResult = 0;
                    for (int i = 0; i < table.TotalBalls; ++i)
                    {
                        if (table.poolBalls[i] != this)
                        {
                            //lock (this)
                            collisionResult = CheckBallWithBallCollision(this, table.poolBalls[i], remainingTime, out remainingTime);
                        }
                    }
                }

                Vector3 movementDelta = velocity * remainingTime + 0.5f * acceleration * remainingTime * remainingTime;
                Vector3 newPosition = this.Position + movementDelta;

                if (this.pocketWhereAt != -1 && newPosition.Y < this.min_Altitute)
                {
                    newPosition.Y = this.min_Altitute;
                    velocity.Y = -velocity.Y * 0.65f;

                    if (Math.Abs(velocity.Y) < MIN_SPEED_Y) velocity.Y = 0.0f;

                    velocity.X = velocity.X * 0.97f;
                    velocity.Z = velocity.Z * 0.97f;
                }

                this.Position = newPosition;


                if (this.pocketWhereAt != -1 && CheckPocketsBoundaries(remainingTime))
                {

                }


                Vector3 movement = movementDelta;
                movement.Y = 0.0f;

                Vector3 frontVector = velocity;
                Vector3 upvector = Vector3.Up;
                Vector3 rightVector = Vector3.Cross(frontVector, upvector);
                if (rightVector.Length() > 0.0f)
                    rightVector.Normalize();

                float angleOnThisFrame = (-movement.Length() / this.radius);
                angleRotation += angleOnThisFrame;

                this.Rotation = Matrix.CreateFromAxisAngle(rightVector, angleRotation);
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
            float e = 0.1f;
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
                ball1.Position = s1pos;
                ball2.Position = s2pos;
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

            int collisionResult = AreSpheresColliding(thisball, ball2, elapsedSeconds, out remainingTime, thisball.Position, ball2.Position, out s1p, out s2p);
            if (collisionResult != 0)
            {
                //while (ball2.collisionFlag || thisball.collisionFlag) { }

                object tmp = ball2.syncObject;
                ball2.syncObject = this.syncObject;
                //lock (thisball)
                {
                    lock (this.syncObject)
                    //lock (ball2)
                    {
                        ball2.collisionFlag = true;
                        thisball.collisionFlag = true;
                        World.ballcollider.AddToQueue(thisball, ball2, BallCollisionType.TwoBalls);
                        World.ballcollider.BeginThread();

                        mutex.WaitOne();
                    }
                }
                ball2.syncObject = tmp;
            }
            else
            {
                //ball1.collisionFlag = false;
                //ball2.collisionFlag = false;
                remainingTime = elapsedSeconds;
            }
            return collisionResult;
        }

        public bool CheckInsidePocketCollision()
        {
            if (World.playerInTurn == -1) return false;

            for (int i = 0; i < table.pockets.Length; i++)
            {
                for (int j = 0; j < 2; ++j)
                {
                    if (table.pockets[i].insideBands[j] == null) continue;


                    float angle = (float)Maths.AngleBetweenVectors(table.pockets[i].insideNormal[j], this.direction);

                    if (obb != null && obb.Intersects(table.pockets[i].insideBands[j]) && this.previousInsideHitRail != (i * 2 + j)
                        && angle > MathHelper.PiOver2 && angle <= MathHelper.Pi)
                    {

                        lock (this.syncObject)
                        {
                            this.collisionFlag = true;
                            World.ballcollider.AddToQueue(this, i, j, BallCollisionType.BallWithInsideRailPocket);
                            World.ballcollider.BeginThread();
                            mutex.WaitOne();
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
            Ray ray = new Ray(this.Position, this.direction);

            float? intersectPos;
            for (int i = 0; i < table.rails.Length; i++)
            {
                intersectPos = ray.Intersects(table.planes[i]);
                if (intersectPos != null)
                {
                    float intersectValue = (float)intersectPos;

                    if ((intersectValue > 0.0f) && (intersectValue < vLength2))
                    {
                        float angle = (float)Maths.AngleBetweenVectors(table.planes[i].Normal, this.direction);
                        Vector3 tmp = this.Position + intersectValue * this.direction;

                        if (((tmp.X >= table.rails[i].Min.X - 5.0f && tmp.X <= table.rails[i].Max.X + 5.0f)
                           || (tmp.Z >= table.rails[i].Min.Z - 5.0f && tmp.Z <= table.rails[i].Max.Z + 5.0f))
                            && angle > MathHelper.PiOver2 && angle <= MathHelper.Pi && this.previousHitRail != i)
                        {

                            lock (this.syncObject)
                            //lock (this)
                            {
                                this.collisionFlag = true;
                                World.ballcollider.AddToQueue(this, i, BallCollisionType.BallWithRail);
                                World.ballcollider.BeginThread();
                                mutex.WaitOne();
                            }
                            return i;
                        }
                    }
                }
            }

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
                    //normal.X += (float)PoolGame.random.NextDouble() * 50.0f;
                    //normal.Z += (float)PoolGame.random.NextDouble() * 50.0f;
                    normal.Normalize();

                    this.PreRotation *= this.Rotation;
                    this.Rotation = Matrix.Identity;
                    this.angleRotation = 0.0f;

                    this.SetVelocity(Vector3.Reflect(this.velocity, normal) * 0.45f);

                    if (PositionY > this.min_Altitute)
                    {
                        //this.acceleration.Y += World.gravity;
                        //this.velocity.Y += acceleration.Y * remainingTime;
                    }

                    Vector3 movementDelta = velocity * remainingTime + 0.5f * acceleration * remainingTime * remainingTime;

                    this.Position += movementDelta;

                    coll = true;
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
                    table.pockets[i].balls.Add(this);
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
            initialvelocity = impulse;

            SetVelocity(velocity + impulse);


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
            if (rightVector.Length() > 0.0f) rightVector.Normalize();

            direction = velocity;
            direction.Normalize();
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
                currentTrajectory = Trajectory.Motion;
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
