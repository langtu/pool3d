using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNA_PoolGame.Graphics.Bloom;
using XNA_PoolGame.Screens;
using XNA_PoolGame.Graphics;
using XNA_PoolGame.Helpers;

namespace XNA_PoolGame.Cameras
{
    /// <summary>
    /// Free camera.
    /// </summary>
    public class FreeCamera : Camera
    {
        public FreeCamera(Game game)
            : base(game)
        {
            
        }
        public FreeCamera(Game game, Vector3 rotation)
            : base(game)
        {
            angle = rotation;
        }

        public override void Initialize()
        {
            base.Initialize();
            viewDirty = true; projDirty = true;
        }

        private Matrix prevPojection = Matrix.Identity;
        public override void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float localspeed = speed;

            lastkb = kb;
            kb = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            int centerX = Game.Window.ClientBounds.Width / 2;
            int centerY = Game.Window.ClientBounds.Height / 2;

            //if (PoolGame.game.IsActive) Mouse.SetPosition(centerX, centerY);

            //if (isMoveablePitchAndYaw && PoolGame.game.IsActive)
            //{
            //    if (mouse.Y != centerY || mouse.X != centerX)
            //    {
            //        angle.X += MathHelper.ToRadians((mouse.Y - centerY) * turnSpeed * 0.01f); // pitch
            //        angle.Y += MathHelper.ToRadians((mouse.X - centerX) * turnSpeed * 0.01f); // yaw
            //    }
            //    viewDirty = true;
            //}

            forward = Vector3.Normalize(new Vector3((float)Math.Sin(-angle.Y), (float)Math.Sin(angle.X), (float)Math.Cos(-angle.Y)));
            Vector3 left = Vector3.Normalize(new Vector3((float)Math.Cos(angle.Y), 0f, (float)Math.Sin(angle.Y)));

            if (kb.IsKeyDown(Keys.LeftShift)) localspeed *= 6;
            if (kb.IsKeyDown(Keys.LeftControl)) localspeed *= 0.025f;

            if (kb.IsKeyDown(Keys.F5) && lastkb.IsKeyUp(Keys.F5))
                isMoveablePitchAndYaw = !isMoveablePitchAndYaw;

            if (kb.IsKeyDown(Keys.W))
            {
                cameraPosition -= forward * localspeed * delta;
                viewDirty = true;
            }

            if (kb.IsKeyDown(Keys.S))
            {
                cameraPosition += forward * localspeed * delta;
                viewDirty = true;
            }
            

            if (kb.IsKeyDown(Keys.D))
            {
                cameraPosition += left * localspeed * delta;
                viewDirty = true;
            }

            if (kb.IsKeyDown(Keys.A))
            {
                cameraPosition -= left * localspeed * delta;
                viewDirty = true;
            }

            if (kb.IsKeyDown(Keys.PageUp))
            {
                cameraPosition += Vector3.Down * localspeed * delta;
                viewDirty = true;
            }

            if (kb.IsKeyDown(Keys.PageDown))
            {
                cameraPosition += Vector3.Up * localspeed * delta;
                viewDirty = true;
            }
            if (kb.IsKeyDown(Keys.T) && lastkb.IsKeyUp(Keys.T))
            {
                World.poolTable.poolBalls[1].min_Altitute = 138.44f;
                World.poolTable.poolBalls[1].Position = new Vector3(World.poolTable.pockets[2].bounds.Center.X, World.poolTable.SURFACE_POSITION_Y + World.ballRadius * 15, World.poolTable.pockets[2].bounds.Center.Z);
                World.poolTable.poolBalls[1].currentTrajectory = Trajectory.Free;
                World.poolTable.poolBalls[1].velocity = new Vector3(0, -10, 0);
                World.poolTable.poolBalls[1].pocketWhereAt = 2;
            }

            if (kb.IsKeyDown(Keys.O) && lastkb.IsKeyUp(Keys.O))
            {
                World.poolTable.poolBalls[2].min_Altitute = 138.44f;
                World.poolTable.poolBalls[2].Position = new Vector3(World.poolTable.pockets[2].bounds.Center.X, World.poolTable.SURFACE_POSITION_Y + World.ballRadius * 15, World.poolTable.pockets[2].bounds.Center.Z);
                World.poolTable.poolBalls[2].currentTrajectory = Trajectory.Free;
                World.poolTable.poolBalls[2].velocity = new Vector3(0, -10, 0);
                World.poolTable.poolBalls[2].pocketWhereAt = 2;
            }

            if (kb.IsKeyDown(Keys.F1) && lastkb.IsKeyUp(Keys.F1))
            {
                World.poolTable.poolBalls[3].min_Altitute = 138.44f;
                World.poolTable.poolBalls[3].Position = new Vector3(World.poolTable.pockets[2].bounds.Center.X, World.poolTable.SURFACE_POSITION_Y + World.ballRadius * 15, World.poolTable.pockets[2].bounds.Center.Z);
                World.poolTable.poolBalls[3].currentTrajectory = Trajectory.Free;
                World.poolTable.poolBalls[3].velocity = new Vector3(0, -10, 0);
                World.poolTable.poolBalls[3].pocketWhereAt = 2;
            }

            if (kb.IsKeyDown(Keys.F4) && lastkb.IsKeyUp(Keys.F4))
            {

                if (!switch_pos)
                {
                    last_position = cameraPosition;
                    last_angle = angle;
                    //cameraPosition = new Vector3(0, 1000, 0);
                    viewMatrix = LightManager.lights[PoolGame.game.currentlight].LightView;


                    angle.X = (float)Maths.AngleBetweenVectors(Vector3.Up, viewMatrix.Up);
                    angle.Y = -(float)Maths.AngleBetweenVectors(Vector3.Right, viewMatrix.Right);
                    angle.Z = MathHelper.ToRadians(0.0f);

                    cameraPosition = LightManager.lights[PoolGame.game.currentlight].Position;
                    prevPojection = this.Projection;
                    projectionMatrix = LightManager.lights[PoolGame.game.currentlight].LightProjection;

                    //angle.X = MathHelper.ToRadians(90.0f);
                    //angle.Y = MathHelper.ToRadians(90.0f);
                    //angle.Z = MathHelper.ToRadians(0.0f);
                }
                else
                {
                    cameraPosition = last_position;
                    angle = last_angle;
                    projectionMatrix = prevPojection;
                }
                switch_pos = !switch_pos;
                viewDirty = true;

            }
            

            if (viewDirty) UpdateCameraMatrices();

            base.Update(gameTime);
        }

        public override void UpdateCameraMatrices()
        {
            viewMatrix = Matrix.Identity;
            viewMatrix *= Matrix.CreateTranslation(-cameraPosition);
            viewMatrix *= Matrix.CreateRotationZ(angle.Z);
            viewMatrix *= Matrix.CreateRotationY(angle.Y);
            viewMatrix *= Matrix.CreateRotationX(angle.X);
        }

        public override void MovePicthYaw(Vector2 movement)
        {
            if (isMoveablePitchAndYaw)
            {
                angle.X += MathHelper.ToRadians(-movement.Y * turnSpeed * 0.01f); // pitch
                angle.Y += MathHelper.ToRadians(movement.X * turnSpeed * 0.01f); // yaw
                viewDirty = true;
            }
        }

        public override void SetMouseCentered()
        {
            int centerX = Game.Window.ClientBounds.Width / 2;
            int centerY = Game.Window.ClientBounds.Width / 2;

            Mouse.SetPosition(centerX, centerY);
        }
    }
}
