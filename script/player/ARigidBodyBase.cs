using Godot;
using System;

public partial class ARigidBodyBase : RigidBody3D
{
	[Export] public float MaxLinearVelocity = 1000.0f;
	[Export] public float MaxAngularVelocity = 10.0f;

	public bool HasGroundContact { get; private set; } = false;
	public Vector3 LastGroundContactNormal { get; private set; }
	public Vector3 LastGroundContactPos { get; private set; }
	public PhysicsBody3D LastGroundContactBody { get; private set; }

	public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
		CheckCollisions(state);

        //state.LinearVelocity = xts.Truncate(state.LinearVelocity, MaxLinearVelocity);
        state.AngularVelocity = Xts.Truncate(state.AngularVelocity, MaxAngularVelocity);
	}

	protected void CheckCollisions(PhysicsDirectBodyState3D state)
	{
		HasGroundContact = false;
		int cc = state.GetContactCount();
		for (int i = 0; i < cc; ++i)
		{
			if (state.GetContactColliderObject(i) is PhysicsBody3D ob)
			{
				if (ob.CollisionLayer == Xts.GROUND_LAYER_VALUE)
				{
					HasGroundContact = true;
					LastGroundContactNormal = state.GetContactLocalNormal(i); // already in world space
					LastGroundContactPos = state.GetContactLocalPosition(i); // in world space - origin
					LastGroundContactPos += ob.GlobalPosition;
					LastGroundContactBody = ob;

                }
			} // if
		} // for
    } // void CheckCollisions
}
