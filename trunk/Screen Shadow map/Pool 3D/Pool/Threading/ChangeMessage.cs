using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;

namespace Extreme_Pool.Threading
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ChangeMessage
    {
        //this appears in all messages
        //identifies how this message should be interpreted
        [FieldOffset(0)]
        public ChangeMessageType MessageType;

        //this is the field required when this message is of type UpdateCameraView
        [FieldOffset(4)]
        public Matrix CameraViewMatrix;

        //this field is used for all messages dealing with entities
        [FieldOffset(4)]
        public int ID;

        //this is the field required when this message is of type UpdateWorldMatrix
        [FieldOffset(8)]
        public Matrix WorldMatrix;

        //this is the field required when this message is of type UpdateHighlightColor
        [FieldOffset(8)]
        public Vector4 HighlightColor;

        //this is the field required when this message is of type CreateNewRenderData
        [FieldOffset(8)]
        public Vector3 Position;
        [FieldOffset(20)]
        public Vector3 Color;

        //nothing is required when this message is of type DeleteRenderData
    }
}
