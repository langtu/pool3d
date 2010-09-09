using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNA_PoolGame.PoolTables;
using XNA_PoolGame.Cameras;
using Microsoft.Xna.Framework;

namespace XNA_PoolGame.Match
{
    /// <summary>
    /// Referee.
    /// </summary>
    public class Referee : GameComponent
    {
        public PoolTable table;
        public Player player1;
        public Player player2;

        /// <summary>
        /// Creates a new instance of Referee.
        /// </summary>
        /// <param name="table"></param>
        public Referee(Game game, PoolTable table, Player player1, Player player2) 
            : base(game)
        {
            this.table = table;
            this.player1 = player1;
            this.player2 = player2;
        }

        public void SetMatchReady()
        {
            table.LagForBreak();
            player1.stick.ballTarget = table.ballslag[0];
            player2.stick.ballTarget = table.ballslag[1];
            if (World.camera is ChaseCamera)
            {
                ((ChaseCamera)World.camera).ChaseDirection = Vector3.Forward;
                ((ChaseCamera)World.camera).ChasePosition = table.Position;
            }
        }

        public override void Initialize()
        {
            this.UpdateOrder = 10;
            SetMatchReady();
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if (table == null) return;

            if (player1 != null && player2 != null && table.phase == MatchPhase.LaggingShot)
            {
                if (player1.waitingforOther && player2.waitingforOther)
                {
                    player1.waitingforOther = false;
                    player2.waitingforOther = false;

                    player1.TakeShot();
                    player2.TakeShot();
                }
                else
                {
                    if (!table.ballsMoving && table.previousBallsMoving)
                        CheckLagPlayersStatus();
                }
            }
            else if (table.phase == MatchPhase.Playing)
            {
                if (!table.ballsMoving && table.previousBallsMoving)
                {
                    table.roundInfo.EndRound();
                    if (table.roundInfo.cueballPotted)
                    {
                        table.roundInfo.cueBallInHand = true;
                        table.UnpottedcueBall();
                    }
                    else
                    {
                        // Check billard rules according to the game match.

                        {
                            World.players[World.playerInTurn].stick.Visible = false;
                            while (World.players[(World.playerInTurn = (World.playerInTurn + 1) % World.playerCount)] == null) { }
                        }
                    }
                }
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// Player fails his lag shot?
        /// </summary>
        /// <param name="who">The player</param>
        /// <returns>True or false.</returns>
        private bool FailLagShot(Player who)
        {            
            if (table.longStringPlanes[(int)who.teamNumber].DotCoordinate(who.stick.ballTarget.Position) < 0.0f)
                return true;

            if (who.stick.ballTarget.ballRailHitsIndexes.Count != 2)
                return true;

            if (who.stick.ballTarget.pocketWhereAt != -1)
                return true;

            if (who.stick.ballTarget.ballRailHitsIndexes[0] != table.footCushionIndex)
                return true;

            return false;
        }

        public void CheckLagPlayersStatus()
        {
            bool fail_player1 = FailLagShot(player1);
            bool fail_player2 = FailLagShot(player2);
            if (fail_player1 && fail_player2)
            {
                // Repeat lag shot for both players.
                player1.aimLagShot = true;
                player2.aimLagShot = true;
                player1.waitingforOther = false;
                player2.waitingforOther = false;

                player1.stick.ballTarget.ballRailHitsIndexes.Clear();
                player2.stick.ballTarget.ballRailHitsIndexes.Clear();

                if (player1.stick.ballTarget.pocketWhereAt != -1)
                {
                    lock (table.pockets[player1.stick.ballTarget.pocketWhereAt].balls)
                    {
                        table.pockets[player1.stick.ballTarget.pocketWhereAt].balls.Remove(player1.stick.ballTarget);
                    }
                    player1.stick.ballTarget.pocketWhereAt = -1;
                    player1.stick.ballTarget.currentTrajectory = Trajectory.Motion;
                    player1.stick.ballTarget.previousHitRail = player1.stick.ballTarget.previousInsideHitRail = -1;
                    player1.stick.ballTarget.Visible = true;
                    player1.stick.ballTarget.Rotation = Matrix.Identity;
                    player1.stick.ballTarget.PreRotation = Matrix.Identity;
                }
                if (player2.stick.ballTarget.pocketWhereAt != -1)
                {
                    lock (table.pockets[player2.stick.ballTarget.pocketWhereAt].balls)
                    {
                        table.pockets[player2.stick.ballTarget.pocketWhereAt].balls.Remove(player2.stick.ballTarget);
                    }
                    player2.stick.ballTarget.pocketWhereAt = -1;
                    player2.stick.ballTarget.currentTrajectory = Trajectory.Motion;
                    player2.stick.ballTarget.previousHitRail = player2.stick.ballTarget.previousInsideHitRail = -1;
                    player2.stick.ballTarget.Visible = true;
                    player2.stick.ballTarget.Rotation = Matrix.Identity;
                    player2.stick.ballTarget.PreRotation = Matrix.Identity;
                }

                player1.stick.ballTarget.SetCenter(table.cueBallStartLagPositionTeam1);
                player2.stick.ballTarget.SetCenter(table.cueBallStartLagPositionTeam2);

                player1.stick.Visible = true;
                player2.stick.Visible = true;
            }
            else
            {
                Player winner = null;
                if (!fail_player1 && !fail_player2)
                {
                    Ray ray1 = new Ray(player1.stick.ballTarget.Position, -table.railsNormals[table.headCushionIndex]);
                    Ray ray2 = new Ray(player2.stick.ballTarget.Position, -table.railsNormals[table.headCushionIndex]);

                    float? intersectPos1 = ray1.Intersects(table.rails[table.headCushionIndex]);
                    float? intersectPos2 = ray2.Intersects(table.rails[table.headCushionIndex]);
                    if (intersectPos1 != null && intersectPos2 != null)
                    {
                        if (intersectPos1 > intersectPos2)
                            winner = player2;
                        else winner = player1;
                        
                    }
                }
                else if (fail_player1)
                    winner = player2;
                else
                    winner = player1;

                World.playerInTurn = winner.playerIndex;


                table.InitializeMatch();
            }
        }


        protected override void Dispose(bool disposing)
        {
            this.table = null;
            player1 = null;
            player2 = null;

            base.Dispose(disposing);
        }
    }
}
