using Godot;
using System;

public partial class AVoidCanonBall : RigidBody3D
{
    [Export] public float ImpactRadius = 1f;


    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        if (ContactMonitor == false)
            return;

        bool hasImpact = false;
        int cc = state.GetContactCount();
        for (int i = 0; i < cc; ++i)
        {
            if (state.GetContactColliderObject(i) is CsgCombiner3D comb)
            {
                var sphere = new CsgSphere3D
                {
                    Operation = CsgShape3D.OperationEnum.Subtraction,
                    Radius = ImpactRadius
                };
                comb.AddChild(sphere);
                sphere.GlobalPosition = state.GetContactLocalPosition(i); // in world space - origin

                hasImpact = true;
            } // if
        } // for

        if (hasImpact)
        {
            ContactMonitor = false;
            var particles = this.FirstChild<CpuParticles3D>();
            if (particles != null)
            {
                particles.Emitting = true;
            }
        }
    }

    void _on_cpu_particles_3d_finished()
    {
        QueueFree();
    }
}
