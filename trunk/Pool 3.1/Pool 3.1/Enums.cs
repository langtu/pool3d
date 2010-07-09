using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNA_PoolGame
{
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
        NormalMapping//,
        //ParallaxMapping
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
        Black,
        NineBalls,
        ESPN
    }
    public enum TeamNumber
    {
        One,
        Two
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

    public enum RenderMode
    {
        BasicRender,
        ShadowMapRender,
        PCFShadowMapRender,
        ScreenSpaceSoftShadowRender,
        PSMRender,
        ShadowPostProcessing,
        MotionBlur,
        DoF,
        DOFCombine,
        Bloom, 
        Menu,
        FPS,
        Light,
        ParticleSystem,
        DistortionParticleSystem
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
        PSMShadowMapping
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
