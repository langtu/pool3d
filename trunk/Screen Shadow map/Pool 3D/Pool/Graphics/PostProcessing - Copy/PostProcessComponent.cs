using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Extreme_Pool.Models.PostProcessing
{

    public delegate void UpdateSceneTextureEventHandler(Texture2D newSceneTex);

    public class PostProcessComponent : PostProcessBase
    {
        public UpdateSceneTextureEventHandler OnUpdateSceneTexture;

        public bool UpdatesSceneTexture;
        public bool RequiresBackbuffer;
        public bool RequiresDepthBuffer;
        public bool RequiresSceneTexture;
        

        public PostProcessComponent()
        {
            UpdatesSceneTexture = false;
            RequiresBackbuffer = false;
            RequiresDepthBuffer = false;
            RequiresSceneTexture = false;
        }

        public void UpdateBackbuffer(ResolveTexture2D resolveTexture)
        {
            backBuffer = resolveTexture;
        }

        public void UpdateSceneTexture(Texture2D newSceneTex)
        {
            backBuffer = newSceneTex;
        }

        public void UpdateDepthBuffer(Texture2D zbuffer)
        {
            depthBuffer = zbuffer;
        }
    }
}
