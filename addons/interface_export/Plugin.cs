#pragma warning disable CS8618
#if TOOLS
namespace Addons.InterfaceExport;
using Godot;
using System;

[Tool]
public partial class Plugin : EditorPlugin
{
    private InterfaceExportInspectorPlugin _inspectorPlugin;

    public override void _EnterTree()
    {
        AddInspectorPlugin(_inspectorPlugin = new InterfaceExportInspectorPlugin());
    }

    public override void _ExitTree()
    {
        RemoveInspectorPlugin(_inspectorPlugin);
    }
}
#endif
