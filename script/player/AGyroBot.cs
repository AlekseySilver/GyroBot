using Godot;
using System;

public partial class AGyroBot : Node3D, IInputReceiver, ICameraTarget
{
	[Export(PropertyHint.File)]
	public string VoidCanonBallResource { get; set; }
	[Export]
	public float VoidCanonBallSpeed { get; set; } = 10f;


	public bool HasGroundContact { get; private set; } = false;
	public Vector3 GroundContactDir { get; private set; }
	public Vector3 GroundContactPos { get; private set; }

	public Vector3 LastCtrlDir { get; private set; } = Vector3.Zero;

	public bool IsSwapSides { get; private set; } = false;

    AInput _input = null;
    ACamera _camera = null;
    AGyroBody _body;
    AGyroWheel _wheel;
    AGyroSlider _gyroSlider;

    AGyroHinge _hingeShoulderL;
    AGyroHinge _hingeShoulderR;
    AGyroHinge _hingeArmUL;
    AGyroHinge _hingeArmUR;
    AGyroHinge _hingeArmDL;
    AGyroHinge _hingeArmDR;

    ARigidBodyBase _bodyBottom;
    ARigidBodyBase _bodyShoulderL;
    ARigidBodyBase _bodyShoulderR;
    ARigidBodyBase _bodyArmUL;
    ARigidBodyBase _bodyArmUR;
    ARigidBodyBase _bodyArmDL;
    ARigidBodyBase _bodyArmDR;

    float _timeStep = 0f;

	Node _animation;

	float _addRotate = 0f;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_body = this.FirstChild<AGyroBody>();
		_wheel = this.FirstChild<AGyroWheel>();
		_gyroSlider = this.FirstChild<AGyroSlider>();

		_hingeShoulderL = this.FirstChild<AGyroHinge>(false, "ShoulderLHinge");
		_hingeShoulderR = this.FirstChild<AGyroHinge>(false, "ShoulderRHinge");
		_hingeArmUL = this.FirstChild<AGyroHinge>(false, "ArmULHinge");
		_hingeArmUR = this.FirstChild<AGyroHinge>(false, "ArmURHinge");
		_hingeArmDL = this.FirstChild<AGyroHinge>(false, "ArmDLHinge");
		_hingeArmDR = this.FirstChild<AGyroHinge>(false, "ArmDRHinge");

		_bodyBottom = this.FirstChild<ARigidBodyBase>(false, "Bottom");
		_bodyShoulderL = this.FirstChild<ARigidBodyBase>(false, "ShoulderL");
		_bodyShoulderR = this.FirstChild<ARigidBodyBase>(false, "ShoulderR");
		_bodyArmUL = this.FirstChild<ARigidBodyBase>(false, "ArmUL");
		_bodyArmUR = this.FirstChild<ARigidBodyBase>(false, "ArmUR");
		_bodyArmDL = this.FirstChild<ARigidBodyBase>(false, "ArmDL");
		_bodyArmDR = this.FirstChild<ARigidBodyBase>(false, "ArmDR");

		_animation = this.FirstChild<Node>(false, "Animation");

		InitFSM();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_timeStep = (float)delta;
		_fsmMove.Update();
		_fsmAction.Update();

	} // _Process

	void UpdateGround(ARigidBodyBase body, ref float maxDot)
	{
		if (body.HasGroundContact)
		{
			float dot = body.LastGroundContactNormal.Y;
			if (dot > maxDot)
			{
				HasGroundContact = true;
				maxDot = dot;
				GroundContactDir = body.LastGroundContactNormal;
				GroundContactPos = body.LastGroundContactPos;
			}
		}
	}


	public override void _PhysicsProcess(double delta)
	{
		// upd ground data
		float maxDot = float.MinValue;
		HasGroundContact = false;
		UpdateGround(_body, ref maxDot);
		UpdateGround(_wheel, ref maxDot);
		UpdateGround(_bodyBottom, ref maxDot);
		UpdateGround(_bodyShoulderL, ref maxDot);
		UpdateGround(_bodyShoulderR, ref maxDot);
		UpdateGround(_bodyArmUL, ref maxDot);
		UpdateGround(_bodyArmUR, ref maxDot);
		UpdateGround(_bodyArmDL, ref maxDot);
		UpdateGround(_bodyArmDR, ref maxDot);


	} // _PhysicsProcess


	public void OnInputAttached(AInput input)
	{
		_input = input;
	}

	public void OnInputDetached(AInput input)
	{
		_input = null;
	}


	public Node3D GetCameraTargetNode(ACamera camera)
	{
		_camera = camera;
		return _body.FirstChild<Node3D>(false, "FP");
	}

	public void DetachCamera(ACamera camera)
	{
		_camera = null;
	}



	public void start_limit_SL(float range)
	{
		(IsSwapSides ? _hingeShoulderR : _hingeShoulderL).StartLimit(range);
	}
	public void start_limit_SR(float range)
	{
		(IsSwapSides ? _hingeShoulderL : _hingeShoulderR).StartLimit(range);
	}
	public void start_limit_UL(float range)
	{
		(IsSwapSides ? _hingeArmUR : _hingeArmUL).StartLimit(range);
	}
	public void start_limit_UR(float range)
	{
		(IsSwapSides ? _hingeArmUL : _hingeArmUR).StartLimit(range);
	}
	public void start_limit_DL(float range)
	{
		(IsSwapSides ? _hingeArmDR : _hingeArmDL).StartLimit(range);
	}
	public void start_limit_DR(float range)
	{
		(IsSwapSides ? _hingeArmDL : _hingeArmDR).StartLimit(range);
	}
	public void set_add_rotate(float range)
	{
		_addRotate = Mathf.Lerp(-Mathf.Pi, Mathf.Pi, range * 0.5f + 0.5f);
		if (IsSwapSides)
			_addRotate = -_addRotate;
	}

} // class A_gyro_bot
