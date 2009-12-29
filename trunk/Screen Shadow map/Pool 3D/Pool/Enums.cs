using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNA_PoolGame
{
    public enum FrustumTest
    {
        Outside,
        Intersect,
        Inside
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
        Potting,
        Free
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
        DoF,
        DOFCombine,
        Bloom, 
        Menu,
        FPS,
        Light,
        ParticleSystem
    }

    public enum Shadow
    {
        Normal, 
        SoftShadow
    }

    public enum ScenarioType
    {
        Cribs,
        Garage,
        Bar
    }
    public enum ChangeMessageType
    {
        UpdateCameraView,
        UpdateWorldMatrix,
        UpdateHighlightColor,
        CreateNewRenderData,
        DeleteRenderData,
    }

    
}
