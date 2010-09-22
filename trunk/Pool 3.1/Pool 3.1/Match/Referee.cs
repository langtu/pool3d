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
            player1.stick.ballTarget = table.laggedBalls[0];
            player2.stick.ballTarget = table.laggedBalls[1];
            if (World.camera is ChaseCamera)
            {
                ((ChaseCamera)World.camera).ChaseDirection = Vector3.Forward;
                ((ChaseCamera)World.camera).ChasePosition = table.Position;
            }
        }

        public override void Initialize()
        {
            this.UpdateOrder = 10;
            //SetMatchReady();
            NoLagShot();
            base.Initialize();
        }

        public void NoLagShot()
        {
            World.playerInTurnIndex = World.players[0].playerIndex;
            foreach (Player player in World.players)
                if (player != null && player.playerIndex != World.playerInTurnIndex) 
                    player.stick.Visible = false;
            
            table.InitializeMatch();
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
                    // Check billard rules according to the game match.

                    // An inning is a player's turn at the table. It ends when at the end of a shot
                    // it is no longer legal for him to take a shot.
                    bool inningOver = false;
                    bool fouled = false;
                    bool breakShotFouled = false;
                    bool cueBallScratch = false;
                    int setState = 0;   // 0 nothing happens
                                        // 1 win
                                        // 2 lose
                    
                    if (table.roundInfo.cueballPotted || table.roundInfo.cueballDrivenOff)
                    {
                        //table.roundInfo.cueBallInHand = true;
                        //table.roundInfo.cueballPotted = false;
                        //table.roundInfo.cueballDrivenOff = false;
                        //table.UnpocketCueBall();
                        cueBallScratch = true;
                    }

                    // The game is considered to have commenced once the cue ball
                    // has been struck by the cue tip and crosses the head string.
                    if (table.roundInfo.cueBallBehindHeadString && table.roundInfo.firstShotOfSet)
                    {
                        //if (table.MIN_HEAD_STRING_X < table.cueBall.Position.X)
                        //    breakShotFouled = true;
                        //else if (table.roundInfo.BallHitFirstThisRound == null)
                        //    breakShotFouled = true;

                        if (!breakShotFouled)
                        {
                            if (table.roundInfo.BallsPottedThisRound.Count == 0)
                            {
                                int hits = 0;
                                for (int k = 1; k < table.TotalBalls; ++k)
                                {
                                    if (table.roundInfo.ballsRailsHit[table.poolBalls[k]])
                                        ++hits;
                                }

                                if (hits < 4)
                                    breakShotFouled = true;
                                else
                                {
                                    table.roundInfo.EndSet();
                                    inningOver = true;
                                }


                            }
                            else
                            {
                                if (EightBallPocketed())
                                {
                                    if (!cueBallScratch)
                                    {
                                        // (1) re-spotting the eight ball and accepting the balls in position,
                                        // or
                                        // (2) re-breaking.
                                        inningOver = false;
                                    }
                                    else
                                    {
                                        // (1) re-spotting the eight ball and shooting with cue ball in hand behind the head string;
                                        // or
                                        // (2) re-breaking.
                                        breakShotFouled = true;
                                    }

                                }
                                else
                                {
                                    breakShotFouled = cueBallScratch;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (table.roundInfo.BallsPottedThisRound.Count == 0)
                        {
                            //if (table.roundInfo.BallHitFirstThisRound == null)
                            //    fouled = true;
                            //else if (World.players[World.playerInTurnIndex].team.BallType == BallGroupType.Solid &&
                            //    table.roundInfo.BallHitFirstThisRound.ballNumber >= 9)
                            //    fouled = true;

                            //else if (World.players[World.playerInTurnIndex].team.BallType == BallGroupType.Stripe &&
                            //    table.roundInfo.BallHitFirstThisRound.ballNumber <= 7)
                            //    fouled = true;
                        }
                        else
                        {
                            if (table.openTable)
                            {
                                bool tableremainsopened = true;
                                foreach (Ball ball in table.roundInfo.BallsPottedThisRound)
                                {
                                    if (ball == table.roundInfo.calledBall)
                                    {
                                        tableremainsopened = false;
                                        if (ball.ballNumber >= 1 && ball.ballNumber <= 7)
                                            World.players[World.playerInTurnIndex].team.BallType = BallGroupType.Solid;
                                        else World.players[World.playerInTurnIndex].team.BallType = BallGroupType.Stripe;

                                        World.players[World.playerInTurnIndex].team.OppositeTeam.BallType = (BallGroupType)(((int)World.players[World.playerInTurnIndex].team.BallType + 1) % 2);
                                        break;
                                    }
                                }
                                bool illegallypocketedballs = false;
                                foreach (Ball ball in table.roundInfo.BallsPottedThisRound)
                                {
                                    if (ball.ballNumber >= 1 && ball.ballNumber <= 7)
                                        if (World.players[World.playerInTurnIndex].team.BallType == BallGroupType.Solid)
                                            World.players[World.playerInTurnIndex].team.IncresePocketedBallsCounter();
                                        else
                                        {
                                            World.players[World.playerInTurnIndex].team.OppositeTeam.IncresePocketedBallsCounter();
                                            illegallypocketedballs = true;
                                        }
                                    else
                                        if (World.players[World.playerInTurnIndex].team.BallType == BallGroupType.Stripe)
                                            World.players[World.playerInTurnIndex].team.IncresePocketedBallsCounter();
                                        else
                                        {
                                            World.players[World.playerInTurnIndex].team.OppositeTeam.IncresePocketedBallsCounter();
                                            illegallypocketedballs = true;
                                        }
                                }
                                if (illegallypocketedballs)
                                    inningOver = true;

                                if (!tableremainsopened)
                                    table.openTable = false;
                            }
                        }
                    }

                    if (breakShotFouled)
                    {
                        //table.roundInfo.cueBallInHand = true;
                        //table.roundInfo.cueballPotted = false;
                        //table.roundInfo.cueballDrivenOff = false;
                        //table.RestoreCueBall();
                    }
                    else
                    {
                        //fouled = CheckBasicRules(World.gameMode);

                        
                        //table.roundInfo.EndSet();
                    }
                    inningOver |= fouled | breakShotFouled | cueBallScratch;

                    // The shooter remains at the table as long as he continues
                    // to legally pocket called balls, or he wins the rack by pocketing
                    // the eight ball.
                    table.roundInfo.EndRound();
                    if (setState == 2) // The team has lost.
                    {
                        World.players[World.playerInTurnIndex].stick.Visible = false;
                        World.players[World.playerInTurnIndex].team.RotatePlayer();
                        World.playerInTurnIndex = World.players[World.playerInTurnIndex].team.OppositeTeam.NextPlayerInTurn();

                        table.InitializeGameSet();
                    }
                    else if (setState == 1) // the team has won.
                    {
                        table.InitializeGameSet();
                    }
                    else if (inningOver && setState == 0)
                    {
                        World.players[World.playerInTurnIndex].stick.Visible = false;
                        World.players[World.playerInTurnIndex].team.RotatePlayer();
                        World.playerInTurnIndex = World.players[World.playerInTurnIndex].team.OppositeTeam.NextPlayerInTurn();

                    }
                    
                }
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// Checks the game rules after a player shoots.
        /// </summary>
        /// <param name="gameMode">Match game mode.</param>
        /// <returns>Returns true if happens a violation.</returns>
        public bool CheckBasicRules(GameMode gameMode)
        {
            switch (gameMode)
            {
                case GameMode.EightBalls:
                    if (table.roundInfo.BallHitFirstThisRound == null)
                        return true;

                    if (table.roundInfo.cueBallBehindHeadString && table.BallBehindHeadString(table.roundInfo.positionHitFirstThisRound))
                        return true;
                    
                    if (World.players[World.playerInTurnIndex].team.BallType == BallGroupType.Solid &&
                        table.roundInfo.BallHitFirstThisRound.ballNumber >= 9)
                        return true;

                    if (World.players[World.playerInTurnIndex].team.BallType == BallGroupType.Stripe &&
                        table.roundInfo.BallHitFirstThisRound.ballNumber <= 7)
                        return true;



                    break;
            }
            return false;
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

            if (who.stick.ballTarget.ballRailHitsIndexes[1] != table.headCushionIndex)
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

                World.playerInTurnIndex = winner.playerIndex;
                table.InitializeMatch();
            }
        }

        private bool EightBallPocketed()
        {
            foreach (Ball ball in table.roundInfo.BallsPottedThisRound)
            {
                if (ball.ballNumber == 8)
                    return true;
            }
            return false;
        }

        private bool ShotsAreCalled()
        {
            //if (World.gameMode == GameMode.EightBalls)
            //    return true;

            return false;
        }

        protected override void Dispose(bool disposing)
        {
            this.table = null;
            this.player1 = null;
            this.player2 = null;

            base.Dispose(disposing);
        }
    }
}
