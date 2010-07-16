using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace XNA_PoolGame.Graphics.Models
{
    internal class InstancedModelPart
    {
        #region Constants


        // This must match the constant at the top of InstancedModel.fx!
        const int MaxShaderMatrices = 60;

        const int SizeOfVector4 = sizeof(float) * 4;
        const int SizeOfMatrix = sizeof(float) * 16;


        #endregion

        #region Fields

        // Disable compiler warning that we never initialize these fields.
        // That's ok, because the XNB deserializer initialises them for us!
#pragma warning disable 649


        // Model data.
        [ContentSerializer]
        public int indexCount;

        [ContentSerializer]
        public int vertexCount;

        [ContentSerializer]
        public int vertexStride;

        [ContentSerializer]
        public VertexDeclaration vertexDeclaration;

        [ContentSerializer]
        public VertexBuffer vertexBuffer;

        [ContentSerializer]
        public IndexBuffer indexBuffer;

        [ContentSerializer(SharedResource = true)]
        public Effect effect;

#pragma warning restore 649

        Texture2D texture;

        // Track whether effect.CurrentTechnique is dirty.
        bool techniqueChanged;


        // Track which graphics device we are using.
        GraphicsDevice graphicsDevice;


        // The maximum number of instances we can draw in a single batch using the
        // VFetch or ShaderInstancing techniques depends not only on the global
        // MaxShaderMatrices constant, but also on how many times we can replicate
        // the index data before overflowing range of the 16 bit index values.
        int maxInstances;


        // Array of temporary matrices for the VFetch and ShaderInstancing techniques.
        Matrix[] tempMatrices = new Matrix[MaxShaderMatrices];


#if !XBOX360

        // Windows only: secondary vertex data stream used for HardwareInstancing.
        DynamicVertexBuffer instanceDataStream;


        // Windows only: we need to adjust the vertex declaration depending on whether
        // we are using ShaderInstancing or HardwareInstancing. We store a copy of
        // the original unmodified declaration so we can update this as required.
        VertexElement[] originalVertexDeclaration;


        // Windows only: in order to use the ShaderInstancing technique, we must
        // expand our vertex and index buffers to include replicated copies of the
        // model data. We don't want to bother with that if we are only going to use
        // HardwareInstancing, though. This flag keeps track of whether our buffers
        // have been set up ready for ShaderInstancing.
        bool vertexDataIsReplicated;

#endif

        #endregion

        #region Initialization


        /// <summary>
        /// Private constructor, for use by the XNB deserializer.
        /// </summary>
        private InstancedModelPart()
        {
        }


        /// <summary>
        /// Initializes the instancing data.
        /// </summary>
        internal void Initialize(GraphicsDevice device, Texture2D texture)
        {
            graphicsDevice = device;

            this.texture = texture;
            // Work out how many shader instances we can fit into a single batch.
            int indexOverflowLimit = ushort.MaxValue / vertexCount;

            maxInstances = Math.Min(indexOverflowLimit, MaxShaderMatrices);

#if XBOX360
            // On Xbox, we must replicate several copies of our index buffer data for
            // the VFetch instancing technique. We could alternatively precompute this
            // in the content processor, but that would bloat the size of the XNB file.
            // It is more efficient to generate the repeated values at load time.
            //
            // We also require replicated index data for the Windows ShaderInstancing
            // technique, but this is computed lazily on Windows, so as to avoid
            // bloating the index buffer if it turns out that we only ever use the
            // HardwareInstancingTechnique (which does not require any repeated data).

            ReplicateIndexData();
#else
            // On Windows, store a copy of the original vertex declaration.
            originalVertexDeclaration = vertexDeclaration.GetVertexElements();
#endif
        }


        /// <summary>
        /// Initializes a model part to use the specified instancing
        /// technique. This is called once at startup, and then again
        /// whenever the instancing technique is changed.
        /// </summary>
        internal void SetInstancingTechnique(InstancingTechnique instancingTechnique)
        {
#if !XBOX360
            switch (instancingTechnique)
            {
                case InstancingTechnique.ShaderInstancing:
                    InitializeShaderInstancing();
                    break;

                case InstancingTechnique.HardwareInstancing:
                    InitializeHardwareInstancing();
                    break;
            }
#endif

            techniqueChanged = true;
        }


#if !XBOX360


        /// <summary>
        /// Windows only. Initializes geometry to use the ShaderInstancing technique.
        /// </summary>
        void InitializeShaderInstancing()
        {
            // If we haven't done so already, generate several
            // repeated copies of our vertex and index data.
            if (!vertexDataIsReplicated)
            {
                ReplicateVertexData();
                ReplicateIndexData();

                vertexDataIsReplicated = true;
            }

            // When using shader instancing, the instance index is specified for
            // each replicated vertex using a float in texture coordinate channel 1.
            // We must modify our vertex declaration to include that channel.
            int instanceIndexOffset = vertexStride - sizeof(float);
            byte usageIndex = 1;
            short stream = 0;

            VertexElement[] extraElements =
            {
                new VertexElement(stream, (short)instanceIndexOffset,
                                  VertexElementFormat.Single,
                                  VertexElementMethod.Default,
                                  VertexElementUsage.TextureCoordinate, usageIndex)
            };

            ExtendVertexDeclaration(extraElements);
        }


        /// <summary>
        /// Windows only. Initializes geometry to use the HardwareInstancing technique.
        /// </summary>
        void InitializeHardwareInstancing()
        {
            // When using hardware instancing, the instance transform matrix is
            // specified using a second vertex stream that provides 4x4 matrices
            // in texture coordinate channels 1 to 4. We must modify our vertex
            // declaration to include these channels.
            VertexElement[] extraElements = new VertexElement[4];

            short offset = 0;
            byte usageIndex = 1;
            short stream = 1;

            for (int i = 0; i < extraElements.Length; i++)
            {
                extraElements[i] = new VertexElement(stream, offset,
                                                VertexElementFormat.Vector4,
                                                VertexElementMethod.Default,
                                                VertexElementUsage.TextureCoordinate,
                                                usageIndex);

                offset += SizeOfVector4;
                usageIndex++;
            }

            ExtendVertexDeclaration(extraElements);
        }


        /// <summary>
        /// Windows only. Modifies our vertex declaration to include additional
        /// vertex input channels. This is necessary when switching between the
        /// ShaderInstancing and HardwareInstancing techniques.
        /// </summary>
        void ExtendVertexDeclaration(VertexElement[] extraElements)
        {
            // Get rid of the existing vertex declaration.
            vertexDeclaration.Dispose();

            // Append the new elements to the original format.
            int length = originalVertexDeclaration.Length + extraElements.Length;

            VertexElement[] elements = new VertexElement[length];

            originalVertexDeclaration.CopyTo(elements, 0);

            extraElements.CopyTo(elements, originalVertexDeclaration.Length);

            // Create a new vertex declaration.
            vertexDeclaration = new VertexDeclaration(graphicsDevice, elements);
        }


        /// <summary>
        /// Windows only. In preparation for using the ShaderInstancing technique,
        /// replicates the vertex buffer data several times, adding an additional
        /// index channel to indicate which instance each vertex belongs to.
        /// </summary>
        void ReplicateVertexData()
        {
            // Read the existing vertex data, then destroy the existing vertex buffer.
            byte[] oldVertexData = new byte[vertexCount * vertexStride];

            vertexBuffer.GetData(oldVertexData);
            vertexBuffer.Dispose();

            // Adjust the vertex stride to include our additional index channel.
            int oldVertexStride = vertexStride;

            vertexStride += sizeof(float);

            // Allocate a temporary array to hold the replicated vertex data.
            byte[] newVertexData = new byte[vertexCount * vertexStride * maxInstances];

            int outputPosition = 0;

            // Replicate one copy of the original vertex buffer for each instance.
            for (int instanceIndex = 0; instanceIndex < maxInstances; instanceIndex++)
            {
                int sourcePosition = 0;

                // Convert the instance index from float into an array of raw bits.
                byte[] instanceIndexBits = BitConverter.GetBytes((float)instanceIndex);

                for (int i = 0; i < vertexCount; i++)
                {
                    // Copy over the existing data for this vertex.
                    Array.Copy(oldVertexData, sourcePosition,
                               newVertexData, outputPosition, oldVertexStride);

                    outputPosition += oldVertexStride;
                    sourcePosition += oldVertexStride;

                    // Set the value of our new index channel.
                    instanceIndexBits.CopyTo(newVertexData, outputPosition);

                    outputPosition += instanceIndexBits.Length;
                }
            }

            // Create a new vertex buffer, and set the replicated data into it.
            vertexBuffer = new VertexBuffer(graphicsDevice, newVertexData.Length,
                                            BufferUsage.None);

            vertexBuffer.SetData(newVertexData);
        }


#endif  // !XBOX360


        /// <summary>
        /// In preparation for using the VFetch or ShaderInstancing techniques,
        /// replicates the index buffer data several times, offseting the values
        /// for each copy of the data.
        /// </summary>
        void ReplicateIndexData()
        {
            // Read the existing index data, then destroy the existing index buffer.
            ushort[] oldIndices = new ushort[indexCount];

            indexBuffer.GetData(oldIndices);
            indexBuffer.Dispose();

            // Allocate a temporary array to hold the replicated index data.
            ushort[] newIndices = new ushort[indexCount * maxInstances];

            int outputPosition = 0;

            // Replicate one copy of the original index buffer for each instance.
            for (int instanceIndex = 0; instanceIndex < maxInstances; instanceIndex++)
            {
                int instanceOffset = instanceIndex * vertexCount;

                for (int i = 0; i < indexCount; i++)
                {
                    newIndices[outputPosition] = (ushort)(oldIndices[i] +
                                                          instanceOffset);

                    outputPosition++;
                }
            }

            // Create a new index buffer, and set the replicated data into it.
            indexBuffer = new IndexBuffer(graphicsDevice,
                                          sizeof(ushort) * newIndices.Length,
                                          BufferUsage.None,
                                          IndexElementSize.SixteenBits);

            indexBuffer.SetData(newIndices);
        }


        #endregion

        #region Draw


        /// <summary>
        /// Draws a batch of instanced model geometry,
        /// using the specified technique and camera matrices.
        /// </summary>
        public void Draw(InstancingTechnique instancingTechnique,
                         Matrix[] instanceTransforms, Effect customEffect)
        {
            if (instancingTechnique == InstancingTechnique.NoInstancingOrStateBatching)
            {
                // This technique is different to all the others. It reinitializes
                // the effect from scratch for each instance, so must be called
                // outside the effect.Begin/End block used below.
                DrawNoInstancingOrStateBatching(instanceTransforms, customEffect);
            }
            else
            {
                SetRenderStates(instancingTechnique, customEffect);

                // Begin the effect, then loop over all the effect passes.
                customEffect.Begin();

                foreach (EffectPass pass in customEffect.CurrentTechnique.Passes)
                {
                    pass.Begin();

                    // Draw instanced geometry using the specified technique.
                    switch (instancingTechnique)
                    {
#if XBOX360
                        case InstancingTechnique.VFetchInstancing:
                            DrawShaderInstancing(instanceTransforms, customEffect);
                            break;
#else
                        case InstancingTechnique.ShaderInstancing:
                            DrawShaderInstancing(instanceTransforms, customEffect);
                            break;

                        case InstancingTechnique.HardwareInstancing:
                            DrawHardwareInstancing(instanceTransforms);
                            break;
#endif
                        case InstancingTechnique.NoInstancing:
                            DrawNoInstancing(instanceTransforms, customEffect);
                            break;
                    }

                    pass.End();
                }

                customEffect.End();
            }
        }


        /// <summary>
        /// Helper function sets up the graphics device and
        /// effect ready for drawing instanced geometry.
        /// </summary>
        void SetRenderStates(InstancingTechnique instancingTechnique, Effect customEffect)
        {
            // Set the graphics device to use our vertex data.
            graphicsDevice.VertexDeclaration = vertexDeclaration;
            graphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, vertexStride);
            graphicsDevice.Indices = indexBuffer;

            // Make sure our effect is set to use the right technique.
            if (techniqueChanged)
            {
                //string techniqueName = instancingTechnique.ToString();
                //customEffect.CurrentTechnique = customEffect.Techniques[techniqueName];
                techniqueChanged = false;
            }


            if (customEffect.Parameters["TexColor"] != null)
            {
                customEffect.Parameters["TexColor"].SetValue(this.texture);
            }

#if XBOX360
            // Set the vertex count (used by the VFetch instancing technique).
            customEffect.Parameters["VertexCount"].SetValue(vertexCount);
#endif
        }


        /// <summary>
        /// Draws instanced geometry using the VFetch or ShaderInstancing techniques.
        /// </summary>
        void DrawShaderInstancing(Matrix[] instanceTransforms, Effect customEffect)
        {
            // We can only fit maxInstances into a single call. If asked to draw
            // more than that, we must split them up into several smaller batches.
            for (int i = 0; i < instanceTransforms.Length; i += maxInstances)
            {
                // How many instances can we fit into this batch?
                int instanceCount = instanceTransforms.Length - i;

                if (instanceCount > maxInstances)
                    instanceCount = maxInstances;

                // Upload transform matrices as shader constants.
                Array.Copy(instanceTransforms, i, tempMatrices, 0, instanceCount);

                customEffect.Parameters["InstanceTransforms"].SetValue(tempMatrices);
                customEffect.CommitChanges();

                // Draw maxInstances copies of our geometry in a single batch.
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                                     0, 0, instanceCount * vertexCount,
                                                     0, instanceCount * indexCount / 3);
            }
        }


#if !XBOX360


        /// <summary>
        /// Windows only. Draws geometry using the HardwareInstancing technique.
        /// </summary>
        void DrawHardwareInstancing(Matrix[] instanceTransforms)
        {
            // Make sure our instance data vertex buffer is big enough.
            int instanceDataSize = SizeOfMatrix * instanceTransforms.Length;

            if ((instanceDataStream == null) ||
                (instanceDataStream.SizeInBytes < instanceDataSize))
            {
                if (instanceDataStream != null)
                    instanceDataStream.Dispose();

                instanceDataStream = new DynamicVertexBuffer(graphicsDevice,
                                                             instanceDataSize,
                                                             BufferUsage.WriteOnly);
            }

            // Upload transform matrices to the instance data vertex buffer.
            instanceDataStream.SetData(instanceTransforms, 0,
                                       instanceTransforms.Length,
                                       SetDataOptions.Discard);

            // Set up two vertex streams for instanced rendering.
            // The first stream provides the actual vertex data, while
            // the second provides per-instance transform matrices.
            VertexStreamCollection vertices = graphicsDevice.Vertices;

            vertices[0].SetFrequencyOfIndexData(instanceTransforms.Length);

            vertices[1].SetSource(instanceDataStream, 0, SizeOfMatrix);
            vertices[1].SetFrequencyOfInstanceData(1);

            // Draw all the instances in a single batch.
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                                 0, 0, vertexCount,
                                                 0, indexCount / 3);

            // Reset the instancing streams.
            vertices[0].SetSource(null, 0, 0);
            vertices[1].SetSource(null, 0, 0);
        }


#endif  // !XBOX360


        /// <summary>
        /// Draws several copies of a piece of geometry without using any
        /// special GPU instancing techniques at all. This just does a
        /// regular loop and issues several draw calls one after another.
        /// </summary>
        void DrawNoInstancing(Matrix[] instanceTransforms, Effect customEffect)
        {
            EffectParameter transform = customEffect.Parameters["World"];

            for (int i = 0; i < instanceTransforms.Length; i++)
            {
                transform.SetValue(instanceTransforms[i]);
                customEffect.CommitChanges();

                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                                     0, 0, vertexCount,
                                                     0, indexCount / 3);
            }
        }


        /// <summary>
        /// This technique is NOT a good idea! It is only included in the sample
        /// for comparison purposes, so you can compare its performance with the
        /// other more sensible approaches. This uses the exact same shader code
        /// as the preceding NoInstancing technique, but with a key difference.
        /// Where the NoInstancing technique worked like this:
        /// 
        ///     SetRenderStates()
        ///     effect.Begin()
        ///     foreach instance
        ///     {
        ///         Update effect with per-instance transform matrix
        ///         DrawIndexedPrimitives()
        ///     }
        ///     effect.End()
        /// 
        /// NoInstancingOrStateBatching works like so:
        /// 
        ///     foreach instance
        ///     {
        ///         Set per-instance transform matrix into the effect
        ///         SetRenderStates()
        ///         effect.Begin()
        ///         DrawIndexedPrimitives()
        ///         effect.End()
        ///     }
        ///      
        /// As you can see, this is repeatedly setting the same renderstates and
        /// beginning and ending the same effect over and over again. Not efficient.
        /// 
        /// The interesting thing about this technique is that it works in a very
        /// similar way to if you used the built-in framework Model class, with
        /// a loop calling ModelMesh.Draw several times to draw multiple instances.
        /// In other words, the built-in Model is pretty inefficient when it comes
        /// to drawing more than one instance! Even without using any fancy shader
        /// techniques, you can get a significant speed boost just by rearranging
        /// your drawing code to work more like the earlier NoInstancing technique.
        /// </summary>
        void DrawNoInstancingOrStateBatching(Matrix[] instanceTransforms, Effect customEffect)
        {
            EffectParameter transform = customEffect.Parameters["NoInstancingTransform"];

            for (int i = 0; i < instanceTransforms.Length; i++)
            {
                transform.SetValue(instanceTransforms[i]);

                SetRenderStates(InstancingTechnique.NoInstancing, customEffect);

                customEffect.Begin();

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();

                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                                         0, 0, vertexCount,
                                                         0, indexCount / 3);

                    pass.End();
                }

                customEffect.End();
            }
        }


        #endregion
    }
}
