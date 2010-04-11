using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Helpers;
using System.Threading;

namespace XNA_PoolGame.Graphics
{
    /// <summary>
    /// Drawable component that might use Thread
    /// </summary>
    public class DrawableComponent : GameComponent, IDrawable, IThreadable, IKey<int>
    {
        //MemoryBarrier
        private int drawOrder;
        private bool visible;

        protected int cpu = 4;
        protected GameTime gameTime;
        protected Thread thread;
        protected AutoResetEvent mutex;
        public object syncObject;

        /// <summary>
        /// Stop/Resume Thread utility.
        /// </summary>
        protected ManualResetEvent stopped;

        /// <summary>
        /// Use thread for this component
        /// </summary>
        private bool useThread;
        protected bool running = false;

        public DrawableComponent(Game game)
            : base(game)
        {
            thread = null;
            useThread = false;
            drawOrder = int.MaxValue;
            visible = true;
            syncObject = new object();
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
            if (useThread) BuildThread();
        }

        public virtual void Draw(GameTime gameTime)
        {
            
        }

        #endregion

        #region Miembros de IThreadable

        public bool UseThread
        {
            get { return useThread; }
            set { useThread = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Run()
        {
#if XBOX
            Thread.CurrentThread.SetProcessorAffinity(cpu);
#endif

            while (true)
            {
                mutex.WaitOne();
                if (!running) stopped.WaitOne();
                this.Update(gameTime);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        public void BeginThread(GameTime gameTime)
        {
            if (!running) return;
            this.gameTime = gameTime;
            mutex.Set();
        }

        /// <summary>
        /// Set the configuration for this thread. It will be running after BeginThread
        /// method is called
        /// </summary>
        public void BuildThread()
        {
            ModelManager.allthreads.Add(this);

            
            this.Enabled = false;
            mutex = new AutoResetEvent(false);
            stopped = new ManualResetEvent(false);
            useThread = true;
            ThreadStart ts = new ThreadStart(Run);
            thread = new Thread(ts);
            thread.Start();

            running = true;
        }

        /// <summary>
        /// Stops the thread
        /// </summary>
        public void StopThread()
        {
            if (!useThread) return;
            running = false;
        }

        /// <summary>
        /// Resume the thread from pause. (i.e. resume a match)
        /// </summary>
        public void ResumeThread()
        {
            if (!useThread) return;
            running = true;
            stopped.Set();
        }

        public bool Running
        {
            get { return running; }
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
            if (useThread)
            {
                mutex.Close();
                stopped.Close();
                thread.Abort();
            }
            base.Dispose(disposing);
        }

    }
}
