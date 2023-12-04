namespace GodotInterfaceExport.Editor;
using System;
using System.Reflection;
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
            if (Attribute.IsDefined(property, typeof(ExportNodeInterface)))
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
            // Get the attributes applied to the property
            object[] attributes = propertyInfo.GetCustomAttributes(true);

            // Find the attribute of the desired type (MyAttribute in this case)
            ExportNodeInterface myAttribute = attributes
                .OfType<ExportNodeInterface>()
                .FirstOrDefault();

            return new AttributeInfo(InterfaceAttributeType.Node, myAttribute.InterfaceType);
        }

        return null;
    }
}
