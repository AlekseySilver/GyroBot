using Godot;
using System;
using System.Collections.Generic;

public partial class AInput : Node
{
	[Export] Vector2 MouseSensitivity = new(0.003f, -0.003f);

	readonly List<IInputReceiver> _receivers = new();

	public void AddReceiver(IInputReceiver receiver)
	{
		_receivers.Add(receiver);
		receiver.OnInputAttached(this);
	}
	public bool RemoveReceiver(IInputReceiver receiver)
	{
		if (_receivers.Remove(receiver))
		{
			receiver.OnInputDetached(this);
			return true;
		}
		return false;
	}

	public Vector2 Move { get; private set; }
	public Vector2 Camera { get; private set; }

	public bool IsFire { get; private set; }
	public bool IsJump { get; private set; }
	public bool IsAim { get; private set; }

	public float JumpPressedTime { get; private set; } = 0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Move = new Vector2(Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"), Input.GetActionStrength("move_up") - Input.GetActionStrength("move_down"));
		Camera = new Vector2(Input.GetActionStrength("camera_right") - Input.GetActionStrength("camera_left"), Input.GetActionStrength("camera_up") - Input.GetActionStrength("camera_down"));
		if (Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			Camera += Input.GetLastMouseVelocity() * MouseSensitivity;
		}

		IsFire = Input.IsActionPressed("fire");
		IsAim = Input.IsActionPressed("aim");
		IsJump = Input.IsActionPressed("jump");
		if (IsJump)
		{
			JumpPressedTime += (float)delta;
		}
		else if (JumpPressedTime > 0f)
		{
			JumpPressedTime = 0f;
		}

		if (Input.IsPhysicalKeyPressed(Key.Escape))
		{
			//Input.MouseMode = Input.MouseModeEnum.Visible;
			GetTree().Quit();
		}
		/*if (Input.IsMouseButtonPressed(MouseButton.Left | MouseButton.Right)) 
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}*/
	}

} // class AInput


public interface IInputReceiver
{
	void OnInputAttached(AInput input);

	void OnInputDetached(AInput input);
}