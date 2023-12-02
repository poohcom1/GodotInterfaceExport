#pragma warning disable RS1035 // These services doesn't deal with source generation
namespace GodotInterfaceExport.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class AnalyzerUtils
{
    public static string GetClassFileFQN(string filepath) {
        var fileClassname = Path.GetFileNameWithoutExtension(filepath);
        var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(filepath));
        var treeRoot = (CompilationUnitSyntax)tree.GetRoot();

        var classNodes = treeRoot.DescendantNodes().OfType<ClassDeclarationSyntax>();

        foreach (var classNode in classNodes)
        {
            var className = classNode.Identifier.Text;
            if (className == fileClassname)
            {
                var classNamespace = AnalyzerUtils.GetNamespaceFrom(classNode);

                return
                    classNamespace == "" ? className : classNamespace + "." + className;
            }
        }

        return "";
    }

    // Thanks to https://stackoverflow.com/a/63686228/15928461
    public static string GetNamespaceFrom(SyntaxNode s) =>
        s.Parent switch
        {
            BaseNamespaceDeclarationSyntax namespaceDeclarationSyntax
                => namespaceDeclarationSyntax.Name.ToString(),
            null => string.Empty,
            _ => GetNamespaceFrom(s.Parent)
        };

    // Thanks to https://stackoverflow.com/a/50248816/15928461
    public static MemberInfo[] GetMembersInclPrivateBase(this Type t, BindingFlags flags)
    {
        var memberList = new List<MemberInfo>();
        memberList.AddRange(t.GetMembers(flags));
        Type? currentType = t;
        while ((currentType = currentType.BaseType) != null)
        {
            memberList.AddRange(currentType.GetMembers(flags));
        }
        return [.. memberList];
    }

    // Thanks to https://stackoverflow.com/a/16043551/15928461
    public static Type? GetUnderlyingType(this MemberInfo member)
    {
        return member.MemberType switch
        {
            MemberTypes.Event => ((EventInfo)member).EventHandlerType,
            MemberTypes.Field => ((FieldInfo)member).FieldType,
            MemberTypes.Method => ((MethodInfo)member).ReturnType,
            MemberTypes.Property => ((PropertyInfo)member).PropertyType,
            _
                => throw new ArgumentException(
                    "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                ),
        };
    }
}
