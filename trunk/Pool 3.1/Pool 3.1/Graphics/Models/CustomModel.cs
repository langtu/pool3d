using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNA_PoolGame.Helpers;

namespace XNA_PoolGame.Graphics.Models
{
    /// <summary>
    /// Custom model loaded from the pipeline.
    /// </summary>
    public class CustomModel
    {
        #region Fields

        // Disable compiler warning that we never initialize these fields.
        // That's ok, because the XNB deserializer initialises them for us!
#pragma warning disable 649


        // Internally our custom model is made up from a list of model parts.
        [ContentSerializer]
        public List<CustomModelPart> modelParts;

        // Each model part represents a piece of geometry that uses one
        // single effect. Multiple parts are needed for models that use
        // more than one effect.
        // See CustomModelPart..

        // Keep track of what graphics device we are using.
        GraphicsDevice graphicsDevice;

#pragma warning restore 649

        #endregion

        /// <summary>
        /// Private constructor, for use by the XNB deserializer.
        /// </summary>
        private CustomModel()
        {
        }

        /// <summary>
        /// Initializes the instancing data.
        /// </summary>
        public void Initialize(GraphicsDevice device)
        {
            graphicsDevice = device;
            foreach (CustomModelPart modelPart in modelParts)
            {
                BasicEffect be = (BasicEffect)modelPart.Effect;
                modelPart.Initialize(device, be.Texture);
            }
            // Choose the best available instancing technique.
            // For default is NoInstancing
            InstancingTechnique technique = InstancingTechnique.NoInstancing;

            SetInstancingTechnique(technique);
        }

        /// <summary>
        /// Initializes the instancing data.
        /// </summary>
        public void Initialize(GraphicsDevice device, InstancingTechnique instancetech)
        {
            graphicsDevice = device;
            foreach (CustomModelPart modelPart in modelParts)
            {
                BasicEffect be = (BasicEffect)modelPart.Effect;
                modelPart.Initialize(device, be.Texture);
            }
            
            SetInstancingTechnique(instancetech);
        }

        #region Technique Selection


        /// <summary>
        /// Gets the current instancing technique.
        /// </summary>
        public InstancingTechnique InstancingTechnique
        {
            get { return instancingTechnique; }
        }

        InstancingTechnique instancingTechnique;


        /// <summary>
        /// Changes which instancing technique we are using.
        /// </summary>
        public void SetInstancingTechnique(InstancingTechnique technique)
        {
            instancingTechnique = technique;

            foreach (CustomModelPart modelPart in modelParts)
            {
                modelPart.SetInstancingTechnique(technique);
            }
        }


        /// <summary>
        /// Checks whether the specified instancing technique
        /// is supported by the current graphics device.
        /// </summary>
        public bool IsTechniqueSupported(InstancingTechnique technique)
        {
#if !XBOX360
            // Hardware instancing is only supported on pixel shader 3.0 devices.
            if (technique == InstancingTechnique.HardwareInstancing)
            {
                return graphicsDevice.GraphicsDeviceCapabilities
                                     .PixelShaderVersion.Major >= 3;
            }
#endif

            // Otherwise, everything is good.
            return true;
        }


        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Texture2D> GetTextures()
        {
            List<Texture2D> list = new List<Texture2D>();
            foreach (CustomModelPart modelPart in modelParts)
            {
                BasicEffect be = (BasicEffect)modelPart.Effect;
                list.Add(be.Texture);
            }
            return list;
        }

        public BoundingBox GetBoundingBox()
        {
            BoundingBox box = new BoundingBox();
            foreach (CustomModelPart modelPart in modelParts)
            {
                box = BoundingBox.CreateMerged(box, modelPart.AABox);
            }
            return box;
        }

        /// <summary>
        /// Draws a batch of instanced models.
        /// </summary>
        public void DrawInstances(Matrix[] instanceTransforms,
                                  Matrix view, Matrix projection, Effect customEffect)
        {
            if (graphicsDevice == null)
            {
                throw new InvalidOperationException(
                    "InstanceModel.Initialize must be called before DrawInstances.");
            }

            if (instanceTransforms.Length == 0)
                return;

            foreach (CustomModelPart modelPart in modelParts)
            {
                modelPart.Draw(instancingTechnique, instanceTransforms,
                               view, projection, customEffect, null);
            }
        }

        /// <summary>
        /// Draws a batch of instanced models.
        /// </summary>
        public void DrawInstances(Matrix[] instanceTransforms,
                                  Matrix view, Matrix projection, Effect customEffect, Texture2D anotherTexture)
        {
            if (graphicsDevice == null)
            {
                throw new InvalidOperationException(
                    "InstanceModel.Initialize must be called before DrawInstances.");
            }

            if (instanceTransforms.Length == 0)
                return;

            foreach (CustomModelPart modelPart in modelParts)
            {
                modelPart.Draw(instancingTechnique, instanceTransforms,
                               view, projection, customEffect, anotherTexture);
            }
        }

        /// <summary>
        /// Draws the model using the specified camera matrices.
        /// </summary>
        public void Draw(Matrix world, Vector3 scale, Effect thisEffect, bool textureenabled, Texture2D customTexture, List<Texture2D> textures, VolumeType volume, BoundingFrustum frustum, bool cull, bool drawvolume, int currentObjectsDrawn, out int objectsDrawnThisFrame)
        {
            objectsDrawnThisFrame = currentObjectsDrawn;
            int i = 0, j = 0;
            foreach (CustomModelPart modelPart in modelParts)
            {
                #region Culling
                if (cull)
                {
                    switch (volume)
                    {
                        case VolumeType.BoundingBoxes:
                            Vector3[] corners = modelPart.AABox.GetCorners();

                            Vector3 pointrotated = Vector3.Transform(corners[0], world);
                            Vector3 min = pointrotated;
                            Vector3 max = pointrotated;

                            for (int k = 1; k < 8; ++k)
                            {
                                pointrotated = Vector3.Transform(corners[k], world);

                                min = Vector3.Min(min, pointrotated);
                                max = Vector3.Max(max, pointrotated);
                            }

                            
                            if (frustum.Contains(new BoundingBox(min, max)) == ContainmentType.Disjoint)
                            {
                                ++i;
                                continue;
                            }
                            break;
                        case VolumeType.BoundingSpheres:
                            if (frustum.Contains(new BoundingSphere(Vector3.Transform(modelPart.Sphere.Center, world), modelPart.Sphere.Radius * Math.Max(scale.X, Math.Max(scale.Y, scale.Z)))) == ContainmentType.Disjoint)
                            {
                                ++i;
                                continue;
                            }
                            break;
                    }
                }
                #endregion

                if (textureenabled)
                {
                    if (customTexture == null) thisEffect.Parameters["Texture"].SetValue(textures[i]);
                    else thisEffect.Parameters["Texture"].SetValue(customTexture);   
                }
                thisEffect.Parameters["World"].SetValue(world);

                // Set the graphics device to use our vertex declaration,
                // vertex buffer, and index buffer.
                GraphicsDevice device = PoolGame.device;

                device.VertexDeclaration = modelPart.VertexDeclaration;

                device.Vertices[0].SetSource(modelPart.VertexBuffer, 0,
                                             modelPart.VertexStride);

                device.Indices = modelPart.IndexBuffer;

                // Begin the effect, and loop over all the effect passes.
                thisEffect.Begin();

                //foreach (EffectPass pass in thisEffect.CurrentTechnique.Passes)
                {
                    thisEffect.CurrentTechnique.Passes[0].Begin();

                    // Draw the geometry.
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                                 0, 0, modelPart.VertexCount,
                                                 0, modelPart.IndexCount);

                    thisEffect.CurrentTechnique.Passes[0].End();
                }

                thisEffect.End();
                ++i; j++;

                #region Draw Bounding Volume
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
                                Vector3[] corners = modelPart.AABox.GetCorners();

                                Vector3 pointrotated = Vector3.Transform(corners[0], world);
                                Vector3 min = pointrotated;
                                Vector3 max = pointrotated;

                                for (int k = 1; k < 8; ++k)
                                {
                                    pointrotated = Vector3.Transform(corners[k], world);

                                    min = Vector3.Min(min, pointrotated);
                                    max = Vector3.Max(max, pointrotated);
                                }

                                vectorRenderer.DrawBoundingBox(new BoundingBox(min, max));
                                
                            }
                            break;
                        case VolumeType.BoundingSpheres:
                            PoolGame.device.RenderState.DepthBufferEnable = false;
                            PoolGame.device.RenderState.DepthBufferWriteEnable = false;

                            vectorRenderer.SetViewProjMatrix(World.camera.ViewProjection);
                            vectorRenderer.SetWorldMatrix(world);
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
            }
            if (j != 0) objectsDrawnThisFrame++;
        }
    }
}