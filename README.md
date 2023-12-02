# Godot Interface Export

Godot C# package to support exporting interface types!

_THIS PROJECT IS NOT PRODUCTION READY!_

## Dev Logs
Here are a couple of solutions I've explored, and their pros and cons and issues.

### 1. Source Generated Exports
This is the current solution implemented. The issue is not a breaking one, but is still a major annoyance.

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

| Pros | Cons | Issues |
| -- | -- | -- |
|  - No wrappers |  - Requires tool <br> - Requires `WireComponent()` generated method to get node from paths <br> - Requires partial methods | Source-generated properties reset on build for some reason (see https://github.com/poohcom1/GodotInterfaceExport/issues/1) |

### 2. Node Container
Use a generic node to trick Godot into accepting any Node. This hasn't worked so far due to the casting issue.

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

| Pros | Cons | Issues |
| -- | -- | -- |
|  - Doesn't require tool <br>  - Doesn't require attribute <br> - Doesn't require source generation | - Requires wrapper object | Godot prevents casting from the actual node type to `GenericNode<T>` |

### 3. Resource Container
Use a generic resource type that contains a NodePath field. This did not work so far due to the resource script issue.

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

| Pros | Cons | Issues |
| -- | -- | -- |
| Same as node container | - Requires one nesting level in export | Godot fails to serialize a non `[GlobalClass]` resource. We cannot make `GenericContainer<T>` a global class because it contains a generic. |

