using System;

public enum PanelType
{
    CLASSIC,
    POPUP
}

[Flags]
public enum SceneSharedEle
{
    NONE = 0,
    KEEP = 1,
    COIN = 128,
    SETTING = 256,
    BACK = 512,
    DISABLE = 1024,
}
