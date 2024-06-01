using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


public static partial class Xts
{
	public const int GROUND_LAYER_ID = 0;   // 2^0 = 1
	public const uint GROUND_LAYER_VALUE = 1u;

	public const float SMALL_FLOAT = .000001f;
	public const float SIN05 = .08715574274765f;
	public const float SIN10 = .17364817766693f;
	public const float SIN15 = .25881904510252f;
	public const float SIN20 = .34202014332567f;
	public const float SIN30 = .5f;
	/// <summary>
	/// sqrt(2) / 2
	/// </summary>
	public const float SIN45 = .707106781186548f;
	public const float SIN50 = .766044443118978f;
	public const float SIN60 = .866025403784439f;
	public const float SIN65 = .90630778703665f;
	public const float SIN75 = .965925826289f;
	public const float SIN80 = .984807753012208f;
	public const float SIN85 = .9961946980917455f;

	public const float deg2rad = 0.017453292519943f;
	public const float rad2deg = 57.2957795130823f;

	/// <summary>
	/// sqrt(2)
	/// </summary>
	public const float SQRT2 = 1.414213562373095f;

	public static Vector3 Truncate(Vector3 v, float max_len)
	{
		Truncate(ref v, max_len);
		return v;
	} // Truncate

	public static void Truncate(ref Vector3 v, float max_len)
	{
		float len = v.LengthSquared();
		if (len > max_len * max_len)
		{
			len = Mathf.Sqrt(len);
			len = max_len / len;
			v *= len;
		}
    } // Truncate

    /// <summary>
	/// Sum = TermFirst * TermSecond
    /// </summary>
    /// <param name="Sum">the result of two consecutive turns</param>
    /// <param name="TermSecond">2nd turn</param>
    /// <returns>1st turn</returns>
    public static Basis TermFirst(in Basis Sum, in Basis TermSecond)
	{
		return Sum * TermSecond.Transposed();
	}

    /// <summary>
    /// Sum = TermFirst * TermSecond
    /// </summary>
    /// <param name="Sum">the result of two consecutive turns</param>
    /// <param name="TermFirst">1st turn</param>
    /// <returns>2nd turn</returns>
    public static Basis TermSecond(Basis Sum, in Basis TermFirst)
	{
		Sum = Sum.Transposed();
		Sum *= TermFirst;
		return Sum.Transposed();

	}

    /// <summary>
    /// direction * direction.Dot(vector);
    /// returns the value of the vector projected onto the axis (direction)
    /// projection of a vector onto a single direction vector
    /// the angle between the result and vector will always be less than 90
    /// regardless of which way the vector is pointing relative to the direction
    /// the result is the same with direction = -direction
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 ProjectionOn(this Vector3 vector, in Vector3 direction)
	{
		return direction * direction.Dot(vector);
	}
} // class Xts
