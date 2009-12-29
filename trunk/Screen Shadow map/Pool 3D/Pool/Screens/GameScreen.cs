using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extreme_Pool.Scenarios;
using Extreme_Pool.Cameras;
using Extreme_Pool.PoolTables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Extreme_Pool.Controllers;

namespace Extreme_Pool.Screens
{
    public class GameScreen : Screen
    {
        #region Variables
        public static bool DebugMatch = false;

        #endregion

        public GameScreen(ExPool _game)
            : base(_game)
        {

        }
        protected override void LoadContent()
        {
            base.LoadContent();
        }
        public override void Initialize()
        {
            World.scenario = new Garage(ExPool.game);


            World.playerCount = 1;
            World.playerInTurn = 0;

            // primero debe crearse el pooltable principal y luego los jugadores
            World.camera = new FreeCamera(ExPool.game);

            World.poolTable = new Classic(ExPool.game);
            World.poolTable.Position = new Vector3(1.1f, 200 - 13, 0);
            World.poolTable.Scale = new Vector3(1, 1, 1) * 0.02f;




            if (GamePad.GetState(PlayerIndex.One).IsConnected)
                World.players[0] = new Player(ExPool.game, (int)PlayerIndex.One,
                    new GPad(PlayerIndex.One), TeamNumber.One, World.poolTable);
            else
                World.players[0] = new Player(ExPool.game, (int)PlayerIndex.One,
                    new KBoard(PlayerIndex.One), TeamNumber.One, World.poolTable);

            ExPool.game.Components.Add(World.camera);
            ExPool.game.Components.Add(World.scenario);
            ExPool.game.Components.Add(World.poolTable);

            for (int i = 0; i < World.playerCount; i++)
                ExPool.game.Components.Add(World.players[i]);

            



            this.UpdateOrder = 6; this.DrawOrder = 6;
            base.Initialize();
        }

        protected override void Dispose(bool disposing)
        {
            World.scenario.Dispose();

            World.camera.Dispose();


            for (int i = 0; i < World.playerCount; i++)
                World.players[i].Dispose();

            World.poolTable.Dispose();

            ExPool.game.Components.Clear();

            base.Dispose(disposing);
        }

    }
}
