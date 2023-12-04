namespace GodotInterfaceExport
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ExportInterface(Type interfaceType, bool generatedProperty = true) : Attribute
    {
        public Type InterfaceType { get; } = interfaceType;
        public bool GeneratedProperty { get; } = generatedProperty;
    }
}
