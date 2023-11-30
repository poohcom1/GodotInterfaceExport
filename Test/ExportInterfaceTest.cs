using Godot;
using GodotComponentExport.Attributes;

namespace Test;

public interface IComponent { }

public partial class ExportInterfaceTest : Node
{
    [ExportInterface]
    public IComponent Test { set; get; }

    [ExportInterface]
    public IComponent Test2 { set; get; }

    public override void _Ready()
    {
        base._Ready();
        WireComponents();
    }
}
