namespace GodotInterfaceExportTest;
using Godot;

public partial class TestNode : Node
{
    [Export]
    public ComponentContainer Components { get; set; }

    public override void _Ready()
    {
        base._Ready();

        Components.WireComponents(this);
        Components.ComponentA.DoSomething();
        Components.ComponentB.DoSomethingElse();
        Components.ResourceA.DoSomething();
    }
}
