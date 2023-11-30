namespace GodotComponentExportTest;
using Godot;
using Godot.Collections;
using GodotComponentExport.Attributes;

public partial class TestNode : Node
{
    [ExportInterface]
    public Node2D Component { set; get; }

    public override void _Ready()
    {
        base._Ready();
        WireComponents();
    }
}
