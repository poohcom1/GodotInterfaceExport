namespace GodotInterfaceExportTest;
using Godot;
using Godot.Collections;
using GodotInterfaceExportTest;

[Tool]
public partial class NodeContainerPrototype : Node
{
    IComponentA ComponentA;
    IComponentB ComponentB;
    IComponentA ResourceB;

    public override void _Ready()
    {
        base._Ready();
    }

    public override partial Array<Dictionary> _GetPropertyList();

    public override partial Variant _Get(StringName property);

    public override partial bool _Set(StringName property, Variant value);
}
