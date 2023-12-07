using GodotInterfaceExport;
using GodotInterfaceExport.Models;
using GodotInterfaceExport.SourceGenerators.Constants;
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
        private static readonly string nodeAttributeType = typeof(ExportInterface).Name;
        private static readonly string nodeAttributeName = Regex.Replace(nodeAttributeType, "Attribute$", "", RegexOptions.Compiled);
        // private static readonly string resourceAttributeType = typeof(ExportResourceInterface).Name;
        // private static readonly string resourceAttributeName = Regex.Replace(resourceAttributeType, "Attribute$", "", RegexOptions.Compiled);
        private static readonly string[] attributeNames = [nodeAttributeName];

        private static readonly string[] validTypes = ["Node", "Node?", "Godot.Node", "Godot.Node?"];

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
                return (node is PropertyDeclarationSyntax type && HasAttributeType(type)) ||
                    (node is FieldDeclarationSyntax field && HasAttributeType(field)); // FIXME: Field isn't working

                static bool HasAttributeType(MemberDeclarationSyntax type)
                {
                    foreach (var attributeList in type.AttributeLists)
                    {
                        foreach (var attribute in attributeList.Attributes)
                        {
                            if (attributeNames.Contains(attribute.Name.ToString()))
                                return true;
                        }
                    }
                    return false;
                }
            }

            static MemberDeclarationSyntax GetSyntaxTarget(
                GeneratorSyntaxContext context,
                CancellationToken _
            ) => (MemberDeclarationSyntax)context.Node;

            void OnExecute(
                SourceProductionContext context,
                Compilation compilation,
                ImmutableArray<MemberDeclarationSyntax> nodes,
                AnalyzerConfigOptionsProvider options
            )
            {
                try
                {
                    Dictionary<INamedTypeSymbol, List<NodeInterfaceModel>> nodeInterfaces = [];

                    foreach (var node in nodes.Distinct())
                    {
                        if (context.CancellationToken.IsCancellationRequested) return;
                        var model = compilation.GetSemanticModel(node.SyntaxTree);
                        var symbol = model.GetDeclaredSymbol(node);
                        if (symbol is null || symbol.ContainingType is null) continue;
                        // Init arrays
                        if (!nodeInterfaces.ContainsKey(symbol.ContainingType))
                            nodeInterfaces[symbol.ContainingType] = [];
                        // Check if [Export] attribute is present
                        bool exportAttribute = true; // symbol.GetAttributes().Any(x => x.AttributeClass?.Name == "[Export]" || x.AttributeClass?.Name == "[Godot.Export]"); // FIXME: Always warn atleast once and then stops
                        // Get our attribtues
                        var attributeSymbol = symbol.GetAttributes().SingleOrDefault(x => x.AttributeClass?.Name == nodeAttributeName);
                        if (attributeSymbol is null) continue;
                        if (node is PropertyDeclarationSyntax prop)
                        {
                            AddMember(prop.Identifier.Text);

                            if (!validTypes.Contains(prop.Type.ToString()))
                            {
                                var descriptor = DiagnosticIssues.InvalidType(prop.Identifier.Text, prop.Type.ToString());
                                context.ReportDiagnostic(Diagnostic.Create(descriptor, attributeSymbol.ApplicationSyntaxReference!.GetSyntax().GetLocation()));
                            }
                        }
                        if (node is FieldDeclarationSyntax field)
                        {
                            foreach (var variable in field.Declaration.Variables)
                            {
                                AddMember(variable.Identifier.Text);
                            }
                        }

                        // Makes this just a little more concise
                        void AddMember(string name)
                        {
                            nodeInterfaces[symbol.ContainingType].Add(new NodeInterfaceModel(
                                name,
                                attributeSymbol.ConstructorArguments[0].Value!.ToString(),
                                (bool)attributeSymbol.ConstructorArguments[1].Value!,
                                attributeSymbol.ApplicationSyntaxReference!.GetSyntax().GetLocation()
                            ));


                            if (!exportAttribute)
                            {
                                var descriptor = DiagnosticIssues.NoExportAttribute(name);
                                context.ReportDiagnostic(Diagnostic.Create(descriptor, attributeSymbol.ApplicationSyntaxReference.GetSyntax().GetLocation()));
                            }
                        }
                    }

                    foreach (var classSymbol in nodeInterfaces.Keys)
                    {
                        if (context.CancellationToken.IsCancellationRequested)
                            return;

                        var generatedProperties = nodeInterfaces[classSymbol].Where(x => x.GenerateField);

                        if (!generatedProperties.Any())
                            continue;

                        var (content, diagnostics) = GenerateNodeInterfaces(classSymbol, generatedProperties);
                        diagnostics.ToList().ForEach(context.ReportDiagnostic);
                        context.AddSource(GenerateFilename(classSymbol), content);
                    }
                }
                catch (Exception e)
                {
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

            // Generated properties
            foreach (var node in nodeDetails)
            {
                if (!node.IsNameValid())
                {
                    var descriptor = DiagnosticIssues.InvalidMemberName(node.PropertyName);
                    diagnostics.Add(Diagnostic.Create(descriptor, node.SyntaxLocation));
                    continue;
                }

                var name = node.GetGeneratedName();

                content.Add("[EditorBrowsable(EditorBrowsableState.Never)]");
                content.Add("[Browsable(false)]");
                content.Add($"private {node.InterfaceType}? __{name} = null;");
                content.Add($"public {node.InterfaceType} {name}");
                content.Add("{");
                content.Add($"    get => __{name} ??= ({node.InterfaceType}){node.PropertyName};");
                content.Add($"    set => __{name} = value;");
                content.Add("}");
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
            Log.Debug($"Generated Filename ({gn.Length}): {gn}\n");
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
