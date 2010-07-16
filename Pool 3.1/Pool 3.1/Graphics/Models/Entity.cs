//#define DRAW_BOUNDINGVOLUME

#region Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNA_PoolGame.Graphics;
using XNA_PoolGame.Graphics.Shadows;
using XNA_PoolGame.Helpers;
using System.Threading;
using System.Collections;
#endregion

namespace XNA_PoolGame.Graphics.Models
{
    /// <summary>
    /// The basic model (without animation).
    /// </summary>
    public class Entity : DrawableComponent
    {
        #region Variables

        protected CustomModel model = null;
        protected string modelName = null;
        protected string textureAsset = null;
        protected bool drawboundingvolume = false;

        #region //// MATERIAL PROPERTIES ////

        /// <summary>
        /// Choose what Texture display for this model.
        /// </summary>
        protected Texture2D useTexture = null;

        public TextureAddressMode TEXTURE_ADDRESS_MODE = TextureAddressMode.Clamp;

        /// <summary>
        /// Determines if a model is shadowable.
        /// </summary>
        public bool occluder = true;

        private float shineness = 96.0f;
        private Vector4 specularColor = Vector4.One;
        private Vector4 materialDiffuseColor = Vector4.One;
        public string normalMapAsset = null;
        public string heightMapAsset = null;
        protected Texture2D normalMapTexture = null;
        protected Texture2D heightMapTexture = null;
        private List<Light> aditionalLights;
        private float[] lightsradius;
        private Vector4[] lightscolors;
        private Vector4[] lightspositions;
        private int[] lightstype;
        #endregion

        #region //// FRUSTUM CULLING ////

        /// <summary>
        /// What frustum volume use? Camera or Light source Frustum.
        /// </summary>
        private BoundingFrustum frustum;

        /// <summary>
        /// Type of volume
        /// </summary>
        protected VolumeType volume;
        BoundingBox boundingBox;
        BoundingSphere boundingSphere;
        
        public bool belongsToScenario = true;

        #endregion

        #region //// THREADS RELATED ////

        

        

        #endregion

        #region //// MODEL RELATED ////

        protected Matrix localWorld;
        private Vector3 position;
        private Matrix rotation;
        private Matrix preRotation;
        private Vector3 scale;
        protected List<Texture2D> textures;
        protected List<BoundingBox> boxes;
        protected Matrix prelocalWorld;

        private bool worldDirty = true;

        #endregion

        /// <summary>
        /// Render parameters delegate
        /// </summary>
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
        public CustomModel Model
        {
            get { return model; }
        }
        public Vector4 MaterialDiffuse
        {
            get { return materialDiffuseColor; }
            set { materialDiffuseColor = value; }
        }
        public Vector4 SpecularColor
        {
            get { return specularColor; }
            set { specularColor = value; }
        }

        public float Shinennes
        {
            get { return shineness; }
            set { shineness = value; }
        }
        public List<Light> AditionalLights
        {
            get { return aditionalLights; }
            set 
            { 
                aditionalLights = value;
                lightsradius = null; lightscolors = null; lightspositions = null; lightstype = null;
                lightsradius = new float[aditionalLights.Count];
                lightscolors = new Vector4[aditionalLights.Count];
                lightspositions = new Vector4[aditionalLights.Count];
                lightstype = new int[aditionalLights.Count];
                UpdateLightsProperties();
            }
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

        public Matrix PreRotation
        {
            get { return preRotation; }
            set { worldDirty = true; preRotation = value; }
        }

        public Vector3 Scale
        {
            get { return scale; }
            set { worldDirty = true; scale = value; }
        }
        public string TextureAsset
        {
            set { textureAsset = value; }
        }
        #endregion

        #region Constructor

        public Entity(Game _game, string _modelName)
            : base(_game)
        {
            this.modelName = _modelName;

            scale = Vector3.One;
            localWorld = Matrix.Identity;
            rotation = Matrix.Identity;
            preRotation = Matrix.Identity;
            position = Vector3.Zero;
            textureAsset = null;
            useTexture = null;
            volume = VolumeType.BoundingBoxes;
            prelocalWorld = Matrix.Identity;

            aditionalLights = new List<Light>();
            worldDirty = true;
        }

        public Entity(Game _game, string _modelName, string _textureAsset)
            : this(_game, _modelName)
        {
            this.textureAsset = _textureAsset;
        }

        public Entity(Game _game, string _modelName, VolumeType volume)
            : this(_game, _modelName)
        {
            this.volume = volume;
        }

        public Entity(Game _game, string _modelName, VolumeType volume, string _textureAsset)
            : this(_game, _modelName)
        {
            this.volume = volume;
            this.textureAsset = _textureAsset;
        }


        #endregion

        #region Initialize

        #endregion

        #region LoadContent

        public override void LoadContent()
        {
            GC.Collect();
            model = PoolGame.content.Load<CustomModel>(modelName);
            
            // Load custom texture
            if (textureAsset != null) useTexture = PoolGame.content.Load<Texture2D>(textureAsset);

            //
            if (normalMapAsset != null) normalMapTexture = PoolGame.content.Load<Texture2D>(normalMapAsset);
            if (heightMapAsset != null) heightMapTexture = PoolGame.content.Load<Texture2D>(heightMapAsset);

            // Setup model.
            textures = model.GetTextures();

            boundingBox = model.GetBoundingBox();
            boundingSphere = new BoundingSphere();
            boxes = new List<BoundingBox>();

            localWorld = preRotation * rotation * Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);
            this.prelocalWorld = localWorld;
            worldDirty = false;

            if (belongsToScenario)
            {
                foreach (CustomModelPart modelPart in model.modelParts)
                {
                    Vector3[] corners = modelPart.AABox.GetCorners();
                    Vector3 pointrotated = Vector3.Transform(corners[0], localWorld);
                    Vector3[] points = new Vector3[2];
                    points[0] = pointrotated;
                    points[1] = pointrotated;

                    for (int k = 1; k < 8; ++k)
                    {
                        pointrotated = Vector3.Transform(corners[k], localWorld);

                        points[0] = Vector3.Min(points[0], pointrotated);
                        points[1] = Vector3.Max(points[1], pointrotated);
                    }

                    boxes.Add(new BoundingBox(points[0], points[1]));
                }
            }
            base.LoadContent();
        }

        #endregion

        #region Update

        protected virtual void updateLocalWorld()
        {
            if (worldDirty)
            {
                bool identity = localWorld == Matrix.Identity;
                localWorld = preRotation * rotation * Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);
                if (identity) this.prelocalWorld = localWorld;

                if (volume == VolumeType.BoundingBoxes)
                {
                    int i = 0;
                    foreach (CustomModelPart modelPart in model.modelParts)
                    {
                        Vector3[] corners = modelPart.AABox.GetCorners();
                        Vector3 pointrotated = Vector3.Transform(corners[0], localWorld);
                        Vector3[] points = new Vector3[2];
                        points[0] = pointrotated;
                        points[1] = pointrotated;

                        for (int k = 1; k < 8; ++k)
                        {
                            pointrotated = Vector3.Transform(corners[k], localWorld);

                            points[0] = Vector3.Min(points[0], pointrotated);
                            points[1] = Vector3.Max(points[1], pointrotated);
                        }

                        boxes[i] = new BoundingBox(points[0], points[1]);
                        ++i;
                    }
                }
                worldDirty = false;
            }
        }
        public override void Update(GameTime gameTime)
        {
            if (World.camera == null) return;
            
            //Matrix proj = Maths.CalcLightProjection(LightManager.lights.Position, LightManager.bounds, PoolGame.device.Viewport);

            base.Update(gameTime);
        }
        #endregion

        #region Draw
        public  override void Draw(GameTime gameTime)
        {
            if (World.camera == null) return;
            RenderMode renderMode = PostProcessManager.currentRenderMode;

            updateLocalWorld();

            switch (renderMode)
            {
                case RenderMode.ShadowMapRender:
                    if (!occluder) return;
                    
                    frustum = LightManager.lights[PostProcessManager.shadows.lightpass].Frustum;
                    DrawModel(false, PostProcessManager.Depth, "DepthMap", delegate { SetParametersShadowMap(LightManager.lights[PostProcessManager.shadows.lightpass]); });

                    break;

                case RenderMode.PCFShadowMapRender:
                    if (!occluder) return;

                    frustum = World.camera.FrustumCulling;
                    DrawModel(false, PostProcessManager.PCFShadowMap, "PCFSMTechnique", delegate { SetParametersPCFShadowMap(LightManager.lights); });
                    break;

                case RenderMode.ScreenSpaceSoftShadowRender:
                    {
                        frustum = World.camera.FrustumCulling;
                        string basicTechnique = "SSSTechnique";

                        if (World.displacementType != DisplacementType.None && this.normalMapAsset != null)
                        {
                            basicTechnique = World.displacementType.ToString() + basicTechnique;
                            
                            PoolGame.device.SamplerStates[2].AddressU = TEXTURE_ADDRESS_MODE;
                            PoolGame.device.SamplerStates[2].AddressV = TEXTURE_ADDRESS_MODE;
                            
                            if (World.displacementType == DisplacementType.ParallaxMapping)
                            {
                                PoolGame.device.SamplerStates[4].AddressU = TEXTURE_ADDRESS_MODE;
                                PoolGame.device.SamplerStates[4].AddressV = TEXTURE_ADDRESS_MODE;
                            }
                        }
                        if (World.motionblurType == MotionBlurType.None && World.dofType == DOFType.None) 
                            basicTechnique = "NoMRT" + basicTechnique;
                        DrawModel(true, PostProcessManager.SSSoftShadow_MRT, basicTechnique, delegate { SetParametersSoftShadowMRT(LightManager.lights); });

                        //DrawModel(true, LightManager.lights, PostProcessManager.SSSoftShadow, "SSSTechnique", delegate { SetParametersSoftShadow(LightManager.lights); });
                    }
                    break;
                case RenderMode.BasicRender:
                    {
                        frustum = World.camera.FrustumCulling;
                        string basicTechnique = "ModelTechnique";
                        if (World.motionblurType == MotionBlurType.None && World.dofType == DOFType.None) basicTechnique = "NoMRT" + basicTechnique;
                        DrawModel(true, PostProcessManager.modelEffect, basicTechnique, delegate { SetParametersModelEffectMRT(LightManager.lights); });
                    }
                    break;
            }

            base.Draw(gameTime);
        }


        /// <summary>
        /// 
        /// </summary>
        public void SetPreviousWorld()
        {
            prelocalWorld = localWorld;
        }


        public virtual void DrawModel(bool enableTexture, Effect effect, string technique, RenderHandler setParameter)
        {
            if (setParameter != null) { setParameter.Invoke(); }

            if (enableTexture)
            {
                PoolGame.device.SamplerStates[0].AddressU = TEXTURE_ADDRESS_MODE;
                PoolGame.device.SamplerStates[0].AddressV = TEXTURE_ADDRESS_MODE;
            }
            effect.CurrentTechnique = effect.Techniques[technique];
            
            bool drawvolume = PostProcessManager.currentRenderMode == RenderMode.ScreenSpaceSoftShadowRender || PostProcessManager.currentRenderMode == RenderMode.BasicRender;
#if !DRAW_BOUNDINGVOLUME
            drawvolume &= drawboundingvolume;
#endif
            int i = 0, j = 0;
            foreach (CustomModelPart modelPart in model.modelParts)
            {
                #region (1) Culling

                if (!ModelPartInFrustumVolume(modelPart, i))
                {
                    ++i;
                    continue;
                }
                
                #endregion

                #region (2) Draw Model Part
                
                if (enableTexture)
                {
                    if (textureAsset != null) effect.Parameters["TexColor"].SetValue(useTexture);
                    else if (textures[i] != null) effect.Parameters["TexColor"].SetValue(textures[i]);
                }
                effect.Parameters["World"].SetValue(localWorld);

                // Set the graphics device to use our vertex declaration,
                // vertex buffer, and index buffer.
                GraphicsDevice device = PoolGame.device;

                device.VertexDeclaration = modelPart.VertexDeclaration;

                device.Vertices[0].SetSource(modelPart.VertexBuffer, 0,
                                             modelPart.VertexStride);

                device.Indices = modelPart.IndexBuffer;

                // Begin the effect, and loop over all the effect passes.
                effect.Begin(SaveStateMode.SaveState);

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();

                    // Draw the geometry.
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                                 0, 0, modelPart.VertexCount,
                                                 0, modelPart.TriangleCount);

                    pass.End();
                }

                effect.End();

                #endregion

                #region (3) Draw Bounding Volume
                if (drawvolume)
                {
                    VectorRenderComponent vectorRenderer = World.poolTable.vectorRenderer;

                    switch (volume)
                    {
                        case VolumeType.BoundingBoxes:
                            vectorRenderer.SetViewProjMatrix(World.camera.ViewProjection);
                            vectorRenderer.SetWorldMatrix(Matrix.Identity);
                            {
                                vectorRenderer.SetColor(Color.Red);
                                vectorRenderer.DrawBoundingBox(boxes[i]);

                            }
                            break;
                        case VolumeType.BoundingSpheres:
                            PoolGame.device.RenderState.DepthBufferEnable = false;
                            PoolGame.device.RenderState.DepthBufferWriteEnable = false;

                            vectorRenderer.SetViewProjMatrix(World.camera.ViewProjection);
                            vectorRenderer.SetWorldMatrix(localWorld);
                            {
                                vectorRenderer.SetColor(Color.Aqua);
                                vectorRenderer.DrawBoundingSphere(modelPart.Sphere);
                            }


                            PoolGame.device.RenderState.DepthBufferEnable = true;
                            PoolGame.device.RenderState.DepthBufferWriteEnable = true;
                            break;
                    }
                }
                #endregion

                ++i; j++;
            }
            if (j != 0) ++World.camera.ItemsDrawn;

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

        public void DrawBoundingBox(BoundingBox box)
        {
            if (PostProcessManager.currentRenderMode != RenderMode.ScreenSpaceSoftShadowRender &&
                PostProcessManager.currentRenderMode != RenderMode.BasicRender) return;

            VectorRenderComponent vectorRenderer = World.poolTable.vectorRenderer;

            vectorRenderer.SetViewProjMatrix(World.camera.ViewProjection);
            vectorRenderer.SetWorldMatrix(Matrix.Identity);
            
            {
                vectorRenderer.SetColor(Color.Red);

                
                Vector3 _min = Vector3.Transform(box.Min, localWorld);
                Vector3 _max = Vector3.Transform(box.Max, localWorld);


                Vector3 min = Vector3.Min(_min, _max);
                Vector3 max = Vector3.Max(_min, _max);

                
                
                vectorRenderer.DrawBoundingBox(new BoundingBox(min, max));
                                
            }
        }
        public bool ModelPartInFrustumVolume(CustomModelPart modelPart, int i)
        {
            if (World.camera.EnableFrustumCulling && frustum != null)
            {
                switch (volume)
                {
                    case VolumeType.BoundingBoxes:

                        if (frustum.Contains(boxes[i]) == ContainmentType.Disjoint)
                            return false;
                        
                        break;
                    case VolumeType.BoundingSpheres:
                        if (frustum.Contains(new BoundingSphere(Vector3.Transform(modelPart.Sphere.Center, localWorld), modelPart.Sphere.Radius * Math.Max(scale.X, Math.Max(scale.Y, scale.Z)))) == ContainmentType.Disjoint)
                            return false;
                        
                        break;
                }
            }
            return true;
        }
        public bool ModelInFrustumVolume()
        {
            if (World.camera.EnableFrustumCulling)
            {
                switch (volume)
                {
                    case VolumeType.BoundingBoxes:
                        Vector3[] corners = boundingBox.GetCorners();

                        Vector3 pointrotated = Vector3.Transform(corners[0], localWorld);
                        Vector3 min = pointrotated;
                        Vector3 max = pointrotated;

                        for (int k = 1; k < 8; ++k)
                        {
                            pointrotated = Vector3.Transform(corners[k], localWorld);

                            min = Vector3.Min(min, pointrotated);
                            max = Vector3.Max(max, pointrotated);
                        }
                        //World.camera.fc.CalcuteFrustum();
                        if (frustum.Contains(new BoundingBox(min, max)) == ContainmentType.Disjoint)
                        //if (World.camera.fc.OBoxInFrustum(box.Min, box.Max) == FrustumTest.Outside)
                            return false;
                        break;
                    case VolumeType.BoundingSpheres:
                        if (frustum.Contains(new BoundingSphere(Vector3.Transform(boundingSphere.Center, localWorld), boundingSphere.Radius * Math.Max(scale.X, Math.Max(scale.Y, scale.Z)))) == ContainmentType.Disjoint)
                            return false;
                        break;
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
                if (World.camera.fc.boxInFrustum(min, max) == FrustumTest.Outside)
                    return false;
            }*/

            
            return true;
        }
        


        #endregion

        #region Set Parameters for Basic Render

        public void SetParametersModelEffectMRT(List<Light> lights)
        {
            PostProcessManager.modelEffect.Parameters["MaxDepth"].SetValue(World.camera.FarPlane);
            PostProcessManager.modelEffect.Parameters["View"].SetValue(World.camera.View);
            PostProcessManager.modelEffect.Parameters["PrevWorldViewProj"].SetValue(this.prelocalWorld * World.camera.PrevView * World.camera.Projection);
            PostProcessManager.modelEffect.Parameters["totalLights"].SetValue(LightManager.totalLights);

            PostProcessManager.modelEffect.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);
            PostProcessManager.modelEffect.Parameters["LightPosition"].SetValue(LightManager.positions);
            PostProcessManager.modelEffect.Parameters["vAmbient"].SetValue(World.scenario.AmbientColor);
            PostProcessManager.modelEffect.Parameters["vDiffuseColor"].SetValue(LightManager.diffuse);
            PostProcessManager.modelEffect.Parameters["materialDiffuseColor"].SetValue(materialDiffuseColor);

            PostProcessManager.modelEffect.Parameters["aditionalLights"].SetValue(aditionalLights.Count);
            if (aditionalLights.Count > 0)
            {
                PostProcessManager.modelEffect.Parameters["vaditionalLightColor"].SetValue(lightscolors);
                PostProcessManager.modelEffect.Parameters["vaditionalLightPositions"].SetValue(lightspositions);
                PostProcessManager.modelEffect.Parameters["vaditionalLightRadius"].SetValue(lightsradius);
            }
            if (this.specularColor.X == 0.0f && this.specularColor.Y == 0.0f && this.specularColor.Z == 0.0f) PostProcessManager.modelEffect.Parameters["vSpecularColor"].SetValue(LightManager.nospecular);
            else PostProcessManager.modelEffect.Parameters["vSpecularColor"].SetValue(LightManager.specular);
            

            PostProcessManager.modelEffect.Parameters["Shineness"].SetValue(shineness);

            PostProcessManager.modelEffect.Parameters["CameraPosition"].SetValue(new Vector4(World.camera.CameraPosition.X, World.camera.CameraPosition.Y, World.camera.CameraPosition.Z, 0.0f));

        }

        public void SetParametersModelEffect(Light light)
        {
            //PostProcessManager.modelEffect.Parameters["View"].SetValue(World.camera.View);
            //PostProcessManager.modelEffect.Parameters["PrevWorldViewProj"].SetValue(this.prelocalWorld * World.camera.PrevView * World.camera.Projection);
            PostProcessManager.modelEffect.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);
            PostProcessManager.modelEffect.Parameters["LightPosition"].SetValue(new Vector4(light.Position.X, light.Position.Y, light.Position.Z, 1.0f));

            PostProcessManager.modelEffect.Parameters["vSpecularColor"].SetValue(specularColor);
            PostProcessManager.modelEffect.Parameters["Shineness"].SetValue(shineness);

            PostProcessManager.modelEffect.Parameters["CameraPosition"].SetValue(new Vector4(World.camera.CameraPosition.X, World.camera.CameraPosition.Y, World.camera.CameraPosition.Z, 0.0f));
            
        }

        #endregion

        #region Set Parameters for Shadow

        public void SetParametersSoftShadowMRT(List<Light> lights)
        {
            PostProcessManager.SSSoftShadow_MRT.Parameters["MaxDepth"].SetValue(World.camera.FarPlane);
            PostProcessManager.SSSoftShadow_MRT.Parameters["View"].SetValue(World.camera.View);
            PostProcessManager.SSSoftShadow_MRT.Parameters["PrevWorldViewProj"].SetValue(this.prelocalWorld * World.camera.PrevView * World.camera.Projection);
            PostProcessManager.SSSoftShadow_MRT.Parameters["totalLights"].SetValue(LightManager.totalLights);

            PostProcessManager.SSSoftShadow_MRT.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);
            PostProcessManager.SSSoftShadow_MRT.Parameters["LightPosition"].SetValue(LightManager.positions);
            PostProcessManager.SSSoftShadow_MRT.Parameters["vAmbient"].SetValue(World.scenario.AmbientColor);
            PostProcessManager.SSSoftShadow_MRT.Parameters["vDiffuseColor"].SetValue(LightManager.diffuse);
            PostProcessManager.SSSoftShadow_MRT.Parameters["materialDiffuseColor"].SetValue(materialDiffuseColor);
            PostProcessManager.SSSoftShadow_MRT.Parameters["aditionalLights"].SetValue(aditionalLights.Count);
            if (aditionalLights.Count > 0)
            {
                PostProcessManager.SSSoftShadow_MRT.Parameters["vaditionalLightColor"].SetValue(lightscolors);
                PostProcessManager.SSSoftShadow_MRT.Parameters["vaditionalLightPositions"].SetValue(lightspositions);
                PostProcessManager.SSSoftShadow_MRT.Parameters["vaditionalLightRadius"].SetValue(lightsradius);
                PostProcessManager.SSSoftShadow_MRT.Parameters["vaditionalLightType"].SetValue(lightstype);
            }
            if (this.specularColor.X == 0.0f && this.specularColor.Y == 0.0f && this.specularColor.Z == 0.0f) PostProcessManager.SSSoftShadow_MRT.Parameters["vSpecularColor"].SetValue(LightManager.nospecular);
            else PostProcessManager.SSSoftShadow_MRT.Parameters["vSpecularColor"].SetValue(LightManager.specular);

            if (PostProcessManager.shadowBlurTech == ShadowBlurTechnnique.SoftShadow)
                PostProcessManager.SSSoftShadow_MRT.Parameters["TexBlurV"].SetValue(PostProcessManager.GBlurVRT.GetTexture());
            else
                PostProcessManager.SSSoftShadow_MRT.Parameters["TexBlurV"].SetValue(PostProcessManager.shadows.ShadowRT.GetTexture());

            PostProcessManager.SSSoftShadow_MRT.Parameters["Shineness"].SetValue(shineness);

            PostProcessManager.SSSoftShadow_MRT.Parameters["CameraPosition"].SetValue(new Vector4(World.camera.CameraPosition.X, World.camera.CameraPosition.Y, World.camera.CameraPosition.Z, 0.0f));

            if (World.displacementType != DisplacementType.None)
            {
                PostProcessManager.SSSoftShadow_MRT.Parameters["NormalMap"].SetValue(normalMapTexture);
                if (World.displacementType == DisplacementType.ParallaxMapping)
                {
                    PostProcessManager.SSSoftShadow_MRT.Parameters["parallaxscaleBias"].SetValue(World.scaleBias);
                    PostProcessManager.SSSoftShadow_MRT.Parameters["HeightMap"].SetValue(heightMapTexture);
                }
            }
        }
        public void SetParametersSoftShadow(Light light)
        {

            PostProcessManager.SSSoftShadow.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);
            PostProcessManager.SSSoftShadow.Parameters["LightViewProj"].SetValue(light.LightViewProjection);

            if (PostProcessManager.shadowBlurTech == ShadowBlurTechnnique.SoftShadow)
                PostProcessManager.SSSoftShadow.Parameters["TexBlurV"].SetValue(PostProcessManager.GBlurVRT.GetTexture());
            else
                PostProcessManager.SSSoftShadow.Parameters["TexBlurV"].SetValue(PostProcessManager.shadows.ShadowRT.GetTexture());


            PostProcessManager.SSSoftShadow.Parameters["LightPosition"].SetValue(new Vector4(light.Position.X, light.Position.Y, light.Position.Z, 1.0f));

            PostProcessManager.SSSoftShadow.Parameters["vSpecularColor"].SetValue(specularColor);
            PostProcessManager.SSSoftShadow.Parameters["Shineness"].SetValue(shineness);

            PostProcessManager.SSSoftShadow.Parameters["CameraPosition"].SetValue(new Vector4(World.camera.CameraPosition.X, World.camera.CameraPosition.Y, World.camera.CameraPosition.Z, 0.0f));

        }

        public void SetParametersPCFShadowMap(List<Light> lights)
        {
            PostProcessManager.PCFShadowMap.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);


            PostProcessManager.PCFShadowMap.Parameters["LightViewProjs"].SetValue(LightManager.viewprojections);
            PostProcessManager.PCFShadowMap.Parameters["MaxDepths"].SetValue(LightManager.maxdepths);
            PostProcessManager.PCFShadowMap.Parameters["totalLights"].SetValue(LightManager.totalLights);
            for (int i = 0; i < LightManager.totalLights; ++i)
            {
                PostProcessManager.PCFShadowMap.Parameters["ShadowMap" + i].SetValue(PostProcessManager.shadows.ShadowMapRT[i].GetTexture());   
            }
            
            PostProcessManager.PCFShadowMap.Parameters["PCFSamples"].SetValue(PostProcessManager.shadows.pcfSamples);
            PostProcessManager.PCFShadowMap.Parameters["depthBias"].SetValue(LightManager.depthbias);
        }

        public void SetParametersShadowMap(Light light)
        {
            PostProcessManager.Depth.Parameters["ViewProj"].SetValue(light.LightViewProjection);
            PostProcessManager.Depth.Parameters["MaxDepth"].SetValue(light.LightFarPlane);
        }

        public void AddLight(Light light)
        {
            aditionalLights.Add(light);
            Array.Resize(ref lightsradius, aditionalLights.Count);
            Array.Resize(ref lightscolors, aditionalLights.Count);
            Array.Resize(ref lightspositions, aditionalLights.Count);
            Array.Resize(ref lightstype, aditionalLights.Count);

            UpdateLightsProperties();
        }

        public void UpdateLightsProperties()
        {
            for (int i = 0; i < aditionalLights.Count; ++i)
            {
                lightsradius[i] = aditionalLights[i].Radius;
                lightscolors[i] = aditionalLights[i].DiffuseColor;
                lightspositions[i] = new Vector4(aditionalLights[i].Position, 1.0f);
                lightstype[i] = (int)aditionalLights[i].LightType;
            }
        }

        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ModelManager.AbortAllThreads();
                PoolGame.game.Components.Remove(this);
                model = null;
                if (boxes != null) boxes.Clear();
                boxes = null;
                modelName = null;
                textureAsset = null;
                if (textures != null) textures.Clear();
                textures = null;
                if (normalMapTexture != null) normalMapTexture.Dispose();
                normalMapTexture = null;

                lightsradius = null;
                lightscolors = null;
                lightspositions = null;
                lightstype = null;
            }
            base.Dispose(disposing);
        }
        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            Entity b1 = (Entity)obj;
            if (DrawOrder > b1.DrawOrder)
                return 1;
            else if (DrawOrder < b1.DrawOrder)
                return -1;
            return 0;

        }

        #endregion
        
    }

    
}
