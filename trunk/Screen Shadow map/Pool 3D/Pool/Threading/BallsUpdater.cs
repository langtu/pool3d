/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Extreme_Pool.Threading
{
    class BallsUpdater : UpdateManager
    {

        List<Plane> collisionPlanes;
        Random rand = new Random();
        // how quickly the sphere can turn from side to side
        const float CameraTurnSpeed = .025f;
        float cameraFacingDirection;
        readonly Vector3 CameraPositionOffset = new Vector3(0, 10, 15);
        readonly Vector3 CameraTargetOffset = new Vector3(0, 5, 0);


        public BallsUpdater(DoubleBuffer db, Game game)
            : base(db, game)
        {

            collisionPlanes = new List<Plane>();
            collisionPlanes.Add(new Plane(Vector3.UnitX, 49));
            collisionPlanes.Add(new Plane(-Vector3.UnitX, 49));
            collisionPlanes.Add(new Plane(Vector3.UnitZ, 49));
            collisionPlanes.Add(new Plane(-Vector3.UnitZ, 49));
        }

        public override void Update(GameTime gameTime)
        {
            messageBuffer.Clear();
            HandleInput();


            for (int i = 0; i < GameDataOjects.Count; i++)
            {
                GameData gd = GameDataOjects[i];
                if (UpdatePhysics(gd, (float)gameTime.ElapsedGameTime.TotalSeconds))
                {
                    Matrix newWorldMatrix = gd.rotation * Matrix.CreateTranslation(gd.position);
                    ChangeMessage msg = new ChangeMessage();
                    msg.ID = i;
                    msg.MessageType = ChangeMessageType.UpdateWorldMatrix;
                    msg.WorldMatrix = newWorldMatrix;
                    messageBuffer.Add(msg);
                }
            }
            UpdateCamera();
            
            Console.WriteLine("virtual void de balls updater...");
            //base.Update(gameTime);
        }



        private bool UpdatePhysics(GameData physicsData, float elapsedSeconds)
        {

            //friction deceleration
            physicsData.acceleration -= physicsData.velocity * 0.5f;

            //recompute new velocity and position
            physicsData.velocity += Vector3.Multiply(physicsData.acceleration, elapsedSeconds);

            if (physicsData.velocity.Length() < 0.2f)
            {
                physicsData.velocity = Vector3.Zero;
                physicsData.acceleration = Vector3.Zero;
            }

            float remainingTime = elapsedSeconds;

            foreach (GameData sd in this.GameDataOjects)
            {
                if (sd != physicsData)
                    CollideSphereWithSphere(physicsData, sd, remainingTime, out remainingTime);
            }

            CollideSphereWithWall(physicsData, remainingTime);

            Vector3 movementDelta = Vector3.Multiply(physicsData.velocity, remainingTime);
            physicsData.position += movementDelta;

            float distanceMoved = movementDelta.Length();

            Vector3 frontVector = physicsData.velocity;
            Vector3 upvector = Vector3.Up;
            Vector3 rightVector = Vector3.Cross(frontVector, upvector);
            if (rightVector.Length() > 0.0f)
                rightVector.Normalize();

            physicsData.rotation *= Matrix.CreateFromAxisAngle(rightVector, -distanceMoved);
            physicsData.acceleration = Vector3.Zero;

            return (distanceMoved > 0.0f);
        }




        private bool AreSpheresColliding(GameData s1, GameData s2, float elapsedSeconds, out float timeAfterCollision)
        {
            timeAfterCollision = elapsedSeconds;
            Vector3 relativeVelocity = s1.velocity - s2.velocity;
            float radius = 1;
            Vector3 relativePosition = s1.position - s2.position;

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

                s1.position = s1.position + s1.velocity * timeFraction;
                s2.position = s2.position + s2.velocity * timeFraction;

                timeAfterCollision = elapsedSeconds * (1.0f - timeFraction);

                relativePosition = s1.position - s2.position;

                if ((relativePosition.LengthSquared() - (radius * radius)) <= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void CollideSphereWithSphere(GameData s1, GameData s2, float elapsedSeconds, out float remainingTime)
        {
            if (AreSpheresColliding(s1, s2, elapsedSeconds, out remainingTime))
            {
                //we have collision, compute new velocities;

                Vector3 relativePosition = s1.position - s2.position;
                Vector3 relativeUnit = Vector3.Normalize(relativePosition);

                float s1VelDotUnit = Vector3.Dot(s1.velocity, relativeUnit);
                float s2VelDotUnit = Vector3.Dot(s2.velocity, relativeUnit);

                float momentumDifference = (2.0f * (s1VelDotUnit - s2VelDotUnit)) / (2.0f);

                // Compute the final velocity of the two spheres.
                s1.velocity = s1.velocity - momentumDifference * relativeUnit;
                s2.velocity = s2.velocity + momentumDifference * relativeUnit;
            }
            else
            {
                remainingTime = elapsedSeconds;
            }
        }
        private void CollideSphereWithWall(GameData physicsData, float elapsedSeconds)
        {
            Vector3 u = Vector3.Normalize(physicsData.velocity);
            float vLength2 = physicsData.velocity.Length() * elapsedSeconds;
            Ray r = new Ray(physicsData.position, u);

            float? intersectPos;
            foreach (Plane p in collisionPlanes)
            {
                intersectPos = r.Intersects(p);
                if (intersectPos != null)
                {
                    float intersectValue = (float)intersectPos;
                    if ((intersectValue > 0) && (intersectValue < vLength2))
                        physicsData.velocity = Vector3.Reflect(physicsData.velocity, p.Normal);
                }
            }
        }

        private void UpdateCamera()
        {
            // The camera's position depends on the sphere's facing direction: when the
            // sphere turns, the camera needs to stay behind it. So, we'll calculate a
            // rotation matrix using the sphere's facing direction, and use it to
            // transform the two offset values that control the camera.
            Matrix cameraFacingMatrix = Matrix.CreateRotationY(cameraFacingDirection);
            Vector3 positionOffset = Vector3.Transform(CameraPositionOffset,
                cameraFacingMatrix);
            Vector3 targetOffset = Vector3.Transform(CameraTargetOffset,
                cameraFacingMatrix);

            // once we've transformed the camera's position offset vector, it's easy to
            // figure out where we think the camera should be.
            Vector3 cameraPosition = GameDataOjects[0].position + positionOffset;
            // next, we need to calculate the point that the camera is aiming it. That's
            // simple enough - the camera is aiming at the sphere, and has to take the 
            // targetOffset into account.
            Vector3 cameraTarget = GameDataOjects[0].position + targetOffset;

            ChangeMessage msg = new ChangeMessage();
            msg.MessageType = ChangeMessageType.UpdateCameraView;
            msg.CameraViewMatrix = Matrix.CreateLookAt(cameraPosition,
                                              cameraTarget,
                                              Vector3.Up);

            messageBuffer.Messages.Add(msg);
        }

        GamePadState lastGamePadState;
        private void HandleInput()
        {
            GamePadState currentGamePadState = GamePad.GetState(PlayerIndex.One);
            GameData playerBall = GameDataOjects[0];

            float turnAmount = -currentGamePadState.ThumbSticks.Left.X;
            // clamp the turn amount between -1 and 1, and then use the finished
            // value to turn the sphere.
            turnAmount = MathHelper.Clamp(turnAmount, -1, 1);
            cameraFacingDirection += turnAmount * CameraTurnSpeed;

            Matrix cameraFacingMatrix = Matrix.CreateRotationY(cameraFacingDirection);

            if ((currentGamePadState.Buttons.A == ButtonState.Pressed))
            {
                Vector3 accel = Vector3.Transform(new Vector3(0, 0, -20), cameraFacingMatrix);
                playerBall.acceleration = accel;
            }
            if (((currentGamePadState.Buttons.Y == ButtonState.Pressed) &&
                (lastGamePadState.Buttons.Y == ButtonState.Released)))
            {
                Vector3 v = Vector3.Transform(new Vector3(0, 0, -50), cameraFacingMatrix);
                playerBall.velocity = v;
            }

            if (((currentGamePadState.Buttons.X == ButtonState.Pressed) &&
                (lastGamePadState.Buttons.X == ButtonState.Released)))
            {
                foreach (GameData sd in GameDataOjects)
                {
                    if (sd == playerBall)
                        continue;

                    Vector3 randomVel = GetRandomVector3();
                    randomVel.Y = 0;
                    randomVel -= new Vector3(0.5f, 0.0f, 0.5f);
                    sd.velocity = 100 * randomVel;

                }
            }

            if (((currentGamePadState.Buttons.B == ButtonState.Pressed) &&
                (lastGamePadState.Buttons.B == ButtonState.Released)))
            {
                RenderData rd;
                GameData gd;
                ThreadUtil.CreateBall(playerBall.position.X, playerBall.position.Z, out gd, out rd);
                this.GameDataOjects.Add(gd);
                gd.velocity = Vector3.Transform(new Vector3(0, 0, -50), cameraFacingMatrix);

                ChangeMessage msg = new ChangeMessage();
                msg.MessageType = ChangeMessageType.CreateNewRenderData;
                msg.ID = GameDataOjects.IndexOf(gd);
                msg.Position = playerBall.position;
                msg.Color = ThreadUtil.GetRandomColor();

                messageBuffer.Add(msg);
            }

            lastGamePadState = currentGamePadState;
        }

        protected Vector3 GetRandomVector3()
        {
            return new Vector3((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());
        }

    }
}
*/