namespace GodotInterfaceExport.Services;
#pragma warning disable RS1035
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GodotInterfaceExport.Editor.Models;
using GodotInterfaceExport.Utilities;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Use reflection to get attributes from a C# script file.
/// </summary>
public class AttributeAnalyzerService(AssemblyReflectionService assemblyReflectionService, Action<string>? log = null)
{
    private readonly AssemblyReflectionService _assemblyReflectionService = assemblyReflectionService;
    private readonly Action<string>? Log = log;

    // (class, property) -> interface attribute type
    private readonly Dictionary<(string, string), Type> _attributeTypesCache = [];
    private readonly HashSet<string> _classesWithAttribute = [];


    public void UpdateCache()
    {
        _attributeTypesCache.Clear();
        _classesWithAttribute.Clear();

        foreach (Type type in _assemblyReflectionService.Types)
        {
            if (type.FullName == null)
                continue;

            MemberInfo[] members = type.GetMembersInclPrivateBase(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public
            );

            foreach (var member in members)
            {
                var interfaceAttributes = member.GetCustomAttributes(typeof(ExportInterface), true)
                    .Cast<ExportInterface>()
                    .Select(exportInterface => exportInterface.InterfaceType)
                    .FirstOrDefault();
                
                if (interfaceAttributes == null)
                    continue;

                _attributeTypesCache[(type.FullName, member.Name)] = interfaceAttributes;
                _classesWithAttribute.Add(type.FullName);
            }
        }
    }

    public bool ClassHasAttribute(string filepath)
    {  
        if (filepath == "")
            return false;

        var fqn = _assemblyReflectionService.GetClassFileFQN(filepath);
        return _classesWithAttribute.Contains(fqn);
    }

    public AttributeInfo? GetAttributeInfo(string filepath, string property)
    {
        if (filepath == "")
            return null;

        var fqn = _assemblyReflectionService.GetClassFileFQN(filepath);
        if (fqn == "")
            return null;
        
        var key = (fqn, property);

        if (!_attributeTypesCache.ContainsKey(key))
            return null;

        var type = _attributeTypesCache[key];
        if (type == null)
            return null;

        return new AttributeInfo(type);
    }
}
