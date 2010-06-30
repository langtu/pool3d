﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNA_PoolGame.PoolTables;
using XNA_PoolGame.Cameras;
using XNA_PoolGame.Scenarios;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Graphics;

namespace XNA_PoolGame
{
    public static class World
    {
        public static Camera camera = null;

        public static bool Debug = false;

        
        public static float gravity = -980.0f; // -9.8 m/s^2 ....
        public static float timeFactor = 1.0f; //2.5f;

        public static Vector3 mGravity = new Vector3(0.0f, gravity, 0.0f);

        public static Scenario scenario = null;
        public static PoolTable poolTable = null;
        public static float ballRadius = 11.275f; //8.75f;

        public static Player[] players = new Player[4];
        public static int playerInTurn = -1;
        public static int playerCount = 0;

        public static ScenarioType scenarioType = ScenarioType.Cribs;
        public static GameMode gameMode = GameMode.Black;

        // DOF SETTINGS
        public static DOFType dofType = DOFType.None;

        // MOTION BLUR
        public static MotionBlurType motionblurType = MotionBlurType.None;

        // SHADOWS SETTINGS
        public static bool displayShadows = true;
        public static bool displayShadowsTextures = false;
        public static bool displaySceneFromLightSource = false;
        public static int shadowMapSize = 1024 / 2;
        public static ShadowTechnnique shadowTechnique = ShadowTechnnique.ScreenSpaceShadowMapping;
        public static int lightpass = 0;
        // NORMAL MAPPING
        public static DisplacementType displacementType = DisplacementType.None;
        

        // BLOOM
        public static bool BloomPostProcessing = true;

        // THREADS
        public static bool UseThreads = true;
        public static BallCollider ballcollider = null;

    }
}