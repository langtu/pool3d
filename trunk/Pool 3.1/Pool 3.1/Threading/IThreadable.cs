using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA_PoolGame.Threading
{
    public interface IThreadable
    {
        void Run();
        void BeginThread();
        void BeginThread(GameTime gameTime);
        void BuildThread(bool addToThreadList);
        void StopThread();
        void ResumeThread();
        bool UseThread { get; set; }
        bool Running { get; }
    }
}
