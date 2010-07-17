﻿//#define DRAW_TEXT
#define USE_DIRTY_STATES

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using XNA_PoolGame.Graphics;

namespace XNA_PoolGame.Cameras
{
    public abstract class Camera : GameComponent
    {
        #region Properties

        #region Frustum Culling
        protected BoundingFrustum frustum;
        //public FrustumCulling fc;
        protected bool enableFrustum;
        public int ItemsDrawn = 0;

        public bool EnableFrustumCulling
        {
            get { return enableFrustum; }
            set { enableFrustum = value; }
        }

        /// <summary>
        /// Frustum culling.
        /// </summary>
        public BoundingFrustum FrustumCulling
        {
            get
            {
                return frustum;
            }
        }
        #endregion

        #region Dirty States
        protected bool viewDirty = true;
        protected bool projDirty = true;
        protected bool viewProjDirty = true;
        protected bool invViewDirty = true;
        protected bool invProjDirty = true;
        protected bool invViewProjDirty = true;

        //
        protected bool isMoveablePitchAndYaw = false;
        protected bool switch_pos = false;
        protected Vector3 cameraPosition = new Vector3(0, 20, 1000);
        protected Vector3 last_position = new Vector3(0, 20, 1000);
        protected Vector3 angle = Vector3.Zero;
        protected Vector3 last_angle;
        protected Vector3 forward = Vector3.Forward;
        protected float speed = 250f;
        protected float turnSpeed = 45f;
        #endregion

        #region Near Plane

        private float nearplane = 1.0f;
        public float NearPlane
        {
            get { return nearplane; }
            set { nearplane = value; projDirty = true; invProjDirty = true; }
        }
        #endregion

        #region Far Plane

        private float farplane = 100000.0f;
        public float FarPlane
        {
            get { return farplane; }
            set { farplane = value; projDirty = true; invProjDirty = true; }
        }
        #endregion

        #region Field Of View

        private float fov = MathHelper.ToRadians(45.0f);
        public float FieldOfView
        {
            get { return fov; }
            set { fov = value; projDirty = true; invProjDirty = true; }
        }
        #endregion

        #region Aspect Ratio

        private float aspectRatio;
        public float AspectRatio
        {
            get { return aspectRatio; }
            set { aspectRatio = value; projDirty = true; invProjDirty = true; }
        }

        #endregion

        #region Matrix Projection

        protected Matrix projectionMatrix;
        public virtual Matrix Projection
        {
            get
            {
                if (projDirty)
                {
                    projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                                    fov,
                                    aspectRatio,
                                    nearplane,
                                    farplane);

                    #if USE_DIRTY_STATES
                    projDirty = false;
                    viewProjDirty = true;
                    #endif
                }

                return projectionMatrix;
            }
            set { }
        }

        #endregion

        #region Matrix View

        protected Matrix viewMatrix;
        protected Matrix prevviewMatrix;
        public virtual Matrix View
        {
            get
            {
                if (viewDirty)
                {
                    //prevviewMatrix = viewMatrix;
                    UpdateCameraMatrices();

                    #if USE_DIRTY_STATES
                    viewDirty = false;
                    invViewDirty = true;
                    viewProjDirty = true;
                    #endif
                }
                return viewMatrix;
            }
            set { }
        }
        public Matrix PrevView
        {
            get
            {
                if (prevviewMatrix == Matrix.Identity) prevviewMatrix = viewMatrix;
                return prevviewMatrix;
            }
            set
            {
                prevviewMatrix = value;
            }
        }
        #endregion

        #region Matrix ViewProjection

        protected Matrix viewProjectionMatrix;
        private Matrix previewProjectionMatrix;
        public virtual Matrix ViewProjection
        {
            get
            {
                if (viewProjDirty || viewDirty || projDirty)
                {
                    viewProjectionMatrix = View * Projection;

                    #if USE_DIRTY_STATES
                    viewProjDirty = false;
                    invViewProjDirty = true;
                    #endif
                }
                return viewProjectionMatrix;
            }
            set { }
        }

        public Matrix PrevViewProjection
        {
            get
            {
                if (previewProjectionMatrix == Matrix.Identity) previewProjectionMatrix = viewProjectionMatrix;
                return previewProjectionMatrix;
            }
            set
            {
                previewProjectionMatrix = value;
            }
        }

        #endregion

        #region Matrix InvProjection

        private Matrix invProjectionMatrix;
        public Matrix InvProjection
        {
            get
            {
                if (invProjDirty)
                {
                    invProjectionMatrix = Matrix.Invert(Projection);

                    #if USE_DIRTY_STATES
                    invProjDirty = false;
                    #endif
                }

                return invProjectionMatrix;
            }
        }
        #endregion

        #region Matrix InvView

        private Matrix invViewMatrix;
        public Matrix InvView
        {
            get
            {
                if (invViewDirty)
                {
                    invViewMatrix = Matrix.Invert(View);

                    #if USE_DIRTY_STATES
                    invViewDirty = false;
                    #endif
                }
                return invViewMatrix;
            }
        }

        #endregion

        #region Matrix InvViewProjection

        private Matrix invViewProjectionMatrix;
        public Matrix InvViewProjection
        {
            get
            {
                if (invViewProjDirty || viewProjDirty || viewDirty || projDirty)
                {
                    invViewProjectionMatrix = Matrix.Invert(ViewProjection);

                    #if USE_DIRTY_STATES
                    invViewProjDirty = false;
                    #endif
                }
                return invViewProjectionMatrix;
            }
        }

        #endregion

        #region Camara Position
        public Vector3 CameraPosition
        {
            get { return cameraPosition; }
            set { cameraPosition = value; viewDirty = true; invViewDirty = true; }
        }
        #endregion

        #region Camera Direction (Vector)
        public Vector3 Forward
        {
            get { return forward; }
            set { forward = value; }
        }

        public Vector3 Angle
        {
            get { return angle; }
            set { angle = value; }
        }

        #endregion

        #endregion

        #region Constructor
        public Camera(Game game)
            : this(game, (float)PoolGame.Width / (float)PoolGame.Height, MathHelper.ToRadians(45.0f), 1f, 4000.0f)
        {



        }

        public Camera(Game game, float ratio, float fieldofview, float nearplane, float farplane)
            : base(game)
        {
            

            this.aspectRatio = ratio;
            this.fov = fieldofview;
            this.nearplane = nearplane;
            this.farplane = farplane;
            

            this.cameraPosition = new Vector3(-265, 338, -144);

            angle.X = MathHelper.ToRadians(90.0f);
            angle.Y = MathHelper.ToRadians(90.0f);

            
            angle.Z = 0.0f;

            //
            prevviewMatrix = Matrix.Identity;
            previewProjectionMatrix = Matrix.Identity;

            // FRUSTUM CULLING
            enableFrustum = true;

            frustum = new BoundingFrustum(ViewProjection);
            //fc = new FrustumCulling(ViewProjection);
            
        }

        public override void Initialize()
        {
            this.UpdateOrder = 3; //this.DrawOrder = 800;

            base.Initialize();
        }
        #endregion

        #region Draw
        /*public override void Draw(GameTime gameTime)
        {
            if (PostProcessManager.currentRenderMode != RenderMode.Menu) { base.Draw(gameTime); return; }
#if DRAW_TEXT
            SpriteFont spriteFont = PoolGame.game.CurrentScreen.spriteFont;
            PoolGame.batch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred,
                SaveStateMode.SaveState);
            string text;
            text = "cam: \n" + "\nPos: \nX: " + cameraPosition.X + "\nY: " + cameraPosition.Y + "\nZ: " + cameraPosition.Z + "\n";
            text += "\ndirector: \nX: " + forward.X + "\nY: " + forward.Y + "\nZ: " + forward.Z + "\n";
            
            PoolGame.batch.DrawString(spriteFont, text, new Vector2(18, 12), Color.Black);
            PoolGame.batch.DrawString(spriteFont, text, new Vector2(17, 11), Color.Tomato);

            PoolGame.batch.End();
#endif
            base.Draw(gameTime);
        }*/
        #endregion

        #region Update

        protected KeyboardState lastkb, kb;
        public override void Update(GameTime gameTime)
        {
            if (enableFrustum)
            {
                frustum.Matrix = viewMatrix * projectionMatrix;
                //fc.Matrix = viewMatrix * projectionMatrix;
            }
            base.Update(gameTime);
        }

        public virtual void MovePicthYaw(Vector2 movement) { }

        public abstract void SetMouseCentered();

        public abstract void UpdateCameraMatrices();

        #endregion

        #region Dispose

        protected override void Dispose(bool disposing)
        {
            PoolGame.game.Components.Remove(this);
            base.Dispose(disposing);
        }

        #endregion
    }
}