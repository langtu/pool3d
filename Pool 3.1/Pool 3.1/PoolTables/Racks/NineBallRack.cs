using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA_PoolGame.PoolTables.Racks
{
    public class NineBallRack : Rack
    {
        /// <summary>
        /// Index of nine ball.
        /// </summary>
        public static int NINEBALLNUMBER = 8;
        /// <summary>
        /// Index of one ball.
        /// </summary>
        public static int ONEBALLNUMBER = 0;

        /// <summary>
        /// Numbers of rows in the triangle.
        /// </summary>
        const int ROWS = 5;

        /// <summary>
        /// Row index of the nine ball.
        /// </summary>
        const int NINEBALL_ROW = 3;
        /// <summary>
        /// Col index of the nine ball.
        /// </summary>
        const int NINEBALL_COL = 2;

        const float RowOffset = 2.65f;

        /// <summary>
        /// Creates a new instance of NineBallRack class.
        /// </summary>
        /// <param name="table">The table that the rack belongs to.</param>
        public NineBallRack(PoolTable table)
        {
            this.table = table;
            BuildPoolBalls();
        }

        protected override void BuildPoolBalls()
        {
            table.TotalBalls = 9 + 1;
            table.poolBalls = new Ball[9 + 1];
            ballsReady = new bool[9];

            for (int i = 0; i < 9; i++)
            {
                table.poolBalls[i + 1] = new Ball(PoolGame.game, i + 1,
                    "Models\\Balls\\newball", "Textures\\Balls\\ball " + (i + 1), table, World.ballRadius);
            }
        }

        public override void BuildsBallsRack()
        {
            for (int k = 0; k < 9; k++)
                ballsReady[k] = false;

            int[] columns = { 1, 2, 3, 2, 1 };
            int ballnumber = 0;

            ballsReady[NINEBALLNUMBER] = true;
            ballsReady[ONEBALLNUMBER] = true;

            float diameter = World.ballRadius * 2.0f;
            Vector3 position = table.footSpotPosition;

            for (int row = 1; row <= ROWS; ++row)
            {
                for (int col = 1; col <= columns[row - 1]; ++col)
                {
                    if (row == NINEBALL_ROW && col == NINEBALL_COL)
                        ballnumber = NINEBALLNUMBER;
                    else if (row == 1 && col == 1)
                        ballnumber = ONEBALLNUMBER;
                    else
                        ballnumber = findRandomBall();

                    table.poolBalls[ballnumber + 1].SetCenter(position);
                    table.poolBalls[ballnumber + 1].pocketWhereAt = -1;
                    table.poolBalls[ballnumber + 1].currentTrajectory = Trajectory.Motion;

                    ballsReady[ballnumber] = true;
                    position.Z += diameter;
                }
                position.X -= (diameter - RowOffset);
                position.Z -= ((diameter * (float)(columns[row - 1])));
                if (row < 3) position.Z -= World.ballRadius;
                else position.Z += World.ballRadius;
                
            }

        }

    }

    /// <summary>
    /// Factory to be used for NineBallRack class objects creation.
    /// </summary>
    public class NineBallRackFactory : RackFactory
    {
        public override Rack CreateRack(PoolTable table)
        {
            return new NineBallRack(table);
        }
    }
}
