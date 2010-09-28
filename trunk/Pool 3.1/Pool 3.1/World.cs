using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNA_PoolGame.PoolTables;
using XNA_PoolGame.Cameras;
using XNA_PoolGame.Scenarios;
using Microsoft.Xna.Framework;
using XNA_PoolGame.Graphics;
using XNA_PoolGame.Match;
using XNA_PoolGame.PoolTables.Racks;
using XNA_PoolGame.Screens;

namespace XNA_PoolGame
{
    /// <summary>
    /// World.
    /// </summary>
    public static class World
    {
        // LIGHTS
        public static int TotalLights = 1;

        /// <summary>
        /// Current camera.
        /// </summary>
        public static Camera camera = null;
        public static EmptyCamera emptycamera = null;

        public static bool Debug = false;

        
        public static float gravity = -980.0f; // -9.8 m/s^2 ....
        public static float timeFactor = 1.0f; //2.5f;

        public static Vector3 mGravity = new Vector3(0.0f, gravity, 0.0f);

        public static Scenario scenario = null;
        public static PoolTable poolTable = null;
        public static float ballRadius = 11.275f; //8.75f;

        public static Screen currentScreen = null;
        public static Cursor cursor = null;


        /// <summary>
        /// Collection of players.
        /// </summary>
        public static Player[] players = new Player[4];
        /// <summary>
        /// Index of the current player.
        /// </summary>
        public static int playerInTurnIndex = -1;
        public static int playerCount = 0;

        public static Player CurrentPlayer
        {
            get
            {
                if (playerInTurnIndex == -1) return null;
                return players[playerInTurnIndex];
            }
        }

        /// <summary>
        /// Match referee.
        /// </summary>
        public static Referee referee;
        public static ScenarioType scenarioType = ScenarioType.Cribs;
        public static GameMode gameMode = GameMode.NineBalls;
        public static Dictionary<GameMode, RackFactory> rackfactories;

        /// <summary>
        /// Match teams.
        /// </summary>
        public static Team[] teams = new Team[2];

        // DOF SETTINGS
        public static DOFType dofType = DOFType.None;

        // MOTION BLUR
        public static MotionBlurType motionblurType = MotionBlurType.None;

        // SHADOWS SETTINGS
        public static bool displayShadows = true;
        public static bool displayShadowsTextures = true;
        public static int shadowMapSize = 1024 / 2;
        public static ShadowTechnnique shadowTechnique = ShadowTechnnique.ScreenSpaceShadowMapping;
        //
        public static ShadingTechnnique shadingTech = ShadingTechnnique.Foward;

        // NORMAL MAPPING
        public static DisplacementType displacementType = DisplacementType.None;
        public static Vector2 scaleBias = new Vector2(0.04f, -0.03f);

        //
        public static bool doNormalPositionPass = false;

        //
        public static bool useSSAOTextures = true;
        public static bool doSSAO = false;
        
        // BLOOM
        public static bool BloomPostProcessing = true;

        // THREADS
        public static bool UseThreads = false;
        public static BallCollider ballcollider = null;

        // DISTORTION
        public static bool doDistortion = false;
        public static bool drawParticles = true;

        // INSTANCING MODELS
        public static InstancingTechnique instancingTech = InstancingTechnique.HardwareInstancing;

        //
        public static EnvironmentType EM = EnvironmentType.DualParaboloid;
        public static int DPDivisor = 2;
        public static int EMSize = 64;

        public static bool doLightshafts = false;
    }
}
