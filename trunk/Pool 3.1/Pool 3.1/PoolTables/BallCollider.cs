using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNA_PoolGame.Threading;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Helpers;
using System.Threading;

namespace XNA_PoolGame.PoolTables
{
    class State
    {
        private readonly Ball ball1;
        private readonly Object ball2;
        private readonly BallCollisionType collisiontype;
        public State(Ball ball1, Object ball2, BallCollisionType collisiontype)
        {
            this.ball1 = ball1;
            this.ball2 = ball2;
            this.collisiontype = collisiontype;
        }
        public Ball Ball1
        {
            get { return ball1; }
        }
        public Object Ball2
        {
            get { return ball2; }
        }
        public BallCollisionType CollisionType
        {
            get { return collisiontype; }
        }
    }
    /// <summary>
    /// Entity in charge of collisions synchronously.
    /// </summary>
    public class BallCollider : ThreadComponent
    {
        private Queue<State> queue = null;
        public BallCollider(Game _game)
            : base(_game)
        {
            queue = new Queue<State>();
            UseThread = true;
        }
        /// <summary>
        /// Add a collision to the queue.
        /// </summary>
        /// <param name="ball1">Ball 1</param>
        /// <param name="ball2">Can be: Ball, integer, etc.</param>
        /// <param name="collision">Collision type.</param>
        public void AddToQueue(Ball ball1, Object ball2, BallCollisionType collision)
        {
            lock (queue)
            {
                //Console.WriteLine(""+ ball1.ballNumber + ", "+ ball2.ballNumber);
                queue.Enqueue(new State(ball1, ball2, collision));
            }
        }

        public override void Update(GameTime gameTime)
        {
            while (queue.Count > 0)
            {
                State state;
                lock (queue)
                {
                    state = queue.Dequeue();
                }

                switch (state.CollisionType)
                {
                    case BallCollisionType.TwoBalls:
                        {
                            Ball ball2 = (Ball)state.Ball2;
                            Vector3 relativePosition = state.Ball1.Position - ball2.Position;
                            Vector3 relativeUnit = Vector3.Normalize(relativePosition);

                            float s1VelDotUnit = Vector3.Dot(state.Ball1.velocity, relativeUnit);
                            float s2VelDotUnit = Vector3.Dot(ball2.velocity, relativeUnit);

                            float momentumDifference = (2.0f * (s1VelDotUnit - s2VelDotUnit)) / (2.0f);
                            state.Ball1.SetVelocity(state.Ball1.velocity - momentumDifference * relativeUnit);
                            state.Ball1.previousHitRail = -1; state.Ball1.previousInsideHitRail = -1;
                            state.Ball1.initialvelocity = state.Ball1.velocity;

                            state.Ball1.PreRotation *= state.Ball1.Rotation;
                            state.Ball1.Rotation = Matrix.Identity;
                            state.Ball1.angleRotation = 0.0f;
                            state.Ball1.totaltime = 0.0f;

                            ball2.SetVelocity(ball2.velocity + momentumDifference * relativeUnit);
                            ball2.previousHitRail = -1; ball2.previousInsideHitRail = -1;
                            ball2.initialvelocity = ball2.velocity;

                            ball2.PreRotation *= ball2.Rotation;
                            ball2.Rotation = Matrix.Identity;
                            ball2.angleRotation = 0.0f;
                            ball2.totaltime = 0.0f;

                            state.Ball1.collisionFlag = false;
                            ball2.collisionFlag = false;
                            state.Ball1.BeginThread();
                        }
                        break;
                    case BallCollisionType.BallWithInsideRailPocket:
                        {

                        }
                        break;
                    case BallCollisionType.BallWithRail:
                        {
                            int i = (int)state.Ball2;
                            state.Ball1.PreRotation *= state.Ball1.Rotation;
                            state.Ball1.Rotation = Matrix.Identity;
                            state.Ball1.previousHitRail = i;
                            state.Ball1.previousInsideHitRail = -1;
                            state.Ball1.angleRotation = 0.0f;


                            state.Ball1.SetVelocity(Vector3.Reflect(state.Ball1.velocity, state.Ball1.table.planes[i].Normal));
                            state.Ball1.initialvelocity = state.Ball1.velocity;
                            state.Ball1.totaltime = 0.0f;

                            state.Ball1.collisionFlag = false;
                            state.Ball1.BeginThread();
                        }
                        break;
                }
                //tuple.Second.BeginThread();
            }
            base.Update(gameTime);
        }
    }
}
