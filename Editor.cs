using Godot;
using System;

public partial class Editor : Control
{
    [Export]
    private float _targetOrthoSize;
    [Export]
    private float _targetFov = 100.0f;
    private Button _playButton;
    private Camera3D _camera;
    private bool _isPerspective = true;
    private float _transitionTime = 0.5f; // Duration of camera transition in seconds
    private float _currentTransition = 0.0f;
    private bool _isTransitioning = false;

    public override void _Ready()
    {
        // Initialize the play button
        _playButton = new Button();
        _playButton.Text = "Play";
        _playButton.Size = new Vector2(100, 50);
        _playButton.Position = new Vector2(10, 10); // Top-left corner with padding
        AddChild(_playButton);

        // Find the Camera3D in the scene
        _camera = GetViewport().GetCamera3D();
        if (_camera == null)
        {
            GD.PrintErr("No Camera3D found in the scene!");
            return;
        }

        // Set initial camera properties
        _camera.Projection = Camera3D.ProjectionType.Orthogonal;
        _camera.Size = _targetOrthoSize; // Default perspective FOV

        // Connect button signals
        _playButton.Pressed += OnPlayButtonPressed;
    }

    public override void _Process(double delta)
    {
        // Handle 'P' key press for color change
        if (Input.IsKeyPressed(Key.P))
        {
            ChangeButtonColor();
        }

        // Handle camera transition
        if (_isTransitioning)
        {
            _currentTransition += (float)delta;
            float t = Mathf.Clamp(_currentTransition / _transitionTime, 0.0f, 1.0f);

            if (_isPerspective)
            {
                // Transitioning to perspective
                _camera.Projection = Camera3D.ProjectionType.Perspective;
                _camera.Fov =  _targetFov;
            }
            else
            {
                // Transitioning to orthographic
                _camera.Projection = Camera3D.ProjectionType.Orthogonal;
                _camera.Size = _targetOrthoSize;
            }

            if (t >= 1.0f)
            {
                _isTransitioning = false;
                _camera.Projection = _isPerspective ? Camera3D.ProjectionType.Perspective : Camera3D.ProjectionType.Orthogonal;
            }
        }
    }

    private void OnPlayButtonPressed()
    {
        // Toggle between perspective and orthographic
        _isPerspective = !_isPerspective;
        _currentTransition = 0.0f;
        _isTransitioning = true;
    }

    private void ChangeButtonColor()
    {
        // Randomize button color
        _playButton.Modulate = new Color(
            (float)GD.RandRange(0.0, 1.0),
            (float)GD.RandRange(0.0, 1.0),
            (float)GD.RandRange(0.0, 1.0)
        );
    }
}
