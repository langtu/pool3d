using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNA_PoolGame.Match
{
    /// <summary>
    /// Define a team.
    /// </summary>
    public class Team
    {
        /// <summary>
        /// Players that belongs to this team.
        /// </summary>
        Player[] players;

        BallGroupType ballType;

        Team oppositeTeam;
        TeamNumber teamNumber;
        /// <summary>
        /// Index of the next player in turn,
        /// relative to his team.
        /// </summary>
        int nextPlayerInTurn;

        int totalBallsPocketed;

        #region Properties
        public BallGroupType BallType
        {
            get { return ballType; }
            set { ballType = value; }
        }

        /// <summary>
        /// Returns the opposite team.
        /// </summary>
        public Team OppositeTeam
        {
            get { return oppositeTeam; }
        }

        public int TotalBallsPocketed
        {
            get { return totalBallsPocketed; }
        }

        #endregion

        /// <summary>
        /// Create a new instance of Team class.
        /// </summary>
        public Team(Player[] players, TeamNumber teamNumber)
        {
            this.players = players;
            this.teamNumber = teamNumber;
            ballType = BallGroupType.None;

            foreach (Player player in this.players)
                player.team = this;
        }

        public void RotatePlayer()
        {
            nextPlayerInTurn = (nextPlayerInTurn + 1) % players.Length;
        }

        public void SetReadyForMatch()
        {
            nextPlayerInTurn = 0;
            totalBallsPocketed = 0;
            oppositeTeam = World.teams[((int)this.teamNumber + 1) % 2];
        }

        public void Dispose()
        {
            oppositeTeam = null;
            players = null;
        }

        public int NextPlayerInTurn()
        {
            return players[this.nextPlayerInTurn].playerIndex;
        }

        public void ResetForGameSet()
        {
            totalBallsPocketed = 0;
            ballType = BallGroupType.None;
        }

        /// <summary>
        /// Increases the balls counter that has been
        /// legally pocketed or increases the balls counter 
        /// of the opposite team.
        /// </summary>
        public void IncresePocketedBallsCounter()
        {
            ++totalBallsPocketed;
        }
    }
}
