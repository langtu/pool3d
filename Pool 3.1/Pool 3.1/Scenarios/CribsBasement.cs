using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Graphics;
using Microsoft.Xna.Framework.Graphics;
using XNA_PoolGame.Graphics.Particles;
using XNA_PoolGame.Graphics.Particles.Fire;
using XNA_PoolGame.Graphics.Models;
using XNA_PoolGame.Graphics.Shadows;

namespace XNA_PoolGame.Scenarios
{
    public class CribsBasement : Scenario
    {
        public Entity floor = null;
        public Entity[] walls;
        public Entity couch = null;
        
        public Entity eightRackTriangle = null;

        public Entity cueRack = null;
        public Entity[] poolBallsOnCueRack = null;
        public Entity[] sticksOnCueRack = null;

        public Entity snowPainting = null;
        public Entity aloneCouch = null;
        public Entity tv = null;
        public Entity rollupDoor = null;
        public Entity roof = null;
        public Entity[] columns;
        public Entity[] tabourets;
        public Entity smokestack = null;
        public Entity smokeStackFireWood = null;
        public Entity stairs = null;

        public Entity[] rooflamps;

        public ParticleSystem fireParticles = null;
        public ParticleSystem smokeParticles = null;

        // Distortion
        public ParticleSystem heatParticles = null;

        private ParticlesCore core;
        public CribsBasement(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            /////////// PARTICLES /////////// 
            fireParticles = new SmokeStackFireParticleSystem(Game, PoolGame.content);
            fireParticles.DrawOrder = 2;
            PoolGame.game.Components.Add(fireParticles);

            smokeParticles = new SmokePlumeParticleSystem(Game, PoolGame.content);
            smokeParticles.DrawOrder = 1;
            PoolGame.game.Components.Add(smokeParticles);
            ////////////////////////////////
            particles.Add(fireParticles);
            particles.Add(smokeParticles);

            ////// DISTORTION PARTICLES ///// 
            heatParticles = new HeatParticleSystem(Game, PoolGame.content);
            heatParticles.DrawOrder = 3;
            PoolGame.game.Components.Add(heatParticles);

            distortionparticles.Add(heatParticles);

            ////////////////////////////////
            core = new ParticlesCore(Game);
            core.Scenario = this;
            core.AddParticlesFromMultiMap(particles);
            core.AddParticlesFromMultiMap(distortionparticles);
            core.BuildThread(true);

            smokestack = new Entity(PoolGame.game, "Models\\Cribs\\smokestack");
            smokestack.Position = new Vector3(1150.0f - 120.0f, 0, 0);
            //smokestack.InitialRotation = Matrix.CreateRotationY(MathHelper.PiOver2);
            smokestack.SpecularColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            smokestack.DrawOrder = 3;

            PoolGame.game.Components.Add(smokestack);

            smokeStackFireWood = new Entity(PoolGame.game, "Models\\Cribs\\firewood");
            smokeStackFireWood.Position = smokestack.Position + new Vector3(0, 40.0f, -40.0f);
            smokeStackFireWood.PreRotation = Matrix.CreateRotationY(-MathHelper.PiOver2);
            smokeStackFireWood.SpecularColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            //smokeStackFireWood.DrawOrder = -1;

            PoolGame.game.Components.Add(smokeStackFireWood);

            tabourets = new Entity[2];
            tabourets[0] = new Entity(PoolGame.game, "Models\\Cribs\\tabouret-design1");
            tabourets[0].Position = new Vector3(-900.0f, 0.0f, -850.0f);
            tabourets[0].PreRotation = Matrix.CreateRotationY(MathHelper.Pi + MathHelper.PiOver4);

            tabourets[1] = new Entity(PoolGame.game, "Models\\Cribs\\tabouret-design1");
            tabourets[1].Position = new Vector3(700.0f, 0.0f, -850.0f);
            tabourets[1].PreRotation = Matrix.CreateRotationY(MathHelper.Pi);

            for (int i = 0; i < tabourets.Length; ++i)
                PoolGame.game.Components.Add(tabourets[i]);

            /////////////// DA L COUCH
            couch = new Entity(PoolGame.game, "Models\\Cribs\\couch");
            couch.Position = new Vector3(-200.0f, 0.0f, -850.0f);
            couch.PreRotation = Matrix.CreateRotationY(MathHelper.PiOver2);
            couch.SpecularColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);

            PoolGame.game.Components.Add(couch);

            /////////////// ALONE COUCH 
            aloneCouch = new Entity(PoolGame.game, "Models\\Cribs\\alone couch");
            aloneCouch.Position = new Vector3(940.0f, 0.0f, -650.0f);
            aloneCouch.PreRotation = Matrix.CreateRotationY(MathHelper.ToRadians(105.0f));
            aloneCouch.SpecularColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);

            PoolGame.game.Components.Add(aloneCouch);

            #region Cue Rack with everything on it
            /////////////// WALL'S CUE RACK
            cueRack = new Entity(PoolGame.game, "Models\\Cribs\\cue rack");
            cueRack.Position = new Vector3(-500, 120, 1075 - 250);
            cueRack.PreRotation = Matrix.CreateRotationY(MathHelper.ToRadians(180.0f));
            cueRack.SpecularColor = new Vector4(0.4f, 0.4f, 0.4f, 1.0f);

            PoolGame.game.Components.Add(cueRack);

            /////////////// EIGHT TRIANGLE
            eightRackTriangle = new Entity(PoolGame.game, "Models\\Racks\\8 balls rack", "Textures\\Racks\\WoodFine0010_S2");
            eightRackTriangle.Position = cueRack.Position + new Vector3(0, 400, -25);
            eightRackTriangle.PreRotation = Matrix.CreateRotationX(MathHelper.ToRadians(90.0f));

            PoolGame.game.Components.Add(eightRackTriangle);
            

            float ballDiameter = World.ballRadius * 2.0f;
            poolBallsOnCueRack = new Entity[7];
            bool[] ballsOnUse = new bool[15] { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };

            for (int i = 0; i < poolBallsOnCueRack.Length; ++i)
            {
                int randomNumber;
                while (ballsOnUse[randomNumber = PoolGame.random.Next(0, 15)]) ;

                poolBallsOnCueRack[i] = new Entity(PoolGame.game, "Models\\Balls\\newball", "Textures\\Balls\\ball " + (randomNumber + 1));
                poolBallsOnCueRack[i].Position = cueRack.Position + new Vector3((ballDiameter * (float)(i - 3)), 101.09f + (16.229f + 11.275f), 0);
                poolBallsOnCueRack[i].Rotation = Matrix.CreateRotationY(MathHelper.Pi * (float)PoolGame.random.NextDouble())
                    * Matrix.CreateRotationZ(MathHelper.Pi * (float)PoolGame.random.NextDouble());

                ballsOnUse[randomNumber] = true;
                PoolGame.game.Components.Add(poolBallsOnCueRack[i]);
            }

            float stickDiameter = 30.928f;
            sticksOnCueRack = new Entity[6];

            /// +- 30,928 
            
            for (int i = 0; i < sticksOnCueRack.Length; ++i)
            {
                sticksOnCueRack[i] = new Entity(PoolGame.game, "Models\\Sticks\\stick_universal");
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
            tv = new Entity(PoolGame.game, "Models\\Cribs\\tv");
            tv.Position = new Vector3(1150-270, 300.0f, 0);
            tv.PreRotation = Matrix.CreateRotationY(-MathHelper.PiOver2);
            tv.SpecularColor = new Vector4(0.1f, 0.1f, 0.1f, 1.0f);
            tv.Shinennes = 256.0f;

            PoolGame.game.Components.Add(tv);

            /////////////// COLUMNS
            columns = new Entity[4];
            columns[0] = new Entity(PoolGame.game, "Models\\Cribs\\column", "Textures\\Cribs\\floor_pavingStone_ceramic");
            columns[0].Position = new Vector3(1250, 0, -960);

            columns[1] = new Entity(PoolGame.game, "Models\\Cribs\\column", "Textures\\Cribs\\floor_pavingStone_ceramic");
            columns[1].Position = new Vector3(-1150, 0, -1100);

            columns[2] = new Entity(PoolGame.game, "Models\\Cribs\\column", "Textures\\Cribs\\floor_pavingStone_ceramic");
            columns[2].Position = new Vector3(1150, 0, 1100 - 250);

            columns[3] = new Entity(PoolGame.game, "Models\\Cribs\\column", "Textures\\Cribs\\floor_pavingStone_ceramic");
            columns[3].Position = new Vector3(-1150, 0, 1100-250);

            foreach (Entity col in columns)
            {
                //////////////////////////////////
                col.SpecularColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
                col.TEXTURE_ADDRESS_MODE = TextureAddressMode.Wrap;
                col.DrawOrder = 1;
                //col.normalMapAsset = "Textures\\Cribs\\BrickBrown_S_N";
                col.normalMapAsset = "Textures\\Cribs\\floor_pavingStone_ceramic_N";
                
                PoolGame.game.Components.Add(col);    
            }


            /////////////// ROLLUP DOOR
            //rollupDoor = new BasicModel(PoolGame.game, "Models\\Cribs\\rollup door");
            //rollupDoor.Position = new Vector3(1250, 0.0f, 0.0f);
            //rollupDoor.InitialRotation = Matrix.CreateRotationY(-MathHelper.PiOver2);

            //PoolGame.game.Components.Add(rollupDoor);

            /////////////// SNOW WALL'S PAINTING
            snowPainting = new Entity(PoolGame.game, "Models\\Painting\\snow painting");
            snowPainting.Position = new Vector3(0, 350, -1092);
            snowPainting.SpecularColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);

            PoolGame.game.Components.Add(snowPainting);
            
            

            /////////////// ROOF
            roof = new Entity(PoolGame.game, "Models\\Cribs\\roof", "Textures\\Cribs\\ConcreteNew0003_S2");

            roof.Position = new Vector3(0, 700, 0);
            roof.Scale = new Vector3(2.0f);
            roof.TEXTURE_ADDRESS_MODE = TextureAddressMode.Mirror;
            roof.SpecularColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            roof.occluder = false;
            roof.normalMapAsset = "Textures\\Cribs\\ceramic_floor_tile_600x600x9_2mm_N";
            PoolGame.game.Components.Add(roof);

            /////////////// FLOOR
            floor = new Entity(PoolGame.game, "Models\\Cribs\\floor_new", "Textures\\Cribs\\floor_tile_ceramic2");

            floor.TEXTURE_ADDRESS_MODE = TextureAddressMode.Wrap;
            floor.SpecularColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            floor.normalMapAsset = "Textures\\Cribs\\floor_tile_ceramic2_N";
            //floor.normalMapAsset = "Textures\\Cribs\\PlanksNew0026_9_L2_N";
            floor.DrawOrder = 1;


            PoolGame.game.Components.Add(floor);

            /////////////// WALLS
            walls = new Entity[3];

            walls[0] = new Entity(PoolGame.game, "Models\\Cribs\\wall");//"Textures\\Cribs\\floor_pavement_stone4_2"");
            walls[0].Position = new Vector3(0, 0, -1100);
            walls[0].Rotation = Matrix.CreateRotationY(MathHelper.PiOver2);
            walls[0].TEXTURE_ADDRESS_MODE = TextureAddressMode.Mirror;

            walls[1] = new Entity(PoolGame.game, "Models\\Cribs\\wall");
            walls[1].Position = new Vector3(1200, 0, 0);

            walls[1].TEXTURE_ADDRESS_MODE = TextureAddressMode.Mirror;

            walls[2] = new Entity(PoolGame.game, "Models\\Cribs\\wall");
            walls[2].Position = new Vector3(0, 0, 850);
            walls[2].Rotation = Matrix.CreateRotationY(MathHelper.PiOver2);
            walls[2].TEXTURE_ADDRESS_MODE = TextureAddressMode.Mirror;

            foreach (Entity wall in walls)
            {
                wall.SpecularColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
                wall.DrawOrder = 1;
                //wall.TextureAsset = "Textures\\Cribs\\floor_tile_stoneIrregular";

                //wall.normalMapAsset = "Textures\\Cribs\\ConcreteNew0003_S2_N";
                //wall.normalMapAsset = "Textures\\Cribs\\floor_pavement_stone4_2_N";
                //wall.normalMapAsset = "Textures\\Cribs\\PlanksNew0026_9_L2_N";
                PoolGame.game.Components.Add(wall);
            }

            /////////////// STAIRS
            stairs = new Entity(PoolGame.game, "Models\\Cribs\\Lstairs");
            stairs.Position = new Vector3(-500.0f, 0.0f, 750.0f);
            stairs.SpecularColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            stairs.PreRotation = Matrix.CreateRotationY(MathHelper.ToRadians(90.0f));
            stairs.TEXTURE_ADDRESS_MODE = TextureAddressMode.Wrap;
            PoolGame.game.Components.Add(stairs);

            //////////////// ROOF LAMPS
            rooflamps = new Entity[2];
            rooflamps[0] = new Entity(PoolGame.game, "Models\\Cribs\\rooflamp");
            rooflamps[0].Position = new Vector3(180.0f, 392.0f, 0.0f);

            rooflamps[1] = new Entity(PoolGame.game, "Models\\Cribs\\rooflamp");
            rooflamps[1].Position = new Vector3(-180.0f, 392.0f, 0.0f);
            foreach (Entity lamp in rooflamps)
            {
                lamp.TEXTURE_ADDRESS_MODE = TextureAddressMode.Wrap;
                lamp.occluder = false;
                PoolGame.game.Components.Add(lamp);
            }


            ////////////////////////////////////////////////
            World.scenario.Objects.Add(smokestack);
            World.scenario.Objects.Add(smokeStackFireWood);
            
            for (int i = 0; i < tabourets.Length; ++i)
                World.scenario.Objects.Add(tabourets[i]);

            World.scenario.Objects.Add(couch);
            World.scenario.Objects.Add(aloneCouch);
            World.scenario.Objects.Add(cueRack);
            World.scenario.Objects.Add(eightRackTriangle);

            for (int i = 0; i < poolBallsOnCueRack.Length; ++i)
                World.scenario.Objects.Add(poolBallsOnCueRack[i]);

            for (int i = 0; i < sticksOnCueRack.Length; ++i)
                World.scenario.Objects.Add(sticksOnCueRack[i]);

            World.scenario.Objects.Add(tv);

            foreach (Entity col in columns)
                World.scenario.Objects.Add(col);

            World.scenario.Objects.Add(snowPainting);
            World.scenario.Objects.Add(roof);
            World.scenario.Objects.Add(floor);
            foreach (Entity wall in walls)
                World.scenario.Objects.Add(wall);

            World.scenario.Objects.Add(stairs);

            foreach (Entity lamp in rooflamps)
                World.scenario.Objects.Add(lamp);

            base.Initialize();
            LoadContent();

        }

        /// <summary>
        /// Set the lights for this scene.
        /// </summary>
        public override void LoadLights()
        {
            lights = new List<Light>();

            Light light1 = new Light(new Vector3(-178, 383, 0));
            //light1.AmbientColor = new Vector4(0.0f, 0.01f, 0.01f, 1.0f);
            //light1.DepthBias = 0.01193f;
            light1.DepthBias = 0.008193f;
            light1.LightFarPlane = 1100.0f;
            light1.LightFOV = 2.604f;

            lights.Add(light1);


            //Light light2 = new Light(new Vector3(200, 500, 144));
            Light light2 = new Light(new Vector3(178, 383, 0));
            //light2.DiffuseColor = new Vector4(0.85f, 1.0f, 1.0f, 1.0f);
            light2.DepthBias = 0.00382496f;//600 x 480

            //light2.LookAt = new Vector3(0, 0, 0);
            //light2.LookAt = new Vector3(-178 * 10, 0, 0);
            //light2.LightFOV = MathHelper.ToRadians(120.0f);
            light2.LightFarPlane = 1900.0f;
            light2.LightFOV = 2.504f;
            lights.Add(light2);

            LightManager.lights = lights;
            LightManager.Load();
        }

        

        public override void LoadContent()
        {
            base.LoadContent();
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            const int fireParticlesPerFrame = 13;
            //////const int fireParticlesPerFrame = 10;

            Vector3 center = new Vector3(1150.0f - 120.0f, 80.0f, -15.0f);
            //Vector3 center = new Vector3(0,200 ,0 );

            for (int i = 0; i < fireParticlesPerFrame; i++)
            {
                //fireParticles.AddParticle(Maths.RandomPointOnCube(center, 90.0f), Vector3.Zero);
                fireParticles.AddParticle(center, Vector3.Zero);
            }
            fireParticles.AddParticle(center + new Vector3(0, 20, 0), Vector3.Zero);
            for (int i = 0; i < 5; ++i)
            {
                heatParticles.AddParticle(center + new Vector3(-35.0f + (float)PoolGame.random.NextDouble() * 85, 60, -15.0f + (float)PoolGame.random.NextDouble() * 35), Vector3.Zero);
            }


            const int smokeParticlesPerFrame = 20;

            for (int i = 0; i < smokeParticlesPerFrame; i++)
            {
                smokeParticles.AddParticle(center + Vector3.Up * 10.0f, Vector3.Zero);
            }

            
            //smokeParticles.AddParticle(new Vector3(0, 200, 0), Vector3.Zero);
            //smokeParticles.AddParticle(new Vector3(0, 200, 0), Vector3.Zero);
            
            //smokeParticles.AddParticle(Maths.RandomPointOnCube(center + Vector3.Up * 60.0f, 90.0f), Vector3.Zero);
        }

        
        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (smokestack != null) smokestack.Dispose();
            if (smokeStackFireWood != null) smokeStackFireWood.Dispose();
            if (floor != null) floor.Dispose();
            if (walls != null)
            {
                foreach (Entity wall in walls)
                    wall.Dispose();
            }

            if (couch != null) couch.Dispose();


            if (eightRackTriangle != null) eightRackTriangle.Dispose();
            if (cueRack != null) cueRack.Dispose();

            if (poolBallsOnCueRack != null)
            {
                foreach (Entity ball in poolBallsOnCueRack)
                    ball.Dispose();
            }

            if (sticksOnCueRack != null)
            {
                foreach (Entity stick in sticksOnCueRack)
                    stick.Dispose();
            }

            if (snowPainting != null) snowPainting.Dispose();
            if (aloneCouch != null) aloneCouch.Dispose();
            if (tv != null) tv.Dispose();

            if (rollupDoor != null) rollupDoor.Dispose();
            if (roof != null) roof.Dispose();

            if (columns != null)
            {
                foreach (Entity col in columns)
                    col.Dispose();
            }

            if (tabourets != null)
            {
                foreach (Entity tab in tabourets)
                    tab.Dispose();
            }

            if (fireParticles != null) fireParticles.Dispose();
            if (smokeParticles != null) smokeParticles.Dispose();
            if (heatParticles != null) heatParticles.Dispose();

            fireParticles = null;
            smokeParticles = null;
            heatParticles = null;

            if (stairs != null) stairs.Dispose();
            if (rooflamps != null)
                foreach (Entity lamp in rooflamps)
                    lamp.Dispose();
                
            core.Dispose();
            base.Dispose(disposing);
        }
        #endregion
    }
}
