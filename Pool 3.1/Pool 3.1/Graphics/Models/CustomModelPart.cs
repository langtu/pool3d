#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace XNA_PoolGame.Graphics.Models
{
    public class CustomModelPart
    {
        #region Constants


        // This must match the constant at the top of InstancedModel.fx!
        const int MaxShaderMatrices = 60;

        const int SizeOfVector4 = sizeof(float) * 4;
        const int SizeOfMatrix = sizeof(float) * 16;


        #endregion

        #region Fields
        // Disable compiler warning that we never initialize these fields.
        // That's ok, because the XNB deserializer initialises them for us!
#pragma warning disable 649

        [ContentSerializer]
        public BoundingBox AABox;

        [ContentSerializer]
        public BoundingSphere Sphere;

        [ContentSerializer]
        public int TriangleCount;

        [ContentSerializer]
        public int VertexCount;

        [ContentSerializer]
        public int VertexStride;

        [ContentSerializer]
        public string TextureFileName;

        [ContentSerializer]
        public VertexDeclaration VertexDeclaration;

        [ContentSerializer]
        public VertexBuffer VertexBuffer;

        [ContentSerializer]
        public IndexBuffer IndexBuffer;

        [ContentSerializer(SharedResource = true)]
        public Effect Effect;


        Texture2D texture;

        /// <summary>
        /// Delegate
        /// </summary>
        public delegate void GatherEffectParameters();
        private GatherEffectParameters parametersdelegate;


        #endregion

        #region Initialization


        /// <summary>
        /// Private constructor, for use by the XNB deserializer.
        /// </summary>
        private CustomModelPart()
        {
        }


        /// <summary>
        /// Initializes the instancing data.
        /// </summary>
        internal void Initialize(Texture2D texture)
        {
            this.texture = texture;
        }

        internal void SetParametersDelegate(GatherEffectParameters delegateparam)
        {
            this.parametersdelegate = delegateparam;
        }
        #endregion

        #region Draw

        #endregion
    }
}