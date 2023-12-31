using GodotInterfaceExport.Models;
using Microsoft.CodeAnalysis;

namespace GodotInterfaceExport.SourceGenerators.Constants;

internal static class DiagnosticIssues
{
    public static DiagnosticDescriptor InvalidMemberName(string memberName)
    {
        return new DiagnosticDescriptor(
            nameof(ExportInterface),
            "Invalid name",
            $"The name of member {memberName} is invalid. It must end with \"{NodeInterfaceModel.NAME_SUFFIX}\".",
            "Usage",
            DiagnosticSeverity.Error,
            true
        );
    }

    public static DiagnosticDescriptor NoExportAttribute(string memberName)
    {
        return new DiagnosticDescriptor(
            nameof(ExportInterface),
            "Missing attribute",
            $"The member {memberName} is missing the [Export] attribute.",
            "Usage",
            DiagnosticSeverity.Warning,
            true
        );
    }

    public static DiagnosticDescriptor InvalidType(string memberName, string typeName)
    {
        return new DiagnosticDescriptor(
            nameof(ExportInterface),
            "Invalid type",
            $"The member {memberName} has an invalid type of {typeName}. It should be a Godot Node.",
            "Usage",
            DiagnosticSeverity.Warning,
            true
        );
    }
}
