namespace GodotInterfaceExportTest;
using Godot;

public partial class GenericResourceContainer<T> : BaseGenericResourceContainer
{
    public T GetComponent(Node node)
    {
        return (T)(object)node.GetNode(Path);
    }
}
