# Godot Interface Export

Godot C# package to support exporting interface types!

## Installation

0. Install dotnet 8.0 sdk. (see https://github.com/poohcom1/GodotInterfaceExport/issues/4)
1. Install the C# package from nuget.

```sh
dotnet add package GodotInterfaceExport
```

2. Add `interface_export` to your addon folder.
3. Activate the plugin from the project settings.

## Usage

To start using interface exports, add the `[ExportInterface]` attribute to an exported property.

```cs
public partial class CharacterMovement : Node2D
{
    [Export]
    [ExportInterface(typeof(ICollisionHitbox), generatedProperty: true)]
    private Node? _hitboxNode { set; get; }
}
```
Make sure that:

1. The exported value is a property. It cannot be a field.
2. The property is also annotated with `[Export]`.
3. The property name ends with "Node". (Similar to how signals need to end with "EventHandler")

The `ICollisionHitbox _hitbox` property will be generated for you, and you can use it in your class.

```cs
public override void _Ready()
{
    _hitbox.CollisionHitboxMethod();
}
```

Furthermore, when selecting the node in the Godot editor, the node picker will now type-check the node for you:

![Node Picker](https://raw.githubusercontent.com/poohcom1/GodotInterfaceExport/master/.readme/node_picker.png)

### Manual Fields

If you don't want to mess with source generators or just prefer your code to be more explicit, you can turn off the property generation:

```cs
public partial class CharacterMovement : Node2D
{
    [Export]
    [ExportInterface(typeof(ICollisionHitbox), generateProperty: false)] // Or leave out the second param
    private Node? _hitbox { set; get; }
    public ICollisionHitbox? Hitbox => _hitbox as ICollisionHitbox;
}
```

This also gives you the freedom the name the exported property anything you want.

## How it Works
There are two main components to this package/plugin: the code analyzer and source generator.
The code analyzer is responsible for the editor plugin part of the package, which allows the custom node picker to detect which scripts implements an interface.
The source generator is responsible for generating the property. Note that if you're turn off `generateProperty` in the attribute, the source generator is completely optional.

### Code Analyzer

`GodotInterfaceExport/EditorPlugin`

In a regular C# codebase, a basic reflection script would be enough to detect the interface and check which class implements them. However, in the Godot editor, C# scripts aren't actually loaded unless they're a tool script, so when an `EditorInspectorPlugin` receives a C# object, it does not have access to it's C# class. (https://github.com/godotengine/godot/issues/80693). Without this info, we cannot gain access to the attributes or type of the object being inspected from a Godot plugin.

In order to get around this, we have to analyze the script ourselves. This can be done by (1) obtaining the .cs file path, (2) loading it into a code analyzer, (3) parsing the syntax for the bare minimum information, and (4) get the current assembly to obtain Type data. With this information, we can create a custom node picker in the addon and utilize the analyzers to check which nodes is a valid implemention of a given interface.

### Source Generator

`GodotInterfaceExport/SourceGenerators`

The current version of the source generator is pretty basic; for each exported interface node, we generate an equivalent property with the interface type and just casts the node to that type. The work is done in reference to this great library by Cat-Lips: https://github.com/Cat-Lips/GodotSharp.SourceGenerators. It was previously heavily modified because the current implementation allows one source per attribute while I wanted to create helper functions to any class without creating a class attribute. Since that idea was scrapped, the current implementation is a bit of a mess.
