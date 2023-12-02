namespace GodotInterfaceExportTest;
using Godot;
using Godot.Collections;
using GodotInterfaceExport.Attributes;

[Tool]
public partial class TestNode : Node
{
    [Export]
    public ComponentContainer Components { get; set; }

    [ExportNodeInterface]
    public IComponentA ComponentA { set; get; }

    public override void _Ready()
    {
        base._Ready();

        if (Engine.IsEditorHint())
            return;

        WireComponents(this);
        ComponentA.DoSomething();

        Components.WireComponents(this);
        Components.ComponentA.DoSomething();
        Components.ComponentB.DoSomethingElse();
        Components.ResourceA.DoSomething();
    }

    public override partial Array<Dictionary> _GetPropertyList();

    public override partial Variant _Get(StringName property);

    public override partial bool _Set(StringName property, Variant value);
}
