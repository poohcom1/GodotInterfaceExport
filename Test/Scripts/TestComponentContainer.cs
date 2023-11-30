using Godot;
using Godot.Collections;

namespace GodotInterfaceExportTest;

[Tool]
[GlobalClass]
public partial class TestComponentContainer : Resource
{
    public IComponentA ComponentA { get; set; }
    public IComponentB ComponentB { get; set; }

    public void WireComponents(Node root)
    {
        ComponentA = root.GetNodeOrNull<IComponentA>(_exportedNodeInterfaces["ComponentA"]);
        ComponentB = root.GetNodeOrNull<IComponentB>(_exportedNodeInterfaces["ComponentB"]);
    }

    private Dictionary<string, string> _exportedNodeInterfaces = new Dictionary<string, string>()
    {
        { "ComponentA", "" },
        { "ComponentB", "" },
    };

    public override Variant _Get(StringName property)
    {
        if (_exportedNodeInterfaces.ContainsKey(property))
        {
            return _exportedNodeInterfaces[property];
        }
        return base._Get(property);
    }

    public override bool _Set(StringName property, Variant value)
    {
        if (_exportedNodeInterfaces.ContainsKey(property))
        {
            _exportedNodeInterfaces[property] = value.AsString();
            return true;
        }
        return base._Set(property, value);
    }

    public override Array<Dictionary> _GetPropertyList()
    {
        var properties = new Array<Dictionary>();

        properties.Add(
            new Dictionary
            {
                { "name", "ComponentA" },
                { "type", (int)Variant.Type.NodePath },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint", (int)PropertyHint.NodePathValidTypes },
                { "hint_string", "Node" },
            }
        );

        properties.Add(
            new Dictionary
            {
                { "name", "ComponentB" },
                { "type", (int)Variant.Type.NodePath },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint", (int)PropertyHint.NodePathValidTypes },
                { "hint_string", "Node" },
            }
        );

        return properties;
    }
}
