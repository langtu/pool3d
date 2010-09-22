using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNA_PoolGame.Helpers;

namespace XNA_PoolGame.PoolTables.Racks
{
    /// <summary>
    /// Abstract rack.
    /// </summary>
    public abstract class Rack
    {
        protected PoolTable table;

        protected bool[] ballsReady;
        /// <summary>
        /// Abstract method. This should be called before
        /// a game set begins.
        /// </summary>
        public abstract void BuildsBallsRack();

        /// <summary>
        /// Abstract method. This should be called after
        /// the concrete rack is created.
        /// </summary>
        protected abstract void BuildPoolBalls();

        /// <summary>
        /// Pick randomly a ball.
        /// </summary>
        /// <param name="ballReady">Array of balls that have already been selected.</param>
        /// <returns>A number.</returns>
        public int findRandomBall()
        {
            bool IsDone = true;
            int num, lower_limit = 0;
            for (int i = 0; i < ballsReady.Length; i++)
            {
                if (!ballsReady[i])
                {
                    lower_limit = i; IsDone = false;
                    break;
                }
            }
            if (IsDone) return -1;

            while (ballsReady[num = Maths.random.Next(lower_limit, ballsReady.Length)]) { }
            return num;
        }

        public void Dispose()
        {
            this.table = null;
        }
    }

    /// <summary>
    /// Factory to be used for Rack class objects creation.
    /// </summary>
    public abstract class RackFactory
    {
        public abstract Rack CreateRack(PoolTable table);
    }
}
