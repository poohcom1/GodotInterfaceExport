using Microsoft.CodeAnalysis;

namespace GodotInterfaceExport.Models {
    public class NodeInterfaceModel(string propertyName, string interfaceType, bool generateField, Location syntaxLocation) {
        const string NAME_SUFFIX = "Node";

        public string PropertyName { get; } = propertyName;
        public string InterfaceType { get; } = interfaceType;
        public bool GenerateField { get; } = generateField;
        public Location SyntaxLocation { get; } = syntaxLocation;

        public bool IsNameValid() {
            return PropertyName.EndsWith(NAME_SUFFIX);
        }

        public string GetGeneratedName() {
            return PropertyName.Substring(0, PropertyName.Length - NAME_SUFFIX.Length);
        }
    }
}