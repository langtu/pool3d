using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;


namespace InstancedModelPipeline
{
    /// <summary>
    /// Content Pipeline processor converts incoming
    /// graphics data into our custom instanced model format.
    /// </summary>
    [ContentProcessor(DisplayName = "Instanced Model")]
    public class InstancedModelProcessor : ContentProcessor<NodeContent,
                                                            InstancedModelContent>
    {
        #region Fields

        NodeContent rootNode;
        ContentProcessorContext context;
        InstancedModelContent outputModel;

        // A single material may be reused on more than one piece of geometry.
        // This dictionary keeps track of materials we have already converted,
        // to make sure we only bother processing each of them once.
        Dictionary<MaterialContent, MaterialContent> processedMaterials =
                            new Dictionary<MaterialContent, MaterialContent>();

        #endregion


        /// <summary>
        /// Converts incoming graphics data into our instanced model format.
        /// </summary>
        public override InstancedModelContent Process(NodeContent input,
                                                      ContentProcessorContext context)
        {
            this.rootNode = input;
            this.context = context;

            outputModel = new InstancedModelContent();

            ProcessNode(input);

            return outputModel;
        }


        /// <summary>
        /// Recursively processes a node from the input data tree.
        /// </summary>
        void ProcessNode(NodeContent node)
        {
            // Bake node transforms into the geometry, so we won't have
            // to bother dealing with these in the runtime drawing code.
            MeshHelper.TransformScene(node, node.Transform);

            node.Transform = Matrix.Identity;

            // Is this node in fact a mesh?
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                // Reorder vertex and index data so triangles will render in
                // an order that makes efficient use of the GPU vertex cache.
                MeshHelper.OptimizeForCache(mesh);

                // Process all the geometry in the mesh.
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    ProcessGeometry(geometry);
                }
            }

            // Recurse over any child nodes.
            foreach (NodeContent child in node.Children)
            {
                ProcessNode(child);
            }
        }


        /// <summary>
        /// Converts a single piece of input geometry into our instanced format.
        /// </summary>
        void ProcessGeometry(GeometryContent geometry)
        {
            int indexCount = geometry.Indices.Count;
            int vertexCount = geometry.Vertices.VertexCount;

            // Validate that the number of vertices is suitable for instancing.
            if (vertexCount > ushort.MaxValue)
            {
                throw new InvalidContentException(
                    string.Format("Geometry contains {0} vertices: " +
                                  "this is too many to be instanced.", vertexCount));
            }

            if (vertexCount > ushort.MaxValue / 8)
            {
                context.Logger.LogWarning(null, rootNode.Identity,
                                          "Geometry contains {0} vertices: " +
                                          "this will only allow it to be instanced " +
                                          "{1} times per batch. A model with fewer " +
                                          "vertices would be more efficient.",
                                          vertexCount, ushort.MaxValue / vertexCount);
            }

            // Validate that the vertex channels we are going to use to pass
            // through our instancing data aren't already in use.
            VertexChannelCollection vertexChannels = geometry.Vertices.Channels;

            for (int i = 1; i <= 4; i++)
            {
                if (vertexChannels.Contains(VertexChannelNames.TextureCoordinate(i)))
                {
                    throw new InvalidContentException(
                        string.Format("Model already contains data for texture " +
                                      "coordinate channel {0}, but instancing " +
                                      "requires this channel for its own use.", i));
                }
            }

            // Flatten the flexible input vertex channel data into
            // a simple GPU style vertex buffer byte array.
            VertexBufferContent vertexBufferContent;
            VertexElement[] vertexElements;

            geometry.Vertices.CreateVertexBuffer(out vertexBufferContent,
                                                 out vertexElements,
                                                 context.TargetPlatform);

            int vertexStride = VertexDeclaration.GetVertexStrideSize(vertexElements, 0);

            // Convert the input material.
            MaterialContent material = ProcessMaterial(geometry.Material);

            // Add the new piece of geometry to our output model.
            outputModel.AddModelPart(indexCount, vertexCount, vertexStride,
                                     vertexElements, vertexBufferContent,
                                     geometry.Indices, material);
        }


        /// <summary>
        /// Converts an input material to use our custom InstancedModel effect.
        /// </summary>
        MaterialContent ProcessMaterial(MaterialContent material)
        {
            // Have we already processed this material?
            if (!processedMaterials.ContainsKey(material))
            {
                // If not, process it now.
                processedMaterials[material] =
                    context.Convert<MaterialContent,
                                    MaterialContent>(material, "InstancedModelMaterialProcessor");
            }

            return processedMaterials[material];
        }
    }
}