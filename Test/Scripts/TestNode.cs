namespace GodotInterfaceExportTest;
using Godot;
using Godot.Collections;
using GodotInterfaceExport;

[Tool]
public partial class TestNode : Node
{
    [Export]
    [ExportNodeInterface(typeof(IComponentA))]
    public Node ComponentANode { set; get; }

    public override void _Ready()
    {
        base._Ready();

        ComponentA.DoSomething();
    }
}
