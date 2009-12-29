using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using Extreme_Pool.Models;

namespace Extreme_Pool.Threading
{
    class RenderManager
    {
        //public List<RenderData> RenderDataOjects { get; set; }
        private DoubleBuffer doubleBuffer;
        private GameTime gameTime;

        protected ChangeBuffer messageBuffer;
        protected Game game;

        public Effect cartoonEffect;
        public RenderTarget2D sceneRenderTarget;
        public RenderTarget2D normalDepthRenderTarget;
        private SpriteBatch spriteBatch;

        public Stopwatch FrameWatch { get; set; }

        public RenderManager(DoubleBuffer doubleBuffer, Game game)
        {
            this.doubleBuffer = doubleBuffer;
            this.game = game;
            //this.RenderDataOjects = new List<RenderData>();
            FrameWatch = new Stopwatch();
            FrameWatch.Reset();
        }

        public virtual void LoadContent()
        {
            cartoonEffect = ExPool.content.Load<Effect>("Effects\\CartoonEffect");

            // Create two custom rendertargets.
            PresentationParameters pp = game.GraphicsDevice.PresentationParameters;

            sceneRenderTarget = new RenderTarget2D(game.GraphicsDevice,
                pp.BackBufferWidth, pp.BackBufferHeight, 1,
                pp.BackBufferFormat, pp.MultiSampleType, pp.MultiSampleQuality);

            normalDepthRenderTarget = new RenderTarget2D(game.GraphicsDevice,
                pp.BackBufferWidth, pp.BackBufferHeight, 1,
                pp.BackBufferFormat, pp.MultiSampleType, pp.MultiSampleQuality);

            spriteBatch = ExPool.batch;

            
        }

        public void DoFrame()
        {
            doubleBuffer.StartRenderProcessing(out messageBuffer, out gameTime);
            this.Draw(gameTime);
            doubleBuffer.SubmitRender();
        }

        public virtual void Draw(GameTime gameTime)
        {
            messageBuffer.Clear();
            foreach (EPModel rd in ExPool.ModelObjects)
            {
                //ThreadUtil.ChangeEffectUsedByModel(rd.Model, cartoonEffect);
            }
            

            foreach (ChangeMessage msg in messageBuffer.Messages)
            {
                switch (msg.MessageType)
                {
                    case ChangeMessageType.UpdateCameraView:
                        //viewMatrix = msg.CameraViewMatrix;
                        break;
                    case ChangeMessageType.UpdateWorldMatrix:
                        //RenderDataOjects[msg.ID].worldMatrix = msg.WorldMatrix;
                        //ExPool.ModelObjects[msg.ID].LocalWorld = msg.WorldMatrix;
                        break;
                    //case ChangeMessageType.CreateNewRenderData:
                    //    if (RenderDataOjects.Count == msg.ID)
                    //    {
                    //        RenderData newRD = new RenderData();
                    //        newRD.color = msg.Color;
                    //        newRD.worldMatrix = Matrix.CreateTranslation(msg.Position);
                    //        RenderDataOjects.Add(newRD);
                    //    }
                    //    else if (msg.ID < RenderDataOjects.Count)
                    //    {
                    //        RenderDataOjects[msg.ID].color = msg.Color;
                    //        RenderDataOjects[msg.ID].worldMatrix = Matrix.CreateTranslation(msg.Position); ;
                    //    }
                    //    break;
                    default:
                        break;
                }
            }

            GraphicsDevice device = ExPool.device;

            device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
            device.RenderState.DepthBufferEnable = true;
            device.RenderState.AlphaBlendEnable = false;

            device.SetRenderTarget(0, normalDepthRenderTarget);
            device.Clear(Color.Black);

            //DrawModel(tableModel, Matrix.CreateTranslation(0, -1, 0), "NormalDepth", Vector3.One);
            //foreach (RenderData rd in RenderDataOjects)
            //{
            //    DrawModel(sphereModel, rd, "NormalDepth");
            //}

            device.SetRenderTarget(0, sceneRenderTarget);
            device.Clear(Color.CornflowerBlue);


            foreach (EPModel rd in ExPool.ModelObjects)
            {
                
                rd.Draw(gameTime);
            }
            //DrawModel(tableModel, Matrix.CreateTranslation(0, -1, 0), "Toon", Vector3.One);
            //foreach (RenderData rd in RenderDataOjects)
            //{
            //    DrawModel(sphereModel, rd, "Toon");
            //}

            device.SetRenderTarget(0, null);
            spriteBatch.Begin(SpriteBlendMode.None,
                              SpriteSortMode.Immediate,
                              SaveStateMode.None);

            spriteBatch.Draw(sceneRenderTarget.GetTexture(), Vector2.Zero, Color.White);

            spriteBatch.End();

            Console.WriteLine("virtual void de Draw...");
        }
    }
}
