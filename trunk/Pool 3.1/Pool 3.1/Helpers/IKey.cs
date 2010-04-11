using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNA_PoolGame.Helpers
{
    public interface IKey<T>
    {
        T Key { get; }
    }
}
