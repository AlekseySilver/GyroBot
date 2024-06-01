using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


public sealed partial class ACamera : Camera3D, IInputReceiver
{
	public enum eCameraMode { _3rdPerson, _1stPerson }


	[Export] public float ArmSpeed = 50f;

	[Export] public float RotateSpeed = 2f;


	[Export]
	public float Gap
	{
		get => _gap;
		set
		{
			_gap = value;
		}
	}

	[Export] public float LookAtSpeed = 3f;
	[Export] public float CloseTargetDist = 7f;
	[Export] public float AutoTurnRayAperture = 15f;
	[Export] public float AutoTurnSpeed = 0.5f;
	[Export] public float ArmGap = 1f;
	[Export] public float UserXOverrideSpeed = 1f;


	public float ArmLength
	{
		get => _armLength;
		set
		{
			_armLength = Mathf.Clamp(value, 5f, 350f);
		}
	}
	float _armLength = 5f;
	float _gap = 25f;
	float _rayHitLength = 0f;

	public bool IsTargetClose => _rayHitLength > 0f && _rayHitLength < CloseTargetDist;

	public float minRadius
	{
		get => _minRadius;
		set
		{
			_minRadius = value;
			_minRadiusSq = value * value;
		}
	}
	float _minRadius = 0f;
	float _minRadiusSq = 0f;

	Vector2 _userFlatRotate = Vector2.Zero;

	Node3D _targetNode;
	Vector3 _targetWorldOffset;


	Vector3 _2lookAtDir = Vector3.Zero;
	Vector3 _2lookAtAround = Vector3.Zero;
	float _2lookAtAngle = 0f;

	Vector3 _linVel = Vector3.Zero;

	float _userXOverride = 0f;
	float _userXOverrideChange = 0f;
	AInput _input = null;


    public eCameraMode CameraMode { get; set; } = eCameraMode._3rdPerson;


	public Node3D TargetNode
	{
		get => _targetNode;
		private set
		{
			_targetNode = value;
			LastTargetPosition = GetTargetPosition();
		}
	}
	public Vector3 TargetWorldOffset
	{
		get => _targetWorldOffset;
		private set
		{
			_targetWorldOffset = value;
			LastTargetPosition = GetTargetPosition();
		}
	}
	public Vector3 CurrentCameraDirection { get; private set; }
	public Vector3 GlobalUp { get; set; } = Vector3.Up;
	public Vector3 CurrentCameraRight { get; set; } = Vector3.Right;

	Vector3 GetTargetPosition() => TargetNode.GlobalTransform.Origin + TargetWorldOffset;


	public Vector3 GetDirectPad2World(in Vector2 pad)
	{
		var forward = GlobalUp.Cross(CurrentCameraRight);
		return CurrentCameraRight * pad.X + forward * pad.Y;
	}


	public void OnInputAttached(AInput input)
	{
		_input = input;
	}

	public void OnInputDetached(AInput input)
	{
		_input = null;
	}


	/// <summary>
	/// the position of the target, where the camera is looking
	/// it may differ from the actual position of the GetTargetPosition() target by the size of the Gap (the radius of the Gap ball)
	/// </summary>
	public Vector3 LastTargetPosition { get; private set; }


	public override void _Process(double delta)
	{
		if (TargetNode == null)
			return;

		if (_input != null)
		{
			_userFlatRotate = _input.Camera;
		}

		float timeStep = (float)delta;

		ImmediateXForm();

		if (_2lookAtAngle > 0f) // look at point
        {
			_2lookAtAngle -= timeStep * LookAtSpeed;
			if (_2lookAtAngle <= 0f)
			{
				CurrentCameraDirection = _2lookAtDir;
			}
			else
			{
				CurrentCameraDirection = _2lookAtDir * new Basis(_2lookAtAround, _2lookAtAngle);
			}
		}
		else
		{
			Vector2 rot2 = _userFlatRotate;

			if (_userXOverrideChange != 0f)
			{
				_userXOverride += _userXOverrideChange * timeStep * AutoTurnSpeed;
			}
			else if (_userXOverride != 0f)
			{
				if (_userXOverride > 0f)
				{
					_userXOverride -= timeStep * AutoTurnSpeed;
					if (_userXOverride < 0f)
						_userXOverride = 0f;
				}
				else
				{
					_userXOverride += timeStep * AutoTurnSpeed;
					if (_userXOverride > 0f)
						_userXOverride = 0f;
				}
			}
			if (_userXOverride != 0f && rot2.X == 0f)
			{
				rot2.X = _userXOverride;
			}

			if (rot2 != Vector2.Zero) // user's rotate
            {
				float d = CurrentCameraDirection.Dot(GlobalUp);
				if (rot2.Y != 0f)
				{
					CurrentCameraDirection = new Basis(CurrentCameraRight, rot2.Y * timeStep * RotateSpeed) * CurrentCameraDirection;

					if (d > Xts.SIN80)
					{
						CurrentCameraDirection = GlobalUp.Cross(CurrentCameraRight) * Xts.SIN10 + GlobalUp * Xts.SIN80;
					}
					else if (d < -Xts.SIN80)
					{
						CurrentCameraDirection = GlobalUp.Cross(CurrentCameraRight) * Xts.SIN10 - GlobalUp * Xts.SIN80;
					}

					CurrentCameraDirection = CurrentCameraDirection.Normalized();
				}

				if (rot2.X != 0f)
				{
					if (d < 0f)
						d = -d;
					d = 1f - d * .75f;
					CurrentCameraDirection = new Basis(Transform.Basis.Y, rot2.X * timeStep * -RotateSpeed * d) * CurrentCameraDirection;
				}
				if (_2lookAtAngle > 0f)
					_2lookAtAngle = 0f; // cancel look at
			}
		}
	} // _Process


	public void ImmediateXForm()
	{
		Vector3 pos;
		Vector3 up;

		pos = GetTargetPosition();
		if (CameraMode == eCameraMode._3rdPerson)
		{
            // the position of the target, taking into account the GAP
            var ln = (pos - LastTargetPosition).LengthSquared();
			if (ln > Gap * Gap)
				LastTargetPosition = pos;
			else
			{
				ln = (float)Math.Sqrt(ln);

				ln /= Gap;
				ln = ln * ln * (3f - 2f * ln);

				LastTargetPosition = LastTargetPosition.Lerp(pos, ln);
			}

            // camera direction
            CurrentCameraRight = CurrentCameraDirection.Cross(GlobalUp).Normalized();
			up = CurrentCameraRight.Cross(CurrentCameraDirection).Normalized();

			var arm_offset = CurrentCameraDirection * -(_rayHitLength != 0f ? _rayHitLength : ArmLength);
			pos = arm_offset + LastTargetPosition;

		}
		else
		{
            // camera direction
            CurrentCameraRight = CurrentCameraDirection.Cross(GlobalUp).Normalized();
			up = CurrentCameraRight.Cross(CurrentCameraDirection).Normalized();
		}

		Transform = new Transform3D(CurrentCameraRight, up, -CurrentCameraDirection, pos);
    } // ImmediateXForm


    


	public void LookAt(in Vector3 dir, bool forced = false)
	{
		_2lookAtDir = dir;

		float d = CurrentCameraDirection.Dot(_2lookAtDir);
		if (d > Xts.SIN85)
			return;

		if (d < -Xts.SIN85)
		{
			_2lookAtAround = GlobalUp;
		}
		else
		{
			_2lookAtAround = CurrentCameraDirection.Cross(_2lookAtDir).Normalized();
		}

		_2lookAtAngle = (float)Math.Acos(d);
    } // LookAt



    #region target_assing

    public void AssignTarget(ICameraTarget target)
	{
		AssignTarget(target.GetCameraTargetNode(this));
	}
	public void AssignTarget(Node3D targetNode, Vector3 offset)
	{
		StopAssignTarget();
		if (targetNode != null)
		{
			TargetNode = targetNode;
			TargetWorldOffset = offset;
			LastTargetPosition = GetTargetPosition();

			var delta = LastTargetPosition - Transform.Origin;
			CurrentCameraDirection = delta.Normalized();
			if (delta.LengthSquared() > 100f || GlobalUp == Vector3.Zero)
				ImmediateXForm();
		}
	}
	public void AssignTarget(Node3D targetNode)
	{
		AssignTarget(targetNode, Vector3.Zero);
	}

	readonly Queue<Action> _onStopAssignQueue = new(1);
	void StopAssignTarget()
	{
		while (_onStopAssignQueue.Count > 0)
			_onStopAssignQueue.Dequeue()?.Invoke();
	}

	#endregion
}


public interface ICameraTarget
{
	Node3D GetCameraTargetNode(ACamera camera);

	void DetachCamera(ACamera camera);
}