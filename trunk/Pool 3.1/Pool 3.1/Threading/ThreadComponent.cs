#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Threading;
using XNA_PoolGame.Graphics;
#endregion

namespace XNA_PoolGame.Threading
{
    public class ThreadComponent : GameComponent, IThreadable
    {
        #region Variables
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
        /// Use thread for this component.
        /// </summary>
        protected bool useThread;
        protected bool running = false;
        #endregion

        public ThreadComponent(Game _game)
            : base(_game)
        {
            useThread = false;
            syncObject = new object();
        }

        #region Miembros de IThreadable

        public bool UseThread
        {
            get { return useThread; }
            set { useThread = value; }
        }

        public override void Initialize()
        {
            base.Initialize();
            if (useThread) BuildThread(true);
        }
        /// <summary>
        /// Run Method. Can be overwritten.
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
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        public void BeginThread()
        {
            if (!running) return;
            mutex.Set();
        }

        /// <summary>
        /// Set the configuration for this thread. Run after BeginThread
        /// method is called
        /// </summary>
        public void BuildThread(bool addToThreadList)
        {
            if (addToThreadList) ModelManager.allthreads.Add(this);

            this.Enabled = false;
            mutex = new AutoResetEvent(false);
            stopped = new ManualResetEvent(false);
            useThread = true;
            running = true;
            ThreadStart ts = new ThreadStart(Run);
            thread = new Thread(ts);
            thread.Start();
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

        protected override void Dispose(bool disposing)
        {
            if (useThread)
            {
                mutex.Close();
                stopped.Close();
                thread.Abort();
                //mutex = null;
                //stopped = null;
                //thread = null;
                ModelManager.allthreads.Remove(this);
            }
            base.Dispose(disposing);
        }
    }
}
