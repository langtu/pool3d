using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Graphics.Shadows;
using TextureInUse = XNA_PoolGame.Graphics.PostProcessManager.TextureInUse;
namespace XNA_PoolGame.Graphics.Shading
{
    public abstract class BaseShading
    {
        public Vector2 halfPixel;
        public Shadow shadows;
        public TextureInUse resultTIU;
        public virtual void Dispose()
        {

        }
        public abstract void Draw(GameTime gameTime);

        public abstract void DrawTextured(GameTime gameTime);

        public abstract string GetBasicRenderTechnique();
    }
}
