using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Extreme_Pool.Models.PostProcessing.Components
{
    public class BuildZBufferComponent : PostProcessComponent
    {
        public string technique;
        public BuildZBufferComponent() : base()
        {

        }

        public override void LoadContent()
        {
            base.LoadContent();

            effect = content.Load<Effect>(@"Shaders/DepthOfField");

            int width = backBuffer.Width;
            int height = backBuffer.Height;

            //if (PostProcessingSample.SupportsR32F)
            {
                outputRT = new RenderTarget2D(graphics, width, height, 1, SurfaceFormat.Single);
                technique = "BuildDepthBuffer_R32F";
            }
            //else
            //{
            //    outputRT = new RenderTarget2D(graphics, width, height, 1, backBuffer.Format);
            //    technique = "BuildDepthBuffer_ARGB32";
            //}

            effect.CurrentTechnique = effect.Techniques[technique];
        }

        public override void Draw(GameTime gameTime)
        {
            effect.CurrentTechnique = effect.Techniques[technique];

            graphics.RenderState.DepthBufferEnable = true;

            graphics.SetRenderTarget(0, outputRT);
            graphics.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1.0f, 1);

            effect.Begin();

            foreach (DrawableGameComponent basemesh in ExPool.game.Components)
            {
                //basemesh.Update(gameTime);
                if (!(basemesh is EPModel)) continue;

                Matrix wvp = ((EPModel)basemesh).LocalWorld * World.camera.ViewProjection;
                effect.Parameters["WorldViewProj"].SetValue(wvp);
                effect.CommitChanges();

                effect.CurrentTechnique.Passes[0].Begin();

                foreach (ModelMesh mesh in ((EPModel)basemesh).ThisModel.Meshes)
                {
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        graphics.Vertices[0].SetSource(
                        mesh.VertexBuffer, meshPart.StreamOffset, meshPart.VertexStride);

                        graphics.VertexDeclaration = meshPart.VertexDeclaration;
                        graphics.Indices = mesh.IndexBuffer;

                        graphics.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList, meshPart.BaseVertex, 0,
                            meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                    }
                }

                effect.CurrentTechnique.Passes[0].End();
            }

            effect.End();

            graphics.SetRenderTarget(0, null);
        }
    }
}
