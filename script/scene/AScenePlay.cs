using Godot;
using System;
using System.Collections.Generic;

public partial class AScenePlay : Node3D
{
	[Export] public NodePath input;
	[Export] public NodePath camera;
	[Export] public NodePath player;


	readonly Queue<Action> _actionQueue = new();


	public void EnqueueAction(Action action)
	{
		_actionQueue.Enqueue(action);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		EnqueueAction(() =>
		{
			var _input = GetNodeOrNull<AInput>(input);
			var _camera = GetNodeOrNull<ACamera>(camera);
			var _player = GetNodeOrNull<AGyroBot>(player);

			_input.AddReceiver(_player);
			_input.AddReceiver(_camera);

			_camera.AssignTarget((ICameraTarget)_player);
		});
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		while (_actionQueue.Count > 0)
		{
			_actionQueue.Dequeue().Invoke();
		}
	}
}
