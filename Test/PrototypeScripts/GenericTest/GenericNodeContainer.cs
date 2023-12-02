namespace GodotInterfaceExportTest;
using Godot;

public partial class GenericNodeContainer<T> : Node
{
    public T GetComponent()
    {
        return (T)(object)this;
    }
}
