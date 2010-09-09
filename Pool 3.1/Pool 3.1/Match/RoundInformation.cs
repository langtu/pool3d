using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA_PoolGame.PoolTables;

namespace XNA_PoolGame.Match
{
    public class PoolBallState
    {
        public Vector3 Position;
        public Matrix RotationInitial;
        public int PocketWhereAt;
        public Trajectory currentTrajectory;

        public PoolBallState(Vector3 _position, Matrix _rotationInitial, int _pocketwhereat, Trajectory _traj)
        {
            this.Position = _position;
            this.RotationInitial = _rotationInitial;
            this.PocketWhereAt = _pocketwhereat;
            this.currentTrajectory = _traj;
        }
    }

    public class RoundInformation
    {
        /// <summary>
        /// The first ball hit in this round.
        /// </summary>
        public Ball ballHitFirstThisRound = null;
        
        public List<Ball> ballsPottedThisRound;
        
        /// <summary>
        /// Collection of ball states.
        /// </summary>
        public List<PoolBallState> ballsState;
        public PoolBallState cueballState;

        /// <summary>
        /// Counts the cue ball hits.
        /// </summary>
        public int cueballRailHits;

        /// <summary>
        /// Index of the first rail hit.
        /// </summary>
        public int cueballRailHitIndex;
        public PoolTable table;

        /// <summary>
        /// Determinate whether the cue ball was potted in a pocket.
        /// </summary>
        public bool cueballPotted;

        /// <summary>
        /// Determinate whether the cue ball can be moved.
        /// </summary>
        public bool cueBallInHand;
        public bool firstShotOfSet;
        public float stickRotation;

        public RoundInformation()
        {
            ballsPottedThisRound = new List<Ball>();
            ballsState = new List<PoolBallState>();
            table = null;
        }

        /// <summary>
        /// Sets all states before a game set.
        /// </summary>
        public void StartSet()
        {
            cueballPotted = false;
            cueBallInHand = true;

            ballHitFirstThisRound = null;
            firstShotOfSet = true;
            ballsPottedThisRound.Clear();
            ballsState.Clear();
            cueballRailHits = 0;

            for (int i = 0; i < table.TotalBalls; i++)
            {
                this.ballsState.Add(new PoolBallState(table.poolBalls[i].Position, table.poolBalls[i].PreRotation, table.poolBalls[i].pocketWhereAt,
                    table.poolBalls[i].currentTrajectory));
                
            }

            this.cueballState = this.ballsState[0];

        }

        public void StartRound()
        {
            ballsPottedThisRound.Clear();
        }

        public void EndRound()
        {
            ballHitFirstThisRound = null;
            cueballRailHits = 0;
            cueballRailHitIndex = -1;
        }

        public void cueBallHitRail(int index)
        {
            if (cueballRailHits == 0)
                cueballRailHitIndex = index;

            ++cueballRailHits;
        }
    }
}
