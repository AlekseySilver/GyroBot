using Godot;
using System;
using System.IO;
using System.Runtime.CompilerServices;

public static partial class Xts
{
	public static bool ForeachChild<T>(this Node node, bool recursive, Func<T, bool> break_func) where T : Node
	{
		var n = node.GetChildCount();
		for (int i = 0; i < n; ++i)
		{
			var r = node.GetChildOrNull<T>(i);
			if (r != null && break_func(r))
				return true;
		}
		if (recursive)
		{
			for (int i = 0; i < n; ++i)
			{
				var r = node.GetChild(i);
				if (r.ForeachChild(true, break_func))
					return true;
			}
		}
		return false;
	} // ForeachChild

	public static T FirstChild<T>(this Node node, bool recursive = false) where T : Node
	{
		var n = node.GetChildCount();
		for (int i = 0; i < n; ++i)
		{
			var r = node.GetChildOrNull<T>(i);
			if (r != null)
				return r;
		}
		if (recursive)
		{
			for (int i = 0; i < n; ++i)
			{
				var r = node.GetChild(i).FirstChild<T>(true);
				if (r != null)
					return r;
			}
		}
		return null;
	} // FirstChild

	public static T FirstChild<T>(this Node node, bool recursive, string name) where T : Node
	{
		var n = node.GetChildCount();
		for (int i = 0; i < n; ++i)
		{
			var r = node.GetChildOrNull<T>(i);
			if (r != null && r.Name == name)
				return r;
		}
		if (recursive)
		{
			for (int i = 0; i < n; ++i)
			{
				var r = node.GetChild(i).FirstChild<T>(true, name);
				if (r != null)
					return r;
			}
		}
		return null;
	} // FirstChild
} // class Xts
