using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;

namespace InstancedModelPipeline
{
    /// <summary>
    /// Content Pipeline class provides a design time equivalent of the runtime
    /// InstancedModel class. This stores the output from the InstancedModelProcessor,
    /// right before it gets written into the XNB binary. This class is similar
    /// in shape to the runtime InstancedModel, but stores the data as simple managed
    /// objects rather than GPU data types.
    /// </summary>
    [ContentSerializerRuntimeType("XNA_PoolGame.Graphics.Models.InstancedModel, XNA_PoolGame")]
    public class InstancedModelContent
    {
        // Internally our instanced model is made up from a list of model parts.
        [ContentSerializer]
        List<ModelPart> modelParts = new List<ModelPart>();


        // Each model part represents a piece of geometry that uses one single effect.
        [ContentSerializerRuntimeType("XNA_PoolGame.Graphics.Models.InstancedModelPart, XNA_PoolGame")]
        class ModelPart
        {
            public int IndexCount;
            public int VertexCount;
            public int VertexStride;

            public VertexElement[] VertexElements;
            public VertexBufferContent VertexBufferContent;
            public IndexCollection IndexCollection;

            [ContentSerializer(SharedResource = true)]
            public MaterialContent MaterialContent;
        }


        /// <summary>
        /// Helper function used by the InstancedModelProcessor
        /// to add new ModelPart information.
        /// </summary>
        public void AddModelPart(int indexCount, int vertexCount, int vertexStride,
                                 VertexElement[] vertexElements,
                                 VertexBufferContent vertexBufferContent,
                                 IndexCollection indexCollection,
                                 MaterialContent materialContent)
        {
            ModelPart modelPart = new ModelPart();

            modelPart.IndexCount = indexCount;
            modelPart.VertexCount = vertexCount;
            modelPart.VertexStride = vertexStride;
            modelPart.VertexElements = vertexElements;
            modelPart.VertexBufferContent = vertexBufferContent;
            modelPart.IndexCollection = indexCollection;
            modelPart.MaterialContent = materialContent;

            modelParts.Add(modelPart);
        }
    }
}
