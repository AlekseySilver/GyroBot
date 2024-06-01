using Godot;
using System;

public partial class AGyroSlider : Generic6DofJoint3D
{
	[Export] public float JumpLimit = 0.5f;

	float _defaultLowLimit;
	float _defaultHighLimit;

	public override void _Ready()
	{
		_defaultLowLimit = GetParamY(Param.LinearLowerLimit);
		_defaultHighLimit = GetParamY(Param.LinearUpperLimit);
	}

	public void JumpPerform()
	{
		//SetFlagY(Flag.EnableLinearMotor, true);
		SetParamY(Param.LinearLowerLimit, JumpLimit);
		SetParamY(Param.LinearUpperLimit, JumpLimit + 0.01f);
	}

	public void JumpRestore()
	{
		SetParamY(Param.LinearLowerLimit, _defaultLowLimit);
		SetParamY(Param.LinearUpperLimit, _defaultHighLimit);
	}
}
