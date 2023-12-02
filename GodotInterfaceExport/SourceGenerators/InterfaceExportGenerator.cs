using GodotInterfaceExport.Attributes;
using GodotSharp.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace GodotInterfaceExport.SourceGenerators
{
    /// <summary>
    /// Adapted from https://github.dev/Cat-Lips/GodotSharp.SourceGenerators.SourceGeneratorForDeclaredMemberWithAttribute.cs
    /// </summary>
    [Generator]
    internal class InterfaceExportGenerator : IIncrementalGenerator
    {

        private static readonly string nodeAttributeType = typeof(ExportNodeInterface).Name;
        private static readonly string resourceAttributeType = typeof(ExportResourceInterface).Name;
        private static readonly string nodeAttributeName = Regex.Replace(nodeAttributeType, "Attribute$", "", RegexOptions.Compiled);
        private static readonly string resourceAttributeName = Regex.Replace(resourceAttributeType, "Attribute$", "", RegexOptions.Compiled);
        private static readonly string[] attributeNames = [nodeAttributeName, resourceAttributeName];

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var syntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(
                IsSyntaxTarget,
                GetSyntaxTarget
            );
            var compilationProvider = context.CompilationProvider
                .Combine(syntaxProvider.Collect())
                .Combine(context.AnalyzerConfigOptionsProvider);
            context.RegisterImplementationSourceOutput(
                compilationProvider,
                (context, provider) =>
                    OnExecute(context, provider.Left.Left, provider.Left.Right, provider.Right)
            );

            static bool IsSyntaxTarget(SyntaxNode node, CancellationToken _)
            {
                return node is PropertyDeclarationSyntax type && HasAttributeType();

                bool HasAttributeType()
                {
                    if (type.AttributeLists.Count is 0)
                        return false;

                    foreach (var attributeList in type.AttributeLists)
                    {
                        foreach (var attribute in attributeList.Attributes)
                        {
                            var name = attribute.Name.ToString();
                            if (attributeNames.Contains(name))
                                return true;
                        }
                    }

                    return false;
                }
            }

            static PropertyDeclarationSyntax GetSyntaxTarget(
                GeneratorSyntaxContext context,
                CancellationToken _
            ) => (PropertyDeclarationSyntax)context.Node;

            void OnExecute(
                SourceProductionContext context,
                Compilation compilation,
                ImmutableArray<PropertyDeclarationSyntax> nodes,
                AnalyzerConfigOptionsProvider options
            )
            {
                // class -> attribute -> properties
                Dictionary<INamedTypeSymbol, Dictionary<string, List<PropertyDeclarationSyntax>>> interfaces = [];


                foreach (var node in nodes.Distinct())
                {
                    if (context.CancellationToken.IsCancellationRequested)
                        return;
                    var model = compilation.GetSemanticModel(node.SyntaxTree);
                    var symbol = model.GetDeclaredSymbol(node);
                    if (node is not PropertyDeclarationSyntax property)
                        continue;
                    if (symbol is null || symbol.ContainingType is null)
                        continue;

                    if (!interfaces.ContainsKey(symbol.ContainingType))
                    {
                        interfaces[symbol.ContainingType] = [];
                        foreach (string attr in attributeNames)
                        {
                            interfaces[symbol.ContainingType][attr] = [];
                        }
                    }

                    foreach (string attr in attributeNames)
                    {
                        var attributeSymbol = symbol
                            .GetAttributes()
                            .SingleOrDefault(x => x.AttributeClass?.Name == attr);
                        if (attributeSymbol is null)
                            continue;
                        interfaces[symbol.ContainingType][attr].Add(property);
                    }
                }

                foreach (var classSymbol in interfaces.Keys)
                {
                    if (context.CancellationToken.IsCancellationRequested)
                        return;

                    var _nodeInterfaces = interfaces[classSymbol][nodeAttributeType];
                    var _resourceInterfaces = interfaces[classSymbol][resourceAttributeType];
                    var content = GenerateResourceContainer(classSymbol, _nodeInterfaces, _resourceInterfaces);
                    context.AddSource(GenerateFilename(classSymbol), content);
                }
            }
        }

        // Generators
        private static string GenerateResourceContainer(INamedTypeSymbol classSymbol, List<PropertyDeclarationSyntax> nodeInterfaces, List<PropertyDeclarationSyntax> resourceInterfaces)
        {
            var content = new List<string>();

            const string dictName = "_exportedInterfaces";

            // Data fields
            content.Add($"private Dictionary<string, Variant> {dictName} = new Dictionary<string, Variant>()");
            content.Add("{");
            foreach (var node in nodeInterfaces)
            {
                content.Add($"    {{ \"{node.Identifier.Text}\", \"\" }},");
            }
            foreach (var res in resourceInterfaces)
            {
                content.Add($"    {{ \"{res.Identifier.Text}\", new Variant() }},");
            }
            content.Add("};");


            // _GetPropertyList()
            content.Add("public override partial Array<Dictionary> _GetPropertyList()");
            content.Add("{");
            content.Add("    var properties = new Array<Dictionary>();");
            foreach (var node in nodeInterfaces)
            {
                string name = node.Identifier.Text;

                content.Add("    properties.Add(new Dictionary {");
                content.Add($"        {{ \"name\", \"{name}\" }},");
                content.Add("        { \"type\", (int)Variant.Type.NodePath },");
                content.Add("        { \"usage\", (int)PropertyUsageFlags.Default },");
                content.Add("        { \"hint\", (int)PropertyHint.NodePathValidTypes },");
                content.Add("        { \"hint_string\", \"Node\" },");
                content.Add("    });");
            }
            foreach (var resource in resourceInterfaces)
            {
                string name = resource.Identifier.Text;

                content.Add("    properties.Add(new Dictionary {");
                content.Add($"        {{ \"name\", \"{name}\" }},");
                content.Add("        { \"type\", (int)Variant.Type.Object },");
                content.Add("        { \"usage\", (int)PropertyUsageFlags.Default },");
                content.Add("        { \"hint\", (int)PropertyHint.ResourceType },");
                content.Add("        { \"hint_string\", \"Resource\" },");
                content.Add("    });");
            }
            content.Add("    return properties;");
            content.Add("}");
            // _Get()
            content.Add("public override partial Variant _Get(StringName property)");
            content.Add("{");
            content.Add($"    if ({dictName}.ContainsKey(property))");
            content.Add($"        return {dictName}[property];");
            content.Add("    return base._Get(property);");
            content.Add("}");

            // _Set()
            content.Add("public override partial bool _Set(StringName property, Variant value)");
            content.Add("{");
            content.Add($"    if ({dictName}.ContainsKey(property))");
            content.Add("    {");
            content.Add($"        {dictName}[property] = value;");
            content.Add("        return true;");
            content.Add("    }");
            content.Add("    return base._Set(property, value);");
            content.Add("}");

            // Wire components
            content.Add("public void WireComponents(Node root)");
            content.Add("{");
            foreach (var node in nodeInterfaces)
            {
                var name = node.Identifier.Text;
                var type = node.Type;
                content.Add($"    {name} = root.GetNodeOrNull<{type}>({dictName}[\"{name}\"].AsString());");
            }
            content.Add("}");

            // Usings
            var usings = new List<string>
                {
                    "using Godot;",
                    "using Godot.Collections;"
                };

            return classSymbol.GeneratePartialClass(content, usings);

        }

        // Helper functions
        protected string GetInterfaceObjectName(string interfaceName)
        {
            char[] charArray = interfaceName.ToCharArray();
            charArray[0] = char.ToLower(charArray[0]);
            return $"__{new string(charArray)}";
        }

        private const string Ext = ".ExportInterface.g.cs";
        private const int MaxFileLength = 255;

        protected virtual string GenerateFilename(ISymbol symbol)
        {
            var gn = $"{Format(symbol)}{Ext}";
            // Log.Debug($"Generated Filename ({gn.Length}): {gn}\n");
            return gn;

            static string Format(ISymbol symbol) =>
                string.Join("_", $"{symbol}".Split(InvalidFileNameChars)).Truncate(MaxFileLength - Ext.Length);
        }

        private static readonly char[] InvalidFileNameChars =
        [
            '\"',
            '<',
            '>',
            '|',
            '\0',
            (char)1,
            (char)2,
            (char)3,
            (char)4,
            (char)5,
            (char)6,
            (char)7,
            (char)8,
            (char)9,
            (char)10,
            (char)11,
            (char)12,
            (char)13,
            (char)14,
            (char)15,
            (char)16,
            (char)17,
            (char)18,
            (char)19,
            (char)20,
            (char)21,
            (char)22,
            (char)23,
            (char)24,
            (char)25,
            (char)26,
            (char)27,
            (char)28,
            (char)29,
            (char)30,
            (char)31,
            ':',
            '*',
            '?',
            '\\',
            '/'
        ];
    }
}
