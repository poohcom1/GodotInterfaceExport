namespace GodotInterfaceExportTest;
using Godot;
using Godot.Collections;
using GodotInterfaceExport;

public partial class TestNode : Node
{
    [Export]
    [ExportInterface(typeof(IComponentA))]
    public Node ComponentANode { set; get; }

    public override void _Ready()
    {
        base._Ready();

        ComponentA.DoSomething();
    }
}
