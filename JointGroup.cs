using Godot;
using System.Collections.Generic;

public partial class JointGroup : Node3D
{
    [Export]
    private Button _button;

    private List<HingeJoint3D> _joints = new List<HingeJoint3D>();
    private List<PhysicsBody3D> _bodies = new List<PhysicsBody3D>();
    private bool _isScaling = false;
    private MeshInstance3D _strutMesh;
    private Vector3 _startPosition;
    private Camera3D _camera;

    public override void _Ready()
    {
        _camera = GetViewport().GetCamera3D();

        // Connect button signals
        _button.Pressed += OnLeftClick; // Left click initiates strut creation
        //_button.MouseDefaultCursorShape = CursorShape.PointingHand;
        //_button.MouseFilter = MouseFilterEnum.Stop;

        // Connect input event for right-click detection
        _button.FocusMode = Control.FocusModeEnum.None; // Remove focus outline

    }

    public override void _Process(double delta)
    {
        UpdateButtonPosition();
        if (_isScaling && _strutMesh != null)
        {
            // Get cursor position in 3D world
            Vector2 mousePos = GetViewport().GetMousePosition();
            Vector3 worldPos = ProjectMouseToWorld(mousePos);

            // Update strut mesh to stretch from start to cursor
            UpdateStrutMesh(worldPos);
        }
    }
    private void UpdateButtonPosition()
    {
        // Project JointGroup's 3D position to screen space
        Vector2 screenPos = _camera.UnprojectPosition(GlobalPosition);
        
        // Offset to top-right (e.g., 20 pixels right, 20 pixels up)
         Vector2 buttonSize = _button.Size;
        Vector2 offset = -buttonSize / 2;
        _button.Position = screenPos + offset;
        _button.Position = screenPos + offset;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Right)
            {
                OnRightClick(); // Cancel stretching on right-click anywhere
            }
        }
    }

    private void OnLeftClick()
    {
        if (_isScaling)
        {
            // Finalize strut on second left click
            FinalizeStrut();
            _isScaling = false;
        }
        else
        {
            // Start new strut on first left click
            StartNewStrut();
            _isScaling = true;
        }
    }

    private void OnRightClick()
    {
        if (_isScaling)
        {
            // Cancel strut creation
            _strutMesh?.QueueFree();
            _strutMesh = null;
            _isScaling = false;
            GD.Print("Strut creation cancelled");
        }
    }

    private void StartNewStrut()
    {
        // Create a new strut mesh (e.g., a cylinder)
        _strutMesh = new MeshInstance3D();
        var cylinderMesh = new CylinderMesh
        {
            TopRadius = 0.1f,
            BottomRadius = 0.1f,
            Height = 1.0f
        };
        _strutMesh.Mesh = cylinderMesh;
        AddChild(_strutMesh);

        // Set start position to this JointGroup's position
        _startPosition = GlobalPosition;
        _strutMesh.GlobalPosition = _startPosition;
        GD.Print("Started new strut at: ", _startPosition);
    }

    private Vector3 ProjectMouseToWorld(Vector2 mousePos)
    {
        // Project mouse position to a plane in 3D space (e.g., z=0 plane)
        Vector3 from = _camera.ProjectRayOrigin(mousePos);
        Vector3 dir = _camera.ProjectRayNormal(mousePos);
        float distance = -from.Z / dir.Z; // Intersect with z=0 plane
        return from + dir * distance;
    }

    private void UpdateStrutMesh(Vector3 endPos)
    {
        // Calculate direction and length
        Vector3 direction = endPos - _startPosition;
        float length = direction.Length();
        if (length < 0.01f) length = 0.01f; // Avoid zero length

        // Update mesh scale and rotation
        _strutMesh.GlobalPosition = (_startPosition + endPos) / 2; // Center of strut
        ((CylinderMesh)_strutMesh.Mesh).Height = length;

        // Orient the strut to point from start to end
        if (direction != Vector3.Zero)
        {
            _strutMesh.LookAt(endPos, Vector3.Up);
            _strutMesh.RotateObjectLocal(Vector3.Right, Mathf.DegToRad(90)); // Adjust for cylinder orientation
        }
    }

    private void FinalizeStrut()
    {
        if (_strutMesh == null) return;

        // Create a physics body for the strut
        var body = new StaticBody3D();
        var collisionShape = new CollisionShape3D();
        var shape = new CylinderShape3D
        {
            Height = ((CylinderMesh)_strutMesh.Mesh).Height,
            Radius = 0.1f
        };
        collisionShape.Shape = shape;
        body.AddChild(collisionShape);
        body.GlobalTransform = _strutMesh.GlobalTransform;
        GetParent().AddChild(body);

        // Add to bodies list

        foreach (var previousBody in _bodies)
        {
          var joint = new HingeJoint3D();
          joint.GlobalPosition = GlobalPosition;
          joint.NodeA = previousBody.GetPath();
          joint.NodeB = body.GetPath();
          GetParent().AddChild(joint);
          _joints.Add(joint);
        }
        _bodies.Add(body);

        // Keep the mesh, reset scaling state
        _strutMesh = null;
        GD.Print("Strut finalized");
    }
}
