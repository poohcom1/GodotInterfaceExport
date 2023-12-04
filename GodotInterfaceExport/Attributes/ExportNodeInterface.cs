namespace GodotInterfaceExport
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class ExportNodeInterface(Type interfaceType, bool generateField = true) : Attribute
    {
        public Type InterfaceType { get; } = interfaceType;
        public bool GenerateField { get; } = generateField;
    }
}
