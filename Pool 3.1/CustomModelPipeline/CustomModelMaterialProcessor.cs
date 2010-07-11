using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;


namespace CustomModelPipeline
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    ///
    /// TODO: change the ContentProcessor attribute to specify the correct
    /// display name for this processor.
    /// </summary>
    [ContentProcessor]
    public class CustomModelMaterialProcessor : MaterialProcessor
    {
        protected override ExternalReference<TextureContent> BuildTexture(string textureName, 
            ExternalReference<TextureContent> texture, ContentProcessorContext context)
        {
            return context.BuildAsset<TextureContent,
                                      TextureContent>(texture, "CustomTextureProcessor");
        }
    }

}