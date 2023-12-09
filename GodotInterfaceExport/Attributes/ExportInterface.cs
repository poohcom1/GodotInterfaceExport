namespace GodotInterfaceExport
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ExportInterface(Type interfaceType, bool generateProperty = false) : Attribute
    {
        public Type InterfaceType { get; } = interfaceType;
        public bool GenerateProperty { get; } = generateProperty;
    }
}
