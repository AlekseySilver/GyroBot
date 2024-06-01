using System.Collections.Generic;

public class FSM<T>
{
	public struct SJumpReason
	{
		public System.Func<bool> NeedJump;
		public T Jump2Action;
	}

	public class FSMJumpAction : FSMAction
	{
		readonly List<SJumpReason> _jumps = new();
		public List<SJumpReason> Jumps => _jumps;
	}


	readonly Dictionary<T, FSMJumpAction> _actions = new();


	FSMJumpAction _activeAction = null;
	T _defaultActionKey;


	public T ActiveActionKey { get; protected set; }
	public T PrevActionKey { get; protected set; }

	public void Update()
	{
		if (_activeAction != null)
		{
			foreach (var j in _activeAction.Jumps)
			{
				if (j.NeedJump != null && j.NeedJump())
				{
					if (SetAsActive(j.Jump2Action) == null)
						return;
					else
						break;
				}
			}
			_activeAction.Update();
		}
		else
			SetAsActive(_defaultActionKey);
    } // public void Update



	public FSMJumpAction GetAction(T key)
	{
		return _actions[key];
	}

	public FSMJumpAction AddAction(T key)
	{
		var a = new FSMJumpAction();
		_actions.Remove(key);
		_actions.Add(key, a);
		return a;
	}

	public bool RemoveAction(T key)
	{
		return _actions.Remove(key);
	}

	bool AddJumpReason(List<SJumpReason> jumpList, T toKey, System.Func<bool> needJump)
	{
		if (!_actions.ContainsKey(toKey))
			return false;

		var r = new SJumpReason
		{
			NeedJump = needJump,
			Jump2Action = toKey
		};
		jumpList.Remove(r);
		jumpList.Add(r);

		return true;
	}


	public bool AddJumpReason(T fromKey, T toKey, System.Func<bool> needJump)
	{
		if (!_actions.TryGetValue(fromKey, out FSMJumpAction f))
			return false;
		return AddJumpReason(f.Jumps, toKey, needJump);
	}

	public bool RemoveJumpReason(T fromKey, T toKey)
	{
		if (!_actions.TryGetValue(fromKey, out FSMJumpAction f))
			return false;

		int i = f.Jumps.FindIndex(r => { return r.Jump2Action.Equals(toKey); });
		if (i < 0)
			return false;
		f.Jumps.RemoveAt(i);

		return true;
	}

	public FSMJumpAction SetAsActive(T key)
	{
		var prev = _activeAction;
		_activeAction = _actions[key];
		PrevActionKey = ActiveActionKey;
		ActiveActionKey = key;

		if (_activeAction == null && !key.Equals(_defaultActionKey))
			return SetAsActive(_defaultActionKey);

		_activeAction?.Start();
		if (prev != _activeAction)
			prev?.OnJumpOff?.Invoke();
		return _activeAction;
	}

	public FSMJumpAction SetAsDefault(T key)
	{
		_defaultActionKey = key;
		return GetAction(key);
	}


	public bool IsActiveActionFinished() => _activeAction.IsFinished;


} // public class FSM
