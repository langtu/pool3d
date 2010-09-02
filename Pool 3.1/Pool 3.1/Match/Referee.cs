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

            if (player1 != null && player2 != null &&
                player1.waitingforOther && player2.waitingforOther && table.phase == MatchPhase.LaggingShot)
            {
                player1.waitingforOther = false;
                player2.waitingforOther = false;

                player1.TakeShot();
                player2.TakeShot();


            }
            else if (table.phase == MatchPhase.Playing)
            {
                //while (World.players[(World.playerInTurn = (World.playerInTurn + 1) % World.playerCount)] == null) { }

            }
            base.Update(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            this.table = null;
            player1 = null;
            player2 = null;

            base.Dispose(disposing);
        }

        /// <summary>
        /// Player fails his lag shot?
        /// </summary>
        /// <param name="who"></param>
        /// <returns></returns>
        private bool FailLagShot(Player who)
        {
            if (who.stick.ballTarget.ballRailHitsIndexes.Count != 1)
                return true;

            if (who.stick.ballTarget.ballRailHitsIndexes[0] != table.footCushionIndex)
                return true;

            return false;
        }

        public void CheckLagPlayersStatus()
        {
            if (FailLagShot(player1) && FailLagShot(player2))
            {
                // Repeat lag shot for both players.
                player1.stick.ballTarget.ballRailHitsIndexes.Clear();
                player2.stick.ballTarget.ballRailHitsIndexes.Clear();

                player1.stick.ballTarget.SetCenter(table.cueBallStartLagPositionTeam1);
                player2.stick.ballTarget.SetCenter(table.cueBallStartLagPositionTeam2);
            }
            else
            {
                Player winner = null;
                if (!FailLagShot(player1) && !FailLagShot(player2))
                {
                    Ray ray1 = new Ray(player1.stick.ballTarget.Position, -table.railsNormals[table.footCushionIndex]);
                    Ray ray2 = new Ray(player2.stick.ballTarget.Position, -table.railsNormals[table.footCushionIndex]);

                    float? intersectPos1 = ray1.Intersects(table.rails[table.footCushionIndex]);
                    float? intersectPos2 = ray2.Intersects(table.rails[table.footCushionIndex]);
                    if (intersectPos1 != null && intersectPos2 != null)
                    {
                        if (intersectPos1 > intersectPos2)
                            winner = player2;
                        else winner = player1;
                        
                    }
                }
                else if (FailLagShot(player1))
                    winner = player2;
                else
                    winner = player1;

                World.playerInTurn = winner.playerIndex;


                table.InitializeMatch();
            }
        }
    }
}
