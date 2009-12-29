using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Extreme_Pool.Threading
{
    class GameData
    {
        public Vector3 acceleration;
        public Vector3 velocity;
        public Vector3 position;
        public Matrix rotation = Matrix.Identity;
    }
}
