using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNA_PoolGame.Models;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Graphics;
using Microsoft.Xna.Framework.Graphics;
using XNA_PoolGame.Graphics.Particles;
using XNA_PoolGame.Graphics.Particles.Fire;

namespace XNA_PoolGame.Scenarios
{
    public class CribsBasement : Scenario
    {
        public BasicModel floor = null;
        public BasicModel[] walls;
        public BasicModel couch = null;
        
        public BasicModel eightRackTriangle = null;

        public BasicModel cueRack = null;
        public BasicModel[] poolBallsOnCueRack = null;
        public BasicModel[] sticksOnCueRack = null;

        public BasicModel snowPainting = null;
        public BasicModel aloneCouch = null;
        public BasicModel tv = null;
        public BasicModel rollupDoor = null;
        public BasicModel roof = null;
        public BasicModel[] columns;
        public BasicModel[] tabourets;
        public BasicModel smokestack = null;
        public BasicModel smokeStackFireWood = null;

        public ParticleSystem fireParticles = null;
        public ParticleSystem smokeParticles = null;

        public CribsBasement(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            fireParticles = new SmokeStackFireParticleSystem(Game, PoolGame.content);
            //fireParticles.DrawOrder = 4000;
            
            PoolGame.game.Components.Add(fireParticles);

            smokeParticles = new SmokePlumeParticleSystem(Game, PoolGame.content);
            //smokeParticles.DrawOrder = 3000;

            PoolGame.game.Components.Add(smokeParticles);

            smokestack = new BasicModel(PoolGame.game, "Models\\Cribs\\smokestack");
            smokestack.Position = new Vector3(1150.0f - 120.0f, 0, 0);
            //smokestack.InitialRotation = Matrix.CreateRotationY(MathHelper.PiOver2);
            smokestack.SpecularColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);

            PoolGame.game.Components.Add(smokestack);

            smokeStackFireWood = new BasicModel(PoolGame.game, "Models\\Cribs\\firewood");
            smokeStackFireWood.Position = smokestack.Position + new Vector3(0, 40.0f, -40.0f);
            smokeStackFireWood.InitialRotation = Matrix.CreateRotationY(-MathHelper.PiOver2);
            smokeStackFireWood.SpecularColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);

            PoolGame.game.Components.Add(smokeStackFireWood);

            tabourets = new BasicModel[2];
            tabourets[0] = new BasicModel(PoolGame.game, "Models\\Cribs\\tabouret-design1");
            tabourets[0].Position = new Vector3(-900.0f, 0.0f, -850.0f);
            tabourets[0].InitialRotation = Matrix.CreateRotationY(MathHelper.Pi + MathHelper.PiOver4);

            tabourets[1] = new BasicModel(PoolGame.game, "Models\\Cribs\\tabouret-design1");
            tabourets[1].Position = new Vector3(700.0f, 0.0f, -850.0f);
            tabourets[1].InitialRotation = Matrix.CreateRotationY(MathHelper.Pi);

            for (int i = 0; i < tabourets.Length; ++i)
                PoolGame.game.Components.Add(tabourets[i]);

            /////////////// DA L COUCH
            couch = new BasicModel(PoolGame.game, "Models\\Cribs\\couch");
            couch.Position = new Vector3(-200.0f, 0.0f, -850.0f);
            couch.InitialRotation = Matrix.CreateRotationY(MathHelper.PiOver2);
            couch.SpecularColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);

            PoolGame.game.Components.Add(couch);

            /////////////// ALONE COUCH 
            aloneCouch = new BasicModel(PoolGame.game, "Models\\Cribs\\alone couch");
            aloneCouch.Position = new Vector3(940.0f, 0.0f, -650.0f);
            aloneCouch.InitialRotation = Matrix.CreateRotationY(MathHelper.ToRadians(105.0f));
            aloneCouch.SpecularColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);

            PoolGame.game.Components.Add(aloneCouch);

            #region Cue Rack with everything on it
            /////////////// WALL'S CUE RACK
            cueRack = new BasicModel(PoolGame.game, "Models\\Cribs\\cue rack");
            cueRack.Position = new Vector3(-500, 120, 1075);
            cueRack.InitialRotation = Matrix.CreateRotationY(MathHelper.ToRadians(180.0f));
            cueRack.SpecularColor = new Vector4(0.4f, 0.4f, 0.4f, 1.0f);

            PoolGame.game.Components.Add(cueRack);

            /////////////// EIGHT TRIANGLE
            eightRackTriangle = new BasicModel(PoolGame.game, "Models\\Racks\\8 balls rack", "Textures\\Racks\\WoodFine0010_S2");
            eightRackTriangle.Position = cueRack.Position + new Vector3(0, 400, -25);
            eightRackTriangle.InitialRotation = Matrix.CreateRotationX(MathHelper.ToRadians(90.0f));

            PoolGame.game.Components.Add(eightRackTriangle);
            

            float ballDiameter = World.ballRadius * 2.0f;
            poolBallsOnCueRack = new BasicModel[7];
            bool[] ballsOnUse = new bool[15] { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };

            for (int i = 0; i < poolBallsOnCueRack.Length; ++i)
            {
                int randomNumber;
                while (ballsOnUse[randomNumber = PoolGame.random.Next(0, 15)]) ;

                poolBallsOnCueRack[i] = new BasicModel(PoolGame.game, "Models\\Balls\\newball", "Textures\\Balls\\ball " + (randomNumber + 1));
                poolBallsOnCueRack[i].Position = cueRack.Position + new Vector3((ballDiameter * (float)(i - 3)), 101.09f + (16.229f + 11.275f), 0);
                poolBallsOnCueRack[i].Rotation = Matrix.CreateRotationY(MathHelper.Pi * (float)PoolGame.random.NextDouble())
                    * Matrix.CreateRotationZ(MathHelper.Pi * (float)PoolGame.random.NextDouble());

                ballsOnUse[randomNumber] = true;
                PoolGame.game.Components.Add(poolBallsOnCueRack[i]);
            }

            float stickDiameter = 30.928f;
            sticksOnCueRack = new BasicModel[6];

            /// +- 30,928 
            
            for (int i = 0; i < sticksOnCueRack.Length; ++i)
            {
                sticksOnCueRack[i] = new BasicModel(PoolGame.game, "Models\\Sticks\\stick_universal");
                if (i < 3)
                    sticksOnCueRack[i].Position = cueRack.Position + new Vector3((stickDiameter * (float)(-i)) - 124.425f, 42.369f, 0);
                else
                    sticksOnCueRack[i].Position = cueRack.Position + new Vector3((stickDiameter * (float)(i - 3)) + 124.425f, 42.369f, 0);

                sticksOnCueRack[i].Rotation = Matrix.CreateRotationY(MathHelper.Pi * (float)PoolGame.random.NextDouble());
                //* Matrix.CreateRotationZ(MathHelper.Pi * (float)PoolGame.random.NextDouble());

                PoolGame.game.Components.Add(sticksOnCueRack[i]);
            }
            #endregion

            /////////////// TV
            tv = new BasicModel(PoolGame.game, "Models\\Cribs\\tv");
            tv.Position = new Vector3(1150-270, 300.0f, 0);
            tv.InitialRotation = Matrix.CreateRotationY(-MathHelper.PiOver2);
            tv.SpecularColor = new Vector4(0.7f, 0.7f, 0.7f, 1.0f);
            tv.Shinennes = 256.0f;

            PoolGame.game.Components.Add(tv);

            /////////////// COLUMNS
            columns = new BasicModel[4];
            columns[0] = new BasicModel(PoolGame.game, "Models\\Cribs\\column");
            columns[0].Position = new Vector3(1250, 0, -960);

            columns[1] = new BasicModel(PoolGame.game, "Models\\Cribs\\column");
            columns[1].Position = new Vector3(-1150, 0, -1100);
            
            columns[2] = new BasicModel(PoolGame.game, "Models\\Cribs\\column");
            columns[2].Position = new Vector3(1150, 0, 1100);

            columns[3] = new BasicModel(PoolGame.game, "Models\\Cribs\\column");
            columns[3].Position = new Vector3(-1150, 0, 1100);

            foreach (BasicModel col in columns)
            {
                //////////////////////////////////
                col.SpecularColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
                col.TEXTURE_ADDRESS_MODE = TextureAddressMode.Wrap;
                PoolGame.game.Components.Add(col);    
            }


            /////////////// ROLLUP DOOR
            //rollupDoor = new BasicModel(PoolGame.game, "Models\\Cribs\\rollup door");
            //rollupDoor.Position = new Vector3(1250, 0.0f, 0.0f);
            //rollupDoor.InitialRotation = Matrix.CreateRotationY(-MathHelper.PiOver2);

            //PoolGame.game.Components.Add(rollupDoor);

            /////////////// SNOW WALL'S PAINTING
            snowPainting = new BasicModel(PoolGame.game, "Models\\Painting\\snow painting");
            snowPainting.Position = new Vector3(0, 350, -1092);
            snowPainting.SpecularColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);

            PoolGame.game.Components.Add(snowPainting);
            
            

            /////////////// ROOF
            roof = new BasicModel(PoolGame.game, "Models\\Cribs\\roof");
            roof.Position = new Vector3(0, 700, 0);
            roof.Scale = new Vector3(2.0f);
            roof.TEXTURE_ADDRESS_MODE = TextureAddressMode.Wrap;
            roof.SpecularColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);

            PoolGame.game.Components.Add(roof);

            /////////////// FLOOR
            floor = new BasicModel(PoolGame.game, "Models\\Cribs\\woodfloor");
            
            floor.TEXTURE_ADDRESS_MODE = TextureAddressMode.Wrap;
            floor.SpecularColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);

            PoolGame.game.Components.Add(floor);

            /////////////// WALLS
            walls = new BasicModel[3];

            walls[0] = new BasicModel(PoolGame.game, "Models\\Cribs\\wall");
            walls[0].Position = new Vector3(0, 0, -1100);
            walls[0].Rotation = Matrix.CreateRotationY(MathHelper.PiOver2);
            walls[0].TEXTURE_ADDRESS_MODE = TextureAddressMode.Mirror;

            walls[1] = new BasicModel(PoolGame.game, "Models\\Cribs\\wall");
            walls[1].Position = new Vector3(1200, 0, 0);

            walls[1].TEXTURE_ADDRESS_MODE = TextureAddressMode.Mirror;

            walls[2] = new BasicModel(PoolGame.game, "Models\\Cribs\\wall");
            walls[2].Position = new Vector3(0, 0, 1100);
            walls[2].Rotation = Matrix.CreateRotationY(MathHelper.PiOver2);
            walls[2].TEXTURE_ADDRESS_MODE = TextureAddressMode.Mirror;

            foreach (BasicModel wall in walls)
            {
                wall.SpecularColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
                PoolGame.game.Components.Add(wall);
            }


            World.scenario.scene.Add(smokestack);
            World.scenario.scene.Add(smokeStackFireWood);
            
            for (int i = 0; i < tabourets.Length; ++i)
                World.scenario.scene.Add(tabourets[i]);

            World.scenario.scene.Add(couch);
            World.scenario.scene.Add(aloneCouch);
            World.scenario.scene.Add(cueRack);
            World.scenario.scene.Add(eightRackTriangle);

            for (int i = 0; i < poolBallsOnCueRack.Length; ++i)
                World.scenario.scene.Add(poolBallsOnCueRack[i]);

            for (int i = 0; i < sticksOnCueRack.Length; ++i)
                World.scenario.scene.Add(sticksOnCueRack[i]);

            World.scenario.scene.Add(tv);

            foreach (BasicModel col in columns)
                World.scenario.scene.Add(col);

            World.scenario.scene.Add(snowPainting);
            World.scenario.scene.Add(roof);
            World.scenario.scene.Add(floor);
            foreach (BasicModel wall in walls)
                World.scenario.scene.Add(wall);

            World.scenario.scene.Add(floor);

            base.Initialize();
            LoadContent();

        }

        /// <summary>
        /// Set the lights for this scene.
        /// </summary>
        public override void LoadLights()
        {
            LightManager.Load();
        }

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (smokestack != null) smokestack.Dispose();
            if (smokeStackFireWood != null) smokeStackFireWood.Dispose();
            if (floor != null) floor.Dispose();
            if (walls != null)
                foreach (BasicModel wall in walls)
                    wall.Dispose();

            if (couch != null) couch.Dispose();
            

            if (eightRackTriangle != null) eightRackTriangle.Dispose();
            if (cueRack != null) cueRack.Dispose();

            if (poolBallsOnCueRack != null)
                foreach (BasicModel ball in poolBallsOnCueRack)
                    ball.Dispose();

            if (sticksOnCueRack != null)
                foreach (BasicModel stick in sticksOnCueRack)
                    stick.Dispose();

            if (snowPainting != null) snowPainting.Dispose();
            if (aloneCouch != null) aloneCouch.Dispose();
            if (tv != null) tv.Dispose();

            if (rollupDoor != null) rollupDoor.Dispose();
            if (roof != null) roof.Dispose();

            if (columns != null)
                foreach (BasicModel col in columns)
                    col.Dispose();

            if (tabourets != null)
                foreach (BasicModel tab in tabourets)
                    tab.Dispose();

            if (fireParticles != null) fireParticles.Dispose();
            if (smokeParticles != null) smokeParticles.Dispose();

            
            base.Dispose(disposing);
        }
        #endregion

        public override void LoadContent()
        {
            base.LoadContent();
        }
        public override void Draw(GameTime gameTime)
        {
            fireParticles.Draw(gameTime);
            smokeParticles.Draw(gameTime);
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            const int fireParticlesPerFrame = 10;

            Vector3 center = new Vector3(1150.0f - 120.0f, 80.0f, -15.0f);
            //Vector3 center = new Vector3(0,200 ,0 );

            for (int i = 0; i < fireParticlesPerFrame; i++)
            {
                //fireParticles.AddParticle(RandomPointOnCube(center, 90.0f), Vector3.Zero);
                fireParticles.AddParticle(center, Vector3.Zero);
            }

            //smokeParticles.AddParticle(new Vector3(0, 200, 0), Vector3.Zero);
            //smokeParticles.AddParticle(new Vector3(0, 200, 0), Vector3.Zero);
            smokeParticles.AddParticle(center + Vector3.Up * 10.0f, Vector3.Zero);
            //smokeParticles.AddParticle(RandomPointOnCube(center + Vector3.Up * 60.0f, 90.0f), Vector3.Zero);
        }

        Vector3 RandomPointOnCircle()
        {
            const float radius = 100;
            const float height = 400;

            double angle = PoolGame.random.NextDouble() * Math.PI * 2;

            float x = (float)Math.Cos(angle);
            float y = (float)Math.Sin(angle);

            return new Vector3(x * radius, y * radius + height, 0);
        }

        Vector3 RandomPointOnCube(Vector3 center, float side)
        {
            float signX = PoolGame.random.NextDouble() < 0.5 ? -1 : 1;
            float signY = PoolGame.random.NextDouble() < 0.5 ? -1 : 1;
            float signZ = PoolGame.random.NextDouble() < 0.5 ? -1 : 1;

            float x = signX * (float)PoolGame.random.NextDouble() * side;
            float y = signY * (float)PoolGame.random.NextDouble() * side;
            float z = signZ * (float)PoolGame.random.NextDouble() * side;

            return new Vector3(x + center.X, y + center.Y, z + center.Z);
        }

        public override void SetParticleSettings()
        {
            fireParticles.SetCamera(World.camera.View, World.camera.Projection);
            smokeParticles.SetCamera(World.camera.View, World.camera.Projection);
        }
    }
}
