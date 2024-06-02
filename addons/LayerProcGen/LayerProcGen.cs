using Godot;
using System;
using System.IO;
using System.Reflection;

namespace Runevision.LayerProcGen;

[Tool]
public partial class LayerProcGen : EditorPlugin
{
    private static string? defaultBasePath; // "res://addons/LayerProcGen/"

    public DebugOptionsWindow? DebugOptionsEditor { get; set; }

    public override void _EnterTree()
    {
        //TODO: Drawing debug elements like that doesn't work in Godot rn.
        // DebugOptionsEditor = new DebugOptionsWindow();
        // DebugOptionsEditor.Name = "Debug Options";
        // AddControlToDock(DockSlot.LeftUl, DebugOptionsEditor);
    }

    public override string _GetPluginName()
    {
        return "LayerProcGen";
    }

    public override Texture2D _GetPluginIcon()
    {
        return ResourceLoader.Load<Texture2D>(ResourcePath("Resources/LayerProcGenLogoSmall.png"));
    }

    public override bool _HasMainScreen()
    {
        return false;
    }

    public override void _MakeVisible(bool visible)
    {
        if (DebugOptionsEditor != null) DebugOptionsEditor.Visible = visible;
    }

    // public override void _SaveExternalData()
    // {
    //     DebugOptionsEditor?.Save();
    // }

    public override void _ExitTree()
    {
        RemoveControlFromDocks(DebugOptionsEditor);
        DebugOptionsEditor?.QueueFree();
        // RemoveInspectorPlugin(DebugWindowPlugin);
    }

    public static string ResourcePath(string subPath)
    {
        defaultBasePath ??= typeof(LayerProcGen).GetCustomAttribute<ScriptPathAttribute>()?.Path.Replace($"{nameof(LayerProcGen)}.cs", "");
        return Path.Join(defaultBasePath, subPath);
    }
}
