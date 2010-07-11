using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNA_PoolGame.Graphics.Models
{
    /// <summary>
    /// Define a InstancedModel.
    /// </summary>
    public class InstancedModel : Entity
    {
        public int totalinstances;
        public Matrix[] transforms;
        InstancingTechnique instancetech = InstancingTechnique.HardwareInstancing;

        public delegate void GatherWorldMatrices();

        public GatherWorldMatrices delegateupdate;

        /// <summary>
        /// Instancing technique
        /// </summary>
        public InstancingTechnique InstancingTechnique
        {
            get { return instancetech; }
        }


        public InstancedModel(Game _game, string _modelName)
            : base(_game, _modelName)
        {
            totalinstances = 0;
        }
        public override void LoadContent()
        {
            base.LoadContent();

            model.Initialize(PoolGame.device, instancetech);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void updateLocalWorld()
        {
            // Do nothing
        }

        public void SetInstancingTechnique(InstancingTechnique technique)
        {
            this.instancetech = technique;
            model.SetInstancingTechnique(technique);
        }

        public override void DrawModel(bool enableTexture, Effect effect, string technique, RenderHandler setParameter)
        {
            if (setParameter != null) { setParameter.Invoke(); }

            if (enableTexture)
            {
                PoolGame.device.SamplerStates[0].AddressU = TEXTURE_ADDRESS_MODE;
                PoolGame.device.SamplerStates[0].AddressV = TEXTURE_ADDRESS_MODE;
            }
            PoolGame.device.Textures[1] = null;
            PoolGame.device.Textures[2] = null;
            PoolGame.device.Textures[3] = null;
            //Texture2D texture = null;
            //if (enableTexture)
            //    texture
            string newTechnique = model.InstancingTechnique.ToString() + technique;

            if (effect.Techniques[newTechnique] == null) 
                return;

            effect.CurrentTechnique = effect.Techniques[newTechnique];
            // Gather instance transform matrices into a single array.
            delegateupdate.Invoke();

            // Draw all the instances in a single call.
            model.DrawInstances(transforms, World.camera.View, World.camera.Projection, effect, textures[0]);
        }
    }
}
