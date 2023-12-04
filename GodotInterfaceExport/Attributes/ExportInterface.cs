namespace GodotInterfaceExport
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ExportInterface(Type interfaceType, bool generateProperty = true) : Attribute
    {
        public Type InterfaceType { get; } = interfaceType;
        public bool GenerateProperty { get; } = generateProperty;
    }
}
