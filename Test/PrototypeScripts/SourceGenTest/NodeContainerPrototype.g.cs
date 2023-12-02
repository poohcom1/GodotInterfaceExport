namespace GodotInterfaceExportTest;
using Godot;
using Godot.Collections;

public partial class NodeContainerPrototype : Node
{


    private Dictionary<string, Variant> _exportedInterfaces = new Dictionary<string, Variant>()
    {
        { "ComponentA", "" },
        { "ComponentB", "" },
        { "ResourceA", new Variant() },
    };

    private static readonly Array<Dictionary> _properties =
        new()
        {
            new Dictionary
            {
                { "name", "ComponentA" },
                { "type", (int)Variant.Type.NodePath },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint", (int)PropertyHint.NodePathValidTypes },
                { "hint_string", "Node" },
            },
            new Dictionary
            {
                { "name", "ComponentB" },
                { "type", (int)Variant.Type.NodePath },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint", (int)PropertyHint.NodePathValidTypes },
                { "hint_string", "Node" },
            },
            new Dictionary
            {
                { "name", "ResourceA" },
                { "type", (int)Variant.Type.Object },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint", (int)PropertyHint.ResourceType },
                { "hint_string", "Resource" },
            }
        };

    public override partial Array<Dictionary> _GetPropertyList()
    {
        return _properties;
    }

    public override partial Variant _Get(StringName property)
    {
        if (_exportedInterfaces.ContainsKey(property))
            return _exportedInterfaces[property];
        return base._Get(property);
    }

    public override partial bool _Set(StringName property, Variant value)
    {
        if (_exportedInterfaces.ContainsKey(property))
        {
            _exportedInterfaces[property] = value;
            return true;
        }
        return base._Set(property, value);
    }

    public void WireComponents(Node root)
    {
        ComponentA = root.GetNodeOrNull<IComponentA>(_exportedInterfaces["ComponentA"].AsString());
        ComponentB = root.GetNodeOrNull<IComponentB>(_exportedInterfaces["ComponentB"].AsString());
    }
}
