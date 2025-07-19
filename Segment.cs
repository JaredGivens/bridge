using Godot;
using System;

public partial class Segment : RigidBody3D
{
    [Export]
    public float MaxLength= 1.0f;

    [Export]
    public float length = 1.0f;

    [Export]
    private float Strength = 100.0f;

    public override void _Ready()
    {
    }

}
