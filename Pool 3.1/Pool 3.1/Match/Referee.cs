using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNA_PoolGame.PoolTables;
using XNA_PoolGame.Cameras;
using Microsoft.Xna.Framework;
using XNA_PoolGame.PoolTables.Racks;

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

            World.cursor.Controller = World.CurrentPlayer.controller;
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

                    switch(World.gameMode)
                    {
                        #region 8 ball
                        case GameMode.EightBalls:
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
                                table.roundInfo.cueBallInHand = true;
                                table.roundInfo.cueballPotted = false;
                                table.roundInfo.cueballDrivenOff = false;
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
                                            inningOver = true;

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
                                            if (cueBallScratch)
                                            {
                                                table.InitializeGameSet();
                                                inningOver = true;
                                            }
                                        }
                                    }
                                }
                                if (!breakShotFouled)
                                    table.roundInfo.EndSet();
                            }
                            else
                            {
                                if (table.roundInfo.BallsPottedThisRound.Count == 0)
                                {
                                    if (!IsALegalShot())
                                        inningOver = true;
                                }
                                else
                                {
                                    if (table.openTable)
                                    {
                                        bool tableremainsopened = IsTableRemainsOpen();
                                        
                                        bool illegallypocketedballs = false;
                                        if (tableremainsopened)
                                            inningOver = true;
                                        else
                                        {
                                            illegallypocketedballs = AreBallsIllegallyPocketed();
                                        }

                                        if (!tableremainsopened)
                                        {
                                            foreach (Ball ball in table.poolBalls)
                                            {
                                                if (ball.pocketWhereAt == -1 || ball == table.cueBall) continue;
                                                bool found = false;
                                                foreach (Ball pocketed in table.roundInfo.BallsPottedThisRound)
                                                {
                                                    if (ball == pocketed)
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                }
                                                if (!found)
                                                {
                                                    if (ball.ballNumber >= 1 && ball.ballNumber <= 7 &&
                                                        World.CurrentPlayer.team.BallType == BallGroupType.Solid)
                                                        World.CurrentPlayer.team.IncresePocketedBallsCounter();
                                                    else if (ball.ballNumber >= 9 && ball.ballNumber <= 15 &&
                                                        World.CurrentPlayer.team.BallType == BallGroupType.Stripe)
                                                        World.CurrentPlayer.team.IncresePocketedBallsCounter();
                                                    else
                                                        World.CurrentPlayer.team.OppositeTeam.IncresePocketedBallsCounter();
                                                }
                                            }
                                            table.openTable = false;
                                        }
                                    }
                                    else
                                    {
                                        if (AreBallsIllegallyPocketed())
                                            inningOver = true;
                                        else if (!IsALegalShot())
                                            inningOver = fouled = true;
                                        
                                        if (EightBallPocketed())
                                        {
                                            if (World.CurrentPlayer.team.TotalBallsPocketed == 7)
                                            {
                                                if (table.poolBalls[EightBallRack.EIGHTBALLNUMBER + 1].pocketWhereAt == table.roundInfo.calledPocket.pocketIndex &&
                                                    table.roundInfo.calledPocket.pocketIndex != World.CurrentPlayer.team.LastPocketIndex)
                                                {
                                                    // the team has won.
                                                    inningOver = false;
                                                    setState = 1;
                                                }
                                                else
                                                {
                                                    inningOver = true;
                                                    setState = 2;
                                                }
                                            }
                                            else
                                            {
                                                inningOver = true;
                                                setState = 2;
                                            }
                                        }
                                    }

                                    if (table.roundInfo.calledPocket != null && !fouled) 
                                        World.CurrentPlayer.team.LastPocketIndex = table.roundInfo.calledPocket.pocketIndex;
                                }
                            }
                            if (cueBallScratch)
                                table.UnpocketCueBall();

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
                                World.CurrentPlayer.stick.Visible = false;
                                World.CurrentPlayer.team.RotatePlayer();
                                World.playerInTurnIndex = World.CurrentPlayer.team.OppositeTeam.NextPlayerInTurn();
                                World.cursor.Controller = World.CurrentPlayer.controller;

                                World.CurrentPlayer.team.ResetForGameSet();
                                World.CurrentPlayer.team.OppositeTeam.ResetForGameSet();
                                table.InitializeGameSet();
                            }
                            else if (setState == 1) // the team has won.
                            {
                                World.CurrentPlayer.team.ResetForGameSet();
                                World.CurrentPlayer.team.OppositeTeam.ResetForGameSet();
                                table.InitializeGameSet();
                            }
                            else if (inningOver && setState == 0)
                            {
                                World.CurrentPlayer.stick.Visible = false;
                                World.CurrentPlayer.team.RotatePlayer();
                                World.playerInTurnIndex = World.CurrentPlayer.team.OppositeTeam.NextPlayerInTurn();
                                World.cursor.Controller = World.CurrentPlayer.controller;
                            }

                            if (World.CurrentPlayer.team.TotalBallsPocketed == 7)
                            {
                                table.roundInfo.calledBall = table.poolBalls[EightBallRack.EIGHTBALLNUMBER + 1];
                                table.roundInfo.enabledCalledBall = false;
                            }
                            else if (!table.roundInfo.cueBallBehindHeadString || !table.roundInfo.firstShotOfSet)
                            {
                                table.roundInfo.enabledCalledBall = true;
                                table.roundInfo.enabledCalledPocket = true;
                            }
                            break;
                        #endregion
                    }
                }
            }
            base.Update(gameTime);
        }


        public bool IsTableRemainsOpen()
        {
            if (table.roundInfo.calledBall == null || table.roundInfo.calledPocket == null) return true;

            if (table.roundInfo.calledBall.pocketWhereAt == table.roundInfo.calledPocket.pocketIndex)
            {
                if (table.roundInfo.calledBall.ballNumber >= 1 && table.roundInfo.calledBall.ballNumber <= 7)
                {
                    World.CurrentPlayer.team.BallType = BallGroupType.Solid;
                    World.CurrentPlayer.team.OppositeTeam.BallType = BallGroupType.Stripe;
                }
                else if (table.roundInfo.calledBall.ballNumber >= 9 && table.roundInfo.calledBall.ballNumber <= 15)
                {
                    World.CurrentPlayer.team.BallType = BallGroupType.Stripe;
                    World.CurrentPlayer.team.OppositeTeam.BallType = BallGroupType.Solid;
                }
                return false;

            }
            return true;
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
                    
                    if (World.CurrentPlayer.team.BallType == BallGroupType.Solid &&
                        table.roundInfo.BallHitFirstThisRound.ballNumber >= 9)
                        return true;

                    if (World.CurrentPlayer.team.BallType == BallGroupType.Stripe &&
                        table.roundInfo.BallHitFirstThisRound.ballNumber <= 7)
                        return true;



                    break;
            }
            return false;
        }

        #region Lagging shot checking
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
        #endregion

        #region Is a Legal Shot?
        public bool IsALegalShot()
        {
            switch (World.gameMode)
            {
                case GameMode.EightBalls:
                    if (table.roundInfo.calledBall == null || table.roundInfo.calledPocket == null)
                        return false;

                    if (table.roundInfo.BallHitFirstThisRound == null)
                        return false;

                    if (table.roundInfo.BallHitFirstThisRound.ballNumber == 8)
                        return false;

                    if (World.CurrentPlayer.team.BallType == BallGroupType.Stripe &&
                        table.roundInfo.BallHitFirstThisRound.ballNumber <= 7)
                        return false;

                    if (World.CurrentPlayer.team.BallType == BallGroupType.Solid &&
                        table.roundInfo.BallHitFirstThisRound.ballNumber >= 9)
                        return false;

                    if (table.roundInfo.calledBall.pocketWhereAt != table.roundInfo.calledPocket.pocketIndex)
                        return false;

                    int hits = 0;
                    foreach (Ball ball in table.poolBalls)
                    {
                        if (table.roundInfo.ballsRailsHit[ball])
                            ++hits;
                    }
                    if (hits == 0) 
                        return false;

                    break;
            }
            return true;
        }
        #endregion

        #region Are balls illegally pockected this round?
        public bool AreBallsIllegallyPocketed()
        {
            bool r = false;            

            foreach (Ball ball in table.roundInfo.BallsPottedThisRound)
            {
                if (ball.ballNumber >= 1 && ball.ballNumber <= 7)
                {
                    if (World.CurrentPlayer.team.BallType == BallGroupType.Solid)
                        World.CurrentPlayer.team.IncresePocketedBallsCounter();
                    else if (World.CurrentPlayer.team.BallType == BallGroupType.Stripe)
                    {
                        World.CurrentPlayer.team.OppositeTeam.IncresePocketedBallsCounter();
                        r = true;
                    }
                }
                else if (ball.ballNumber >= 9 && ball.ballNumber <= 15)
                {
                    if (World.CurrentPlayer.team.BallType == BallGroupType.Stripe)
                        World.CurrentPlayer.team.IncresePocketedBallsCounter();
                    else if (World.CurrentPlayer.team.BallType == BallGroupType.Solid)
                    {
                        World.CurrentPlayer.team.OppositeTeam.IncresePocketedBallsCounter();
                        r = true;
                    }
                }
            }
            return r;
        }
        #endregion

        private bool EightBallPocketed()
        {
            return (table.poolBalls[EightBallRack.EIGHTBALLNUMBER + 1].pocketWhereAt != -1);
        }

        public bool ShotsAreCalled()
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
