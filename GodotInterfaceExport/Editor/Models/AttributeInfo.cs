namespace GodotInterfaceExport.Editor.Models;

public enum InterfaceAttributeType { 
    Node,
    Resource
}

/// <summary>
/// Details about an interface attributes.
/// </summary>
public struct AttributeInfo(InterfaceAttributeType attributeType, Type interfaceType) { 
    public InterfaceAttributeType AttributeType = attributeType;
    public Type InterfaceType = interfaceType;
}
