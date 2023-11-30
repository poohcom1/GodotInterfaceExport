using Godot;

namespace GodotInterfaceExportTest;

public partial class ComponentAImpl : Node, IComponentA
{
    public void DoSomething()
    {
        GD.Print("Hello from Component A!");
    }
}
