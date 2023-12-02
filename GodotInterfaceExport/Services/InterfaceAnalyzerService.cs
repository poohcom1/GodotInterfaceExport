namespace GodotInterfaceExport.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using GodotInterfaceExport.Utilities;

public class InterfaceAnalyzerService
{
    private readonly HashSet<Type> _nodeTypes = [];
    private readonly HashSet<Type> _resourceTypes = [];

    private Guid _cachedModuleVersionId = Guid.Empty;
    private readonly Dictionary<string, string> _cachedClassFileFQNs = [];

    private Action<string>? _log;

    public void SetLogger(Action<string> logger)
    {
        _log = logger;
    }

    public void UpdateCache(Assembly assembly, Type nodeType, Type resourceType)
    {
        var guid = assembly.ManifestModule.ModuleVersionId;
        if (_cachedModuleVersionId == guid)
        {
            return;
        }
        _cachedModuleVersionId = guid;
        _cachedClassFileFQNs.Clear();

        Type[] types = assembly.GetTypes();

        foreach (Type type in types)
        {
            if (type.FullName == null)
                continue;

            if (nodeType.IsAssignableFrom(type))
            {
                _nodeTypes.Add(type);
            }
            else if (resourceType.IsAssignableFrom(type))
            {
                _resourceTypes.Add(type);
            }
        }
    }

    public bool NodeImplementsInterface(string filepath, Type @interface)
    {
        if (filepath == "")
        {
            return false;
        }
        var fqn = GetClassFileFQN(filepath);
        if (fqn == "")
        {
            return false;
        }
        var nodeType = _nodeTypes.FirstOrDefault(t => t.FullName == fqn);
        if (nodeType == null)
        {
            return false;
        }
        return @interface.IsAssignableFrom(nodeType);
    }

    public bool ResourceImplementsInterface(string filepath, Type @interface)
    {
        var fqn = GetClassFileFQN(filepath);
        if (fqn == "")
        {
            return false;
        }
        var resourceType = _resourceTypes.FirstOrDefault(t => t.FullName == fqn);
        if (resourceType == null)
        {
            return false;
        }
        return @interface.IsAssignableFrom(resourceType);
    }

    /// <summary>
    /// Caches the result of <see cref="AnalyzerUtils.GetClassFileFQN(string)"/>.
    /// </summary>
    private string GetClassFileFQN(string filepath)
    {
        if (_cachedClassFileFQNs.ContainsKey(filepath))
        {
            return _cachedClassFileFQNs[filepath];
        }
        var fqn = AnalyzerUtils.GetClassFileFQN(filepath);
        _cachedClassFileFQNs.Add(filepath, fqn);
        return fqn;
    }
}
