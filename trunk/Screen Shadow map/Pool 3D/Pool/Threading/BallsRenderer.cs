/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Extreme_Pool.Threading
{
    class BallsRenderer : RenderManager
    {

        Model tableModel;
        Model sphereModel;

        Matrix projectionMatrix;
        Matrix viewMatrix;

        // Effect used to apply the edge detection and pencil sketch postprocessing.
        Effect postprocessEffect;

        // Custom rendertargets.
        RenderTarget2D sceneRenderTarget;
        RenderTarget2D normalDepthRenderTarget;
        SpriteBatch spriteBatch;


        public BallsRenderer(DoubleBuffer doubleBuffer, Game game)
            : base(doubleBuffer, game)
        {

        }

        public override void LoadContent()
        {
            tableModel = game.Content.Load<Model>("Content\\Models\\ClassicPoolTable");
            sphereModel = game.Content.Load<Model>("Content\\Models\\Balls\\ball1");
            postprocessEffect = game.Content.Load<Effect>("Content\\Effects\\PostprocessEffect");

            // Change the model to use our custom cartoon shading effect.
            Effect cartoonEffect = game.Content.Load<Effect>("Content\\Effects\\CartoonEffect");
            ThreadUtil.ChangeEffectUsedByModel(tableModel, cartoonEffect);
            ThreadUtil.ChangeEffectUsedByModel(sphereModel, cartoonEffect);

            // Create two custom rendertargets.
            PresentationParameters pp = game.GraphicsDevice.PresentationParameters;

            sceneRenderTarget = new RenderTarget2D(game.GraphicsDevice,
                pp.BackBufferWidth, pp.BackBufferHeight, 1,
                pp.BackBufferFormat, pp.MultiSampleType, pp.MultiSampleQuality);

            normalDepthRenderTarget = new RenderTarget2D(game.GraphicsDevice,
                pp.BackBufferWidth, pp.BackBufferHeight, 1,
                pp.BackBufferFormat, pp.MultiSampleType, pp.MultiSampleQuality);


            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f), game.GraphicsDevice.Viewport.AspectRatio, 1f, 1000);

            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {

            foreach (ChangeMessage msg in messageBuffer.Messages)
            {
                switch (msg.MessageType)
                {
                    case ChangeMessageType.UpdateCameraView:
                        viewMatrix = msg.CameraViewMatrix;
                        break;
                    case ChangeMessageType.UpdateWorldMatrix:
                        RenderDataOjects[msg.ID].worldMatrix = msg.WorldMatrix;
                        break;
                    case ChangeMessageType.CreateNewRenderData:
                        if (RenderDataOjects.Count == msg.ID)
                        {
                            RenderData newRD = new RenderData();
                            newRD.color = msg.Color;
                            newRD.worldMatrix = Matrix.CreateTranslation(msg.Position);
                            RenderDataOjects.Add(newRD);
                        }
                        else if (msg.ID < RenderDataOjects.Count)
                        {
                            RenderDataOjects[msg.ID].color = msg.Color;
                            RenderDataOjects[msg.ID].worldMatrix = Matrix.CreateTranslation(msg.Position); ;
                        }
                        break;
                    default:
                        break;
                }
            }


            GraphicsDevice device = game.GraphicsDevice;

            device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
            device.RenderState.DepthBufferEnable = true;
            device.RenderState.AlphaBlendEnable = false;
            



            device.SetRenderTarget(0, normalDepthRenderTarget);
            device.Clear(Color.Black);

            DrawModel(tableModel, Matrix.CreateTranslation(0, -1, 0), "NormalDepth", Vector3.One);
            foreach (RenderData rd in RenderDataOjects)
            {
                DrawModel(sphereModel, rd, "NormalDepth");
            }

            device.SetRenderTarget(0, sceneRenderTarget);
            device.Clear(Color.CornflowerBlue);

            DrawModel(tableModel, Matrix.CreateTranslation(0, -1, 0), "Toon", Vector3.One);
            foreach (RenderData rd in RenderDataOjects)
            {
                DrawModel(sphereModel, rd, "Toon");
            }

            device.SetRenderTarget(0, null);
            spriteBatch.Begin(SpriteBlendMode.None,
                              SpriteSortMode.Immediate,
                              SaveStateMode.None);

            spriteBatch.Draw(sceneRenderTarget.GetTexture(), Vector2.Zero, Color.White);

            spriteBatch.End();
            //ApplyPostprocess();
        }


        private void DrawModel(Model model, RenderData renderData,
                       string effectTechniqueName)
        {

            DrawModel(model, renderData.worldMatrix, effectTechniqueName, renderData.color);
        }

        /// <summary>
        /// Helper for drawing the spinning model using the specified effect technique.
        /// </summary>
        void DrawModel(Model model, Matrix world, string effectTechniqueName, Vector3 diffuseColor)
        {
            // Set suitable renderstates for drawing a 3D model.
            RenderState renderState = game.GraphicsDevice.RenderState;

            renderState.AlphaBlendEnable = false;
            renderState.AlphaTestEnable = false;
            renderState.DepthBufferEnable = true;

            // Draw the model.
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    // Specify which effect technique to use.
                    effect.CurrentTechnique = effect.Techniques[effectTechniqueName];

                    Matrix localWorld = world;

                    effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
                    effect.Parameters["World"].SetValue(localWorld);
                    effect.Parameters["View"].SetValue(viewMatrix);
                    effect.Parameters["Projection"].SetValue(projectionMatrix);
                }

                mesh.Draw();
            }
        }



        /// <summary>
        /// Helper applies the edge detection and pencil sketch postprocess effect.
        /// </summary>
        void ApplyPostprocess()
        {
            EffectParameterCollection parameters = postprocessEffect.Parameters;
            string effectTechniqueName;


            Vector2 resolution = new Vector2(sceneRenderTarget.Width,
                                             sceneRenderTarget.Height);

            Texture2D normalDepthTexture = normalDepthRenderTarget.GetTexture();

            parameters["EdgeWidth"].SetValue(0.75f);
            parameters["EdgeIntensity"].SetValue(1);
            parameters["ScreenResolution"].SetValue(resolution);
            parameters["NormalDepthTexture"].SetValue(normalDepthTexture);
            effectTechniqueName = "EdgeDetect";


            // Activate the appropriate effect technique.
            postprocessEffect.CurrentTechnique =
                                    postprocessEffect.Techniques[effectTechniqueName];

            // Draw a fullscreen sprite to apply the postprocessing effect.
            spriteBatch.Begin(SpriteBlendMode.None,
                              SpriteSortMode.Immediate,
                              SaveStateMode.None);

            postprocessEffect.Begin();
            postprocessEffect.CurrentTechnique.Passes[0].Begin();

            spriteBatch.Draw(sceneRenderTarget.GetTexture(), Vector2.Zero, Color.White);

            spriteBatch.End();

            postprocessEffect.CurrentTechnique.Passes[0].End();
            postprocessEffect.End();
        }



    }
}
*/