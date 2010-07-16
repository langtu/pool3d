using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace InstancedModelPipeline
{
    [ContentProcessor]
    class InstancedModelMaterialProcessor : MaterialProcessor
    {
        protected override ExternalReference<TextureContent> BuildTexture(string textureName,
               ExternalReference<TextureContent> texture, ContentProcessorContext context)
        {
            return context.BuildAsset<TextureContent,
                                      TextureContent>(texture, "CustomTextureProcessor");
        }
    }
}
