using Godot;
using System;

public static class Asset
{
    /// <summary>
    /// instatntiate asset by name
    /// </summary>
    public static T Instantiate<T>(string resource) where T : Node
    {
        T i = GD.Load<PackedScene>(resource).Instantiate<T>();
        return i;
    }

    public static T Instantiate<T>(string resource, Node parent) where T : Node
    {
        T i = Instantiate<T>(resource);
        parent.AddChild(i);
        parent.MoveChild(i, 0);
        return i;
    }


}