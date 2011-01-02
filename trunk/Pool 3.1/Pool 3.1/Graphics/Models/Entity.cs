﻿//#define DRAW_BOUNDINGVOLUME

#region Using Statements
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
using XNA_PoolGame.Cameras;
using System.IO;
using Microsoft.Xna.Framework.Content;
#endregion

namespace XNA_PoolGame.Graphics.Models
{
    public enum VolumeType
    {
        BoundingBoxes,
        BoundingSpheres
    }

    public enum EnvironmentType
    {
        None,
        Dynamic,
        Static,
        DualParaboloid
    }

    public enum DisplacementType
    {
        None,
        NormalMapping,
        ParallaxMapping
    }

    /// <summary>
    /// The basic model (without animation).
    /// </summary>
    public class Entity : DrawableComponent
    {
        #region Variables

        /// <summary>
        /// 3D object. (1st level detail)
        /// </summary>
        protected CustomModel modelL1 = null;
        protected string modelNameL1 = null;
        protected string textureAsset = null;
        protected bool drawboundingvolume = false;
        protected Color bvColor = Color.Red;

        public bool isScatterObject = false;
        /// <summary>
        /// 3D object. (2nd level detail)
        /// </summary>
        protected CustomModel modelL2 = null;
        protected string modelNameL2 = null;

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

        /// <summary>
        /// List of textures
        /// </summary>
        protected List<Texture2D> textures;

        private float shineness = 96.0f;
        private Vector4 specularColor = Vector4.One;
        private Vector4 materialDiffuseColor = Vector4.One;
        public string customnormalMapAsset = null;
        public string customheightMapAsset = null;
        public string customssaoMapAsset = null;
        public string customspecularMapAsset = null;
        
        protected Texture2D customnormalMapTexture = null;
        protected Texture2D customheightMapTexture = null;
        protected Texture2D customssaoMapTexture = null;
        protected Texture2D customspecularMapTexture = null;
        public List<Texture2D> normalMapTextures;
        public List<Texture2D> heightMapTextures;
        public List<Texture2D> ssaoMapTextures;
        public List<Texture2D> specularMapTextures;

        private List<Light> aditionalLights;
        private float[] lightsradius;
        private Vector4[] lightscolors;
        private Vector4[] lightspositions;
        private int[] lightstype;

        private bool useNormalMapTextures = false;
        private bool useHeightMapTextures = false;
        private bool useSSAOMapTextures = false;
        private bool useSpecularMapTextures = false;
        private EnvironmentType emType = EnvironmentType.None;
        public RenderTargetCube refCubeMap = null;
        public TextureCube environmentMap;
        public static DepthStencilBuffer DEMdepthstencil;
        public RenderTarget2D mDPMapFront, mDPMapBack;
        #endregion

        #region //// FRUSTUM CULLING ////

        /// <summary>
        /// What frustum volume use? Camera or Light source Frustum.
        /// </summary>
        protected BoundingFrustum frustum;
        protected BoundingFrustum frustum2;

        /// <summary>
        /// Type of volume
        /// </summary>
        protected VolumeType volume;
        BoundingBox boundingBox;
        BoundingSphere boundingSphere;
        protected BoundingBox[] boxes;
        protected BoundingSphere[] spheres;
        protected bool useModelPartBB = true;
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
        protected Matrix prelocalWorld;

        private bool worldDirty = true;

        #endregion

        /// <summary>
        /// Render parameters delegate
        /// </summary>
        public delegate void RenderHandler();

        #endregion

        #region Properties

        public BoundingBox[] Boxes
        {
            get { return boxes; }
        }
        public BoundingSphere BoundingSphere
        {
            get { return boundingSphere; }
        }
        public bool UseModelPartBB
        {
            get { return useModelPartBB; }
            set { useModelPartBB = value; }
        }
        
        public Vector3 Position
        {
            get { return position; }
            set { worldDirty = true; position = value; }
        }
        public Matrix LocalWorld
        {
            get { return localWorld; }
        }
        public CustomModel ModelL1
        {
            get { return modelL1; }
        }

        public CustomModel ModelL2
        {
            get { return modelNameL2 == null ? null : modelL2; }
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

        public bool UseNormalMapTextures
        {
            get { return useNormalMapTextures; }
            set { useNormalMapTextures = value; customnormalMapAsset = null; }
        }

        public bool UseSpecularMapTextures
        {
            get { return useSpecularMapTextures; }
            set { useSpecularMapTextures = value; customspecularMapAsset = null; }
        }

        public bool UseHeightMapTextures
        {
            get { return useHeightMapTextures; }
            set { useHeightMapTextures = value; }
        }

        public bool UseSSAOMapTextures
        {
            get { return useSSAOMapTextures; }
            set { useSSAOMapTextures = value; }
        }


        public EnvironmentType EMType
        {
            get { return emType; }
            set { emType = value; }
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

        #region Constructors

        public Entity(Game _game, string _modelNameL1)
            : base(_game)
        {
            this.modelNameL1 = _modelNameL1;

            scale = Vector3.One;
            localWorld = Matrix.Identity;
            rotation = Matrix.Identity;
            preRotation = Matrix.Identity;
            position = Vector3.Zero;
            volume = VolumeType.BoundingBoxes;
            prelocalWorld = Matrix.Identity;

            aditionalLights = new List<Light>();
            normalMapTextures = new List<Texture2D>();
            heightMapTextures = new List<Texture2D>();
            ssaoMapTextures = new List<Texture2D>();
            specularMapTextures = new List<Texture2D>();
            worldDirty = true;
        }

        public Entity(Game _game, string _modelNameL1, bool loadLOD2)
            : this(_game, _modelNameL1)
        {
            if (loadLOD2) this.modelNameL2 = this.modelNameL1 + "_LOD2";
        }

        public Entity(Game _game, string _modelNameL1, string _textureAsset)
            : this(_game, _modelNameL1)
        {
            this.textureAsset = _textureAsset;
        }

        public Entity(Game _game, string _modelNameL1, bool loadLOD2, string _textureAsset)
            : this(_game, _modelNameL1)
        {
            this.textureAsset = _textureAsset;
            if (loadLOD2) this.modelNameL2 = this.modelNameL1 + "_LOD2";
        }

        public Entity(Game _game, string _modelNameL1, VolumeType volume)
            : this(_game, _modelNameL1)
        {
            this.volume = volume;
        }

        public Entity(Game _game, string _modelNameL1, VolumeType volume, string _textureAsset)
            : this(_game, _modelNameL1)
        {
            this.volume = volume;
            this.textureAsset = _textureAsset;
        }
        public Entity(Game _game, string _modelNameL1, bool loadLOD2, VolumeType volume, string _textureAsset)
            : this(_game, _modelNameL1)
        {
            this.volume = volume;
            this.textureAsset = _textureAsset;
            if (loadLOD2) this.modelNameL2 = this.modelNameL1 + "_LOD2";
        }

        #endregion

        #region LoadContent

        public override void LoadContent()
        {
            GC.Collect();
            modelL1 = PoolGame.content.Load<CustomModel>(modelNameL1);

            //if (modelNameL2 != null)
            {
                if (EMType == EnvironmentType.Dynamic || EMType == EnvironmentType.DualParaboloid)
                {
                    World.scenario.dems_basicrender.Add(this);
                    World.scenario.dems.Add(this);
                }
                if (modelNameL2 != null)
                    modelL2 = PoolGame.content.Load<CustomModel>(modelNameL2);
                
            }

            if (EMType == EnvironmentType.Dynamic && refCubeMap == null && World.EM == EnvironmentType.Dynamic)
            {
                DEMdepthstencil = new DepthStencilBuffer(PoolGame.device, PoolGame.Width, PoolGame.Height, PoolGame.device.PresentationParameters.AutoDepthStencilFormat);
                refCubeMap = new RenderTargetCube(PoolGame.device, World.EMSize, 1, PoolGame.device.PresentationParameters.BackBufferFormat);
            }
            else if (EMType == EnvironmentType.DualParaboloid && World.EM == EnvironmentType.DualParaboloid)
            {
                mDPMapFront = new RenderTarget2D(PoolGame.device, PoolGame.Width / World.DPDivisor, PoolGame.Height / World.DPDivisor, 1, SurfaceFormat.Color);
                mDPMapBack = new RenderTarget2D(PoolGame.device, PoolGame.Width / World.DPDivisor, PoolGame.Height / World.DPDivisor, 1, SurfaceFormat.Color);
            }

            // Load custom texture
            if (textureAsset != null) useTexture = PoolGame.content.Load<Texture2D>(textureAsset);

            //
            List<string> baseTexturesFileNames = modelL1.GetDiffuseTexturesFilename();

            if (customnormalMapAsset != null) customnormalMapTexture = PoolGame.content.Load<Texture2D>(customnormalMapAsset);
            else if (UseNormalMapTextures)
            {
                foreach (string baseasset in baseTexturesFileNames)
                {
                    
                    string normalasset = baseasset + "N";

                    Texture2D texLoaded = null;
                    try
                    {
                        texLoaded = PoolGame.content.Load<Texture2D>(normalasset);
                    }
                    catch (ContentLoadException conexception)
                    {
                        texLoaded = PostProcessManager.normalMapNull;
                    }
                    finally
                    {
                        normalMapTextures.Add(texLoaded);
                    }
                }
            }
            if (customheightMapAsset != null) customheightMapTexture = PoolGame.content.Load<Texture2D>(customheightMapAsset);
            else if (UseHeightMapTextures)
            {
                foreach (string baseasset in baseTexturesFileNames)
                {
                    string heightasset = baseasset + "H";
                    Texture2D texLoaded = null;
                    try
                    {
                        texLoaded = PoolGame.content.Load<Texture2D>(heightasset);
                    }
                    catch (ContentLoadException conexception)
                    {
                        texLoaded = PostProcessManager.whiteTexture;
                    }
                    finally
                    {
                        heightMapTextures.Add(texLoaded);
                    }
                }
            }
            if (customssaoMapAsset != null) customssaoMapTexture = PoolGame.content.Load<Texture2D>(customssaoMapAsset);
            else if (useSSAOMapTextures)
            {
                foreach (string baseasset in baseTexturesFileNames)
                {
                    string ssaoasset = baseasset + "AO";
                    Texture2D texLoaded = null;
                    try
                    {
                        texLoaded = PoolGame.content.Load<Texture2D>(ssaoasset);
                    }
                    catch (ContentLoadException conexception)
                    {
                        texLoaded = PostProcessManager.whiteTexture;
                    }
                    finally
                    {
                        ssaoMapTextures.Add(texLoaded);
                    }
                }
            }

            if (customspecularMapAsset != null) customspecularMapTexture = PoolGame.content.Load<Texture2D>(customspecularMapAsset);
            else if (UseSpecularMapTextures)
            {
                foreach (string baseasset in baseTexturesFileNames)
                {
                    string specularasset = baseasset + "S";
                    Texture2D texLoaded = null;
                    try
                    {
                        texLoaded = PoolGame.content.Load<Texture2D>(specularasset);
                    }
                    catch (ContentLoadException conexception)
                    {
                        texLoaded = PostProcessManager.whiteTexture;
                    }
                    finally
                    {
                        specularMapTextures.Add(texLoaded);
                    }
                }
            }
            // Setup model.
            textures = modelL1.GetDiffuseTextures();

            boundingBox = modelL1.GetBoundingBox();
            boundingSphere = new BoundingSphere();
            if (volume== VolumeType.BoundingBoxes)
                boxes = new BoundingBox[modelL1.modelParts.Count];
            else
                spheres = new BoundingSphere[modelL1.modelParts.Count];

            localWorld = preRotation * rotation * Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);
            this.prelocalWorld = localWorld;
            worldDirty = false;

            if (belongsToScenario)
            {
                int modelPartIndex = 0;
                Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                Vector3 diff;
                foreach (CustomModelPart modelPart in modelL1.modelParts)
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

                    min = Vector3.Min(min, points[0]);
                    max = Vector3.Max(max, points[1]);
                    if (volume == VolumeType.BoundingBoxes)
                        boxes[modelPartIndex] = new BoundingBox(points[0], points[1]);
                    else
                    {
                        spheres[modelPartIndex] = new BoundingSphere();
                        diff = max - min;
                        spheres[modelPartIndex].Center = (max + min) / 2.0f;
                        spheres[modelPartIndex].Radius = Math.Max(diff.X, Math.Max(diff.Y, diff.Z));
                    }
                    ++modelPartIndex;
                }
                boundingBox.Min = min;
                boundingBox.Max = max;
                diff = max - min;
                boundingSphere.Center = (max + min) / 2.0f;
                boundingSphere.Radius = Math.Max(diff.X, Math.Max(diff.Y, diff.Z));
            }
            base.LoadContent();
        }

        #endregion

        #region Update

        protected virtual void UpdateLocalWorld()
        {
            if (worldDirty)
            {
                bool identity = localWorld == Matrix.Identity;
                localWorld = preRotation * rotation * Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);
                if (identity) this.prelocalWorld = localWorld;

                if (useModelPartBB && volume == VolumeType.BoundingBoxes)
                {
                    int i = 0;
                    foreach (CustomModelPart modelPart in modelL1.modelParts)
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
                        
                        boxes[i].Min = points[0];
                        boxes[i].Max = points[1];
                        
                        ++i;
                    }
                }
                else if (useModelPartBB && volume == VolumeType.BoundingSpheres)
                {
                    int i = 0;
                    foreach (CustomModelPart modelPart in modelL1.modelParts)
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

                        Vector3 diff = points[1] - points[0];
                        spheres[i].Center = position;
                        spheres[i].Radius = Math.Max(diff.X, Math.Max(diff.Y, diff.Z));
                        ++i;
                    }
                }
                else if (!useModelPartBB && volume == VolumeType.BoundingBoxes)
                {
                    Vector3[] corners = modelL1.GetBoundingBox().GetCorners();
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

                    boundingBox.Min = points[0];
                    boundingBox.Max = points[1];
                }
                else if (!useModelPartBB && volume == VolumeType.BoundingSpheres)
                {
                    boundingSphere.Center = position;
                }
                worldDirty = false;
            }
        }
        public override void Update(GameTime gameTime)
        {
            if (World.camera == null) return;
            
            //Matrix proj = Maths.CalcLightProjection(LightManager.lights.Position, LightManager.bounds, PoolGame.device.Viewport);

            UpdateLocalWorld();
            base.Update(gameTime);
        }
        #endregion

        #region Draw
        public override void Draw(GameTime gameTime)
        {
            if (World.camera == null) return;
            RenderPassMode renderMode = PostProcessManager.currentRenderMode;

            frustum2 = null;
            switch (renderMode)
            {
                case RenderPassMode.ShadowMapRender:
                    #region Depth Map
                    {
                        //if (!occluder) return;

                        //frustum = World.camera.Frustum;
                        //frustum2 = LightManager.lights[PostProcessManager.shading.Shadows.lightpass].Frustum;
                        frustum = LightManager.lights[PostProcessManager.shading.Shadows.lightpass].Frustum;
                        DrawModel(false, PostProcessManager.DepthEffect, PostProcessManager.shading.Shadows.GetDepthMapTechnique(), null);
                    }
                    #endregion
                    break;
                case RenderPassMode.CubeShadowMapPass:
                    #region Cube Shadow Map
                    {
                        //if (!occluder) return;

                        frustum = ((CubeShadowMapping)PostProcessManager.shading.Shadows).CurrentFrustum;
                        DrawModel(false, PostProcessManager.DepthEffect, PostProcessManager.shading.Shadows.GetDepthMapTechnique(), null);
                    }
                    #endregion
                    break;
                case RenderPassMode.PCFShadowMapRender:
                    #region PCF Shadow Render
                    {
                        frustum = World.camera.Frustum;
                        if (World.shadowTechnique == ShadowTechnnique.VarianceShadowMapping)
                            DrawModel(false, PostProcessManager.VSMEffect, "PCFSMTechnique", null);
                        else if (World.shadowTechnique == ShadowTechnnique.CubeShadowMapping)
                            DrawModel(false, PostProcessManager.PCFShadowMap, "cubicShadowMapping", null);
                        else
                            DrawModel(false, PostProcessManager.PCFShadowMap, "PCFSMTechnique", null);
                    }
                    #endregion
                    break;
                case RenderPassMode.ScreenSpaceSoftShadowRender:
                    #region Screen Space Soft Shadow
                    {
                        frustum = World.camera.Frustum;
                        string basicTechnique = "SSSTechnique";

                        if (World.displacementType != DisplacementType.None && (this.customnormalMapAsset != null || useNormalMapTextures))
                            basicTechnique = World.displacementType.ToString() + basicTechnique;

                        bool DEM = this.EMType != EnvironmentType.None && this.EMType != EnvironmentType.DualParaboloid && World.EM != EnvironmentType.None && World.EM != EnvironmentType.DualParaboloid;
                        if (DEM) basicTechnique = "EM" + basicTechnique;
                        if (World.motionblurType == MotionBlurType.None && World.dofType == DOFType.None) basicTechnique = "NoMRT" + basicTechnique;
                        
                        DrawModel(true, PostProcessManager.SSSoftShadow_MRT, basicTechnique, delegate { SetParametersSoftShadowMRT(ref LightManager.lights); });
                    }
                    #endregion
                    break;
                case RenderPassMode.SSAOPrePass:
                    #region SSAO prepass
                    {
                        frustum = World.camera.Frustum;
                        DrawModel(false, PostProcessManager.SSAOPrePassEffect, "SSAO", delegate { SetParametersSSAO(); });
                    }
                    #endregion
                    break;
                case RenderPassMode.DEMBasicRender:
                    #region Dynamic Environment Mapping Basic Render
                    {
                        frustum = World.camera.Frustum;
                        string basicTechnique = "ModelTechnique";
                        //if (World.motionblurType == MotionBlurType.None && World.dofType == DOFType.None) 
                            basicTechnique = "NoMRT" + basicTechnique;
                        CustomModel tmpmodel = modelL1;
                        if (modelL2 != null) modelL1 = modelL2;
                        DrawModel(true, PostProcessManager.modelEffect, basicTechnique, delegate { SetParametersModelEffectMRT(ref LightManager.lights); });
                        modelL1 = tmpmodel;
                    }
                    #endregion
                    break;
                case RenderPassMode.BasicRender:
                    {
                        frustum = World.camera.Frustum;
                        string basicTechnique = PostProcessManager.shading.GetBasicRenderTechnique();
                        //if (World.motionblurType == MotionBlurType.None && World.dofType == DOFType.None) basicTechnique = "NoMRT" + basicTechnique;

                        DrawModel(true, PostProcessManager.modelEffect, basicTechnique, delegate { SetParametersModelEffectMRT(ref LightManager.lights); });
                    }
                    break;
                case RenderPassMode.RenderGBufferPass:
                    #region Render G-Buffer
                    {
                        frustum = World.camera.Frustum;
                        string basicTechnique = PostProcessManager.shading.GetBasicRenderTechnique();

                        if (World.displacementType == DisplacementType.ParallaxMapping && (this.customheightMapAsset != null || useHeightMapTextures))
                            basicTechnique = "ParallaxMapping" + basicTechnique;

                        if (World.doLightshafts) basicTechnique += "LightShafts";
                        DrawModel(true, PostProcessManager.renderGBuffer_DefEffect, basicTechnique, delegate { SetParameterRenderGBuffer(); });
                    }
                    #endregion
                    break;
                case RenderPassMode.DPBasicRender:
                    {
                        frustum = World.camera.Frustum;
                        string basicTechnique = "DPModelTechnique";
                        //if (World.motionblurType == MotionBlurType.None && World.dofType == DOFType.None) 
                            basicTechnique = "NoMRT" + basicTechnique;
                        CustomModel tmpmodel = modelL1;
                        if (modelL2 != null) modelL1 = modelL2;
                        DrawModel(true, PostProcessManager.modelEffect, basicTechnique, delegate { SetParametersDPEffect(); });
                        modelL1 = tmpmodel;
                    }
                    break;
                case RenderPassMode.DualParaboloidRenderMapsPass:
                    #region Dual Paraboloid Maps
                    {
                        
                    }
                    #endregion
                    break;

                case RenderPassMode.DEMPass:
                    #region Dynamic Environment Mapping
                    {
                        if (EMType == EnvironmentType.Dynamic)
                        {
                            PostProcessManager.ChangeRenderMode(RenderPassMode.DEMBasicRender);
                            Camera oldCamera = World.camera;

                            World.camera = World.emptycamera;

                            this.Visible = false;

                            World.emptycamera.CameraPosition = this.position;
                            World.emptycamera.Projection = oldCamera.Projection;

                            DepthStencilBuffer oldBuffer = PoolGame.device.DepthStencilBuffer;
                            // Render our cube map, once for each cube face (6 times).
                            for (int i = 0; i < 6; i++)
                            {
                                // Render the scene to all cubemap faces.
                                CubeMapFace cubeMapFace = (CubeMapFace)i;

                                #region Cube Faces
                                switch (cubeMapFace)
                                {
                                    case CubeMapFace.NegativeX:
                                        {
                                            World.emptycamera.View = Matrix.CreateLookAt(this.position, this.position + Vector3.Left, Vector3.Up);
                                            break;
                                        }
                                    case CubeMapFace.NegativeY:
                                        {
                                            World.emptycamera.View = Matrix.CreateLookAt(this.position, this.position + Vector3.Down, Vector3.Forward);
                                            break;
                                        }
                                    case CubeMapFace.NegativeZ:
                                        {
                                            World.emptycamera.View = Matrix.CreateLookAt(this.position, this.position + Vector3.Forward, Vector3.Up);
                                            break;
                                        }
                                    case CubeMapFace.PositiveX:
                                        {
                                            World.emptycamera.View = Matrix.CreateLookAt(this.position, this.position + Vector3.Right, Vector3.Up);
                                            break;
                                        }
                                    case CubeMapFace.PositiveY:
                                        {
                                            World.emptycamera.View = Matrix.CreateLookAt(this.position, this.position + Vector3.Up, Vector3.Backward);
                                            break;
                                        }
                                    case CubeMapFace.PositiveZ:
                                        {
                                            World.emptycamera.View = Matrix.CreateLookAt(this.position, this.position + Vector3.Backward, Vector3.Up);
                                            break;
                                        }
                                }
                                #endregion

                                PoolGame.device.SetRenderTarget(0, refCubeMap, cubeMapFace);
                                PoolGame.device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, Color.White, 1.0f, 0);
                                World.emptycamera.ViewProjection = World.emptycamera.View * World.emptycamera.Projection;
                                World.scenario.DrawDEMBasicRenderObjects(gameTime);
                            }
                            PoolGame.device.DepthStencilBuffer = oldBuffer;
                            PoolGame.device.SetRenderTarget(0, null);
                            environmentMap = refCubeMap.GetTexture();

                            World.camera = oldCamera;
                            PostProcessManager.ChangeRenderMode(RenderPassMode.DEMPass);
                            this.Visible = true;
                        }
                    }
                    #endregion
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


        protected virtual void DrawModel(bool enableTexture, Effect effect, string technique, RenderHandler setParameter)
        {
            if (setParameter != null) { setParameter.Invoke(); }

            if (enableTexture)
            {
                PoolGame.device.SamplerStates[0].AddressU = TEXTURE_ADDRESS_MODE;
                PoolGame.device.SamplerStates[0].AddressV = TEXTURE_ADDRESS_MODE;
            }
            effect.CurrentTechnique = effect.Techniques[technique];

            bool drawvolume = PostProcessManager.isFinalSceneRenderMode();
#if !DRAW_BOUNDINGVOLUME
            drawvolume &= drawboundingvolume;
#endif

            int i = 0, j = 0;

            if (!UseModelPartBB && !ModelInFrustumVolume()) return;

            foreach (CustomModelPart modelPart in modelL1.modelParts)
            {
                #region (1) Culling

                if (UseModelPartBB && !ModelPartInFrustumVolume(modelPart, i))
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

                    if (World.displacementType != DisplacementType.None && useNormalMapTextures)
                    {
                        effect.Parameters["NormalMap"].SetValue(normalMapTextures[i]);
                        if (World.displacementType == DisplacementType.ParallaxMapping && UseHeightMapTextures)
                            effect.Parameters["HeightMap"].SetValue(heightMapTextures[i]);
                    }

                    if (useSSAOMapTextures)
                    {
                        if (World.useSSAOTextures) effect.Parameters["SSAOMap"].SetValue(ssaoMapTextures[i]);
                        else effect.Parameters["SSAOMap"].SetValue(PostProcessManager.whiteTexture);
                    }

                    /*if (useSpecularMapTextures)
                    {
                        effect.Parameters["SpecularMap"].SetValue(specularMapTextures[i]);
                    }
                    else effect.Parameters["SpecularMap"].SetValue(PostProcessManager.whiteTexture);*/
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
                                vectorRenderer.SetColor(bvColor);
                                vectorRenderer.DrawBoundingBox(boxes[i]);

                            }
                            break;
                        case VolumeType.BoundingSpheres:
                            PoolGame.device.RenderState.DepthBufferEnable = false;
                            PoolGame.device.RenderState.DepthBufferWriteEnable = false;

                            vectorRenderer.SetViewProjMatrix(World.camera.ViewProjection);
                            vectorRenderer.SetWorldMatrix(localWorld);
                            {
                                vectorRenderer.SetColor(bvColor);
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
            if (j != 0 && PostProcessManager.isFinalSceneRenderMode()) ++World.camera.ItemsDrawn;

        }

        public void DrawBoundingSphere()
        {
            if (!PostProcessManager.isFinalSceneRenderMode()) return;

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
            if (!PostProcessManager.isFinalSceneRenderMode()) return;

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
                if (frustum2 != null)
                {
                    switch (volume)
                    {
                        case VolumeType.BoundingBoxes:
                            if (frustum.Contains(boxes[i]) == ContainmentType.Disjoint &&
                                frustum2.Contains(boxes[i]) == ContainmentType.Disjoint)
                                return false;

                            break;
                        case VolumeType.BoundingSpheres:
                            if (frustum.Contains(spheres[i]) == ContainmentType.Disjoint &&
                                frustum2.Contains(spheres[i]) == ContainmentType.Disjoint)
                                return false;

                            break;
                    }
                }
                else
                {

                    switch (volume)
                    {
                        case VolumeType.BoundingBoxes:
                            if (frustum.Contains(boxes[i]) == ContainmentType.Disjoint)
                                return false;

                            break;
                        case VolumeType.BoundingSpheres:
                            if (frustum.Contains(spheres[i]) == ContainmentType.Disjoint)
                                return false;

                            break;
                    }
                }
            }
            return true;
        }
        public bool ModelInFrustumVolume()
        {
            if (World.camera.EnableFrustumCulling && frustum != null)
            {
                if (frustum2 != null)
                {
                    switch (volume)
                    {
                        case VolumeType.BoundingBoxes:
                            if (frustum.Contains(boundingBox) == ContainmentType.Disjoint ||
                                frustum2.Contains(boundingBox) == ContainmentType.Disjoint)
                                return false;
                            break;
                        case VolumeType.BoundingSpheres:
                            if (frustum.Contains(boundingSphere) == ContainmentType.Disjoint ||
                                frustum2.Contains(boundingBox) == ContainmentType.Disjoint)
                                return false;
                            break;
                    }
                }
                else
                {
                    switch (volume)
                    {
                        case VolumeType.BoundingBoxes:
                            if (frustum.Contains(boundingBox) == ContainmentType.Disjoint)
                                return false;
                            break;
                        case VolumeType.BoundingSpheres:
                            if (frustum.Contains(boundingSphere) == ContainmentType.Disjoint)
                                return false;
                            break;
                    }
                }
            }
            
            return true;
        }
        


        #endregion

        #region Set Parameters for RenderGBuffer

        private void SetParameterRenderGBuffer()
        {
            bool DEM = this.EMType != EnvironmentType.None && this.EMType != EnvironmentType.DualParaboloid && World.EM != EnvironmentType.None && World.EM != EnvironmentType.DualParaboloid;
            PostProcessManager.renderGBuffer_DefEffect.Parameters["bDEM"].SetValue(DEM);
            if (DEM)
                PostProcessManager.renderGBuffer_DefEffect.Parameters["EnvironmentMap"].SetValue(environmentMap);
            
            PostProcessManager.renderGBuffer_DefEffect.Parameters["isScatterObject"].SetValue(isScatterObject);

            if ((this.customnormalMapAsset != null || useNormalMapTextures) && World.displacementType != DisplacementType.None)
            {
                PoolGame.device.SamplerStates[2].AddressU = TEXTURE_ADDRESS_MODE;
                PoolGame.device.SamplerStates[2].AddressV = TEXTURE_ADDRESS_MODE;

                if (customnormalMapAsset != null) PostProcessManager.renderGBuffer_DefEffect.Parameters["NormalMap"].SetValue(customnormalMapTexture);

                if (World.displacementType == DisplacementType.ParallaxMapping && (this.customheightMapAsset != null || useHeightMapTextures))
                {
                    PoolGame.device.SamplerStates[3].AddressU = TEXTURE_ADDRESS_MODE; // Height Map
                    PoolGame.device.SamplerStates[3].AddressV = TEXTURE_ADDRESS_MODE;

                    PostProcessManager.renderGBuffer_DefEffect.Parameters["parallaxscaleBias"].SetValue(World.scaleBias);
                    if (customheightMapAsset != null) PostProcessManager.renderGBuffer_DefEffect.Parameters["HeightMap"].SetValue(customheightMapTexture);
                }

            }
            else
                PostProcessManager.renderGBuffer_DefEffect.Parameters["NormalMap"].SetValue(PostProcessManager.normalMapNull);

            PostProcessManager.renderGBuffer_DefEffect.Parameters["SpecularMap"].SetValue(PostProcessManager.specularMapNull);
        }

        #endregion

        #region Set Parameters for Basic Render

        public void SetParametersDPEffect()
        {
            Matrix worldIT = Matrix.Invert(localWorld);
            worldIT = Matrix.Transpose(worldIT);
            PostProcessManager.modelEffect.Parameters["WorldInvTrans"].SetValue(worldIT);
            //PostProcessManager.modelEffect.Parameters["NearPlane"].SetValue(World.camera.NearPlane);
            //PostProcessManager.modelEffect.Parameters["FarPlane"].SetValue(World.camera.FarPlane);
            PostProcessManager.modelEffect.Parameters["WorldViewProj"].SetValue(localWorld * World.camera.ViewProjection);
            
            SetParametersModelEffectMRT(ref LightManager.lights);
        }

        public void SetParametersModelEffectMRT(ref List<Light> lights)
        {
            PostProcessManager.modelEffect.Parameters["MaxDepth"].SetValue(World.camera.FarPlane);
            PostProcessManager.modelEffect.Parameters["View"].SetValue(World.camera.View);
            PostProcessManager.modelEffect.Parameters["PrevWorldViewProj"].SetValue(this.prelocalWorld * World.camera.PreviousView * World.camera.Projection);
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

        #region Set Parameters for SSAO
        public void SetParametersSSAO()
        {
            PostProcessManager.SSAOPrePassEffect.Parameters["View"].SetValue(World.camera.View);
            PostProcessManager.SSAOPrePassEffect.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);
            PostProcessManager.SSAOPrePassEffect.Parameters["CameraPosition"].SetValue(new Vector4(World.camera.CameraPosition.X, World.camera.CameraPosition.Y, World.camera.CameraPosition.Z, 0.0f));

            /*if (World.displacementType != DisplacementType.None)
            {
                if (customnormalMapAsset != null) PostProcessManager.SSSoftShadow_MRT.Parameters["NormalMap"].SetValue(customnormalMapTexture);
                if (World.displacementType == DisplacementType.ParallaxMapping)
                {
                    PostProcessManager.SSSoftShadow_MRT.Parameters["parallaxscaleBias"].SetValue(World.scaleBias);
                    if (customheightMapAsset != null) PostProcessManager.SSSoftShadow_MRT.Parameters["HeightMap"].SetValue(customheightMapTexture);
                }
            }*/
        }
        #endregion

        #region Set Parameters for Shadow

        public void SetParametersSoftShadowMRT(ref List<Light> lights)
        {
            PostProcessManager.SSSoftShadow_MRT.Parameters["MaxDepth"].SetValue(World.camera.FarPlane);
            PostProcessManager.SSSoftShadow_MRT.Parameters["View"].SetValue(World.camera.View);
            PostProcessManager.SSSoftShadow_MRT.Parameters["PrevWorldViewProj"].SetValue(this.prelocalWorld * World.camera.PreviousView * World.camera.Projection);
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

            // Shadow Occlussion
            PostProcessManager.SSSoftShadow_MRT.Parameters["TexBlurV"].SetValue(PostProcessManager.shading.Shadows.shadowOcclussionTIU.renderTarget.GetTexture());

            PostProcessManager.SSSoftShadow_MRT.Parameters["Shineness"].SetValue(shineness);

            PostProcessManager.SSSoftShadow_MRT.Parameters["CameraPosition"].SetValue(new Vector4(World.camera.CameraPosition.X, World.camera.CameraPosition.Y, World.camera.CameraPosition.Z, 0.0f));

            if (World.displacementType != DisplacementType.None && (this.customnormalMapAsset != null || useNormalMapTextures))
            {
                PoolGame.device.SamplerStates[2].AddressU = TEXTURE_ADDRESS_MODE; // Normal Map
                PoolGame.device.SamplerStates[2].AddressV = TEXTURE_ADDRESS_MODE;

                if (World.displacementType == DisplacementType.ParallaxMapping && (this.customheightMapAsset != null || useHeightMapTextures))
                {
                    PoolGame.device.SamplerStates[3].AddressU = TEXTURE_ADDRESS_MODE; // Height Map
                    PoolGame.device.SamplerStates[3].AddressV = TEXTURE_ADDRESS_MODE;

                    PostProcessManager.SSSoftShadow_MRT.Parameters["parallaxscaleBias"].SetValue(World.scaleBias);
                    if (customheightMapAsset != null) PostProcessManager.SSSoftShadow_MRT.Parameters["HeightMap"].SetValue(customheightMapTexture);
                }

                if (customnormalMapAsset != null) PostProcessManager.SSSoftShadow_MRT.Parameters["NormalMap"].SetValue(customnormalMapTexture);
            }

            PoolGame.device.SamplerStates[4].AddressU = TEXTURE_ADDRESS_MODE; // SSAO map
            PoolGame.device.SamplerStates[4].AddressV = TEXTURE_ADDRESS_MODE;
            PoolGame.device.SamplerStates[5].AddressU = TEXTURE_ADDRESS_MODE; // Specular
            PoolGame.device.SamplerStates[5].AddressV = TEXTURE_ADDRESS_MODE; 

            if (!useSSAOMapTextures)
            {
                if (customssaoMapAsset != null && World.useSSAOTextures)
                    PostProcessManager.SSSoftShadow_MRT.Parameters["SSAOMap"].SetValue(customssaoMapTexture);
                else
                    PostProcessManager.SSSoftShadow_MRT.Parameters["SSAOMap"].SetValue(PostProcessManager.whiteTexture);
            }
            bool DEM = this.EMType != EnvironmentType.None && this.EMType != EnvironmentType.DualParaboloid && World.EM != EnvironmentType.None && World.EM != EnvironmentType.DualParaboloid;
            if (DEM)
            {
                PostProcessManager.SSSoftShadow_MRT.Parameters["EnvironmentMap"].SetValue(environmentMap);
            }
        }
        public void SetParametersSoftShadow(Light light)
        {

            PostProcessManager.SSSoftShadow.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);
            PostProcessManager.SSSoftShadow.Parameters["LightViewProj"].SetValue(light.LightViewProjection);

            if (PostProcessManager.shadowBlurTech == ShadowBlurTechnnique.SoftShadow)
                PostProcessManager.SSSoftShadow.Parameters["TexBlurV"].SetValue(PostProcessManager.GBlurVRT.GetTexture());
            else
                PostProcessManager.SSSoftShadow.Parameters["TexBlurV"].SetValue(PostProcessManager.shading.Shadows.ShadowRT.GetTexture());


            PostProcessManager.SSSoftShadow.Parameters["LightPosition"].SetValue(new Vector4(light.Position.X, light.Position.Y, light.Position.Z, 1.0f));

            PostProcessManager.SSSoftShadow.Parameters["vSpecularColor"].SetValue(specularColor);
            PostProcessManager.SSSoftShadow.Parameters["Shineness"].SetValue(shineness);

            PostProcessManager.SSSoftShadow.Parameters["CameraPosition"].SetValue(new Vector4(World.camera.CameraPosition.X, World.camera.CameraPosition.Y, World.camera.CameraPosition.Z, 0.0f));

        }

        public void SetParametersPCFShadowMap(ref List<Light> lights)
        {
            PostProcessManager.PCFShadowMap.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);


            PostProcessManager.PCFShadowMap.Parameters["LightViewProjs"].SetValue(LightManager.viewprojections);
            PostProcessManager.PCFShadowMap.Parameters["MaxDepths"].SetValue(LightManager.maxdepths);
            PostProcessManager.PCFShadowMap.Parameters["totalLights"].SetValue(LightManager.totalLights);
            for (int i = 0; i < LightManager.totalLights; ++i)
            {
                PostProcessManager.PCFShadowMap.Parameters["ShadowMap" + i].SetValue(PostProcessManager.shading.Shadows.ShadowMapRT[i].GetTexture());   
            }

            PostProcessManager.PCFShadowMap.Parameters["PCFSamples"].SetValue(PostProcessManager.shading.Shadows.pcfSamples);
            PostProcessManager.PCFShadowMap.Parameters["depthBias"].SetValue(LightManager.depthbias);
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
                //ModelManager.AbortAllThreads();
                
                PoolGame.game.Components.Remove(this);
                modelL1 = null; modelL2 = null;

                boxes = null;
                modelNameL1 = null;
                textureAsset = null;
                if (textures != null) textures.Clear();
                textures = null;
                if (customnormalMapTexture != null) customnormalMapTexture.Dispose();
                customnormalMapTexture = null;

                if (customheightMapTexture != null) customheightMapTexture.Dispose();
                customheightMapTexture = null;

                if (customssaoMapTexture != null) customssaoMapTexture.Dispose();
                customssaoMapTexture = null;

                if (refCubeMap != null) refCubeMap.Dispose();
                refCubeMap = null;

                if (mDPMapFront != null) mDPMapFront.Dispose();
                if (mDPMapBack != null) mDPMapFront.Dispose();

                mDPMapFront = mDPMapBack = null;
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
