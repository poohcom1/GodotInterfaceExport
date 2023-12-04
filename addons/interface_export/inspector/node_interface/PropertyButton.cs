#if TOOLS
namespace Addons.InterfaceExport.NodeInterface;
using System;
using Godot;
using GodotInterfaceExport.Services;

internal partial class PropertyButton : Button
{
    private InterfaceAnalyzerService _interfaceAnalyzer;

    public void Init(InterfaceAnalyzerService interfaceAnalyzer)
    {
        _interfaceAnalyzer = interfaceAnalyzer;
    }

    public void Update(Node node)
    {
        if (node != null)
        {
            string nodename = node.Name;
            Texture2D icon = EditorInterface.Singleton
                .GetBaseControl()
                .GetThemeIcon(node.GetClass(), "EditorIcons");

            Text = nodename;
            Icon = icon;
        }
        else
        {
            Text = "Assign...";
            Icon = null;
        }
    }

    public void OnNodeDropped(Action<Node> callback) { }
}
#endif
