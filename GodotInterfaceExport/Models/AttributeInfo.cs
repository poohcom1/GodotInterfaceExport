namespace GodotInterfaceExport.Editor.Models;


/// <summary>
/// Details about an interface attributes.
/// </summary>
public struct AttributeInfo(Type interfaceType) { 
    public Type InterfaceType = interfaceType;
}
