#if TOOLS
namespace Addons.InterfaceExport;

using System.Reflection;
using Godot;
using GodotInterfaceExport.Editor;
using GodotInterfaceExport.Editor.Models;
using GodotInterfaceExport.Services;
using GodotInterfaceExportTest;

internal partial class InterfaceExportInspectorPlugin : EditorInspectorPlugin
{
    private readonly InspectorPluginService _inspectorPluginService = new();
    private readonly InterfaceAnalyzerService _interfaceAnalyzerService = new();

    public InterfaceExportInspectorPlugin()
    {
        _interfaceAnalyzerService.UpdateCache(
            Assembly.GetAssembly(typeof(InterfaceExportInspectorPlugin)),
            typeof(Node),
            typeof(Resource)
        );
    }

    public override bool _CanHandle(GodotObject @object)
    {
        return true;
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
        if (name == "ComponentA")
        {
            NodeInterface.PropertyEditor propertyEditor = new();
            propertyEditor.Init(typeof(IComponentA), _interfaceAnalyzerService);

            AddPropertyEditor(name, propertyEditor);
            return true;
        }

        // Old
        if (
            _inspectorPluginService.GetAttributeInfo(@object, name)
            is not AttributeInfo attributeInfo
        )
            return false;

        switch (attributeInfo.AttributeType)
        {
            case InterfaceAttributeType.Node:
                NodeInterface.PropertyEditor propertyEditor = new();
                propertyEditor.Init(attributeInfo.InterfaceType, _interfaceAnalyzerService);

                AddPropertyEditor(name, propertyEditor);
                return true;
            case InterfaceAttributeType.Resource:
                return true;
        }

        return base._ParseProperty(@object, type, name, hintType, hintString, usageFlags, wide);
    }
}
#endif
