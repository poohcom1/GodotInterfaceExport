using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using GodotComponentExport.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using GodotSharp.SourceGenerators;

namespace GodotComponentExport.SourceGenerators
{
    /// <summary>
    /// Adapted from https://github.dev/Cat-Lips/GodotSharp.SourceGenerators.SourceGeneratorForDeclaredMemberWithAttribute.cs
    /// </summary>
    [Generator]
    internal class InterfaceExportGenerator : IIncrementalGenerator
    {
        private static readonly string attributeType = typeof(ExportInterfaceAttribute).Name;
        private static readonly string attributeName = Regex.Replace(
            attributeType,
            "Attribute$",
            "",
            RegexOptions.Compiled
        );

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
                            if (attribute.Name.ToString() == attributeName)
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
                try
                {
                    INamedTypeSymbol? classSymbol = null;
                    List<PropertyDeclarationSyntax> exportedInterfaces = [];

                    foreach (var node in nodes.Distinct())
                    {
                        if (context.CancellationToken.IsCancellationRequested)
                            return;

                        var model = compilation.GetSemanticModel(node.SyntaxTree);
                        var symbol = model.GetDeclaredSymbol(Node(node));
                        if (node is not PropertyDeclarationSyntax property)
                            continue;

                        if (symbol is null)
                            continue;

                        classSymbol ??= symbol.ContainingType;

                        var attribute = symbol
                            .GetAttributes()
                            .SingleOrDefault(x => x.AttributeClass?.Name == attributeType);
                        if (attribute is null)
                            continue;
                        exportedInterfaces.Add(property);
                    }

                    var (nsOpen, nsClose, nsIndent) = classSymbol.GetNamespaceDeclaration();

                    var content = new List<string>();
                    // Exports
                    foreach (var exportedInterface in exportedInterfaces)
                    {
                        content.Add("[Export]");
                        content.Add($"private Node {GetInterfaceObjectName(exportedInterface.Identifier.Text)} = null!;");
                    }
                    // Wire components
                    content.Add("");
                    content.Add("private void WireComponents()");
                    content.Add("{");
                    foreach (var exportedInterface in exportedInterfaces)
                    {
                        content.Add($"    {exportedInterface.Identifier.Text} = {GetInterfaceObjectName(exportedInterface.Identifier.Text)} as {exportedInterface.Type};");
                    }
                    content.Add("}");

                    var source = $@"
using Godot;

{nsOpen?.Trim()}
{nsIndent}partial class {classSymbol.ClassDef()}
{nsIndent}{{
{nsIndent}    {string.Join($"\n{nsIndent}    ", content)}
{nsIndent}}}
{nsClose?.Trim()}
".TrimStart();

                    context.AddSource(GenerateFilename(classSymbol), source);

                }
                catch (Exception e)
                {
                    // Log.Error(e);
                    throw;
                }
            }
        }

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
                Truncate(string.Join("_", $"{symbol}".Split(InvalidFileNameChars)), Ext.Length);
        }

        protected virtual SyntaxNode Node(PropertyDeclarationSyntax node) => node;

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

        private static string Truncate(string source, int maxChars) =>
            source.Length <= maxChars ? source : source.Substring(0, maxChars);
    }
}
