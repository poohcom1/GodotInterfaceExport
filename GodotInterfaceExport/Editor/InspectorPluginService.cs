namespace GodotInterfaceExport.Editor;
using System;
using System.Reflection;
using GodotInterfaceExport.Attributes;
using GodotInterfaceExport.Editor.Models;

public class InspectorPluginService
{
    public bool CanHandle(object @object)
    {
        PropertyInfo[] properties = @object
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (PropertyInfo property in properties)
        {
            if (
                Attribute.IsDefined(property, typeof(ExportNodeInterface))
                || Attribute.IsDefined(property, typeof(ExportResourceInterface))
            )
            {
                return true;
            }
        }

        return false;
    }

    public AttributeInfo? GetAttributeInfo(object @object, string property)
    {
        PropertyInfo propertyInfo = @object
            .GetType()
            .GetProperty(property, BindingFlags.Public | BindingFlags.Instance);

        if (propertyInfo == null)
        {
            return null;
        }

        if (Attribute.IsDefined(propertyInfo, typeof(ExportNodeInterface)))
        {
            return new AttributeInfo(InterfaceAttributeType.Node, propertyInfo.PropertyType);
        }

        if (Attribute.IsDefined(propertyInfo, typeof(ExportResourceInterface)))
        {
            return new AttributeInfo(InterfaceAttributeType.Resource, propertyInfo.PropertyType);
        }

        return null;
    }
}
