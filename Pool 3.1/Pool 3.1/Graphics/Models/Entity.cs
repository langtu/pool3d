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
using ModelPart = XNA_PoolGame.Graphics.Models.CustomModel.ModelPart;
#endregion

namespace XNA_PoolGame.Graphics.Models
{
    /// <summary>
    /// The basic model (no animation).
    /// </summary>
    public class Entity : DrawableComponent
    {
        #region Variables

        private CustomModel model = null;
        private String modelName = null;
        protected String textureAsset = null;
        
        #region //// MATERIAL PROPERTIES ////

        /// <summary>
        /// Choose what Texture display for this model.
        /// </summary>
        protected Texture2D useTexture = null;

        public TextureAddressMode TEXTURE_ADDRESS_MODE = TextureAddressMode.Clamp;

        public bool occluder = true;

        private float shineness = 96.0f;
        private Vector4 specularColor = Vector4.One;
        public String normalMapAsset = null;
        protected Texture2D normalMapTexture = null;

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
        
        public bool isObjectAtScenario = true;

        #endregion

        #region //// THREADS RELATED ////

        

        

        #endregion

        #region //// MODEL RELATED ////

        private Matrix localWorld;
        private Vector3 position;
        private Matrix rotation;
        private Matrix preRotation;
        private Vector3 scale;
        private List<Texture2D> textures;
        private List<BoundingBox> boxes;
        private Matrix prelocalWorld;
        public bool isMotionBlurred;

        private bool worldDirty = true;

        #endregion

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
        #endregion

        #region Constructor

        public Entity(Game _game, String _modelName)
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

            isMotionBlurred = false;
            worldDirty = true;
        }

        public Entity(Game _game, String _modelName, String _textureAsset)
            : this(_game, _modelName)
        {
            this.textureAsset = _textureAsset;
        }

        public Entity(Game _game, String _modelName, VolumeType volume)
            : this(_game, _modelName)
        {
            this.volume = volume;
        }

        public Entity(Game _game, String _modelName, VolumeType volume, String _textureAsset)
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
        
        public override void LoadContent()
        {
            GC.Collect();
            model = PoolGame.content.Load<CustomModel>(modelName);
                        
            // Load custom texture
            if (textureAsset != null) useTexture = PoolGame.content.Load<Texture2D>(textureAsset);

            //
            if (normalMapAsset != null) normalMapTexture = PoolGame.content.Load<Texture2D>(normalMapAsset);

            // Setup model.
            textures = model.GetTextures();

            boundingBox = model.GetBoundingBox();
            boundingSphere = new BoundingSphere();
            boxes = new List<BoundingBox>();
            localWorld = preRotation * rotation * Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);
            worldDirty = false;
            if (isObjectAtScenario)
            {
                foreach (ModelPart modelPart in model.modelParts)
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

        private void updateLocalWorld()
        {
            if (worldDirty)
            {
                bool identity = localWorld == Matrix.Identity;
                localWorld = preRotation * rotation * Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);
                if (identity) this.prelocalWorld = localWorld;

                if (volume == VolumeType.BoundingBoxes)
                {
                    int i = 0;
                    foreach (ModelPart modelPart in model.modelParts)
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
                    {
                        frustum = LightManager.lights[World.lightpass].Frustum;
                        DrawModel(false, PostProcessManager.Depth, "DepthMap", delegate { SetParametersShadowMap(LightManager.lights[World.lightpass]); });
                    }
                    
                    break;

                case RenderMode.PCFShadowMapRender:
                    frustum = World.camera.FrustumCulling;
                    DrawModel(false, PostProcessManager.PCFShadowMap, "PCFSMTechnique", delegate { SetParametersPCFShadowMap(LightManager.lights); });
                    break;

                case RenderMode.ScreenSpaceSoftShadowRender:
                    frustum = World.camera.FrustumCulling;
                    String basicTechnique = "SSSTechnique";

                    if (World.displacementType != DisplacementType.None && this.normalMapAsset != null)
                    {
                        basicTechnique = World.displacementType.ToString() + basicTechnique;
                        PoolGame.device.Textures[3] = normalMapTexture;
                        PoolGame.device.SamplerStates[3].AddressU = TEXTURE_ADDRESS_MODE;
                        PoolGame.device.SamplerStates[3].AddressV = TEXTURE_ADDRESS_MODE;
                    }
                    DrawModel(true, PostProcessManager.SSSoftShadow_MRT, basicTechnique, delegate { SetParametersSoftShadowMRT(LightManager.lights); });
                    
                    //DrawModel(true, LightManager.lights, PostProcessManager.SSSoftShadow, "SSSTechnique", delegate { SetParametersSoftShadow(LightManager.lights); });
                    break;

                case RenderMode.DoF:
                    frustum = World.camera.FrustumCulling;
                    DrawModel(false, PostProcessManager.Depth, "DepthMap", delegate { SetParametersDoFMap(); });
                    break;

                case RenderMode.MotionBlur:
                    
                    break;
                case RenderMode.BasicRender:
                    frustum = World.camera.FrustumCulling;
                    DrawModel(true, PostProcessManager.modelEffect, "ModelTechnique", delegate { SetParametersModelEffectMRT(LightManager.lights); });
                    
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


        public void DrawModel(bool enableTexture, Effect effect, String technique, RenderHandler setParameter)
        {
            if (setParameter != null) setParameter.Invoke();

            if (enableTexture)
            {
                PoolGame.device.SamplerStates[0].AddressU = TEXTURE_ADDRESS_MODE;
                PoolGame.device.SamplerStates[0].AddressV = TEXTURE_ADDRESS_MODE;
            }
            effect.CurrentTechnique = effect.Techniques[technique];
            #region Old
            /*foreach (ModelMesh mesh in model.Meshes)
            {
                if (!ModelInFrustumVolume(bboxes[index++])) continue;
                
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
                drawn++;
            }*/
            #endregion

            bool drawvolume = PostProcessManager.currentRenderMode == RenderMode.ScreenSpaceSoftShadowRender || PostProcessManager.currentRenderMode == RenderMode.BasicRender;
#if !DRAW_BOUNDINGVOLUME
            drawvolume = false;
#endif
            int i = 0, j = 0;
            foreach (ModelPart modelPart in model.modelParts)
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
        public bool ModelPartInFrustumVolume(ModelPart modelPart, int i)
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
        public void BasicDraw()
        {
            BasicEffect basicEffect = PostProcessManager.basicEffect;

            basicEffect.View = World.camera.View;
            basicEffect.Projection = World.camera.Projection;

            basicEffect.EnableDefaultLighting();
            basicEffect.PreferPerPixelLighting = true;
            Vector3 light0Direction = Vector3.Normalize(-LightManager.lights[0].Position);
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

            /*foreach (ModelMesh mesh in model.Meshes)
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
                
            }*/

            
        }


        #endregion

        #region Set Parameters for Basic Render

        public void SetParametersModelEffectMRT(List<Light> lights)
        {
            PostProcessManager.modelEffect.Parameters["MaxDepth"].SetValue(World.camera.FarPlane);
            PostProcessManager.modelEffect.Parameters["View"].SetValue(World.camera.View);
            PostProcessManager.modelEffect.Parameters["PrevWorldViewProj"].SetValue(this.prelocalWorld * World.camera.PrevView * World.camera.Projection);

            PostProcessManager.modelEffect.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);
            for (int i = 0; i < lights.Count; ++i)
            {
                PostProcessManager.modelEffect.Parameters["LightPosition"].SetValue(new Vector4(lights[i].Position.X, lights[i].Position.Y, lights[i].Position.Z, 1.0f));
            }

            PostProcessManager.modelEffect.Parameters["vSpecularColor"].SetValue(specularColor);
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

            PostProcessManager.SSSoftShadow_MRT.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);
            for (int i = 0; i < LightManager.totalLights; ++i)
            {
                PostProcessManager.SSSoftShadow_MRT.Parameters["LightViewProj"].SetValue(lights[i].LightViewProjection);
                PostProcessManager.SSSoftShadow_MRT.Parameters["LightPosition"].SetValue(new Vector4(lights[i].Position.X, lights[i].Position.Y, lights[i].Position.Z, 1.0f));
            }

            if (PostProcessManager.shadowBlurTech == ShadowBlurTechnnique.SoftShadow)
                PostProcessManager.SSSoftShadow_MRT.Parameters["TexBlurV"].SetValue(PostProcessManager.GBlurVRT.GetTexture());
            else
                PostProcessManager.SSSoftShadow_MRT.Parameters["TexBlurV"].SetValue(PostProcessManager.shadows.ShadowRT.GetTexture());


            

            PostProcessManager.SSSoftShadow_MRT.Parameters["vSpecularColor"].SetValue(specularColor);
            PostProcessManager.SSSoftShadow_MRT.Parameters["Shineness"].SetValue(shineness);

            PostProcessManager.SSSoftShadow_MRT.Parameters["CameraPosition"].SetValue(new Vector4(World.camera.CameraPosition.X, World.camera.CameraPosition.Y, World.camera.CameraPosition.Z, 0.0f));

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
            for (int i = 0; i < LightManager.totalLights; ++i)
            {
                PostProcessManager.PCFShadowMap.Parameters["LightViewProj" + i].SetValue(lights[i].LightViewProjection);
                PostProcessManager.PCFShadowMap.Parameters["ShadowMap" + i].SetValue(PostProcessManager.shadows.ShadowMapRT[i].GetTexture());
                PostProcessManager.PCFShadowMap.Parameters["MaxDepth" + i].SetValue(lights[i].LightFarPlane);
            }
            
            PostProcessManager.PCFShadowMap.Parameters["PCFSamples"].SetValue(PostProcessManager.shadows.pcfSamples);
            PostProcessManager.PCFShadowMap.Parameters["depthBias"].SetValue(PostProcessManager.shadows.depthBias);
        }

        public void SetParametersShadowMap(Light light)
        {
            {
                PostProcessManager.Depth.Parameters["ViewProj"].SetValue(light.LightViewProjection);
            }
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
            if (disposing)
            {
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

                
            }
            base.Dispose(disposing);
        }
        #endregion

        #region IDraw Members

        

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
