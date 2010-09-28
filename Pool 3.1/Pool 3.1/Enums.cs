using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNA_PoolGame
{
    /// <summary>
    /// Enum describes the various possible techniques
    /// that can be chosen to implement instancing.
    /// </summary>
    public enum InstancingTechnique
    {
#if XBOX360
        VFetchInstancing,
#else
        HardwareInstancing,
        ShaderInstancing,
#endif
        NoInstancing,
        NoInstancingOrStateBatching
    }

    public enum EnvironmentType
    {
        None,
        Dynamic,
        Static,
        DualParaboloid
    }
    public enum LightType : int
    {
        PointLight = 0,
        DirectionalLight
    }
    public enum VolumeType
    {
        BoundingBoxes,
        BoundingSpheres
    }
    public enum FrustumTest
    {
        Outside,
        Intersect,
        Inside
    }
    public enum DisplacementType
    {
        None,
        NormalMapping,
        ParallaxMapping
    }

    public enum IntermediateBuffer
    {
        PreBloom,
        BlurredHorizontally,
        BlurredBothWays,
        FinalResult,
    }

    public enum ScreenState
    {
        TransitionOn,
        Active,
        TransitionOff,
        Hidden,
    }

    public enum Trajectory
    {
        Motion,
        Free,
        English
    }
    
    public enum GameMode
    {
        EightBalls,
        NineBalls,
        ESPN
    }
    
    public enum TeamNumber
    {
        One = 0,
        Two = 1
    }

    public enum MenuState
    {
        None,
        SplashMode,
        LoadingGameMode,
        MainMenuMode,
        GameMode,
        ExitMode,
        PauseMenuMode

    }

    public enum MatchPhase
    {
        None,
        LaggingShot,
        Playing
    }


    public enum RenderMode
    {
        BasicRender,
        ShadowMapRender,
        PCFShadowMapRender,
        ScreenSpaceSoftShadowRender,
        PSMRender,
        ShadowPostProcessing,
        DEMPass,
        DEMBasicRender,
        MotionBlur,
        DoF,
        DOFCombine,
        Bloom, 
        Menu,
        FPS,
        Light,
        ParticleSystem,
        DistortionParticleSystem,
        RenderGBuffer,
        SSAOPrePass,
        DualParaboloidRenderMaps,
        DPBasicRender
    }
    /// <summary>
    /// Controls the DOF effect being applied
    /// </summary>
    public enum DOFType
    {
        None = 0,
        BlurBuffer = 1,
        BlurBufferDepthCorrection = 2,
        DiscBlur = 3
    }
    public enum MotionBlurType
    {
        None = 0,
        DepthBuffer4Samples = 1,
        DepthBuffer8Samples = 2,
        DepthBuffer12Samples = 3,
        VelocityBuffer4Samples = 4,
        VelocityBuffer8Samples = 5,
        VelocityBuffer12Samples = 6,
        DualVelocityBuffer4Samples = 7,
        DualVelocityBuffer8Samples = 8,
        DualVelocityBuffer12Samples = 9
    }
    public enum ShadowBlurTechnnique
    {
        Normal, 
        SoftShadow
    }
    public enum ShadowTechnnique
    {
        ScreenSpaceShadowMapping,
        PSMShadowMapping,
        VarianceShadowMapping
    }
    public enum ShadingTechnnique
    {
        Foward,
        Deferred
    }

    public enum BallGroupType
    {
        None,
        Stripe,
        Solid
    }
    public enum ScenarioType
    {
        Cribs,
        Garage,
        Bar,
        CristalHotel
    }

    public enum BallCollisionType
    {
        None,
        TwoBalls,
        BallWithRail,
        BallWithInsideRailPocket,
        BallWithPocket
    }

    
}
