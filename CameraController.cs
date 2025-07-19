using Godot;

public partial class CameraController : Camera3D
{
	[Export] public float perspectiveFOV = 40;
	[Export] public float flattenedFOV = 5; // Very narrow FOV for flattened view
	[Export] public float MinX = -10f;
	[Export] public float MaxX = 10f;
	[Export] public float MinY = 0;
	[Export] public float MaxY = 10f;
	[Export] public float MoveSpeed = 10.0f;
	[Export] public bool PlayMode = false;

	public override void _Ready()
	{
		// Ensure camera starts within bounds
		ClampToBounds();
		// Always use perspective projection, just adjust FOV
		Projection = ProjectionType.Perspective;
		// Set initial FOV based on PlayMode
		Fov = PlayMode ? perspectiveFOV : flattenedFOV;
		GD.Print($"Initial FOV set to: {Fov}");
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		// Toggle PlayMode with spacebar - this prevents multiple triggers
		if (@event.IsActionPressed("ui_accept"))
		{
			PlayMode = !PlayMode;

			// Update FOV immediately when toggling
			if (PlayMode)
			{
				GD.Print("Entering Play Mode");
				// Fov = perspectiveFOV;
				GD.Print($"FOV set to: {Fov}");
			}
			else
			{
				GD.Print("Exiting Play Mode - Flattening View");
				// Fov = flattenedFOV;
				GD.Print($"FOV set to: {Fov}");
			}

			GetViewport().SetInputAsHandled(); // Prevent other nodes from processing this input
		}
	}

	public override void _Process(double delta)
	{
		HandleMovement(delta);
		ClampToBounds();
	}

	private void HandleMovement(double delta)
	{
		Vector3 input = Vector3.Zero;

		// Get 2D movement input (X and Y only) - WASD
		if (Input.IsActionPressed("ui_left") || Input.IsKeyPressed(Key.A))
			input.X += 1;
		if (Input.IsActionPressed("ui_right") || Input.IsKeyPressed(Key.D))
			input.X -= 1;
		if (Input.IsActionPressed("ui_up") || Input.IsKeyPressed(Key.W))
			input.Y += 1;
		if (Input.IsActionPressed("ui_down") || Input.IsKeyPressed(Key.S))
			input.Y -= 1;

		// Normalize and apply speed
		if (input.Length() > 0)
		{
			input = input.Normalized() * MoveSpeed;
		}

		// Apply movement only to X and Y, preserve Z
		Vector3 newPosition = GlobalPosition;
		newPosition.X += input.X * (float)delta;
		newPosition.Y += input.Y * (float)delta;
		GlobalPosition = newPosition;
	}

	private void ClampToBounds()
	{
		Vector3 position = GlobalPosition;
		// Clamp only X and Y axes
		position.X = Mathf.Clamp(position.X, MinX, MaxX);
		position.Y = Mathf.Clamp(position.Y, MinY, MaxY);
		GlobalPosition = position;
	}
}
