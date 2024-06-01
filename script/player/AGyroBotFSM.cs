using Godot;
using System;


public partial class AGyroBot
{
	enum eMove
	{
		RELAX,
		IDLE,
		WALK,
		JUMP,
	}
	enum eAction
	{
		IDLE,
		PUNCH,
		AIM,
		FIRE,
	}

	readonly FSM<eMove> _fsmMove = new();
	readonly FSM<eAction> _fsmAction = new();

	float _fsmTimeRest = 0.0f;


	void InitFSM()
	{
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		var a = _fsmMove.AddAction(eMove.RELAX);
		a.OnStart = () =>
		{
			//GD.Print($"{Name} - FSM - RELAX");
			_body.GyroEnable = false;
			_hingeShoulderL.Relax();
			_hingeShoulderR.Relax();
			_hingeArmUL.Relax();
			_hingeArmUR.Relax();
			_hingeArmDL.Relax();
			_hingeArmDR.Relax();
		};
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		a = _fsmMove.AddAction(eMove.IDLE);
		a.OnStart = () =>
		{
			//GD.Print($"{Name} - FSM - IDLE");
			_body.GyroEnable = true;
			_hingeShoulderL.Relax();
			_hingeShoulderR.Relax();
			_hingeArmUL.Relax();
			_hingeArmUR.Relax();
			_hingeArmDL.Relax();
			_hingeArmDR.Relax();
		};
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		a = _fsmMove.AddAction(eMove.WALK);
		a.OnStart = () =>
		{
			//GD.Print($"{Name} - FSM - WALK");
			_body.GyroEnable = true;
			_body.HorizontalEnable = true;
			_hingeShoulderL.StartLimit(0.6f);
			_hingeShoulderR.StartLimit(0.6f);
			_hingeArmUL.StartLimit(0.2f);
			_hingeArmUR.StartLimit(0.2f);
			_hingeArmDL.StartLimit(0.9f);
			_hingeArmDR.StartLimit(0.9f);
		};
		a.CheckFinish = WalkCheckFinish;
		a.OnJumpOff = () =>
		{
			_wheel.AngularVelocityOverride = Vector3.Zero;
			_body.HorizontalEnable = false;
		};
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		a = _fsmMove.AddAction(eMove.JUMP);
		a.OnStart = () =>
		{
			//GD.Print($"{Name} - FSM - JUMP");

			_body.GyroEnable = true;
			_hingeShoulderL.StartLimit(0.4f);
			_hingeShoulderR.StartLimit(0.4f);
			_hingeArmUL.StartLimit(0.8f);
			_hingeArmUR.StartLimit(0.8f);
			_hingeArmDL.StartLimit(0.9f);
			_hingeArmDR.StartLimit(0.9f);

			_gyroSlider.JumpPerform();
		};
		a.OnJumpOff = () =>
		{
			_gyroSlider.JumpRestore();
		};
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		var b = _fsmAction.AddAction(eAction.IDLE);
		b.OnStart = () =>
		{
			//GD.Print($"{Name} - FSM - IDLE");
			_hingeShoulderL.Relax();
			_hingeShoulderR.Relax();
			_hingeArmUL.Relax();
			_hingeArmUR.Relax();
			_hingeArmDL.Relax();
			_hingeArmDR.Relax();
		};
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		b = _fsmAction.AddAction(eAction.PUNCH);
		b.OnStart = () =>
		{
			IsSwapSides = Random.Shared.Next(2) > 0;
			_animation.Call(Random.Shared.Next(2) > 0 ? "punch_hook" : "punch_jab");
			_body.HorizontalEnable = true;
		};
		b.CheckFinish = () =>
		{
			WalkCheckFinish();
			return _animation.Get("is_animating").AsBool() == false;
		};
		b.OnJumpOff = () =>
		{
			_body.HorizontalEnable = false;
		};
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		b = _fsmAction.AddAction(eAction.AIM);
		b.OnStart = () =>
		{
			_animation.Call("aim");
			if (_camera != null)
			{
				_camera.CameraMode = ACamera.eCameraMode._1stPerson;
			}
			_body.HorizontalEnable = true;
		};
		b.CheckFinish = () =>
		{
			if (_camera != null)
			{
				_body.HorizontalDirection = Vector3.Up.Cross(_camera.CurrentCameraRight);
			}
			return isAim() == false;
		};
		b.OnJumpOff = () =>
		{
			_body.HorizontalEnable = false;
			if (_camera != null)
			{
				_camera.CameraMode = ACamera.eCameraMode._3rdPerson;
			}
		};
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		b = _fsmAction.AddAction(eAction.FIRE);
		b.OnStart = () =>
		{
			if (_camera != null)
			{
				_camera.CameraMode = ACamera.eCameraMode._1stPerson;

				var ball = Asset.Instantiate<RigidBody3D>(VoidCanonBallResource, _body);
				ball.GlobalPosition = _body.GlobalPosition;
				ball.LinearVelocity = _camera.CurrentCameraDirection * VoidCanonBallSpeed;
			}
			_body.HorizontalEnable = true;
			_fsmTimeRest = 1f;
		};
		b.CheckFinish = () =>
		{
			_fsmTimeRest -= _timeStep;
			return _fsmTimeRest < 0f;
		};
		b.OnJumpOff = () =>
		{
			_body.HorizontalEnable = false;
			if (_camera != null)
			{
				_camera.CameraMode = ACamera.eCameraMode._3rdPerson;
			}
		};
		//////////////////////////////////////////////////////////////////////////////////////////////////////


		_fsmMove.SetAsDefault(eMove.RELAX);
		_fsmMove.SetAsActive(eMove.IDLE);

		_fsmAction.SetAsDefault(eAction.IDLE);
		_fsmAction.SetAsActive(eAction.IDLE);



		_fsmMove.AddJumpReason(eMove.RELAX, eMove.IDLE, () =>
		{
			return HasGroundContact; // TODO
		});
		_fsmMove.AddJumpReason(eMove.IDLE, eMove.WALK, isWalk);
		_fsmMove.AddJumpReason(eMove.WALK, eMove.IDLE, isIdle);

		_fsmMove.AddJumpReason(eMove.IDLE, eMove.JUMP, isJump);
		_fsmMove.AddJumpReason(eMove.WALK, eMove.JUMP, isJump);
		_fsmMove.AddJumpReason(eMove.JUMP, eMove.RELAX, _fsmMove.IsActiveActionFinished);
		_fsmMove.AddJumpReason(eMove.JUMP, eMove.IDLE, isOnGround);

		_fsmAction.AddJumpReason(eAction.IDLE, eAction.PUNCH, isFire);
		_fsmAction.AddJumpReason(eAction.PUNCH, eAction.IDLE, _fsmAction.IsActiveActionFinished);

		_fsmAction.AddJumpReason(eAction.IDLE, eAction.AIM, isAim);
		_fsmAction.AddJumpReason(eAction.AIM, eAction.IDLE, _fsmAction.IsActiveActionFinished);
		_fsmAction.AddJumpReason(eAction.AIM, eAction.FIRE, isFire);
		_fsmAction.AddJumpReason(eAction.FIRE, eAction.AIM, _fsmAction.IsActiveActionFinished);


		bool isIdle() => _input == null || (_input.Move == Vector2.Zero && _input.IsJump == false);
		bool isWalk() => _input != null && _input.Move != Vector2.Zero;
		bool isJump() => _input != null && _input.IsJump;
		bool isFire() => _input != null && _input.IsFire;
		bool isAim() => _input != null && _input.IsAim;

		bool isOnGround() => HasGroundContact;

		bool WalkCheckFinish()
		{
			if (_input != null && _camera != null)
			{
				var dir = _camera.GetDirectPad2World(_input.Move);
				if (dir != Vector3.Zero)
				{
					LastCtrlDir = dir.Normalized();
				}

				if (_addRotate != 0f)
					_body.HorizontalDirection = LastCtrlDir.Rotated(Vector3.Up, _addRotate);
				else
					_body.HorizontalDirection = LastCtrlDir;

				dir *= MoveForce;
				_wheel.AngularVelocityOverride = Vector3.Up.Cross(dir);
				return false;
			}
			return true;
		}


    } // void InitFSM

} // class AGyroBot