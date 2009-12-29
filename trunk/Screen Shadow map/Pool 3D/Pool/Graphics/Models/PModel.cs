using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNA_PoolGame.Graphics;
using XNA_PoolGame.Graphics.Shadows;

namespace XNA_PoolGame.Models
{
    /// <summary>
    /// The model.
    /// </summary>
    public class PModel : DrawableGameComponent
    {
        #region Variables
        private Model model = null;
        private String modelName = "";

        private Matrix localWorld;
        private Vector3 position;
        private Matrix rotation;
        private Matrix initialRotation;
        private Vector3 scale;
        private Matrix[] bonetransforms;
        private BoundingFrustum frustum;

        public bool isObjectAtScenario = true;
        //private Effect modelEffect;
        //private EffectParameter textureParam;
        //private EffectParameter worldParam;
        private Dictionary<ModelMeshPart, Texture2D> effectMapping;

        public delegate void RenderHandler();

        #endregion

        #region Properties
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }
        public Matrix LocalWorld
        {
            get { return localWorld; }
        }
        public Model TheModel
        {
            get { return model; }
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

        public Matrix InitialRotation
        {
            get { return initialRotation; }
            set { initialRotation = value; }
        }

        public Vector3 Scale
        {
            get { return scale; }
            set { scale = value; }
        }
        #endregion

        #region Constructor

        public PModel(Game _game, String _modelName)
            : base(_game)
        {
            this.modelName = _modelName;

            scale = Vector3.One;
            localWorld = Matrix.Identity;
            rotation = Matrix.Identity;
            initialRotation = Matrix.Identity;
            position = Vector3.Zero;

            //effectMapping = new Dictionary<ModelMeshPart, Texture2D>();
        }

        #endregion

        #region Initialize

        public override void Initialize()
        {
            base.Initialize();
        }

        #endregion

        #region LoadContent
        public void Load()
        {
            LoadContent();
        }
        protected override void LoadContent()
        {
            GC.Collect();
            model = PoolGame.content.Load<Model>(modelName);
            bonetransforms = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(bonetransforms);

            //modelEffect = PostProcessManager.shadowMapEffect;
            //textureParam = modelEffect.Parameters["Texture"];
            //worldParam = modelEffect.Parameters["World"];

            // Setup model.
            effectMapping = ModelManager.allEffectMapping[modelName];

            if (isObjectAtScenario)
            {
                foreach (ModelMesh m in model.Meshes)
                {
                    LightManager.bounds = BoundingSphere.CreateMerged(LightManager.bounds, m.BoundingSphere);
                }
            }
            
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
            if (World.camera == null) return;
            localWorld = initialRotation * rotation * Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);

            //Matrix proj = LightManager.CalcLightProjection(LightManager.lights.Position, LightManager.bounds, PoolGame.device.Viewport);

            base.Update(gameTime);
        }
        #endregion

        #region Draw
        public override void Draw(GameTime gameTime)
        {
            if (World.camera == null) return;
            RenderMode renderMode = PostProcessManager.currentRenderMode;
            if (renderMode > RenderMode.ScreenSpaceSoftShadowRender) { base.Draw(gameTime); return; }

            localWorld = initialRotation * rotation * Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);
            
            //PoolGame.game.PrepareRenderStates();

            frustum = null;
            
            switch (renderMode)
            {
                case RenderMode.ShadowMapRender:
                    frustum = new BoundingFrustum(LightManager.lights.LightView * LightManager.lights.LightProjection);
                    DrawModel(false, LightManager.lights, PostProcessManager.ShadowMap, delegate { SetParametersShadowMap(LightManager.lights); });
                    break;

                case RenderMode.PCFShadowMapRender:
                    frustum = World.camera.FrustumCulling;
                    DrawModel(false, LightManager.lights, PostProcessManager.PCFShadowMap, delegate { SetParametersPCFShadowMap(LightManager.lights); });
                    break;

                case RenderMode.ScreenSpaceSoftShadowRender:
                    frustum = World.camera.FrustumCulling;
                    DrawModel(true, LightManager.lights, PostProcessManager.SSSoftShadow, delegate { SetParametersSoftShadow(LightManager.lights); });
                    break;

                case RenderMode.BasicRender:
                    frustum = World.camera.FrustumCulling;
                    BasicDraw();
                    break;
            }


            #region Old stuff
            // OBJECT RENDER.
                /*Light light = LightManager.lights;

                if (World.displayShadows)
                {
                    //Shadow MyEffect = PostProcessManager.shadow;

                    foreach (ModelMesh meshes in model.Meshes)
                    {
                        MyEffect.mWorld.SetValue(bonetransforms[meshes.ParentBone.Index] * localWorld);
                        foreach (ModelMeshPart parts in meshes.MeshParts)
                        {
                            MyEffect.effect.CurrentTechnique = PostProcessManager.shadow.shadows;
                            Dictionary<ModelMeshPart, Texture2D> effectMapping = ModelManager.allEffectMapping[modelName];
                            MyEffect.MeshTexture.SetValue(effectMapping[parts]);

                        }
                        meshes.Draw();
                    }*/

                    /*foreach (ModelMesh mesh in redtorus.Meshes)
                    {
                        foreach (Effect effect in mesh.Effects)
                        {
                            effect.CurrentTechnique = technique;
                            mesh.Draw();
                        }
                    }*/

            /*if (World.displaySceneFromLightSource)
            {
                modelEffect.Parameters["View"].SetValue(light.LightView);
                modelEffect.Parameters["Projection"].SetValue(light.LightProject);
            }
            else
            {
                modelEffect.Parameters["View"].SetValue(World.camera.View);
                modelEffect.Parameters["Projection"].SetValue(World.camera.Projection);
            }
            modelEffect.Parameters["LightView"].SetValue(light.LightView);
            modelEffect.Parameters["LightProjection"].SetValue(light.LightProject);
            modelEffect.Parameters["ShadowMap"].SetValue(PostProcessManager.depthBlurred);
            modelEffect.Parameters["FarClip"].SetValue(World.camera.FarPlane);
            modelEffect.Parameters["xLightPos"].SetValue(light.Position);
            modelEffect.Parameters["xLightPower"].SetValue(light.LightPower);

            DrawByUserEffect(modelEffect, true);
        }
        else
        {
            BasicDraw();
        }
    }
    else if (renderMode == RenderMode.Light)
    {
        Shadow MyEffect = PostProcessManager.shadow;

        foreach (ModelMesh meshes in model.Meshes)
        {
            MyEffect.mWorld.SetValue(bonetransforms[meshes.ParentBone.Index] * localWorld);
            foreach (ModelMeshPart parts in meshes.MeshParts)
            {
                MyEffect.effect.CurrentTechnique = PostProcessManager.shadow.texture;
                Dictionary<ModelMeshPart, Texture2D> effectMapping = ModelManager.allEffectMapping[modelName];
                MyEffect.MeshTexture.SetValue(effectMapping[parts]);

            }
            meshes.Draw();
        }*/
            #endregion


            base.Draw(gameTime);
        }

        public void DrawModel(bool enableTexture, Light light, Effect effect, RenderHandler setParameter)
        {
            if (setParameter != null)
                setParameter.Invoke();

            if (!World.camera.EnableFrustumCulling) frustum = null;
            
            foreach (ModelMesh mesh in model.Meshes)
            {
                BoundingSphere bound = new BoundingSphere(mesh.BoundingSphere.Center + position, mesh.BoundingSphere.Radius);
                if ((frustum != null && frustum.Contains(bound) != ContainmentType.Disjoint) || frustum == null)
                {
                    foreach (ModelMeshPart parts in mesh.MeshParts)
                    {
                        
                        effect.Parameters["World"].SetValue(bonetransforms[mesh.ParentBone.Index] * localWorld);
                        if (enableTexture && effectMapping[parts] != null)
                            effect.Parameters["TexColor"].SetValue(effectMapping[parts]);
                        
                        parts.Effect = effect;
                    }
                    mesh.Draw();
                }
            }

        }

        public void BasicDraw()
        {
            if (!World.camera.EnableFrustumCulling) frustum = null;
            BasicEffect basicEffect = PostProcessManager.basicEffect;

            basicEffect.View = World.camera.View;
            basicEffect.Projection = World.camera.Projection;

            basicEffect.EnableDefaultLighting();
            //basicEffect.AmbientLightColor = Vector3.One
            // Override the default specular color to make it nice and bright,
            // so we'll get some decent glints that the bloom can key off.
            basicEffect.SpecularColor = Vector3.One * 0.8f;
            basicEffect.DiffuseColor = Vector3.One;

            foreach (ModelMesh mesh in model.Meshes)
            {
                BoundingSphere bound = new BoundingSphere(mesh.BoundingSphere.Center + position, mesh.BoundingSphere.Radius);

                if ((frustum != null && frustum.Contains(bound) != ContainmentType.Disjoint) || frustum == null)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        basicEffect.World = bonetransforms[mesh.ParentBone.Index] * localWorld;
                        if (effectMapping[part] != null)
                        {
                            basicEffect.TextureEnabled = true;
                            basicEffect.Texture = effectMapping[part];
                        }
                        part.Effect = basicEffect;
                    }

                    mesh.Draw();
                }
            }
        }


        #endregion

        #region Set Parameters for Shadow
        public void SetParametersSoftShadow(Light light)
        {
            Matrix camMat = World.camera.ViewProjection;
            Matrix ligMat = light.LightViewProjection;
            PostProcessManager.SSSoftShadow.Parameters["ViewProj"].SetValue(camMat);
            PostProcessManager.SSSoftShadow.Parameters["LightViewProj"].SetValue(ligMat);

            if (PostProcessManager.ShadowTechnique == Shadow.SoftShadow)
                PostProcessManager.SSSoftShadow.Parameters["TexBlurV"].SetValue(PostProcessManager.GBlurVRT.GetTexture());
            else
                PostProcessManager.SSSoftShadow.Parameters["TexBlurV"].SetValue(PostProcessManager.ShadowRT.GetTexture());

            PostProcessManager.SSSoftShadow.Parameters["LightPos"].SetValue(new Vector4(light.Position.X, light.Position.Y, light.Position.Z, 1));
            //SSSoftShadow.Parameters["Mask"].SetValue(mask);
        }

        public void SetParametersPCFShadowMap(Light light)
        {
            Matrix camMat = World.camera.ViewProjection;
            Matrix ligMat = light.LightViewProjection;
            PostProcessManager.PCFShadowMap.Parameters["ViewProj"].SetValue(camMat);
            PostProcessManager.PCFShadowMap.Parameters["LightViewProj"].SetValue(ligMat);
            PostProcessManager.PCFShadowMap.Parameters["MaxDepth"].SetValue(light.LightFarPlane);
            PostProcessManager.PCFShadowMap.Parameters["ShadowMap"].SetValue(PostProcessManager.ShadowMapRT.GetTexture());
            PostProcessManager.PCFShadowMap.Parameters["PCFSamples"].SetValue(PostProcessManager.pcfSamples);
            PostProcessManager.PCFShadowMap.Parameters["depthBias"].SetValue(PostProcessManager.depthBias);
        }

        public void SetParametersShadowMap(Light light)
        {
            Matrix ligMat = light.LightViewProjection;
            PostProcessManager.ShadowMap.Parameters["ViewProj"].SetValue(ligMat);
            PostProcessManager.ShadowMap.Parameters["MaxDepth"].SetValue(light.LightFarPlane);
        }
        #endregion

        

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            //if (disposing)
            {
                model = null;
                bonetransforms = null;
                PoolGame.game.Components.Remove(this);

                /*Console.WriteLine(PoolGame.game.Components.Count+ " " + this.GetType().ToString());

                if (PoolGame.game.Components.Count < 5)
                {
                    for (int i = 0; i < PoolGame.game.Components.Count; ++i)
                    {

                        Console.WriteLine("---- " + i + " - " + PoolGame.game.Components[i].GetType().ToString());
                    }
                }*/
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
