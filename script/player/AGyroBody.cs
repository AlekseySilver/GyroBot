using Godot;
using System;

public sealed partial class AGyroBody : ARigidBodyBase
{
	[Export] public bool GyroEnable = false;
	[Export] public float GyroForce = 15f;

	[Export] public float HorizontalForce = 30f;

	public Vector3 HorizontalDirection { get; set; } = Vector3.Zero;

	public bool HorizontalEnable { get; set; } = false;


	public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
		base._IntegrateForces(state);

		var basis = GlobalTransform.Basis;

		if (GyroEnable)
		{
			state.AngularVelocity += basis.Y.Cross(Vector3.Up) * GyroForce;
		}
		if (HorizontalEnable && HorizontalDirection != Vector3.Zero)
		{
			var velocity = basis.Z.Cross(HorizontalDirection);
			velocity = velocity.ProjectionOn(basis.Y); // only rotate around up axis
			velocity *= HorizontalForce;
			velocity -= state.AngularVelocity.ProjectionOn(velocity.Normalized());
			state.AngularVelocity += velocity;
		}
	}
}
