#pragma warning disable CS8604
#if TOOLS
namespace Addons.InterfaceExport;

using System.Reflection;
using Godot;
using GodotInterfaceExport.Editor.Models;
using GodotInterfaceExport.Services;

internal partial class InterfaceExportInspectorPlugin : EditorInspectorPlugin
{
    private readonly AssemblyReflectionService _assemblyReflectionService = new();
    private readonly InterfaceAnalyzerService _interfaceAnalyzerService;
    private readonly AttributeAnalyzerService _attributeAnalyzerService;

    public InterfaceExportInspectorPlugin()
    {
        _interfaceAnalyzerService = new(_assemblyReflectionService);
        _attributeAnalyzerService = new(_assemblyReflectionService, GD.Print);

        _assemblyReflectionService.UpdateCache(
            Assembly.GetAssembly(typeof(InterfaceExportInspectorPlugin))
        );
        _attributeAnalyzerService.UpdateCache();
    }

    public override bool _CanHandle(GodotObject @object)
    {
        if (@object is Node node && (Script)node.GetScript() is CSharpScript script)
        {
            return _attributeAnalyzerService.ClassHasAttribute(
                ProjectSettings.GlobalizePath(script.ResourcePath)
            );
        }
        return false;
    }

    public override bool _ParseProperty(
        GodotObject @object,
        Variant.Type type,
        string name,
        PropertyHint hintType,
        string hintString,
        PropertyUsageFlags usageFlags,
        bool wide
    )
    {
        if (@object is null || (Script)@object.GetScript() is not CSharpScript)
        {
            return false;
        }
    
        if (
            _attributeAnalyzerService.GetAttributeInfo(
                ProjectSettings.GlobalizePath(((Script)@object.GetScript()).ResourcePath),
                name
            )
            is AttributeInfo attributeInfo
        )
        {
            NodeInterface.PropertyEditor propertyEditor = new();
            propertyEditor.Init(attributeInfo.InterfaceType, _interfaceAnalyzerService);

            AddPropertyEditor(name, propertyEditor);
            return true;
        }

        return false;
    }
}
#endif
