using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Extreme_Pool.Models.PostProcessing
{
    //delegates for updating component backbuffers and depth buffers
    public delegate void BackBufferResolveEventHandler(ResolveTexture2D resolveTexture);
    public delegate void DepthBufferResolveEventHandler(Texture2D depthBuffer);

    public class PostProcessManager
    {
        //events for updating component backbuffers and depth buffers
        public event BackBufferResolveEventHandler OnBackBufferResolve;
        public event DepthBufferResolveEventHandler OnDepthBufferResolve;
        public List<PostProcessEffect> effects;

        public RenderTarget2D renderTarget;
        public ResolveTexture2D resolveTexture;
        public BuildZBufferComponent depthBuffer;
        public PostProcessManager()
        {
            effects = new List<PostProcessEffect>();
        }
        
        public void AddEffect(PostProcessEffect _effect)
        {
            effects.Add(_effect);
        }

        public void RemoveEffect(PostProcessEffect _effect)
        {
            effects.Remove(_effect);
        }

        public void EnableEffect(PostProcessEffect _effect, bool _enable)
        {
            int index = effects.IndexOf(_effect);

            if (index >= 0)
                effects[index].IsEnabled = _enable;
        }

        public void EnableEffect(int index, bool enable)
        {
            if (effects.Count > 0 && index >= 0 && index < effects.Count)
            {
                effects[index].IsEnabled = enable;
            }
        }
        /// <summary>
        /// Loads all of the effects in the chain.
        /// </summary>
        /// <param name="loadAllContent"></param>
        public void LoadContent()
        {
            #region Create common textures to be used by the effects
            PresentationParameters pp = ExPool.game.GraphicsDevice.PresentationParameters;
            int width = pp.BackBufferWidth;
            int height = pp.BackBufferHeight;

            SurfaceFormat format = pp.BackBufferFormat;

            resolveTexture = new ResolveTexture2D(ExPool.game.GraphicsDevice, width, height, 1, format);
            #endregion

            int i = 0;
            foreach (PostProcessEffect effect in effects)
            {
                effect.LoadContent();

                int j = 0;
                //if a component requires a backbuffer, add their function to the event handler
                foreach (PostProcessComponent component in effect.Components)
                {
                    //if the component updates/modifies the "scene texture"
                    //find all the components who need an up to date scene texture
                    if (component.UpdatesSceneTexture)
                    {
                        int k = 0;
                        foreach (PostProcessEffect e in effects)
                        {
                            int l = 0;
                            foreach (PostProcessComponent c in e.Components)
                            {
                                //skip previous components and ourself
                                if (k < i)
                                {
                                    continue;
                                }
                                else if (k == i && l <= j)
                                {
                                    l++;
                                    continue;
                                }
                                else if (c.RequiresSceneTexture)
                                {
                                    component.OnUpdateSceneTexture += new UpdateSceneTextureEventHandler(c.UpdateSceneTexture);
                                }

                                l++;
                            }

                            k++;
                        }
                    }

                    //add the compontent's UpdateBackBuffer method to the event handler
                    if (component.RequiresBackbuffer ||
                        (effect == effects[0] && component == effect.Components[0]))
                        OnBackBufferResolve += new BackBufferResolveEventHandler(component.UpdateBackbuffer);

                    if (component.RequiresDepthBuffer)
                        OnDepthBufferResolve += new DepthBufferResolveEventHandler(component.UpdateDepthBuffer);

                    j++;
                } //components foreach

                i++;
            } //effects foreach

            if (effects.Count > 0)
            {
                //ensure the last component renders to the backbuffer
                effects[effects.Count - 1].IsFinal = true;

                if (OnDepthBufferResolve != null)
                {
                    depthBuffer = new BuildZBufferComponent(mContent, mGraphicsDevice);
                    depthBuffer.Camera = camera;
                    depthBuffer.Models = models;
                    depthBuffer.LoadContent();
                }
            }
        }
        public void Draw(GameTime gameTime)
        {

        }
    }
}
