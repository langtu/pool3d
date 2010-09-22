#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Helpers;
#endregion

namespace XNA_PoolGame.Graphics.Models
{
    /// <summary>
    /// Volumetric Light Entity.
    /// </summary>
    public class VolumetricLightEntity : DrawableComponent
    {
        #region Variables
        private string modelName;
        private CustomModel model;
        private string alphachannelAsset = null;
        private BoundingFrustum frustum;

        protected string textureAsset = null;
        protected Texture2D useTexture;
        protected Texture2D alphaChannelTexture;

        /// <summary>
        /// List of textures
        /// </summary>
        protected List<Texture2D> textures;
        public TextureAddressMode TEXTURE_ADDRESS_MODE = TextureAddressMode.Clamp;

        // Alpha blending settings.
        public Blend SourceBlend = Blend.SourceAlpha; // color de la textura del modelo
        public Blend DestinationBlend = Blend.One;


        private Matrix prelocalWorld;
        private Matrix localWorld;
        private Matrix rotation;
        private Matrix preRotation;
        private Vector3 position;
        private Vector3 scale;
        private bool worldDirty;
        private CullMode cullmode;

        protected VolumeType volume;
        private bool useModelPartBB = true;
        BoundingBox boundingBox;
        BoundingSphere boundingSphere;
        protected BoundingBox[] boxes;
        protected bool drawboundingvolume = false;

        /// <summary>
        /// Render parameters delegate
        /// </summary>
        public delegate void RenderHandler();

        #endregion

        #region Properties
        public Vector3 Position
        {
            get { return position; }
            set { position = value; worldDirty = true; }
        }

        public Matrix Rotation
        {
            get { return rotation; }
            set { worldDirty = true; rotation = value; }
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

        public bool UseModelPartBB
        {
            get { return useModelPartBB; }
            set { useModelPartBB = value; }
        }

        public CullMode Cull
        {
            get { return cullmode; }
            set { cullmode = value; }
        }


        #endregion

        #region Constructors
        public VolumetricLightEntity(Game _game, string _modelName)
            : base(_game)
        {
            this.modelName = _modelName;

            scale = Vector3.One;
            localWorld = Matrix.Identity;
            rotation = Matrix.Identity;
            preRotation = Matrix.Identity;
            position = Vector3.Zero;
            volume = VolumeType.BoundingBoxes;
            prelocalWorld = Matrix.Identity;

            cullmode = CullMode.CullClockwiseFace;
            worldDirty = true;
        }
        public VolumetricLightEntity(Game _game, string _modelName, string _alphachannelAsset)
            : base(_game)
        {
            this.modelName = _modelName;
            this.alphachannelAsset = _alphachannelAsset;

            scale = Vector3.One;
            localWorld = Matrix.Identity;
            rotation = Matrix.Identity;
            preRotation = Matrix.Identity;
            position = Vector3.Zero;
            volume = VolumeType.BoundingBoxes;
            prelocalWorld = Matrix.Identity;

            cullmode = CullMode.CullClockwiseFace;
            worldDirty = true;
        }

        #endregion

        #region LoadContent
        public override void LoadContent()
        {
            GC.Collect();
            model = PoolGame.content.Load<CustomModel>(modelName);
            if (alphachannelAsset != null) alphaChannelTexture = PoolGame.content.Load<Texture2D>(alphachannelAsset);

            // Setup model.
            textures = model.GetDiffuseTextures();

            boundingBox = model.GetBoundingBox();
            boundingSphere = new BoundingSphere();
            boxes = new BoundingBox[model.modelParts.Count];

            localWorld = preRotation * rotation * Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);
            this.prelocalWorld = localWorld;
            worldDirty = false;

            //if (belongsToScenario)
            {
                int boxindex = 0;
                Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
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

                    min = Vector3.Min(min, points[0]);
                    max = Vector3.Max(max, points[1]);
                    boxes[boxindex] = new BoundingBox(points[0], points[1]);
                    ++boxindex;
                }
                boundingBox.Min = min;
                boundingBox.Max = max;
            }
            base.LoadContent();
        }
        #endregion

        #region Draw
        public override void Draw(GameTime gameTime)
        {
            if (World.camera == null) return;
            RenderMode renderMode = PostProcessManager.currentRenderMode;

            updateLocalWorld();

            switch (renderMode)
            {
                case RenderMode.RenderGBuffer:
                case RenderMode.ScreenSpaceSoftShadowRender:
                    {
                        frustum = World.camera.FrustumCulling;
                        string basicTechnique = PostProcessManager.shading.GetBasicRenderTechnique();

                        // Set the alpha blend mode.
                        PoolGame.device.RenderState.AlphaBlendEnable = true;
                        PoolGame.device.RenderState.AlphaBlendOperation = BlendFunction.Add;
                        
                        PoolGame.device.RenderState.SourceBlend = SourceBlend; // color de la textura del modelo
                        PoolGame.device.RenderState.DestinationBlend = DestinationBlend; // color del framebuffer
                        PoolGame.device.RenderState.BlendFunction = BlendFunction.Add;

                        // Set the alpha test mode.
                        //PoolGame.device.RenderState.AlphaTestEnable = true;
                        //PoolGame.device.RenderState.AlphaFunction = CompareFunction.Greater;
                        //PoolGame.device.RenderState.ReferenceAlpha = 0;

                        CullMode oldCullMode = PoolGame.device.RenderState.CullMode;
                        PoolGame.device.RenderState.CullMode = cullmode;

                        bool depthbufferWrite_old = PoolGame.device.RenderState.DepthBufferWriteEnable;
                        PoolGame.device.RenderState.DepthBufferEnable = true;
                        PoolGame.device.RenderState.DepthBufferWriteEnable = false;

                        DrawModel(true, PostProcessManager.shaftsEffect, basicTechnique, delegate { SetParametersVolumetricLightModel(); });


                        PoolGame.device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
                        //DrawModel(true, PostProcessManager.shaftsEffect, basicTechnique, delegate { SetParametersVolumetricLightModel(); });

                        PoolGame.device.RenderState.CullMode = oldCullMode;

                        PoolGame.device.RenderState.AlphaTestEnable = false;
                        PoolGame.device.RenderState.AlphaBlendEnable = false;
                        PoolGame.device.RenderState.DepthBufferWriteEnable = depthbufferWrite_old;
                        PoolGame.device.RenderState.DepthBufferEnable = true;
                    }
                    break;
            }
        }

        private void updateLocalWorld()
        {
            if (worldDirty)
            {
                bool identity = localWorld == Matrix.Identity;
                localWorld = preRotation * rotation * Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);
                if (identity) this.prelocalWorld = localWorld;

                if (useModelPartBB && volume == VolumeType.BoundingBoxes)
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

                        boxes[i].Min = points[0];
                        boxes[i].Max = points[1];

                        ++i;
                    }
                }
                else if (!useModelPartBB && volume == VolumeType.BoundingBoxes)
                {
                    Vector3[] corners = model.GetBoundingBox().GetCorners();
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
                worldDirty = false;
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
            if (World.camera.EnableFrustumCulling && frustum != null)
            {
                switch (volume)
                {
                    case VolumeType.BoundingBoxes:
                        if (frustum.Contains(boundingBox) == ContainmentType.Disjoint)
                            return false;
                        break;
                    case VolumeType.BoundingSpheres:
                        if (frustum.Contains(new BoundingSphere(Vector3.Transform(boundingSphere.Center, localWorld), boundingSphere.Radius * Math.Max(scale.X, Math.Max(scale.Y, scale.Z)))) == ContainmentType.Disjoint)
                            return false;
                        break;
                }
            }

            return true;
        }

        private void DrawModel(bool enableTexture, Effect effect, string technique, RenderHandler setParameter)
        {
            if (setParameter != null) { setParameter.Invoke(); }

            if (enableTexture)
            {
                PoolGame.device.SamplerStates[0].AddressU = TEXTURE_ADDRESS_MODE;
                PoolGame.device.SamplerStates[0].AddressV = TEXTURE_ADDRESS_MODE;
            }
            effect.CurrentTechnique = effect.Techniques[technique];

            bool drawvolume = PostProcessManager.currentRenderMode == RenderMode.ScreenSpaceSoftShadowRender || PostProcessManager.currentRenderMode == RenderMode.BasicRender || PostProcessManager.currentRenderMode == RenderMode.RenderGBuffer;
#if !DRAW_BOUNDINGVOLUME
            drawvolume &= drawboundingvolume;
#endif
            //effect.CommitChanges();
            int i = 0, j = 0;

            if (!UseModelPartBB && !ModelInFrustumVolume()) return;

            foreach (CustomModelPart modelPart in model.modelParts)
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

        #endregion

        #region Set Parameters for Volumetric Light model
        public void SetParametersVolumetricLightModel()
        {
            PostProcessManager.shaftsEffect.Parameters["World"].SetValue(localWorld);
            PostProcessManager.shaftsEffect.Parameters["View"].SetValue(World.camera.View);
            PostProcessManager.shaftsEffect.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);
            
            PostProcessManager.shaftsEffect.Parameters["CameraPosition"].SetValue(World.camera.CameraPosition);

            PostProcessManager.shaftsEffect.Parameters["LightColor"].SetValue(new Color(240, 220, 0, 255).ToVector3());
            
            if (alphaChannelTexture != null) 
                PostProcessManager.shaftsEffect.Parameters["AlphaMap"].SetValue(alphaChannelTexture);

        }
        #endregion
    }
}
