using System;
using System.Collections.Generic;
using System.Text;

namespace Extreme_Pool.Threading
{
    public class ChangeBuffer
    {
        public List<ChangeMessage> Messages { get; set; }

        public ChangeBuffer()
        {
            Messages = new List<ChangeMessage>();
        }
        public void Add(ChangeMessage msg)
        {
            Messages.Add(msg);
        }
        public void Clear()
        {
            Messages.Clear();
        }

    }
}
