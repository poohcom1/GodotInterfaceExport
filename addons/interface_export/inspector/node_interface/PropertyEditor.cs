#if TOOLS
namespace Addons.InterfaceExport.NodeInterface;
using System;
using System.Collections.Generic;
using Godot;
using GodotInterfaceExport.Services;

[Tool]
internal partial class PropertyEditor : EditorProperty
{
    // Controls
    private PropertyButton _nodePathButton;

    private SceneTreeDialog _sceneTreeDialog;
    const string _sceneTreeDialogScene =
        "res://addons/interface_export/inspector/node_interface/interface_scene_tree_dialog.tscn";

    // Avoid using constructors in tools
    public void Init(Type interfaceType, InterfaceAnalyzerService interfaceAnalyzer)
    {
        _sceneTreeDialog = GD.Load<PackedScene>(_sceneTreeDialogScene)
            .Instantiate<SceneTreeDialog>();
        _sceneTreeDialog.Init(interfaceAnalyzer, interfaceType);
        _sceneTreeDialog.NodeSelected += OnTreeNodeSelected;
        AddChild(_sceneTreeDialog);

        _nodePathButton = new PropertyButton();
        _nodePathButton.Init(interfaceAnalyzer);
        _nodePathButton.Pressed += OnNodePathButtonPressed;
        AddChild(_nodePathButton);

        UpdateProperty();
    }

    public override void _UpdateProperty()
    {
        // Setup
        var root = EditorInterface.Singleton.GetEditedSceneRoot();
        var editedObject = GetEditedObject();

        if (editedObject == null)
        {
            _nodePathButton.Update(root, "");
        }
        else
        {
            _nodePathButton.Update(root, GetEditedObject().Get(GetEditedProperty()).AsString());
        }
    }

    private void OnNodePathButtonPressed()
    {
        _sceneTreeDialog.PopupScenetreeDialog();
    }

    private void OnTreeNodeSelected(Node node)
    {
        var path = EditorInterface.Singleton.GetEditedSceneRoot().GetPathTo(node);

        EmitChanged(GetEditedProperty(), path);
    }
}


#endif
