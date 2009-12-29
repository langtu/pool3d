using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UPool
{
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
    public enum GameModes
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

    public enum ChangeMessageType
    {
        UpdateCameraView,
        UpdateWorldMatrix,
        UpdateHighlightColor,
        CreateNewRenderData,
        DeleteRenderData,
    }

    
}
