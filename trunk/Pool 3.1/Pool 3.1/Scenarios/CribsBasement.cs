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
using XNA_PoolGame.Helpers;

namespace XNA_PoolGame.Scenarios
{
    /// <summary>
    /// Cribs basement.
    /// </summary>
    public class CribsBasement : Scenario
    {
        #region Variables

        public Entity floor = null;
        public Entity[] walls;
        public Entity couch = null;
        
        public Entity eightRackTriangle = null;

        public Entity cueRack = null;
        public Entity[] poolBallsOnCueRack = null;
        public Entity[] sticksOnCueRack = null;

        public Entity snowPainting = null;
        public Entity patriciaPainting = null;
        public Entity aloneCouch = null;
        public Entity tv = null;
        public Entity rollupDoor = null;
        public Entity roof = null;
        public Entity[] columns;
        public Entity[] tabourets;
        public Entity smokestack = null;
        public Entity smokeStackFireWood = null;
        public Entity stairs = null;
        public Entity smokeFireWoodKeeper = null;
        public Entity smokeFireWoodOut = null;
        public Entity carpet = null;
        public Entity bar = null;
        public Entity[] vents = null;
        public Entity vase = null;

        public Entity[] rooflamps;

        public ParticleSystem fireParticles = null;
        public ParticleSystem smokeParticles = null;

        public Vector3[] smokeWoodPositions;

        public VolumetricLightEntity fakeLightShafts;

        // Distortion
        public ParticleSystem heatParticles = null;

        //
        private ParticlesCore core;

        //
        public InstancedEntity ballsinstanced;

        #endregion

        public CribsBasement(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            ///////// INSTANCED MODEL ///////
            ballsinstanced = new InstancedEntity(PoolGame.game, "Models\\Balls\\instanced_newball");
            ballsinstanced.DrawOrder = 5;
            ballsinstanced.DelegateUpdate = delegate { UpdateInstancedWorldMatrix(); };
            PoolGame.game.Components.Add(ballsinstanced);

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
            core = new ParticlesCore(PoolGame.game);
            core.Scenario = this;
            core.AddParticlesFromMultiMap(particles);
            core.AddParticlesFromMultiMap(distortionparticles);
            PoolGame.game.Components.Add(core);


            smokestack = new Entity(PoolGame.game, "Models\\Cribs\\smokestack");
            smokestack.Position = new Vector3(1150.0f - 120.0f, 0, 0);
            smokestack.SpecularColor = Vector4.Zero;
            smokestack.UseNormalMapTextures = true;
            smokestack.UseHeightMapTextures = true;
            smokestack.DrawOrder = 3;
            smokestack.UseModelPartBB = false;
            smokestack.TEXTURE_ADDRESS_MODE = TextureAddressMode.Mirror;
            Light light1 = new Light(smokestack.Position + Vector3.Up * 20.0f);
            light1.LightType = LightType.PointLight;
            light1.DiffuseColor = new Vector4(0.5f, .15f, .075f, 1.0f);
            light1.Radius = 300.0f;
            smokestack.AddLight(light1);

            PoolGame.game.Components.Add(smokestack);

            smokeStackFireWood = new Entity(PoolGame.game, "Models\\Cribs\\firewood");
            smokeStackFireWood.Position = smokestack.Position + new Vector3(0, 40.0f, -40.0f);
            smokeStackFireWood.PreRotation = Matrix.CreateRotationY(-MathHelper.PiOver2);
            smokeStackFireWood.SpecularColor = Vector4.Zero;
            smokeStackFireWood.AditionalLights = smokestack.AditionalLights;

            smokeWoodPositions = new Vector3[2];
            smokeWoodPositions[0] = smokeStackFireWood.Position + new Vector3(0, 35, -20);
            smokeWoodPositions[1] = smokeStackFireWood.Position + new Vector3(15, 45, 20);

            PoolGame.game.Components.Add(smokeStackFireWood);

            smokeFireWoodKeeper = new Entity(PoolGame.game, "Models\\Cribs\\smokefirewoodkeeper");
            smokeFireWoodKeeper.Position = new Vector3(1150.0f - 120.0f, 0.0f, -555.0f);
            smokeFireWoodKeeper.SpecularColor = Vector4.Zero;
            smokeFireWoodKeeper.TEXTURE_ADDRESS_MODE = TextureAddressMode.Wrap;
            smokeFireWoodKeeper.DrawOrder = 3;

            PoolGame.game.Components.Add(smokeFireWoodKeeper);

            smokeFireWoodOut = new Entity(PoolGame.game, "Models\\Cribs\\smokefirewoodout2");
            smokeFireWoodOut.Position = new Vector3(1150.0f - 120.0f, 0.0f, -440.0f);
            smokeFireWoodOut.PreRotation = Matrix.CreateRotationY(-MathHelper.PiOver2);
            smokeFireWoodOut.SpecularColor = Vector4.Zero;
            smokeFireWoodOut.DrawOrder = 3;
            smokeFireWoodOut.UseModelPartBB = false;
            //smokeFireWoodOut.occluder = false;

            PoolGame.game.Components.Add(smokeFireWoodOut);

            tabourets = new Entity[2];
            tabourets[0] = new Entity(PoolGame.game, "Models\\Cribs\\tabouret-design1");
            tabourets[0].Position = new Vector3(-360.0f - 200, 0.0f, -650.0f);
            tabourets[0].PreRotation = Matrix.CreateRotationY(MathHelper.Pi + MathHelper.PiOver4);

            tabourets[1] = new Entity(PoolGame.game, "Models\\Cribs\\tabouret-design1");
            tabourets[1].Position = new Vector3(450.0f - 200, 0.0f, -650.0f);
            tabourets[1].PreRotation = Matrix.CreateRotationY(MathHelper.Pi);

            for (int i = 0; i < tabourets.Length; ++i)
                PoolGame.game.Components.Add(tabourets[i]);

            /////////////// DA L COUCH
            //couch = new Entity(PoolGame.game, "Models\\Cribs\\couch");
            //couch.Position = new Vector3(-200.0f, 0.0f, -850.0f);
            //couch.PreRotation = Matrix.CreateRotationY(MathHelper.PiOver2);
            //couch.SpecularColor = Vector4.Zero;

            //PoolGame.game.Components.Add(couch);

            /////////////// ALONE COUCH 
            //aloneCouch = new Entity(PoolGame.game, "Models\\Cribs\\alone couch");
            ////aloneCouch.Position = new Vector3(940.0f, 0.0f, -650.0f);
            //aloneCouch.Position = new Vector3(940.0f, 0.0f, 650.0f);
            ////aloneCouch.PreRotation = Matrix.CreateRotationY(MathHelper.ToRadians(105.0f));
            //aloneCouch.PreRotation = Matrix.CreateRotationY(MathHelper.ToRadians(105.0f-90.0f));
            //aloneCouch.SpecularColor = Vector4.Zero;

            //PoolGame.game.Components.Add(aloneCouch);

            #region Cue Rack with everything on it
            /////////////// WALL'S CUE RACK
            cueRack = new Entity(PoolGame.game, "Models\\Cribs\\cue rack");
            cueRack.Position = new Vector3(500, 120, 1075 - 250);
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
                while (ballsOnUse[randomNumber = Maths.random.Next(0, 15)]) ;

                poolBallsOnCueRack[i] = new Entity(PoolGame.game, "Models\\Balls\\newball", "Textures\\Balls\\ball " + (randomNumber + 1));
                poolBallsOnCueRack[i].Position = cueRack.Position + new Vector3((ballDiameter * (float)(i - 3)), 101.09f + (16.229f + 11.275f), 0);
                poolBallsOnCueRack[i].Rotation = Matrix.CreateRotationY(MathHelper.Pi * (float)Maths.random.NextDouble())
                    * Matrix.CreateRotationZ(MathHelper.Pi * (float)Maths.random.NextDouble());

                ballsOnUse[randomNumber] = true;
                poolBallsOnCueRack[i].Visible = false;
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

                sticksOnCueRack[i].Rotation = Matrix.CreateRotationY(MathHelper.Pi * (float)Maths.random.NextDouble());
                //* Matrix.CreateRotationZ(MathHelper.Pi * (float)Maths.random.NextDouble());

                PoolGame.game.Components.Add(sticksOnCueRack[i]);
            }
            #endregion

            /////////////// TV
            tv = new Entity(PoolGame.game, "Models\\Cribs\\tv");
            tv.Position = new Vector3(1150-270, 350.0f, 0);
            tv.PreRotation = Matrix.CreateRotationY(-MathHelper.PiOver2);
            tv.SpecularColor = new Vector4(0.1f, 0.1f, 0.1f, 1.0f);
            tv.Shinennes = 256.0f;

            PoolGame.game.Components.Add(tv);

            /////////////// COLUMNS
            columns = new Entity[4];
            columns[0] = new Entity(PoolGame.game, "Models\\Cribs\\column", "Textures\\Cribs\\BrickLargeBare0039_2_S");
            columns[0].Position = new Vector3(1150, 0, -900);

            columns[1] = new Entity(PoolGame.game, "Models\\Cribs\\column", "Textures\\Cribs\\BrickLargeBare0039_2_S");
            columns[1].Position = new Vector3(-1150, 0, -900);

            columns[2] = new Entity(PoolGame.game, "Models\\Cribs\\column", "Textures\\Cribs\\BrickLargeBare0039_2_S");
            columns[2].Position = new Vector3(1150, 0, 1100 - 250);

            columns[3] = new Entity(PoolGame.game, "Models\\Cribs\\column", "Textures\\Cribs\\BrickLargeBare0039_2_S");
            columns[3].Position = new Vector3(-1150, 0, 1100-250);

            foreach (Entity col in columns)
            {
                //////////////////////////////////
                col.SpecularColor = Vector4.Zero;
                col.TEXTURE_ADDRESS_MODE = TextureAddressMode.Wrap;
                col.DrawOrder = 1;
                //col.normalMapAsset = "Textures\\Cribs\\BrickBrown_S_N";
                col.customnormalMapAsset = "Textures\\Cribs\\BrickLargeBare0039_2_S_N";
                col.customheightMapAsset = "Textures\\Cribs\\BrickLargeBare0039_2_S_H";
                
                PoolGame.game.Components.Add(col);    
            }

            /////////////// SNOW WALL'S PAINTING
            snowPainting = new Entity(PoolGame.game, "Models\\Painting\\snow painting");
            snowPainting.Position = new Vector3(0, 350, -892);
            snowPainting.SpecularColor = Vector4.Zero;

            PoolGame.game.Components.Add(snowPainting);

            /////////////// PATRICIA WALL'S PAINTING
            patriciaPainting = new Entity(PoolGame.game, "Models\\Painting\\patriciapainting");
            patriciaPainting.Position = new Vector3(-500, 350, -892);
            patriciaPainting.SpecularColor = Vector4.Zero;

            PoolGame.game.Components.Add(patriciaPainting);

            /////////////// ROOF
            roof = new Entity(PoolGame.game, "Models\\Cribs\\roof", true);

            roof.Position = new Vector3(20, 700, 0);
            roof.Scale = new Vector3(2.0f);
            roof.TEXTURE_ADDRESS_MODE = TextureAddressMode.Mirror;
            roof.SpecularColor = Vector4.Zero;
            roof.occluder = false;
            roof.UseHeightMapTextures = roof.UseNormalMapTextures = true;
            PoolGame.game.Components.Add(roof);

            /////////////// FLOOR
            floor = new Entity(PoolGame.game, "Models\\Cribs\\floor");
            floor.Position = new Vector3(-200.0f, 0.0f, 0.0f);
            floor.TEXTURE_ADDRESS_MODE = TextureAddressMode.Wrap;
            floor.SpecularColor = Vector4.Zero;
            
            floor.UseNormalMapTextures = true;
            floor.UseHeightMapTextures = true;
            //floor.UseHeightMapTextures = floor.UseNormalMapTextures = floor.UseSSAOMapTextures = true;
            
            floor.DrawOrder = 1;
            floor.AditionalLights = smokestack.AditionalLights;

            PoolGame.game.Components.Add(floor);

            /////////////// CARPET
            carpet = new Entity(PoolGame.game, "Models\\Cribs\\carpet");

            carpet.Position = new Vector3(0.0f, 10.0f, 0.0f);
            carpet.TEXTURE_ADDRESS_MODE = TextureAddressMode.Wrap;
            carpet.SpecularColor = Vector4.Zero;
            carpet.customnormalMapAsset = "Textures\\Cribs\\pic81_N";
            carpet.customheightMapAsset = "Textures\\Cribs\\pic81_H";
            //carpet.ssaoMapAsset = "Textures\\Cribs\\PlanksNew0026_9_L2_AO";
            carpet.DrawOrder = 1;

            PoolGame.game.Components.Add(carpet);
            

            /////////////// WALLS
            walls = new Entity[4];

            walls[0] = new Entity(PoolGame.game, "Models\\Cribs\\longwall");
            walls[0].Position = new Vector3(-200, -25, -900);
            walls[0].Rotation = Matrix.CreateRotationY(MathHelper.PiOver2);
            walls[0].TEXTURE_ADDRESS_MODE = TextureAddressMode.Mirror;

            walls[1] = new Entity(PoolGame.game, "Models\\Cribs\\wall2");
            walls[1].Position = new Vector3(1200, -35, 0);

            walls[1].TEXTURE_ADDRESS_MODE = TextureAddressMode.Mirror;

            walls[2] = new Entity(PoolGame.game, "Models\\Cribs\\longwall");
            walls[2].Position = new Vector3(-200, -25, 850);
            walls[2].Rotation = Matrix.CreateRotationY(MathHelper.PiOver2);
            walls[2].TEXTURE_ADDRESS_MODE = TextureAddressMode.Mirror;

            walls[3] = new Entity(PoolGame.game, "Models\\Cribs\\tallestwall");
            walls[3].Position = new Vector3(-1750, -25, 0);
            walls[3].TEXTURE_ADDRESS_MODE = TextureAddressMode.Mirror;

            foreach (Entity wall in walls)
            {
                wall.SpecularColor = Vector4.Zero;
                wall.DrawOrder = 1;

                wall.UseHeightMapTextures = wall.UseNormalMapTextures = true;
                //wall.customnormalMapAsset = "Textures\\Cribs\\ConcreteNew0003_S2_N";
                //wall.customheightMapAsset = "Textures\\Cribs\\ConcreteNew0003_S2_H";

                //wall.MaterialDiffuse = new Vector4(0.7f, 1.0f, 1.0f, 1.0f);
                //wall.TextureAsset = "Textures\\Cribs\\floor_tile_stoneIrregular";

                //wall.normalMapAsset = "Textures\\Cribs\\ConcreteNew0003_S2_N";
                //wall.normalMapAsset = "Textures\\Cribs\\floor_pavement_stone4_2_N";
                //wall.normalMapAsset = "Textures\\Cribs\\PlanksNew0026_9_L2_N";
                PoolGame.game.Components.Add(wall);
            }

            /////////////// STAIRS
            stairs = new Entity(PoolGame.game, "Models\\Cribs\\woodstairs");
            stairs.Position = new Vector3(-1050.0f, 0.0f, 850.0f);
            stairs.SpecularColor = Vector4.Zero;
            stairs.PreRotation = Matrix.CreateRotationY(MathHelper.ToRadians(90.0f));
            stairs.UseSpecularMapTextures = true;
            stairs.SpecularColor = Vector4.One;
            stairs.TEXTURE_ADDRESS_MODE = TextureAddressMode.Wrap;
            
            PoolGame.game.Components.Add(stairs);

            //////////////// ROOF LAMPS
            rooflamps = new Entity[2];
            rooflamps[0] = new Entity(PoolGame.game, "Models\\Cribs\\rooflamp", true);
            rooflamps[0].Position = new Vector3(180.0f, 392.0f, 0.0f);
            
            PostProcessManager.scattering.Position = lights[0].Position + Vector3.Down * 100.0f;

            rooflamps[1] = new Entity(PoolGame.game, "Models\\Cribs\\rooflamp", true);
            rooflamps[1].Position = new Vector3(-180.0f, 392.0f, 0.0f);
            foreach (Entity lamp in rooflamps)
            {
                lamp.TEXTURE_ADDRESS_MODE = TextureAddressMode.Wrap;
                lamp.occluder = false;
                lamp.isScatterObject = true;
                PoolGame.game.Components.Add(lamp);
            }

            ///////////////// BAR
            bar = new Entity(PoolGame.game, "Models\\Cribs\\bar");
            bar.Position = new Vector3(-830, 0, -450);
            bar.Rotation = Matrix.CreateRotationY(-MathHelper.PiOver2);
            bar.SpecularColor = Vector4.Zero;
            PoolGame.game.Components.Add(bar);


            ////////////////// VENTS
            vents = new Entity[2];
            vents[0] = new Entity(PoolGame.game, "Models\\Cribs\\vent");
            vents[0].Position = new Vector3(-400, 600, 840);


            vents[1] = new Entity(PoolGame.game, "Models\\Cribs\\vent");
            vents[1].Position = new Vector3(1000, 600, 840);

            foreach (Entity vent in vents)
            {
                vent.SpecularColor = Vector4.Zero;
                PoolGame.game.Components.Add(vent);
            }

            //////////////////////// VASE
            vase = new Entity(PoolGame.game, "Models\\Cribs\\vase");
            vase.Position = bar.Position + new Vector3(0, 283, -200);
            PoolGame.game.Components.Add(vase);

            fakeLightShafts = new VolumetricLightEntity(PoolGame.game, "Models\\coneAR");
            fakeLightShafts.Position = new Vector3(180.0f, 375.0f - 100.0f, 0.0f);
            fakeLightShafts.TEXTURE_ADDRESS_MODE = TextureAddressMode.Mirror;
            fakeLightShafts.Scale = Vector3.One * 1.75f;
            fakeLightShafts.Cull = CullMode.None;
            ////lightScatter.Scale = new Vector3(5.0f);
            PoolGame.game.Components.Add(fakeLightShafts);

            ////////////////////////////////////////////////
            //World.scenario.Objects.Add(ballsinstanced);
            
            World.scenario.Objects.Add(smokestack);
            World.scenario.Objects.Add(smokeStackFireWood);
            World.scenario.Objects.Add(smokeFireWoodKeeper);
            //World.scenario.Objects.Add(smokeFireWoodOut);
            

            for (int i = 0; i < tabourets.Length; ++i)
                World.scenario.Objects.Add(tabourets[i]);

            //World.scenario.Objects.Add(couch);
            //World.scenario.Objects.Add(aloneCouch);
            World.scenario.Objects.Add(cueRack);
            World.scenario.Objects.Add(eightRackTriangle);

            //for (int i = 0; i < poolBallsOnCueRack.Length; ++i)
            //    World.scenario.Objects.Add(poolBallsOnCueRack[i]);

            for (int i = 0; i < sticksOnCueRack.Length; ++i)
                World.scenario.Objects.Add(sticksOnCueRack[i]);

            World.scenario.Objects.Add(tv);

            //foreach (Entity col in columns)
            //    World.scenario.Objects.Add(col);

            World.scenario.Objects.Add(snowPainting);
            World.scenario.Objects.Add(patriciaPainting);
            World.scenario.Objects.Add(roof);
            World.scenario.Objects.Add(floor);
            World.scenario.Objects.Add(carpet);
            World.scenario.Objects.Add(bar);
            World.scenario.Objects.Add(vase);
            
            
            foreach (Entity wall in walls)
                World.scenario.Objects.Add(wall);

            World.scenario.Objects.Add(stairs);

            //foreach (Entity lamp in rooflamps)
            //    World.scenario.Objects.Add(lamp);

            foreach (Entity vent in vents)
                World.scenario.Objects.Add(vent);

            //World.scenario.objects.Add(fakeLightShafts);
            //volumetriclights.Add(fakeLightShafts);

            base.Initialize();
            LoadContent();

        }

        /// <summary>
        /// Creates the lights for this scene.
        /// </summary>
        public override void CreateLights()
        {
            lights = new List<Light>();

            //Light light1 = new Light(new Vector3(-178, 383, 0));
            Light light1 = new Light(new Vector3(0, 383, 0));

            light1.DiffuseColor = new Vector4(0.9f, 0.85f, 0.9f, 1.0f);
            //light1.DepthBias = 0.01193f; // 2048
            light1.DepthBias = 0.012937f; // 1024
            //light1.DepthBias = 0.008193f;
            light1.LightFarPlane = 1100.0f;
            light1.LightFOV = 2.604f;

            lights.Add(light1);


            //Light light2 = new Light(new Vector3(200, 500, 144));
            Light light2 = new Light(new Vector3(178, 383, 0));
            light2.DiffuseColor = new Vector4(0.6f, 0.6f, 0.6f, 1.0f);
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

        private void UpdateInstancedWorldMatrix()
        {
            if (ballsinstanced.totalinstances != poolBallsOnCueRack.Length)
            {
                ballsinstanced.totalinstances = poolBallsOnCueRack.Length;
                Array.Resize(ref ballsinstanced.transforms, ballsinstanced.totalinstances);
            }

            for (int i = 0; i < ballsinstanced.totalinstances; ++i)
                ballsinstanced.transforms[i] = poolBallsOnCueRack[i].LocalWorld;
            
        }

        public override void LoadContent()
        {
            base.LoadContent();
        }

        public override void UpdateParticles(GameTime gameTime)
        {
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
            lock (syncobject)
            {
                if (World.doDistortion)
                {
                    for (int i = 0; i < 2; ++i)
                    {
                        //heatParticles.AddParticle(center + new Vector3(-45.0f + (float)Maths.random.NextDouble() * 95.0f, 45.0f + (float)Maths.random.NextDouble() * 20.0f, -20.0f + (float)Maths.random.NextDouble() * 40), Vector3.Zero);
                        heatParticles.AddParticle(center + new Vector3(-15.0f, 45.0f , -20.0f), Vector3.Zero);
                    }
                }
            }


            const int smokeParticlesPerFrame = 4/2;

            for (int i = 0; i < smokeParticlesPerFrame; i++)
            {
                //smokeParticles.AddParticle(center + Vector3.Up * 25.0f, Vector3.Zero);
                smokeParticles.AddParticle(smokeWoodPositions[0], Vector3.Zero);
                smokeParticles.AddParticle(smokeWoodPositions[1], Vector3.Zero);
                
            }


            //smokeParticles.AddParticle(new Vector3(0, 200, 0), Vector3.Zero);
            //smokeParticles.AddParticle(new Vector3(0, 200, 0), Vector3.Zero);

            //smokeParticles.AddParticle(Maths.RandomPointOnCube(center + Vector3.Up * 60.0f, 90.0f), Vector3.Zero);
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            smokestack.AditionalLights[0].Position = smokestack.Position + new Vector3(Maths.RamdomNumberBetween(-5.0f, 50.0f), Maths.RamdomNumberBetween(20.0f, 45.0f), Maths.RamdomNumberBetween(-40.0f, 40.0f));
            smokestack.AditionalLights[0].Radius = Maths.RamdomNumberBetween(150, 300);

            smokestack.UpdateLightsProperties();
            smokeStackFireWood.UpdateLightsProperties();
            floor.UpdateLightsProperties();

            base.Update(gameTime);
            
        }

        public override void PrefetchData()
        {
            
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
            if (patriciaPainting != null) patriciaPainting.Dispose();
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

            if (vents != null)
            {
                foreach (Entity vent in vents)
                    vent.Dispose();
            }
            vents = null;

            if (bar != null) bar.Dispose();
            bar = null;
            

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

            if (smokeFireWoodKeeper != null) smokeFireWoodKeeper.Dispose();
            smokeFireWoodKeeper = null;

            if (smokeFireWoodOut != null) smokeFireWoodOut.Dispose();
            smokeFireWoodOut = null;

            if (core != null) core.Dispose();
            core = null;

            if (ballsinstanced != null) ballsinstanced.Dispose();
            ballsinstanced = null;

            //if (lightScatter != null) lightScatter.Dispose();
            //lightScatter = null;
            base.Dispose(disposing);
        }
        #endregion


    }
}
