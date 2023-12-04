namespace GodotInterfaceExport.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using GodotInterfaceExport.Utilities;

public class InterfaceAnalyzerService(AssemblyReflectionService assemblyReflectionService)
{
    private readonly AssemblyReflectionService _assemblyReflectionService = assemblyReflectionService;

    private readonly HashSet<Type> _nodeTypes = [];
    private readonly HashSet<Type> _resourceTypes = [];

    private readonly Dictionary<string, string> _cachedClassFileFQNs = [];
    private readonly Dictionary<(string, Type), bool> _memberAttributeCache = [];

    public bool NodeImplementsInterface(string filepath, Type @interface)
    {
        if (filepath == "")
            return false;
        var fqn = _assemblyReflectionService.GetClassFileFQN(filepath);
        if (fqn == "")
            return false;

        var key = (fqn, @interface);

        if (_memberAttributeCache.ContainsKey(key))
            return _memberAttributeCache[key];

        var nodeType = _assemblyReflectionService.Types.FirstOrDefault(t => t.FullName == fqn);
        if (nodeType == null)
        {
            return false;
        }
        bool implements = @interface.IsAssignableFrom(nodeType);

        _memberAttributeCache.Add(key, implements);
        
        return implements;
    }
}
