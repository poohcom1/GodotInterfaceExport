using Godot;

namespace GodotInterfaceExportTest;

public partial class ComponentBImpl : Node, IComponentB
{
    public void DoSomethingElse()
    {
        GD.Print("Hello from Component B!");
    }
}
