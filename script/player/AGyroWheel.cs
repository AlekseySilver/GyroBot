using Godot;
using System;

public partial class AGyroWheel : ARigidBodyBase
{
	public Vector3 AngularVelocityOverride { get; set; } = Vector3.Zero;


	public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
		base._IntegrateForces(state);

		state.AngularVelocity = AngularVelocityOverride;
	}
}
