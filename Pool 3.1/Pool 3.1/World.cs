using System;
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
        // LIGHTS
        public static int TotalLights = 1;

        public static Camera camera = null;
        public static EmptyCamera emptycamera = null;

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
        public static int shadowMapSize = 1024 / 2;
        public static ShadowTechnnique shadowTechnique = ShadowTechnnique.ScreenSpaceShadowMapping;
        // NORMAL MAPPING
        public static DisplacementType displacementType = DisplacementType.None;
        public static Vector2 scaleBias = new Vector2(0.04f, -0.03f);
        public static bool useSSAO = true;
        
        // BLOOM
        public static bool BloomPostProcessing = true;

        // THREADS
        public static bool UseThreads = false;
        public static BallCollider ballcollider = null;

        // DISTORTION
        public static bool doDistortion = false;

        // INSTANCING MODELS
        public static InstancingTechnique instancingTech = InstancingTechnique.HardwareInstancing;

        //
        public static EnvironmentType dem = EnvironmentType.None;

        //
        public static ShadingTechnnique shadingTech = ShadingTechnnique.Foward;
    }
}
