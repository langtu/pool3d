﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Graphics;
using Microsoft.Xna.Framework.Graphics;
using XNA_PoolGame.GameControllers;

namespace XNA_PoolGame.Screens
{
    public class Cursor : DrawableComponent
    {
        Texture2D cursorTexture;
        Vector2 cursorPosition;
        GameController controller;

        #region Properties

        public Vector2 CursorPosition
        {
            get { return cursorPosition; }
            set { cursorPosition = value; }
        }
        public GameController Controller
        {
            get { return controller; }
            set { controller = value; }
        }
        public Texture2D CursorTexture
        {
            get { return cursorTexture; }
        }

        #endregion

        public Cursor(Game _game)
            : base(_game)
        {

            Visible = false;
            cursorPosition = Vector2.One * 0.5f;
        }

        public override void LoadContent()
        {
             cursorTexture = PoolGame.content.Load<Texture2D>("Textures\\Cursors\\Cursor 32");
            base.LoadContent();
        }

        public Ray CalculateCursorRay()
        {
            Vector4 nearSource;
            nearSource.X = this.cursorPosition.X * 2.0f - 1.0f;
            nearSource.Y = -(this.cursorPosition.Y * 2.0f - 1.0f);
            nearSource.Z = 0.0f;
            nearSource.W = 1.0f;
            Vector4 farSource;
            farSource.X = this.cursorPosition.X * 2.0f - 1.0f;
            farSource.Y = -(this.cursorPosition.Y * 2.0f - 1.0f);
            farSource.Z = 1.0f;
            farSource.W = 1.0f;

            Vector4 farPointV4 = Vector4.Transform(farSource, World.camera.InvViewProjection);
            Vector4 nearPointV4 = Vector4.Transform(nearSource, World.camera.InvViewProjection);

            farPointV4 /= farPointV4.W;
            nearPointV4 /= nearPointV4.W;

            Vector3 nearPoint = new Vector3(nearPointV4.X, nearPointV4.Y, nearPointV4.Z);
            Vector3 farPoint = new Vector3(farPointV4.X, farPointV4.Y, farPointV4.Z);

            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            return new Ray(nearPoint, direction);
        }

        public override void Update(GameTime gameTime)
        {
            if (World.currentScreen == null) return;
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (World.currentScreen is GameplayScreen)
            {
                Player player = World.players[World.playerInTurnIndex];
                if (!player.table.ballsMoving && !player.stick.charging && controller.isRightShoulderPressed)
                {
                    Vector2 newPosition = this.cursorPosition;
                    newPosition.Y -= (controller.LeftStick.Y) * dt * 0.35f;
                    newPosition.X += (controller.LeftStick.X) * dt * 0.35f;

                    this.cursorPosition = Vector2.Max(Vector2.Zero, Vector2.Min(Vector2.One, newPosition));
                }
            }
            else if (World.currentScreen is MenuScreen)
            {

            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            controller = null;
            if (cursorTexture != null)
                cursorTexture.Dispose();
            cursorTexture = null;

            PoolGame.game.Components.Remove(this);

            base.Dispose(disposing);
        }
    }
}