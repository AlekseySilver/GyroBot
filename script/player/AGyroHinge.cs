using Godot;
using System.Runtime.CompilerServices;

public partial class AGyroHinge : HingeJoint3D
{
	[Export] public float LimitSpeed { get; set; } = 10.0f;
	[Export] public float LimitBias { get; set; } = 0.01f;


	bool _isMotorEnabled = false;

	bool IsMotorEnabled
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _isMotorEnabled;
		set
		{
			_isMotorEnabled = value;
			SetFlag(Flag.EnableMotor, value);
		}
	}

	bool _isLimiting = false;
	public bool IsLimiting
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _isLimiting;
		private set
		{
			_isLimiting = value;
		}
	}

	/// <summary>
	/// целевой угол необходимого поворота и фиксации сустава
	/// </summary>
	public float TargetAngle { get; private set; }


	RigidBody3D _bodyA;
	RigidBody3D _bodyB;

	Basis _A2BZero;
	Vector3 _localAxisAroundInB;


	bool _defaultLimitEnabled;
	float _defaultLimitLower;
	float _defaultLimitUpper;

	float _limitSpeed;

	float LimitLower
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => GetParam(Param.LimitLower);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => SetParam(Param.LimitLower, value);
	}
	float LimitUpper
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => GetParam(Param.LimitUpper);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => SetParam(Param.LimitUpper, value);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_defaultLimitEnabled = GetFlag(Flag.UseLimit);
		_defaultLimitLower = GetParam(Param.LimitLower);
		_defaultLimitUpper = GetParam(Param.LimitUpper);

		//GD.Print($"_Ready {Name} - {_default_limit_enabled} L:{_default_limit_lower} U:{_default_limit_upper}");


		_bodyA = GetNode<RigidBody3D>(NodeA);
		_bodyB = GetNode<RigidBody3D>(NodeB);

		var A = _bodyA.GlobalTransform.Basis;
		var B = _bodyB.GlobalTransform.Basis;

		_A2BZero = Xts.TermSecond(B, A);
		_localAxisAroundInB = GlobalTransform.Basis.Column0 * B;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double timeStep)
	{
		float delta = (float)timeStep;

		if (IsLimiting)
		{
			float h = delta * _limitSpeed;
			float l = LimitLower + h;
			h = LimitUpper - h;

			var target_angle_upper = TargetAngle + LimitBias;

			if (l > TargetAngle)
			{
				if (h < target_angle_upper)
					UpdateLimitAngle(TargetAngle);
				else
				{
					LimitLower = TargetAngle;
					LimitUpper = h;
				}
			}
			else
			{
				LimitLower = l;
				LimitUpper = h < target_angle_upper ? target_angle_upper : h;
			}
		}
	} // _PhysicsProcess



	/// <summary>
	/// начать ограничение положения кости, кость сохраняет один угол с родительской
	/// </summary>
	/// <param name="range">от 0 до 1 (0 - low, 1 - high)</param>
	/// <param name="speed">скорость в градусах в секунду</param>
	public void StartLimit(float range, float speed)
	{
		Limit2Default();
		SetFlag(Flag.UseLimit, true);
		TargetAngle = GetAngle(range);
		IsLimiting = true;
		_limitSpeed = speed * LimitSpeed;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void StartLimit(float range)
	{
		StartLimit(range, 1f);
	}


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void StopLimit() => IsLimiting &= false;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void UpdateLimitAngle(float angle)
	{
		StopLimit();
		LimitLower = angle;
		LimitUpper = angle + LimitBias;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void Limit2Default()
	{
		StopLimit();
		SetFlag(Flag.UseLimit, _defaultLimitEnabled);
		LimitLower = _defaultLimitLower;
		LimitUpper = _defaultLimitUpper;
	}

	/// <summary>
	/// angle Between limits
	/// </summary>
	/// <param name="range">from 0 to 1</param>
	public float GetAngle(float range)
	{
		return Mathf.Lerp(_defaultLimitLower, _defaultLimitUpper, range);
	}

	float GetAngle()
	{
		var A = _bodyA.GlobalTransform.Basis;
		var B = _bodyB.GlobalTransform.Basis;

		// b = a + a2b
		// a2b = z + d
		// b = z + d + a
		var A2B = Xts.TermSecond(B, A);
		var delta = Xts.TermSecond(A2B, _A2BZero);
		var q = delta.GetRotationQuaternion();
		var AxisAroundInB = B * _localAxisAroundInB;
		var angle = -q.GetAngle();
		if (q.GetAxis().Dot(AxisAroundInB) < 0f)
			angle = -angle;
		return Mathf.RadToDeg(angle);
	}

	public void Relax()
	{
		Limit2Default();
	}

} // AGyroHinge
