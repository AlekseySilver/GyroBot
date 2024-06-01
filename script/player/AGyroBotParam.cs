using Godot;

public partial class AGyroBot
{
	[Export] public float MoveForce { get; set; } = 15f;

	[Export] public float PunchMaxTime { get; set; } = 0.5f;

	[Export] public float PunchForce { get; set; } = 50f;
}



