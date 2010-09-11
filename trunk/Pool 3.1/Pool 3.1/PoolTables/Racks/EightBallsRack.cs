using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNA_PoolGame.Helpers;
using Microsoft.Xna.Framework;

namespace XNA_PoolGame.PoolTables.Racks
{
    /// <summary>
    /// 8 balls rack. The fifteen balls are racked as tightly
    /// as possible in a triangle, with the apex ball
    /// on the foot spot and the eight ball as the first ball
    /// that is directly below the apex ball. One from each 
    /// group of seven will be on the two lower corners 
    /// of the triangle. The other balls are placed in 
    /// the triangle without purposeful or intentional pattern.
    /// </summary>
    public class EightBallsRack : Rack
    {
        const int eightballnumber = 7;
        /// <summary>
        /// Numbers of rows in the triangle.
        /// </summary>
        const int ROWS = 5;
        /// <summary>
        /// Row index of the eight ball.
        /// </summary>
        const int EIGHTBALL_ROW = 3;
        /// <summary>
        /// Col index of the eight ball.
        /// </summary>
        const int EIGHTBALL_COL = 2;

        const float RowOffset = 2.65f;

        /// <summary>
        /// Creates a new instance of EightBallsRack class.
        /// </summary>
        /// <param name="table">The table that this rack belongs to.</param>
        public EightBallsRack(PoolTable table)
        {
            this.table = table;
            BuildPoolBalls();
        }

        protected override void BuildPoolBalls()
        {
            table.TotalBalls = 16;
            table.poolBalls = new Ball[15 + 1];
            ballsReady = new bool[15];

            for (int i = 0; i < 15; i++)
            {
                table.poolBalls[i + 1] = new Ball(PoolGame.game, i + 1, 
                    "Models\\Balls\\newball", "Textures\\Balls\\ball " + (i + 1), table, World.ballRadius);
            }
        }

        public override void BuildsBallsRack()
        {
            for (int k = 0; k < 15; k++) ballsReady[k] = false;
            int ballnumber = 0, solidballnumber, stripeballnumber;

            solidballnumber = Maths.random.Next(0, 7);
            stripeballnumber = Maths.random.Next(8, 15);

            ballsReady[solidballnumber] = true; ballsReady[stripeballnumber] = true;
            ballsReady[eightballnumber] = true;

            float diameter = World.ballRadius * 2.0f;
            Vector3 position = table.footSpotPosition;
            
            for (int row = 1; row <= ROWS; ++row)
            {
                for (int col = 1; col <= row; ++col)
                {
                    if (row == EIGHTBALL_ROW && col == EIGHTBALL_COL)
                    {
                        ballnumber = eightballnumber;
                    }
                    else if (row == 5 && (col == 1 || col == 5))
                    {
                        if (col == 1)
                            ballnumber = solidballnumber;
                        else ballnumber = stripeballnumber;
                    } 
                    else
                    {
                        ballnumber = findRandomBall();
                    }
                    table.poolBalls[ballnumber + 1].SetCenter(position);
                    table.poolBalls[ballnumber + 1].pocketWhereAt = -1;
                    table.poolBalls[ballnumber + 1].currentTrajectory = Trajectory.Motion;

                    ballsReady[ballnumber] = true;
                    position.Z += diameter;
                }
                position.X -= (diameter - RowOffset);
                position.Z -= ((diameter * (float)(row)) + World.ballRadius);
            }
        }
    }

    /// <summary>
    /// Factory to be used for EightBallRack class objects creation.
    /// </summary>
    public class EightBallRackFactory : RackFactory
    {
        public override Rack CreateRack(PoolTable table)
        {
            return new EightBallsRack(table);
        }
    }
}
