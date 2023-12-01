using Godot;

namespace GodotInterfaceExportTest;

[GlobalClass]
public partial class ResourceA : Resource, IComponentA
{
    public void DoSomething()
    {
        GD.Print("Hello from Resource A!");
    }
}
