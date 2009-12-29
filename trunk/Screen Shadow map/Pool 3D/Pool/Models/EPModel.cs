using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Extreme_Pool.Models
{
    public class EPModel : DrawableGameComponent
    {
        #region Variables
        private Model model;
        private String modelName;

        private Matrix localWorld = Matrix.Identity;
        private Vector3 position = Vector3.Zero;
        private Matrix rotation = Matrix.Identity;
        private Matrix rotationInitial = Matrix.Identity;
        private Vector3 scale = new Vector3(1.0f, 1.0f, 1.0f);
        private Matrix[] bonetransforms;

        #endregion

        #region Properties
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Matrix Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public float PositionY
        {
            get { return position.Y; }
            set { position.Y = value; }
        }
        public float PositionZ
        {
            get { return position.Z; }
            set { position.Z = value; }
        }
        public float PositionX
        {
            get { return position.X; }
            set { position.X = value; }
        }

        public Matrix RotationInitial
        {
            get { return rotationInitial; }
            set { rotationInitial = value; }
        }

        public Vector3 Scale
        {
            get { return scale; }
            set { scale = value; }
        }
        #endregion

        #region Constructor
        public EPModel(ExPool _game, String _modelName)
            : base(_game)
        {
            this.modelName = _modelName;
        }
        #endregion

        #region Initialize
        public override void Initialize()
        {

            base.Initialize();
        }
        #endregion

        #region LoadContent
        protected override void LoadContent()
        {
            model = ExPool.content.Load<Model>(modelName);
            bonetransforms = new Matrix[model.Bones.Count];
            base.LoadContent();

        }

        #endregion

        #region Update

        public virtual bool UpdateLogic(GameTime gameTime)
        {
            return false;
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
        #endregion

        #region Draw
        public override void Draw(GameTime gameTime)
        {
            if (World.camera == null) return;
            model.CopyAbsoluteBoneTransformsTo(bonetransforms);


            //ExPool.device.RenderState.DepthBufferEnable = true;

            Matrix thisworld = rotationInitial * rotation * Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {

                    


                    effect.World = bonetransforms[mesh.ParentBone.Index] * thisworld;
                    effect.View = World.camera.View;
                    effect.Projection = World.camera.Projection;

                    effect.EnableDefaultLighting();

                    // Override the default specular color to make it nice and bright,
                    // so we'll get some decent glints that the bloom can key off.
                    //effect.SpecularColor = Vector3.One;

                    //effect.SpecularColor = new Vector3(0.0f, 0.2f, 0.1f);
                    //effect.SpecularColor = new Vector3(0.1f);
                    effect.SpecularColor = new Vector3(0.5f);

                }
                mesh.Draw();
            }
            base.Draw(gameTime);
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                model = null;
                ExPool.game.Components.Remove(this);

                Console.WriteLine(ExPool.game.Components.Count+ " " + this.GetType().ToString());

                if (ExPool.game.Components.Count < 5)
                {
                    for (int i = 0; i < ExPool.game.Components.Count; ++i)
                    {

                        Console.WriteLine("---- " + i + " - " + ExPool.game.Components[i].GetType().ToString());
                    }
                }
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
