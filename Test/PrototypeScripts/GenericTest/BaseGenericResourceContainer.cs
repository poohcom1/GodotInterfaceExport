namespace GodotInterfaceExportTest;
using Godot;

[GlobalClass]
public partial class BaseGenericResourceContainer : Resource
{
    [Export]
    public NodePath Path;
}
