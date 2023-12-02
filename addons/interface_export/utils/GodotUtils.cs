namespace Addons.InterfaceExport;
using Godot;

internal static class GodotUtils
{
    public static string GetNodeScriptPath(Node node)
    {
        Script script = (Script)node.GetScript();

        if (script is null || script is not CSharpScript)
        {
            return "";
        }

        return ProjectSettings.GlobalizePath(script.ResourcePath);
    }
}
