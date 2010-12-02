using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace XNA_PoolGame.Graphics.Models
{
    /// <summary>
    /// Enum describes the various possible techniques
    /// that can be chosen to implement instancing.
    /// </summary>
    public enum InstancingTechnique
    {
#if XBOX360
        VFetchInstancing,
#else
        HardwareInstancing,
        ShaderInstancing,
#endif
        NoInstancing,
        NoInstancingOrStateBatching
    }

    public class InstancedModel
    {
        #region Fields


        // Internally our custom model is made up from a list of model parts.
        // Most of the interesting code lives in the InstancedModelPart class.
        [ContentSerializer]
        List<InstancedModelPart> modelParts = null;


        // Keep track of what graphics device we are using.
        GraphicsDevice graphicsDevice;


        #endregion

        #region Initialization


        /// <summary>
        /// Private constructor, for use by the XNB deserializer.
        /// </summary>
        private InstancedModel()
        {
        }


        /// <summary>
        /// Initializes the instancing data.
        /// </summary>
        public void Initialize(GraphicsDevice device)
        {
            graphicsDevice = device;

            foreach (InstancedModelPart modelPart in modelParts)
            {
                BasicEffect be = (BasicEffect)modelPart.effect;
                modelPart.Initialize(device, be.Texture);
            }

            // Choose the best available instancing technique.
            InstancingTechnique technique = 0;

            while (!IsTechniqueSupported(technique))
                technique++;

            SetInstancingTechnique(technique);
        }

        public void Initialize(GraphicsDevice device, InstancingTechnique instancedtech)
        {
            graphicsDevice = device;

            foreach (InstancedModelPart modelPart in modelParts)
            {
                BasicEffect be = (BasicEffect)modelPart.effect;
                modelPart.Initialize(device, be.Texture);
            }

            SetInstancingTechnique(instancedtech);
        }


        #endregion

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

            foreach (InstancedModelPart modelPart in modelParts)
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
        /// Draws a batch of instanced models.
        /// </summary>
        public void DrawInstances(Matrix[] instanceTransforms, Effect customEffect)
        {
            if (graphicsDevice == null)
            {
                throw new InvalidOperationException(
                    "InstanceModel.Initialize must be called before DrawInstances.");
            }

            if (instanceTransforms.Length == 0)
                return;

            foreach (InstancedModelPart modelPart in modelParts)
            {
                modelPart.Draw(instancingTechnique, instanceTransforms,
                                customEffect);
            }
        }

        
    }
}
