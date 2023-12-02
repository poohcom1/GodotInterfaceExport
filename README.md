# Godot Interface Export

Godot C# package to support exporting interface types!

_THIS PROJECT IS NOT PRODUCTION READY!_

## Dev Logs
Here are a couple of solutions I've explored, and their pros and cons and issues

### Source Generated NodePath and Resources (Current)
Annotate an interface property with an attribute, and a source generator will generate NodePath/Resource properties to represent each interface.
An `InpectorPlugin` can be used to enforce the generic with reflection.

```cs
[Tool]
public partial class MyNode : Node
{
    public override partial Array<Dictionary> _GetPropertyList();
    public override partial Variant _Get(StringName property);
    public override partial bool _Set(StringName property, Variant value);

    [ExportNodeInterface]
    public IComponent ComponentNode;

    [ExportResourceInterface]
    public IComponent ComponentNode;

    public override voide _Ready()
    {
        WireComponents(this);
    }
}
```

Pros:
 - Straightforward syntax

Cons:
 - Requires tool
 - Requires `WireComponent()` generated method to get node from paths
 - Requires partial methods

Issues:
 - Source-generated properties reset on build for some reason (see https://github.com/poohcom1/GodotInterfaceExport/issues/1)

### Node Container
Use a generic node to trick Godot into accepting any Node.

```cs
// GenericNode
public partial class GenericNode<T> : Node
{
    public T GetComponent() => (T)(object)this;
}

// Usage
public partial class MyNode : Node
{
    [Export]
    private GenericNode<IComponent> ComponentNode;

    public override void _Ready()
    {
        IComponent component = ComponentNode.GetComponent();
    }
}
```

Pros:
 - Doesn't require tool
 - Doesn't require attribute
 - Doesn't require source generation

Cons:
 - Requires wrapper object

Issues:
 - Godot prevents casting from the actual node type to `GenericNode<T>`

### Resource Container
Use a generic resource type that contains a NodePath field

```cs
// GenericNode
public partial class GenericContainer<T> : Resource
{
    private NodePath _path;
    public T GetComponent(Node root) => (T)root.GetNode(_path);
}

// Usage
public partial class MyNode : Node
{
    [Export]
    private GenericContainer<IComponent> ComponentNode;

    public override void _Ready()
    {
        IComponent component = ComponentNode.GetComponent(this);
    }
}
```

Pros:
 - Same as node container

Cons:
 - Requires one nesting level in export

Issues:
 - Godot fails to serialize a non `[GlobalClass]` resource. We cannot make `GenericContainer<T>` a global class because it contains a generic.
