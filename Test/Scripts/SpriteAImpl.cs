namespace GodotInterfaceExportTest;
using Godot;
using System;

public partial class SpriteAImpl : Sprite2D, IComponentA
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() { }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }

    public void DoSomething()
    {
        GD.Print("SpriteAImpl.DoSomething()");
    }
}
