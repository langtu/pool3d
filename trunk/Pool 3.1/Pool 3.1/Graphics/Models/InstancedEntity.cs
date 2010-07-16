using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNA_PoolGame.Graphics.Shadows;

namespace XNA_PoolGame.Graphics.Models
{
    /// <summary>
    /// Define a Instanced model.
    /// </summary>
    public class InstancedEntity : DrawableComponent
    {
        public int totalinstances;
        public Matrix[] transforms;
        InstancingTechnique instancetech = InstancingTechnique.HardwareInstancing;

        InstancedModel model = null;
        string modelName = null;

        BoundingFrustum frustum = null;


        private float shineness = 96.0f;
        private Vector4 specularColor = Vector4.One;


        /// <summary>
        /// Delegate
        /// </summary>
        public delegate void GatherWorldMatrices();
        public GatherWorldMatrices delegateupdate;

        /// <summary>
        /// Render parameters delegate
        /// </summary>
        public delegate void RenderHandler();

        /// <summary>
        /// Instancing technique
        /// </summary>
        public InstancingTechnique InstancingTechnique
        {
            get { return instancetech; }
        }


        public InstancedEntity(Game _game, string _modelName)
            : base(_game)
        {
            this.modelName = _modelName;
            totalinstances = 0;
        }
        public override void LoadContent()
        {
            GC.Collect();
            model = PoolGame.content.Load<InstancedModel>(modelName);

            model.Initialize(PoolGame.device, instancetech);
            
            base.LoadContent();
        }

        public void SetInstancingTechnique(InstancingTechnique technique)
        {
            this.instancetech = technique;
            model.SetInstancingTechnique(technique);
        }

        public override void Draw(GameTime gameTime)
        {
            if (World.camera == null) return;
            RenderMode renderMode = PostProcessManager.currentRenderMode;

            switch (renderMode)
            {
                case RenderMode.ShadowMapRender:
                    //if (!occluder) return;

                    frustum = LightManager.lights[PostProcessManager.shadows.lightpass].Frustum;
                    DrawModel(false, PostProcessManager.Depth, "DepthMap", delegate { SetParametersShadowMap(LightManager.lights[PostProcessManager.shadows.lightpass]); });

                    break;

                case RenderMode.PCFShadowMapRender:
                    //if (!occluder) return;

                    frustum = World.camera.FrustumCulling;
                    DrawModel(false, PostProcessManager.PCFShadowMap, "PCFSMTechnique", delegate { SetParametersPCFShadowMap(LightManager.lights); });
                    break;

                case RenderMode.ScreenSpaceSoftShadowRender:
                    {
                        frustum = World.camera.FrustumCulling;
                        string basicTechnique = "SSSTechnique";

                        /*if (World.displacementType != DisplacementType.None && this.normalMapAsset != null)
                        {
                            basicTechnique = World.displacementType.ToString() + basicTechnique;
                            PoolGame.device.Textures[3] = normalMapTexture;
                            PoolGame.device.SamplerStates[3].AddressU = TEXTURE_ADDRESS_MODE;
                            PoolGame.device.SamplerStates[3].AddressV = TEXTURE_ADDRESS_MODE;
                        }*/
                        if (World.motionblurType == MotionBlurType.None && World.dofType == DOFType.None) basicTechnique = "NoMRT" + basicTechnique;
                        DrawModel(true, PostProcessManager.SSSoftShadow_MRT, basicTechnique, delegate { SetParametersSoftShadowMRT(LightManager.lights); });

                        //DrawModel(true, LightManager.lights, PostProcessManager.SSSoftShadow, "SSSTechnique", delegate { SetParametersSoftShadow(LightManager.lights); });
                    }
                    break;
                case RenderMode.BasicRender:
                    {
                        frustum = World.camera.FrustumCulling;
                        string basicTechnique = "ModelTechnique";
                        if (World.motionblurType == MotionBlurType.None && World.dofType == DOFType.None) basicTechnique = "NoMRT" + basicTechnique;
                        DrawModel(true, PostProcessManager.modelEffect, basicTechnique, delegate { SetParametersModelEffectMRT(LightManager.lights); });
                    }
                    break;
            }
            base.Draw(gameTime);
        }
        public void DrawModel(bool enableTexture, Effect effect, string technique, RenderHandler setParameter)
        {
            if (setParameter != null) { setParameter.Invoke(); }

            if (enableTexture)
            {
                PoolGame.device.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
                PoolGame.device.SamplerStates[0].AddressV = TextureAddressMode.Clamp;
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
            model.DrawInstances(transforms, effect);
        }

        #region Set Parameters for Basic Render

        public void SetParametersModelEffectMRT(List<Light> lights)
        {
            /*PostProcessManager.modelEffect.Parameters["MaxDepth"].SetValue(World.camera.FarPlane);
            PostProcessManager.modelEffect.Parameters["View"].SetValue(World.camera.View);
            PostProcessManager.modelEffect.Parameters["PrevWorldViewProj"].SetValue(this.prelocalWorld * World.camera.PrevView * World.camera.Projection);
            PostProcessManager.modelEffect.Parameters["totalLights"].SetValue(LightManager.totalLights);

            PostProcessManager.modelEffect.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);
            PostProcessManager.modelEffect.Parameters["LightPosition"].SetValue(LightManager.positions);
            PostProcessManager.modelEffect.Parameters["vAmbient"].SetValue(World.scenario.AmbientColor);
            PostProcessManager.modelEffect.Parameters["vDiffuseColor"].SetValue(LightManager.diffuse);
            PostProcessManager.modelEffect.Parameters["materialDiffuseColor"].SetValue(materialDiffuseColor);

            PostProcessManager.modelEffect.Parameters["aditionalLights"].SetValue(aditionalLights.Count);
            if (aditionalLights.Count > 0)
            {
                PostProcessManager.modelEffect.Parameters["vaditionalLightColor"].SetValue(lightscolors);
                PostProcessManager.modelEffect.Parameters["vaditionalLightPositions"].SetValue(lightspositions);
                PostProcessManager.modelEffect.Parameters["vaditionalLightRadius"].SetValue(lightsradius);
            }
            if (this.specularColor.X == 0.0f && this.specularColor.Y == 0.0f && this.specularColor.Z == 0.0f) PostProcessManager.modelEffect.Parameters["vSpecularColor"].SetValue(LightManager.nospecular);
            else PostProcessManager.modelEffect.Parameters["vSpecularColor"].SetValue(LightManager.specular);


            PostProcessManager.modelEffect.Parameters["Shineness"].SetValue(shineness);

            PostProcessManager.modelEffect.Parameters["CameraPosition"].SetValue(new Vector4(World.camera.CameraPosition.X, World.camera.CameraPosition.Y, World.camera.CameraPosition.Z, 0.0f));
            */
        }

        public void SetParametersModelEffect(Light light)
        {
            //PostProcessManager.modelEffect.Parameters["View"].SetValue(World.camera.View);
            //PostProcessManager.modelEffect.Parameters["PrevWorldViewProj"].SetValue(this.prelocalWorld * World.camera.PrevView * World.camera.Projection);
            /*PostProcessManager.modelEffect.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);
            PostProcessManager.modelEffect.Parameters["LightPosition"].SetValue(new Vector4(light.Position.X, light.Position.Y, light.Position.Z, 1.0f));

            PostProcessManager.modelEffect.Parameters["vSpecularColor"].SetValue(specularColor);
            PostProcessManager.modelEffect.Parameters["Shineness"].SetValue(shineness);

            PostProcessManager.modelEffect.Parameters["CameraPosition"].SetValue(new Vector4(World.camera.CameraPosition.X, World.camera.CameraPosition.Y, World.camera.CameraPosition.Z, 0.0f));
            */
        }

        #endregion

        #region Set Parameters for Shadow

        public void SetParametersSoftShadowMRT(List<Light> lights)
        {
            /*
            PostProcessManager.SSSoftShadow_MRT.Parameters["MaxDepth"].SetValue(World.camera.FarPlane);
            PostProcessManager.SSSoftShadow_MRT.Parameters["View"].SetValue(World.camera.View);
            PostProcessManager.SSSoftShadow_MRT.Parameters["PrevWorldViewProj"].SetValue(this.prelocalWorld * World.camera.PrevView * World.camera.Projection);
            PostProcessManager.SSSoftShadow_MRT.Parameters["totalLights"].SetValue(LightManager.totalLights);

            PostProcessManager.SSSoftShadow_MRT.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);
            PostProcessManager.SSSoftShadow_MRT.Parameters["LightPosition"].SetValue(LightManager.positions);
            PostProcessManager.SSSoftShadow_MRT.Parameters["vAmbient"].SetValue(World.scenario.AmbientColor);
            PostProcessManager.SSSoftShadow_MRT.Parameters["vDiffuseColor"].SetValue(LightManager.diffuse);
            PostProcessManager.SSSoftShadow_MRT.Parameters["materialDiffuseColor"].SetValue(materialDiffuseColor);
            PostProcessManager.SSSoftShadow_MRT.Parameters["aditionalLights"].SetValue(aditionalLights.Count);
            if (aditionalLights.Count > 0)
            {
                PostProcessManager.SSSoftShadow_MRT.Parameters["vaditionalLightColor"].SetValue(lightscolors);
                PostProcessManager.SSSoftShadow_MRT.Parameters["vaditionalLightPositions"].SetValue(lightspositions);
                PostProcessManager.SSSoftShadow_MRT.Parameters["vaditionalLightRadius"].SetValue(lightsradius);
                PostProcessManager.SSSoftShadow_MRT.Parameters["vaditionalLightType"].SetValue(lightstype);
            }
            if (this.specularColor.X == 0.0f && this.specularColor.Y == 0.0f && this.specularColor.Z == 0.0f) PostProcessManager.SSSoftShadow_MRT.Parameters["vSpecularColor"].SetValue(LightManager.nospecular);
            else PostProcessManager.SSSoftShadow_MRT.Parameters["vSpecularColor"].SetValue(LightManager.specular);

            if (PostProcessManager.shadowBlurTech == ShadowBlurTechnnique.SoftShadow)
                PostProcessManager.SSSoftShadow_MRT.Parameters["TexBlurV"].SetValue(PostProcessManager.GBlurVRT.GetTexture());
            else
                PostProcessManager.SSSoftShadow_MRT.Parameters["TexBlurV"].SetValue(PostProcessManager.shadows.ShadowRT.GetTexture());

            PostProcessManager.SSSoftShadow_MRT.Parameters["Shineness"].SetValue(shineness);

            PostProcessManager.SSSoftShadow_MRT.Parameters["CameraPosition"].SetValue(new Vector4(World.camera.CameraPosition.X, World.camera.CameraPosition.Y, World.camera.CameraPosition.Z, 0.0f));
            */
        }
        public void SetParametersSoftShadow(Light light)
        {
            /*
            PostProcessManager.SSSoftShadow.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);
            PostProcessManager.SSSoftShadow.Parameters["LightViewProj"].SetValue(light.LightViewProjection);

            if (PostProcessManager.shadowBlurTech == ShadowBlurTechnnique.SoftShadow)
                PostProcessManager.SSSoftShadow.Parameters["TexBlurV"].SetValue(PostProcessManager.GBlurVRT.GetTexture());
            else
                PostProcessManager.SSSoftShadow.Parameters["TexBlurV"].SetValue(PostProcessManager.shadows.ShadowRT.GetTexture());


            PostProcessManager.SSSoftShadow.Parameters["LightPosition"].SetValue(new Vector4(light.Position.X, light.Position.Y, light.Position.Z, 1.0f));

            PostProcessManager.SSSoftShadow.Parameters["vSpecularColor"].SetValue(specularColor);
            PostProcessManager.SSSoftShadow.Parameters["Shineness"].SetValue(shineness);

            PostProcessManager.SSSoftShadow.Parameters["CameraPosition"].SetValue(new Vector4(World.camera.CameraPosition.X, World.camera.CameraPosition.Y, World.camera.CameraPosition.Z, 0.0f));
            */
        }

        public void SetParametersPCFShadowMap(List<Light> lights)
        {
            
            PostProcessManager.PCFShadowMap.Parameters["ViewProj"].SetValue(World.camera.ViewProjection);


            PostProcessManager.PCFShadowMap.Parameters["LightViewProjs"].SetValue(LightManager.viewprojections);
            PostProcessManager.PCFShadowMap.Parameters["MaxDepths"].SetValue(LightManager.maxdepths);
            PostProcessManager.PCFShadowMap.Parameters["totalLights"].SetValue(LightManager.totalLights);
            for (int i = 0; i < LightManager.totalLights; ++i)
            {
                PostProcessManager.PCFShadowMap.Parameters["ShadowMap" + i].SetValue(PostProcessManager.shadows.ShadowMapRT[i].GetTexture());
            }

            PostProcessManager.PCFShadowMap.Parameters["PCFSamples"].SetValue(PostProcessManager.shadows.pcfSamples);
            PostProcessManager.PCFShadowMap.Parameters["depthBias"].SetValue(LightManager.depthbias);
            
        }

        public void SetParametersShadowMap(Light light)
        {
            
            PostProcessManager.Depth.Parameters["ViewProj"].SetValue(light.LightViewProjection);
            PostProcessManager.Depth.Parameters["MaxDepth"].SetValue(light.LightFarPlane);
            
        }

        /*public void AddLight(Light light)
        {
            aditionalLights.Add(light);
            Array.Resize(ref lightsradius, aditionalLights.Count);
            Array.Resize(ref lightscolors, aditionalLights.Count);
            Array.Resize(ref lightspositions, aditionalLights.Count);
            Array.Resize(ref lightstype, aditionalLights.Count);

            UpdateLightsProperties();
        }

        public void UpdateLightsProperties()
        {
            for (int i = 0; i < aditionalLights.Count; ++i)
            {
                lightsradius[i] = aditionalLights[i].Radius;
                lightscolors[i] = aditionalLights[i].DiffuseColor;
                lightspositions[i] = new Vector4(aditionalLights[i].Position, 1.0f);
                lightstype[i] = (int)aditionalLights[i].LightType;
            }
        }*/

        #endregion
    }
}
