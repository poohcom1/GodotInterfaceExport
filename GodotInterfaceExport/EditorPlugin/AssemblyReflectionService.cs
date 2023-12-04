using System.Reflection;
using GodotInterfaceExport.Utilities;

namespace GodotInterfaceExport.Services
{
    public class AssemblyReflectionService
    {
        public Type[] Types { get; private set; } = [];

        private Guid _cachedModuleVersionId = Guid.Empty;
        private readonly Dictionary<string, string> _cachedClassFileFQNs = [];

        public bool UpdateCache(Assembly assembly)
        {
            var guid = assembly.ManifestModule.ModuleVersionId;
            if (_cachedModuleVersionId == guid)
            {
                return false;
            }
            _cachedModuleVersionId = guid;
            _cachedClassFileFQNs.Clear();

            Types = assembly.GetTypes();

            return true;
        }

        public string GetClassFileFQN(string filepath) {
            if (_cachedClassFileFQNs.ContainsKey(filepath))
            {
                return _cachedClassFileFQNs[filepath];
            }
            
            var fqn = AnalyzerUtils.GetClassFileFQN(filepath);
            _cachedClassFileFQNs.Add(filepath, fqn);
            return fqn;
        } 
    }
}