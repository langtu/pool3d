//#define DRAW_BOUNDINGVOLUME

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNA_PoolGame.Graphics;
using XNA_PoolGame.Graphics.Shadows;
using XNA_PoolGame.Helpers;

namespace XNA_PoolGame.Models
{
    /// <summary>
    /// The basic model (no animation).
    /// </summary>
    public class BasicModel : GameComponent
    {
        #region Variables
        private Model model = null;
        private String modelName = null;
        protected String textureAsset = null;
        private bool visible = true;
        /// <summary>
        /// Choose what Texture display for this model.
        /// </summary>
        protected Texture2D useTexture = null;
        
        private Dictionary<ModelMeshPart, XNA_PoolGame.Graphics.ModelManager.ModelBoundingBoxTexture> effectMapping;
        public TextureAddressMode TEXTURE_ADDRESS_MODE = TextureAddressMode.Clamp;

        private float shineness = 96.0f;
        private Vector4 specularColor = Vector4.One;

        private Matrix localWorld;
        private Vector3 position;
        private Matrix rotation;
        private Matrix initialRotation;
        private Vector3 scale;
        private Matrix[] bonetransforms;
        private BoundingFrustum frustum;

        private bool worldDirty = true;

        protected VolumeType volume;
        BoundingBox boundingBox;
        BoundingSphere boundingSphere;
        
        public bool isObjectAtScenario = true;

        public delegate void RenderHandler();

        #endregion

        #region Properties
        public Vector3 Position
        {
            get { return position; }
            set { worldDirty = true; position = value; }
        }
        public Matrix LocalWorld
        {
            get { return localWorld; }
        }
        public Model TheModel
        {
            get { return model; }
        }

        public Vector4 SpecularColor
        {
            get { return specularColor; }
            set { specularColor = value; }
        }

        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }
        public float Shinennes
        {
            get { return shineness; }
            set { shineness = value; }
        }

        public Matrix Rotation
        {
            get { return rotation; }
            set { worldDirty = true; rotation = value; }
        }

        public float PositionY
        {
            get { return position.Y; }
            set { worldDirty = true; position.Y = value; }
        }
        public float PositionZ
        {
            get { return position.Z; }
            set { worldDirty = true; position.Z = value; }
        }
        public float PositionX
        {
            get { return position.X; }
            set { worldDirty = true; position.X = value; }
        }

        public Matrix InitialRotation
        {
            get { return initialRotation; }
            set { worldDirty = true; initialRotation = value; }
        }

        public Vector3 Scale
        {
            get { return scale; }
            set { worldDirty = true; scale = value; }
        }
        #endregion

        #region Constructor

        public BasicModel(Game _game, String _modelName)
            : base(_game)
        {
            this.modelName = _modelName;

            scale = Vector3.One;
            localWorld = Matrix.Identity;
            rotation = Matrix.Identity;
            initialRotation = Matrix.Identity;
            position = Vector3.Zero;
            textureAsset = null;
            useTexture = null;
            volume = VolumeType.BoundingBoxes;

            worldDirty = true;
        }

        public BasicModel(Game _game, String _modelName, String _textureAsset)
            : this(_game, _modelName)
        {
            this.textureAsset = _textureAsset;
        }

        public BasicModel(Game _game, String _modelName, VolumeType volume, String _textureAsset)
            : this(_game, _modelName)
        {
            this.volume = volume;
            this.textureAsset = _textureAsset;
        }

        #endregion

        #region Initialize

        public override void Initialize()
        {
            base.Initialize();
            LoadContent();
        }

        #endregion

        #region LoadContent
        
        public virtual void LoadContent()
        {
            GC.Collect();
            model = PoolGame.content.Load<Model>(modelName);
            bonetransforms = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(bonetransforms);

            // Load custom texture
            if (textureAsset != null) useTexture = PoolGame.content.Load<Texture2D>(textureAsset);

            // Setup model.
            effectMapping = ModelManager.allEffectMapping[modelName];

            Vector3 centre = new Vector3();
            boundingBox = new BoundingBox();
            boundingSphere = new BoundingSphere();
            if (isObjectAtScenario)
            {
                foreach (ModelMesh m in model.Meshes)
                {
                    LightManager.bounds = BoundingSphere.CreateMerged(LightManager.bounds, m.BoundingSphere);
                    boundingSphere = BoundingSphere.CreateMerged(boundingSphere, m.BoundingSphere);
                    foreach (ModelMeshPart part in m.MeshParts)
                    {
                        boundingBox.Min = Vector3.Min(boundingBox.Min, Vector3.Transform(effectMapping[part].boundingbox.Min, bonetransforms[m.ParentBone.Index]));
                        boundingBox.Max = Vector3.Max(boundingBox.Max, Vector3.Transform(effectMapping[part].boundingbox.Max, bonetransforms[m.ParentBone.Index]));
                    }
                }
                centre = (boundingBox.Max + boundingBox.Min) / 2.0f;
                /*boundingBox.Min = boundingBox.Min - centre;
                boundingBox.Max = boundingBox.Max - centre;*/
            }
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
            
            //Matrix proj = LightManager.CalcLightProjection(LightManager.lights.Position, LightManager.bounds, PoolGame.device.Viewport);

            base.Update(gameTime);
        }
        #endregion

        #region Draw
        public virtual void Draw(GameTime gameTime)
        {
            if (World.camera == null) return;
            RenderMode renderMode = PostProcessManager.currentRenderMode;
            //if (renderMode > RenderMode.ScreenSpaceSoftShadowRender) { return; }

            if (worldDirty)
            {
                localWorld = initialRotation * rotation * Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);
                worldDirty = false;
            }

            switch (renderMode)
            {
                case RenderMode.ShadowMapRender:
                    frustum = LightManager.lights.Frustum;
                    DrawModel(false, LightManager.lights, PostProcessManager.Depth, "DepthMap", delegate { SetParametersShadowMap(LightManager.lights); });
                    break;

                case RenderMode.PCFShadowMapRender:
                    frustum = World.camera.FrustumCulling;
                    DrawModel(false, LightManager.lights, PostProcessManager.PCFShadowMap, "PCFSMTechnique", delegate { SetParametersPCFShadowMap(LightManager.lights); });
                    break;

                case RenderMode.ScreenSpaceSoftShadowRender:
                    frustum = World.camera.FrustumCulling;
                    DrawModel(true, LightManager.lights, PostProcessManager.SSSoftShadow, "SSSTechnique", delegate { SetParametersSoftShadow(LightManager.lights); });
                    break;

                case RenderMode.DoF:
                    frustum = World.camera.FrustumCulling;
                    DrawModel(false, null, PostProcessManager.Depth, "DepthMap", delegate { SetParametersDoFMap(); });
                    break;

                case RenderMode.BasicRender:
                    frustum = World.camera.FrustumCulling;
                    DrawModel(true, LightManager.lights, PostProcessManager.modelEffect, "ModelTechnique", delegate { SetParametersModelEffect(LightManager.lights); });
                    break;
            }
        }

        public void DrawModel(bool enableTexture, Light light, Effect effect, String technique, RenderHandler setParameter)
        {
            if (setParameter != null)
                setParameter.Invoke();

            if (enableTexture)
            {
                PoolGame.device.SamplerStates[0].AddressU = TEXTURE_ADDRESS_MODE;
                PoolGame.device.SamplerStates[0].AddressV = TEXTURE_ADDRESS_MODE;
            }

            effect.CurrentTechnique = effect.Techniques[technique];
            foreach (ModelMesh mesh in model.Meshes)
            {
                if (!ModelInFrustumVolume()) continue;

                foreach (ModelMeshPart parts in mesh.MeshParts)
                {
                    effect.Parameters["World"].SetValue(bonetransforms[mesh.ParentBone.Index] * localWorld);
                    if (enableTexture)
                        if (textureAsset != null)
                            effect.Parameters["TexColor"].SetValue(useTexture);
                        else if (effectMapping[parts] != null)
                            effect.Parameters["TexColor"].SetValue(effectMapping[parts].texture);
                        
                    
                    parts.Effect = effect;
                }
                mesh.Draw();

            }

#if DRAW_BOUNDINGVOLUME
            if (volume == VolumeType.BoundingSpheres) DrawBoundingSphere();
            else DrawBoundingBox();
#endif
        }
        public void DrawBoundingSphere()
        {
            if (PostProcessManager.currentRenderMode != RenderMode.ScreenSpaceSoftShadowRender &&
                PostProcessManager.currentRenderMode != RenderMode.BasicRender) return;


            PoolGame.device.RenderState.DepthBufferEnable = false;
            PoolGame.device.RenderState.DepthBufferWriteEnable = false;

            VectorRenderComponent vectorRenderer = World.poolTable.vectorRenderer;

            vectorRenderer.SetViewProjMatrix(World.camera.ViewProjection);
            vectorRenderer.SetWorldMatrix(localWorld);

            vectorRenderer.SetColor(Color.Aqua);
            
            {
                vectorRenderer.DrawBoundingSphere(boundingSphere);
            }


            PoolGame.device.RenderState.DepthBufferEnable = true;
            PoolGame.device.RenderState.DepthBufferWriteEnable = true;
        }

        public void DrawBoundingBox()
        {
            if (PostProcessManager.currentRenderMode != RenderMode.ScreenSpaceSoftShadowRender &&
                PostProcessManager.currentRenderMode != RenderMode.BasicRender) return;

            VectorRenderComponent vectorRenderer = World.poolTable.vectorRenderer;

            vectorRenderer.SetViewProjMatrix(World.camera.ViewProjection);
            vectorRenderer.SetWorldMatrix(Matrix.Identity);

            {
                vectorRenderer.SetColor(Color.Red);

                Vector3 _min = Vector3.Transform(boundingBox.Min, localWorld);
                Vector3 _max = Vector3.Transform(boundingBox.Max, localWorld);
                Vector3 min = Vector3.Min(_min, _max);
                Vector3 max = Vector3.Max(_min, _max);

                vectorRenderer.DrawBoundingBox(new BoundingBox(min, max));
            }
        }
        public bool ModelInFrustumVolume()
        {
            if (World.camera.EnableFrustumCulling)
            {
                if (volume == VolumeType.BoundingBoxes)
                {
                    Vector3 _min = Vector3.Transform(boundingBox.Min, localWorld);
                    Vector3 _max = Vector3.Transform(boundingBox.Max, localWorld);
                    Vector3 min = Vector3.Min(_min, _max);
                    Vector3 max = Vector3.Max(_min, _max);

                    if (frustum.Contains(new BoundingBox(min, max)) == ContainmentType.Disjoint)
                        return false;
                }
                else
                {
                    if (frustum.Contains(new BoundingSphere(Vector3.Transform(boundingSphere.Center, localWorld), boundingSphere.Radius * Math.Max(scale.X, Math.Max(scale.Y, scale.Z)))) == ContainmentType.Disjoint)
                        return false;
                }
            }
            
            /*if (World.camera.EnableFrustumCulling)
            {
                Vector3 _min = Vector3.Transform(boundingBox.Min, localWorld);
                Vector3 _max = Vector3.Transform(boundingBox.Max, localWorld);
                Vector3 min = Vector3.Min(_min, _max);
                Vector3 max = Vector3.Max(_min, _max);
                //if (frustum.boxInFrustum(min, max) == FrustumTest.Outside)
                //BoundingSphere bs = new BoundingSphere(boundingSphere.Center, boundingSphere.Radius);
                if (frustum.SphereInFrustum(boundingSphere.Center, boundingSphere.Radius) == FrustumTest.Outside)
                    return false;
            }*/
            return true;
        }
        public void BasicDraw()
        {
            BasicEffect basicEffect = PostProcessManager.basicEffect;

            basicEffect.View = World.camera.View;
            basicEffect.Projection = World.camera.Projection;

            basicEffect.EnableDefaultLighting();
            basicEffect.PreferPerPixelLighting = true;
            Vector3 light0Direction = Vector3.Normalize(-LightManager.lights.Position);
            basicEffect.DirectionalLight0.Direction = light0Direction;
            basicEffect.DirectionalLight0.DiffuseColor = Vector3.One;
            
            basicEffect.AmbientLightColor = Vector3.Zero;

            // Override the default specular color to make it nice and bright,
            // so we'll get some decent glints that the bloom can key off.
            basicEffect.SpecularColor = Vector3.One * 0.4f;
            basicEffect.DiffuseColor = Vector3.One;

            {
                PoolGame.device.SamplerStates[0].AddressU = TEXTURE_ADDRESS_MODE;
                PoolGame.device.SamplerStates[0].AddressV = TEXTURE_ADDRESS_MODE;
            }

            foreach (ModelMesh mesh in model.Meshes)
            {
                if (!ModelInFrustumVolume())
                    continue;

                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    basicEffect.World = bonetransforms[mesh.ParentBone.Index] * localWorld;
                    basicEffect.TextureEnabled = true;

                    if (textureAsset != null) basicEffect.Texture = useTexture;
                    else basicEffect.Texture = effectMapping[part].texture;
                    
                    part.Effect = basicEffect;
                }

                mesh.Draw();
                
            }

            
        }


        #endregion

        #region Set Parameters for Basic Render

        public void SetParametersModelEffect(Light light)
        {
            PostProcessManager.modelEffect.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);
            PostProcessManager.modelEffect.Parameters["LightPos"].SetValue(new Vector4(light.Position.X, light.Position.Y, light.Position.Z, 1.0f));

            PostProcessManager.modelEffect.Parameters["vSpecularColor"].SetValue(specularColor);
            PostProcessManager.modelEffect.Parameters["shineness"].SetValue(shineness);

            PostProcessManager.modelEffect.Parameters["vecEye"].SetValue(new Vector4(World.camera.CameraPosition.X, World.camera.CameraPosition.Y, World.camera.CameraPosition.Z, 0.0f));
            
        }

        #endregion

        #region Set Parameters for Shadow

        public void SetParametersSoftShadow(Light light)
        {
            PostProcessManager.SSSoftShadow.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);
            PostProcessManager.SSSoftShadow.Parameters["LightViewProj"].SetValue(light.LightViewProjection);

            if (PostProcessManager.ShadowTechnique == Shadow.SoftShadow)
                PostProcessManager.SSSoftShadow.Parameters["TexBlurV"].SetValue(PostProcessManager.GBlurVRT.GetTexture());
            else
                PostProcessManager.SSSoftShadow.Parameters["TexBlurV"].SetValue(PostProcessManager.ShadowRT.GetTexture());

            PostProcessManager.SSSoftShadow.Parameters["LightPos"].SetValue(new Vector4(light.Position.X, light.Position.Y, light.Position.Z, 1.0f));

            PostProcessManager.SSSoftShadow.Parameters["vSpecularColor"].SetValue(specularColor);
            PostProcessManager.SSSoftShadow.Parameters["shineness"].SetValue(shineness);

            PostProcessManager.SSSoftShadow.Parameters["vecEye"].SetValue(new Vector4(World.camera.CameraPosition.X, World.camera.CameraPosition.Y, World.camera.CameraPosition.Z, 0.0f));

        }

        public void SetParametersPCFShadowMap(Light light)
        {
            PostProcessManager.PCFShadowMap.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);
            PostProcessManager.PCFShadowMap.Parameters["LightViewProj"].SetValue(light.LightViewProjection);
            PostProcessManager.PCFShadowMap.Parameters["MaxDepth"].SetValue(light.LightFarPlane);
            PostProcessManager.PCFShadowMap.Parameters["ShadowMap"].SetValue(PostProcessManager.ShadowMapRT.GetTexture());
            PostProcessManager.PCFShadowMap.Parameters["PCFSamples"].SetValue(PostProcessManager.pcfSamples);
            PostProcessManager.PCFShadowMap.Parameters["depthBias"].SetValue(PostProcessManager.depthBias);
        }

        public void SetParametersShadowMap(Light light)
        {
            PostProcessManager.Depth.Parameters["ViewProj"].SetValue(light.LightViewProjection);
            PostProcessManager.Depth.Parameters["MaxDepth"].SetValue(light.LightFarPlane);
        }

        public void SetParametersDoFMap()
        {
            PostProcessManager.Depth.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);
            PostProcessManager.Depth.Parameters["MaxDepth"].SetValue(2000);
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
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
