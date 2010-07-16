using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Helpers;
using System.Threading;
using XNA_PoolGame.Threading;

namespace XNA_PoolGame.Graphics
{
    /// <summary>
    /// Drawable component that might use a thread
    /// </summary>
    public class DrawableComponent : ThreadComponent, IDrawable, IKey<int>
    {
        //MemoryBarrier
        private int drawOrder;
        private bool visible;

        public DrawableComponent(Game game)
            : base(game)
        {
            thread = null;
            
            drawOrder = int.MaxValue;
            visible = true;
        }

        public override void Initialize()
        {
            base.Initialize();
            LoadContent();
        }

        #region Miembros de IDrawable

        public int DrawOrder
        {
            get
            {
                return drawOrder;
            }
            set
            {
                drawOrder = value;
            }
        }

        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                visible = value;
            }
        }

        public virtual void LoadContent()
        {
            if (useThread) BuildThread(true);
        }

        public virtual void Draw(GameTime gameTime)
        {
            
        }

        #endregion

        #region Miembros de IKey<int>

        public int Key
        {
            get { return drawOrder; }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

    }
}
