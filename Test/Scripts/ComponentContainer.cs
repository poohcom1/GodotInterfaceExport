using Godot;
using Godot.Collections;
using GodotInterfaceExport.Attributes;

namespace GodotInterfaceExportTest;

[Tool]
[GlobalClass]
public partial class ComponentContainer : Resource
{
    [ExportNodeInterface]
    public IComponentA ComponentA { get; set; }

    [ExportNodeInterface]
    public IComponentB ComponentB { get; set; }

    public override partial Array<Dictionary> _GetPropertyList();

    public override partial Variant _Get(StringName property);

    public override partial bool _Set(StringName property, Variant value);
}
