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

    /// <summary>
    /// Gather round information.
    /// </summary>
    public class RoundInformation
    {
        /// <summary>
        /// The first ball hit in this round.
        /// </summary>
        private Ball ballHitFirstThisRound = null;
        public Vector3 positionHitFirstThisRound;

        private List<Ball> ballsPottedThisRound;
        
        /// <summary>
        /// Collection of ball states.
        /// </summary>
        public List<PoolBallState> ballStates;
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
        /// Determinates whether the cue ball was potted in a pocket.
        /// </summary>
        public bool cueballPotted;

        /// <summary>
        /// Determinates whether the cue ball was driven off the table.
        /// </summary>
        public bool cueballDrivenOff;

        /// <summary>
        /// Determinates whether the cue ball can be moved.
        /// </summary>
        public bool cueBallInHand;
        public bool cueBallBehindHeadString;
        public bool firstShotOfSet;
        public float stickRotation;

        public bool enabledCalledBall;
        public bool enabledCalledPocket;
        public Ball calledBall;
        public Pocket calledPocket;

        public Dictionary<Ball, bool> ballRailsHit;

        public Ball BallHitFirstThisRound
        {
            get { return ballHitFirstThisRound; }
            set 
            {
                ballHitFirstThisRound = value;
                if (ballHitFirstThisRound != null)
                    positionHitFirstThisRound = ballHitFirstThisRound.Position;
            }
        }

        public List<Ball> BallsPottedThisRound
        {
            get { return ballsPottedThisRound; }
        }

        public RoundInformation()
        {
            ballsPottedThisRound = new List<Ball>();
            ballStates = new List<PoolBallState>();
            ballRailsHit = new Dictionary<Ball, bool>();
            table = null;
            calledBall = null;
            calledPocket = null;
            enabledCalledBall = false;
            enabledCalledPocket = false;
        }

        /// <summary>
        /// Sets all the states before a game set.
        /// </summary>
        public void StartSet()
        {
            cueballPotted = cueballDrivenOff = false;
            cueBallInHand = true;
            cueBallBehindHeadString = true;
            firstShotOfSet = true;

            ballHitFirstThisRound = null;
            ballsPottedThisRound.Clear();
            ballStates.Clear();
            cueballRailHits = 0;

            for (int i = 0; i < table.TotalBalls; i++)
            {
                this.ballStates.Add(new PoolBallState(table.poolBalls[i].Position, table.poolBalls[i].PreRotation, table.poolBalls[i].pocketWhereAt,
                    table.poolBalls[i].currentTrajectory));

                table.roundInfo.ballRailsHit[table.poolBalls[i]] = false;
            }

            this.cueballState = this.ballStates[0];
        }

        public void EndSet()
        {
            cueBallBehindHeadString = false;
            firstShotOfSet = false;
        }

        public void StartRound()
        {
            ballsPottedThisRound.Clear();
            for (int i = 0; i < table.TotalBalls; ++i)
                table.roundInfo.ballRailsHit[table.poolBalls[i]] = false;
        }

        public void EndRound()
        {
            ballHitFirstThisRound = null;
            cueballRailHits = 0;
            cueballRailHitIndex = -1;
            ballsPottedThisRound.Clear();
            calledBall = null;
            calledPocket = null;
        }

        public void CueBallHitRail(int index)
        {
            if (cueballRailHits == 0)
                cueballRailHitIndex = index;

            ++cueballRailHits;
        }

        public void Dispose()
        {
            ballsPottedThisRound.Clear();
            ballRailsHit.Clear();
            ballsPottedThisRound = null;
            ballHitFirstThisRound = null;
            table = null;
            cueballState = null;
            ballStates.Clear();
            ballStates = null; 
            calledBall = null;
            calledPocket = null;
        }
    }
}
