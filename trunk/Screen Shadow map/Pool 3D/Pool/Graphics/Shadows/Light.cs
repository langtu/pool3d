﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNA_PoolGame.Graphics.Shadows
{
    public class Light
    {
        #region Fields
        private Vector3 position;
        private Vector3 lookAt;
        private Vector4 ambientColor;
        private Vector4 diffuseColor;
        private Vector4 specularColor;
        private float lightPower;

        private float lightFOV = 2.18770885f;
        private Matrix viewMatrix;
        private Matrix viewProjectionMatrix;
        private Matrix projectionMatrix;
        private BoundingFrustum frustum;

        private float lightNearPlane = 0.01f;
        private float lightFarPlane = 1620.0f;

        #endregion

        #region Dirty States
        private bool viewDirty = true;
        private bool projDirty = true;
        private bool viewProjDirty = true;

        #endregion

        #region Properties
        public float LightPower
        {
            get { return lightPower; }
            set { lightPower = value; }
        }
        public BoundingFrustum Frustum
        {
            get { return frustum; }
            set { frustum = value; }
        }

        public float LightFOV
        {
            get { return lightFOV; }
            set { viewProjDirty = true; projDirty = true; lightFOV = value; }
        }

        public float LightNearPlane
        {
            get { return lightNearPlane; }
            set { viewProjDirty = true; projDirty = true; lightNearPlane = value; }
        }

        public float LightFarPlane
        {
            get { return lightFarPlane; }
            set { viewProjDirty = true; projDirty = true; lightFarPlane = value; }
        }

        public Matrix LightViewProjection
        {
            get 
            {
                if (viewDirty || projDirty || viewProjDirty)
                {
                    viewProjectionMatrix = LightView * LightProjection;
                    viewProjDirty = false; 
                }
                return viewProjectionMatrix; 
            }
        }
        public Matrix LightView
        {
            get 
            {
                if (viewDirty)
                {
                    viewMatrix = Matrix.CreateLookAt(Position, new Vector3(lookAt.X, lookAt.Y, lookAt.Z), new Vector3(0, 1, 0));
                    viewDirty = false; viewProjDirty = true;
                }

                return viewMatrix;
            }
        }
        public Matrix LightProjection
        {
            get 
            {
                if (projDirty)
                {
                    //projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.Pi * 0.90f, (float)PoolGame.Width / (float)PoolGame.Height, lightNearPlane, lightFarPlane);
                    projectionMatrix = Matrix.CreatePerspectiveFieldOfView(lightFOV, (float)PoolGame.Width / (float)PoolGame.Height, lightNearPlane, lightFarPlane);
                    projDirty = false; viewProjDirty = true;
                }
                return projectionMatrix;
            }
            
        }

        public Vector3 LookAt
        {
            get { return lookAt; }
            set { viewDirty = true; viewProjDirty = true; lookAt = value; }
        }

        public Vector4 SpecularColor
        {
            get { return specularColor; }
            set { specularColor = value; }
        }

        public Vector4 DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; }
        }

        public Vector4 AmbientColor
        {
            get { return ambientColor; }
            set { ambientColor = value; }
        }

        public Vector3 Position
        {
            get { return position; }
            set { viewDirty = true; viewProjDirty = true; position = value; }
        }


        #endregion

        #region Constructor
        public Light(Vector3 position)
        {
            this.position = position;
            this.lookAt = Vector3.Zero;
            this.ambientColor = new Vector4(0.1843f, 0.3098f, 0.3098f, 1);
            //this.ambientColor = new Vector4(0, 0, 0, 1);
            this.diffuseColor = new Vector4(0.3921f, 0.5843f, 0.9294f, 1);
            this.diffuseColor = new Vector4(1f, 1f, 1f, 1);
            this.specularColor = new Vector4(1, 1, 1, 1);
            this.lightPower = 2f;

            frustum = new BoundingFrustum(LightViewProjection);
        }
        #endregion
    }
}
