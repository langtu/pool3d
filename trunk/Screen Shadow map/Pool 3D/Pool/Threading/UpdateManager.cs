using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Threading;
using System.Diagnostics;
using Extreme_Pool.Models;

namespace Extreme_Pool.Threading
{
    class UpdateManager
    {
        //public List<GameData> GameDataOjects { get; set; }
        private DoubleBuffer doubleBuffer;
        private GameTime gameTime;


        protected ChangeBuffer messageBuffer;
        protected Game game;

        public Thread RunningThread { get; set; }
        volatile public Stopwatch FrameWatch;

        public UpdateManager(DoubleBuffer doubleBuffer, Game game)
        {
            this.doubleBuffer = doubleBuffer;
            this.game = game;
            ExPool.ModelObjects = new List<EPModel>();
            FrameWatch = new Stopwatch();
            FrameWatch.Reset();
        }

        private void run()
        {
#if XBOX
            Thread.CurrentThread.SetProcessorAffinity(4);
#endif

            while (true)
            {
                DoFrame();
            }
        }

        public void StartOnNewThread()
        {
            ThreadStart ts = new ThreadStart(run);
            RunningThread = new Thread(ts);
            RunningThread.Start();
        }

        public void DoFrame()
        {
            doubleBuffer.StartUpdateProcessing(out messageBuffer, out gameTime);
            FrameWatch.Reset();
            FrameWatch.Start();
            this.Update(gameTime);
            FrameWatch.Stop();
            doubleBuffer.SubmitUpdate();
        }

        public virtual void Update(GameTime gameTime)
        {
            for (int i = 0; i < ExPool.ModelObjects.Count; i++)
            {
                EPModel gd = ExPool.ModelObjects[i];
                if (gd.UpdateLogic(gameTime))
                {
                    Matrix newWorldMatrix = gd.RotationInitial*gd.Rotation*Matrix.CreateScale(gd.Scale) * Matrix.CreateTranslation(gd.Position);
                    ChangeMessage msg = new ChangeMessage();
                    msg.ID = i;
                    msg.MessageType = ChangeMessageType.UpdateWorldMatrix;
                    msg.WorldMatrix = newWorldMatrix;
                    messageBuffer.Add(msg);
                }
            }
            Console.WriteLine("virtual void de Update...");
        }


    }
}
