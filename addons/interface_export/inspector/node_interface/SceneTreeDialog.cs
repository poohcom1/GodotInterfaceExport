#pragma warning disable CS8618
#if TOOLS
namespace Addons.InterfaceExport.NodeInterface;
using Godot;
using GodotInterfaceExport.Services;
using System;
using System.Collections.Generic;

[Tool]
public partial class SceneTreeDialog : ConfirmationDialog
{
    [Signal]
    public delegate void NodeSelectedEventHandler(Node node);

    private Label _interfaceTypeLabel;
    private Tree _nodeTree;

    private InterfaceAnalyzerService _analyzerService;
    private Type _interface;

    private Node _selectedNode;

    public void Init(InterfaceAnalyzerService analyzerService, Type @interface)
    {
        _analyzerService = analyzerService;
        _interface = @interface;
        Visible = false;
    }

    public override void _Ready()
    {
        _interfaceTypeLabel = GetNode<Label>("%InterfaceTypeLabel");
        _nodeTree = GetNode<Tree>("%NodeTree");
        if (_interface != null)
            _interfaceTypeLabel.Text = _interface.Name;

        _nodeTree.ItemSelected += OnTreeItemSelected;
        _nodeTree.ItemActivated += OnItemActivated;
        Confirmed += OnConfirmButtonPressed;

        GetOkButton().Disabled = true;
    }

    public void PopupScenetreeDialog()
    {
        _nodeTree.Clear();
        DrawTree();

        Vector2 size = new Vector2(350, 700) * EditorInterface.Singleton.GetEditorScale();
        PopupCenteredClamped(new Vector2I((int)size.X, (int)size.Y));
    }

    // Draw
    private void DrawTree(string filter = "")
    {
        TreeItem treeRoot = _nodeTree.CreateItem();
        Node rootNode = EditorInterface.Singleton.GetEditedSceneRoot();
        // Draw whole tree
        static void DrawTreeItemRecursive(TreeItem item, Node node)
        {
            DrawTreeItem(item, node);

            foreach (Node child in node.GetChildren())
            {
                TreeItem childItem = item.CreateChild();
                DrawTreeItemRecursive(childItem, child);
            }
        }

        DrawTreeItemRecursive(treeRoot, rootNode);

        // Remove nodes that don't implement interface
        Stack<TreeItem> itemStack = new();
        HashSet<TreeItem> visitedItems = new();

        itemStack.Push(treeRoot);

        while (itemStack.Count > 0)
        {
            TreeItem current = itemStack.Peek();

            bool childrenAllVisited = true;
            bool childrenVisible = false;

            foreach (TreeItem child in current.GetChildren())
            {
                childrenAllVisited = childrenAllVisited && visitedItems.Contains(child);
                childrenVisible = childrenVisible || child.Visible;
            }

            if (childrenAllVisited)
            {
                itemStack.Pop();

                bool interfaceImplemented = _analyzerService.NodeImplementsInterface(
                    GodotUtils.GetNodeScriptPath((Node)current.GetMetadata(0)),
                    _interface
                );
                bool filterMatch = current.GetText(0).ToLower().Contains(filter.ToLower());

                current.Visible =
                    (interfaceImplemented && filterMatch) || current == treeRoot || childrenVisible;
                current.SetSelectable(0, interfaceImplemented);
                current.SetCustomColor(
                    0,
                    interfaceImplemented ? new Color(1, 1, 1) : new Color(0.5f, 0.5f, 0.5f)
                );

                visitedItems.Add(current);
            }
            else
            {
                foreach (TreeItem child in current.GetChildren())
                {
                    itemStack.Push(child);
                }
            }
        }
    }

    static void DrawTreeItem(TreeItem item, Node node)
    {
        item.SetText(0, node.Name);
        item.SetIcon(
            0,
            EditorInterface.Singleton.GetBaseControl().GetThemeIcon(node.GetClass(), "EditorIcons")
        );
        item.SetMetadata(0, node);
    }

    // Signal callbacks
    private void OnTreeItemSelected()
    {
        var selectedItem = _nodeTree.GetSelected();
        GetOkButton().Disabled = !selectedItem.IsSelectable(0);
        _selectedNode = (Node)selectedItem.GetMetadata(0);
    }

    private void OnItemActivated()
    {
        var selectedItem = _nodeTree.GetSelected();
        var node = (Node)selectedItem.GetMetadata(0);

        EmitSignal(SignalName.NodeSelected, node);
        Hide();
    }

    private void OnConfirmButtonPressed()
    {
        EmitSignal(SignalName.NodeSelected, _selectedNode);
        Hide();
    }
}
#endif
