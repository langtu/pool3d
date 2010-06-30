using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNA_PoolGame.Helpers
{
    /// <summary>
    /// Tuples (like Pairs)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class Tuple<T1, T2>
    {
        public Tuple(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }

        public T1 First { get; set; }
        public T2 Second { get; set; }
    }
}
