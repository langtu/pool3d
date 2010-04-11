using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace CustomModelPipeline
{
    /// <summary>
    /// Content Pipeline processor converts incoming
    /// graphics data into our normalmapping model format.
    /// </summary>
    [ContentProcessor(DisplayName = "Normal Mapping Model")]
    public class NormalMappingModelProcessor : CustomModelProcessor
    {
        public override CustomModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            return base.Process(input, context);
        }
    }
}
