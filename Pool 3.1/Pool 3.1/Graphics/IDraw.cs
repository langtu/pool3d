using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA_PoolGame.Graphics
{
    public interface IDraw
    {
        int DrawOrder { get; set; }

        bool Visible { get; set; }
        void Draw(GameTime gameTime);
    }
}
