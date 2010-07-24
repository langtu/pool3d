#region File Description
//-----------------------------------------------------------------------------
// CustomModelProcessor.cs
//
// 
// 
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.ComponentModel;
using System;
using System.IO;
#endregion

namespace CustomModelPipeline
{
    /// <summary>
    /// Content Pipeline processor converts incoming
    /// graphics data into our custom model format.
    /// </summary>
    [ContentProcessor(DisplayName = "Custom Model")]

    public class CustomModelProcessor : ContentProcessor<NodeContent,
                                                         CustomModelContent>
    {
        #region Fields

        string directory;
        private string effectFileName = "basicEffect.fx";
        [DisplayName("Effect File")]
        [DefaultValue("Effects\\basicEffect.fx")]
        [Description("Effect applied to the model.")]
        public string EffectName
        {
            get { return effectFileName; }
            set { effectFileName = value; }
        }

        ContentProcessorContext context;
        CustomModelContent outputModel;

        // A single material may be reused on more than one piece of geometry.
        // This dictionary keeps track of materials we have already converted,
        // to make sure we only bother processing each of them once.
        Dictionary<MaterialContent, MaterialContent> processedMaterials =
                            new Dictionary<MaterialContent, MaterialContent>();

        #endregion

        /// <summary>
        /// Converts incoming graphics data into our custom model format.
        /// </summary>
        public override CustomModelContent Process(NodeContent input,
                                                   ContentProcessorContext context)
        {
            this.context = context;
            //System.Diagnostics.Debugger.Launch();
            directory = Path.GetDirectoryName(input.Identity.SourceFilename);
            outputModel = new CustomModelContent();

            ProcessNode(input);

            return outputModel;
        }


        /// <summary>
        /// Recursively processes a node from the input data tree.
        /// </summary>
        void ProcessNode(NodeContent node)
        {
            // Meshes can contain internal hierarchy (nested tranforms, joints, bones,
            // etc), but this sample isn't going to bother storing any of that data.
            // Instead we will just bake any node transforms into the geometry, after
            // which we can reset them to identity and forget all about them.
            MeshHelper.TransformScene(node, node.Transform);

            node.Transform = Matrix.Identity;

            // Is this node in fact a mesh?
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                // Reorder vertex and index data so triangles will render in
                // an order that makes efficient use of the GPU vertex cache.
                MeshHelper.OptimizeForCache(mesh);
                bool bAlreadyContainsTangent = false;

                //
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    if (geometry.Vertices.Channels.Contains(VertexChannelNames.Tangent(0)))
                    {
                        bAlreadyContainsTangent = true;
                        break;
                    }
                }
                if (!bAlreadyContainsTangent)
                {
                    MeshHelper.CalculateTangentFrames(mesh,
                        VertexChannelNames.TextureCoordinate(0),
                        VertexChannelNames.Tangent(0),
                        VertexChannelNames.Binormal(0));
                }

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
        /// Converts a single piece of input geometry into our custom format.
        /// </summary>
        void ProcessGeometry(GeometryContent geometry)
        {
            int triangleCount = geometry.Indices.Count / 3;
            int vertexCount = geometry.Vertices.VertexCount;

            // Calculate boundingbox
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (int index in geometry.Indices)
            {
                Vector3 vertex = geometry.Vertices.Positions[index];
                min = Vector3.Min(min, vertex);
                max = Vector3.Max(max, vertex);
            }

            // Flatten the flexible input vertex channel data into
            // a simple GPU style vertex buffer byte array.
            VertexBufferContent vertexBufferContent;
            VertexElement[] vertexElements;

            geometry.Vertices.CreateVertexBuffer(out vertexBufferContent,
                                                 out vertexElements,
                                                 context.TargetPlatform);

            int vertexStride = VertexDeclaration.GetVertexStrideSize(vertexElements, 0);

            //geometry.Material.Textures.Add()
            // Convert the input material.
            MaterialContent material = ProcessMaterial(geometry.Material);

            BasicMaterialContent basicMaterial = (BasicMaterialContent)geometry.Material;
            string filename = null;
            if (basicMaterial.Texture != null) filename = basicMaterial.Texture.Filename;

            // Add the new piece of geometry to our output model.
            outputModel.AddModelPart(triangleCount, vertexCount, vertexStride,
                                     vertexElements, vertexBufferContent,
                                     geometry.Indices, material, new BoundingBox(min, max), filename);

        }


        /// <summary>
        /// Converts an input material by chaining to the built-in MaterialProcessor
        /// class. This will automatically go off and build any effects or textures
        /// that are referenced by the material. When you load the resulting material
        /// at runtime, you will get back an Effect instance that has the appropriate
        /// textures already loaded into it and ready to go.
        /// </summary>
        public virtual MaterialContent ProcessMaterial(MaterialContent material)
        {
            // Have we already processed this material?
            if (!processedMaterials.ContainsKey(material))
            {
                //EffectMaterialContent customModelMaterial = new EffectMaterialContent();
                //customModelMaterial.Effect = new ExternalReference<EffectContent>(effectFileName);

                //// Copy texture data across from the original material.
                //BasicMaterialContent basicMaterial = (BasicMaterialContent)material;

                //if (basicMaterial.Texture != null)
                //{
                //    customModelMaterial.Textures.Add("Texture", basicMaterial.Texture);

                //    //
                    
                //    //string folder = Path.GetFileName(Path.GetDirectoryName(basicMaterial.Texture.Filename));
                //    //string newDir = Path.GetFullPath(Path.GetDirectoryName(basicMaterial.Texture.Filename) + "..\\..\\" + folder + "Textures");

                //    //string normalmapFileName = newDir + Path.GetFileNameWithoutExtension(basicMaterial.Texture.Filename) + "_N" + Path.GetExtension(basicMaterial.Texture.Filename);
                //    string normalmapFileName = Path.GetDirectoryName(basicMaterial.Texture.Filename) + "\\" + Path.GetFileNameWithoutExtension(basicMaterial.Texture.Filename) + "_N" + Path.GetExtension(basicMaterial.Texture.Filename);
                //    if (File.Exists(normalmapFileName))
                //    {
                //        customModelMaterial.Textures.Add("NormalMap",
                //            new ExternalReference<TextureContent>(normalmapFileName));
                //    }
                //}

                // If not, process it now.
                processedMaterials[material] =
                    context.Convert<MaterialContent,
                                    MaterialContent>(material, "CustomModelMaterialProcessor");
            }

            return processedMaterials[material];
        }
    }
}
