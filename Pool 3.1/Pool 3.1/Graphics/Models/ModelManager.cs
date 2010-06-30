using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Graphics;
using XNA_PoolGame.Graphics.Models;
using XNA_PoolGame.Threading;
using XNA_PoolGame.PoolTables;

namespace XNA_PoolGame.Graphics
{
    public static class ModelManager
    {

        public static Dictionary<String, Dictionary<ModelMeshPart, ModelBoundingBoxTexture>> allEffectMapping;
        public static List<IThreadable> allthreads;
        public static int TotalMeshParts;

        public static void AbortAllThreads()
        {
            allthreads.Clear();
        }
        public static void Load()
        {
            List<String> models = new List<String>();
            allthreads = new List<IThreadable>();

            allEffectMapping = new Dictionary<String, Dictionary<ModelMeshPart, ModelBoundingBoxTexture>>();
            models.Add("Models\\Balls\\newball");
            //models.Add("Models\\Balls\\ball8");
            models.Add("Models\\Painting\\snow painting");
            models.Add("Models\\Cribs\\tabouret-design1");
            models.Add("Models\\Cribs\\couch");
            models.Add("Models\\Cribs\\alone couch");
            models.Add("Models\\Cribs\\cue rack");
            models.Add("Models\\Racks\\8 balls rack");
            models.Add("Models\\Cribs\\ornatefloor");
            models.Add("Models\\Cribs\\shark painting");
            models.Add("Models\\Cribs\\smokestack");
            models.Add("Models\\Cribs\\woodfloor");
            models.Add("Models\\Cribs\\firewood");
            models.Add("Models\\Cribs\\wall");
            models.Add("Models\\Cribs\\column");
            models.Add("Models\\Cribs\\roof");
            models.Add("Models\\Cribs\\rollup door");
            models.Add("Models\\Cribs\\tv");
            models.Add("Models\\poolTable2");
            models.Add("Models\\Sticks\\stick");
            models.Add("Models\\Sticks\\stick_universal");
            models.Add("Models\\Balls\\test");

            foreach (String str in models)
                LoadModel(str);

            
            
        }

        public static void RemapModel(Model model, Effect effect)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                }
            }
        }

        private static void LoadModel(String modelName)
        {
            CustomModel model = PoolGame.content.Load<CustomModel>(modelName);
            
            #region Old
            /*Model model = PoolGame.content.Load<Model>(modelName);

            Dictionary<ModelMeshPart, ModelBoundingBoxTexture> temp = new Dictionary<ModelMeshPart, ModelBoundingBoxTexture>();
            Dictionary<Effect, Texture2D> effectDictionary = new Dictionary<Effect, Texture2D>();
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect oldEffect in mesh.Effects)
                {
                    if (!effectDictionary.ContainsKey(oldEffect))
                        effectDictionary.Add(oldEffect, oldEffect.Texture);
                }

                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    Vector3 min = Vector3.One * float.MaxValue;
                    Vector3 max = Vector3.One * float.MinValue;

                    VertexPosition[] vertices = new VertexPosition[meshPart.NumVertices];

                    
                    mesh.VertexBuffer.GetData<VertexPosition>(
                        meshPart.BaseVertex * meshPart.VertexStride,
                        vertices,
                        0,
                        meshPart.NumVertices,
                        meshPart.VertexStride);

                    for (int i = 0; i < meshPart.NumVertices; ++i)
                    {
                        min = Vector3.Min(min, vertices[i].Position);
                        max = Vector3.Max(max, vertices[i].Position);
                    }

                    ModelBoundingBoxTexture mbbt = new ModelBoundingBoxTexture(effectDictionary[meshPart.Effect], new BoundingBox(min, max));

                    temp.Add(meshPart, mbbt);
                    
                }
            }

            allEffectMapping[modelName] = temp;
             */
            //allEffectMapping.Add(modelName, temp);
            #endregion

        }

        public static void UnloadContent()
        {
            allEffectMapping.Clear();
        }

        public struct VertexPosition
        {
            public Vector3 Position;

            public VertexPosition(Vector3 pos)
            {
                Position = pos;
            }
        }

        public struct ModelBoundingBoxTexture : IEquatable<ModelBoundingBoxTexture>
        {
            public Texture2D texture;
            public BoundingBox boundingbox;
            public ModelBoundingBoxTexture(Texture2D _texture, BoundingBox _boundingbox)
            {
                texture = _texture; boundingbox = _boundingbox;
            }
            public static bool operator !=(ModelBoundingBoxTexture p1, ModelBoundingBoxTexture p2)
            {
                return !(p1 != p2);
            }

            public static bool operator ==(ModelBoundingBoxTexture p1, ModelBoundingBoxTexture p2)
            {
                return (p1.texture == p2.texture && p1.boundingbox == p2.boundingbox);
            }

            public override int GetHashCode()
            {
                return texture.GetHashCode() ^ boundingbox.GetHashCode();
            }
           

            #region Miembros de IEquatable<ModelBoundingBoxTexture>

            bool IEquatable<ModelBoundingBoxTexture>.Equals(ModelBoundingBoxTexture other)
            {
                return (texture.Equals(other.texture) && boundingbox.Equals(other.boundingbox));
            }

            public override bool Equals(Object other)
            {
                if (other is ModelBoundingBoxTexture && other != null)
                {
                    return (texture.Equals(((ModelBoundingBoxTexture)other).texture) && boundingbox.Equals(((ModelBoundingBoxTexture)other).boundingbox));
                }
                else
                    return false;
            }

            #endregion
        }
    }

   
}
