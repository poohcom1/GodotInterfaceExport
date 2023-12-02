namespace GodotInterfaceExportTest;
using Godot;
using System;

public partial class MainNodeTest : Node
{
    [Export]
    public GenericNodeContainer<IComponentA> ComponentA = new();

    [Export]
    public Node Node;

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
            return;

        ComponentA.GetComponent().DoSomething();
    }
}
