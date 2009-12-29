using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA_PoolGame.Match
{
    public class PoolBallState
    {
        public Vector3 Position;
        public bool isInplay;
        public Matrix RotationInitial;
        public int PocketWhereAt;
        public Trajectory currentTrajectory;

        public PoolBallState(Vector3 _position, bool _isinplay, Matrix _rotationInitial, int _pocketwhereat, Trajectory _traj)
        {
            this.Position = _position;
            this.isInplay = _isinplay;
            this.RotationInitial = _rotationInitial;
            this.PocketWhereAt = _pocketwhereat;
            this.currentTrajectory = _traj;
        }
    }
    public class RoundInformation
    {
        public Ball ballHitFirstThisRound = null;
        public List<Ball> ballsPottedThisRound;
        public List<PoolBallState> ballsState;
        public PoolBallState cueballState;

        public bool cueballPotted = false;
        public bool cueBallInHand = true;
        public bool firstShotOfSet = true;
        public float stickRotation;

        public RoundInformation()
        {
            ballsPottedThisRound = new List<Ball>();
        }

        public void StartSet()
        {
            cueballPotted = false;
            cueBallInHand = true;

            ballHitFirstThisRound = null;
            firstShotOfSet = true;
            ballsPottedThisRound = new List<Ball>();
            ballsState = new List<PoolBallState>();


        }
        public void EndRound()
        {
            ballHitFirstThisRound = null;

        }
    }
}
