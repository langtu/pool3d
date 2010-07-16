using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace CustomModelPipeline
{
    [ContentProcessor]
    public class CustomTextureProcessor : TextureProcessor
    {        
        public override TextureContent Process(TextureContent input, ContentProcessorContext context)
        {
            // Convert the input to standard Color format, for ease of processing.
            TextureContent textcont = base.Process(input, context);
            textcont.ConvertBitmapType(typeof(PixelBitmapContent<Color>));

            foreach (MipmapChain imageFace in textcont.Faces)
            {
                for (int i = 0; i < imageFace.Count; i++)
                {
                    PixelBitmapContent<Color> mip = (PixelBitmapContent<Color>)imageFace[i];

                    // Apply color keying.
                    //mip.ReplaceColor(Color.Magenta, Color.TransparentBlack);

                    // Make sure the image is a power of two in size.
                    int width = MakePowerOfTwo(mip.Width);
                    int height = MakePowerOfTwo(mip.Height);

                    if ((width != mip.Width) || (height != mip.Height))
                    {
                        context.Logger.LogWarning(null, textcont.Identity,
                            "Bitmap was not a power of two. Scaled from {0}x{1} to {2}x{3}.",
                            mip.Width, mip.Height, width, height);

                        PixelBitmapContent<Color> scaledMip = new PixelBitmapContent<Color>(width, height);

                        BitmapContent.Copy(mip, scaledMip);

                        imageFace[i] = scaledMip;
                    }
                }
            }

            textcont.GenerateMipmaps(false);

            // Compress the output texture.
            textcont.ConvertBitmapType(typeof(Dxt1BitmapContent));

            return textcont;
        }


        private int MakePowerOfTwo(int value)
        {
            int bit = 1;

            while (bit < value)
                bit *= 2;

            return bit;
        }
    }
}
