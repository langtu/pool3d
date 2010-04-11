using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA_PoolGame.Graphics
{
    public interface IThreadable
    {
        void Run();
        void BeginThread(GameTime gameTime);
        void BuildThread();
        void StopThread();
        void ResumeThread();
        bool UseThread { get; set; }
        bool Running { get; }
    }
}
