using GodotInterfaceExport;
using GodotInterfaceExport.Models;
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
        // private static readonly string resourceAttributeType = typeof(ExportResourceInterface).Name;
        private static readonly string nodeAttributeName = Regex.Replace(nodeAttributeType, "Attribute$", "", RegexOptions.Compiled);
        // private static readonly string resourceAttributeName = Regex.Replace(resourceAttributeType, "Attribute$", "", RegexOptions.Compiled);
        private static readonly string[] attributeNames = [nodeAttributeName];

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
                try {
                    // class -> attribute -> properties
                    Dictionary<INamedTypeSymbol, List<NodeInterfaceModel>> nodeInterfaces = [];

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

                        // Init arrays
                        if (!nodeInterfaces.ContainsKey(symbol.ContainingType))
                        {
                            nodeInterfaces[symbol.ContainingType] = [];
                        }

                        var attributeSymbol = symbol
                            .GetAttributes()
                            .SingleOrDefault(x => x.AttributeClass?.Name == nodeAttributeName);
                        if (attributeSymbol is null)
                            continue;
                        
                        nodeInterfaces[symbol.ContainingType].Add(new NodeInterfaceModel(
                            property.Identifier.Text,
                            attributeSymbol.ConstructorArguments[0].Value!.ToString(),
                            (bool)attributeSymbol.ConstructorArguments[1].Value!,
                            attributeSymbol.ApplicationSyntaxReference!.GetSyntax().GetLocation()
                        ));
                    }

                    foreach (var classSymbol in nodeInterfaces.Keys)
                    {
                        if (context.CancellationToken.IsCancellationRequested)
                            return;

                        var generateFields = nodeInterfaces[classSymbol].Where(x => x.GenerateField);

                        if (!generateFields.Any())
                            continue;

                        var (content, diagnostics) = GenerateNodeInterfaces(classSymbol, generateFields);
                        diagnostics.ToList().ForEach(context.ReportDiagnostic);
                        context.AddSource(GenerateFilename(classSymbol), content);
                    }
                } catch (Exception e) {
                    Log.Error(e);
                    throw;
                }
            }
        }

        // Generators
        private static (string, IEnumerable<Diagnostic>) GenerateNodeInterfaces(INamedTypeSymbol classSymbol, IEnumerable<NodeInterfaceModel> nodeDetails)
        {
            var content = new List<string>();
            var diagnostics = new List<Diagnostic>();

            // Generated fields
            foreach (var node in nodeDetails)
            {
                if (!node.IsNameValid())
                {
                    var descriptor = new DiagnosticDescriptor(nameof(ExportNodeInterface), "Invalid name", $"The name of the property {node.PropertyName} is invalid. It must end with \"Interface\".", "Usage", DiagnosticSeverity.Error, true);
                    diagnostics.Add(Diagnostic.Create(descriptor, node.SyntaxLocation));
                    continue;
                }

                content.Add($"public {node.InterfaceType} {node.GetGeneratedName()} => ({node.InterfaceType}){node.PropertyName};");
            }

            // Usings
            var usings = new List<string>
                {
                    "using System.ComponentModel;"
                };

            return (classSymbol.GeneratePartialClass(content, usings), diagnostics);
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
