using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using GodotComponentExport.Attributes;
using GodotSharp.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

// namespace GodotComponentExport.SourceGenerators
// {
//     [Generator]
//     internal class InterfaceExportGenerator
//         : SourceGeneratorForDeclaredPropertyWithAttribute<ExportInterfaceAttribute>
//     {
//         protected override (string GeneratedCode, DiagnosticDetail Error) GenerateCode(
//             Compilation compilation,
//             SyntaxNode node,
//             IPropertySymbol symbol,
//             AttributeData attribute,
//             AnalyzerConfigOptions options
//         )
//         {
//             if (node is PropertyDeclarationSyntax property)
//             {
//                 string propertyName = property.Identifier.Text;
//                 var content = symbol.ContainingType.GeneratePartialClass(
//                     Content(propertyName),
//                     Usings()
//                 );

//                 return (content, null);
//             }
//             else
//             {
//                 return ("", null);
//             }

//             static IEnumerable<string> Content(string propertyName)
//             {
//                 char[] charArray = propertyName.ToCharArray();
//                 charArray[0] = char.ToLower(charArray[0]);

//                 yield return "[Export]";
//                 yield return $"public Node __{new string(charArray)};";
//             }

//             static IEnumerable<string> Usings()
//             {
//                 yield return "using Godot;";
//             }
//         }
//     }
// }
