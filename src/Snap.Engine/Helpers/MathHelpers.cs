namespace Snap.Engine.Helpers;

/// <summary>
/// Collection of helpful mathematical utility functions.
/// </summary>
public static class MathHelpers
{
	/// <summary>
	/// Represents the mathematical constant π (pi), approximately 3.14159265358979.
	/// </summary>
	public const float Pi = MathF.PI;

	/// <summary>
	/// Represents the mathematical constant 2π (two times pi), approximately 6.28318530717959.
	/// </summary>
	public const float TwoPi = MathF.PI * 2f;

	/// <summary>
	/// Represents π/2 (90 degrees in radians), approximately 1.5707963267949.
	/// </summary>
	public const float PiOver2 = MathF.PI / 2f;

	/// <summary>
	/// Represents π/4 (45 degrees in radians), approximately 0.785398163397448.
	/// </summary>
	public const float PiOver4 = MathF.PI / 4f;

	/// <summary>
	/// Represents the reciprocal of π (1/π), approximately 0.318309886183791.
	/// </summary>
	public const float InvPi = 1f / MathF.PI;

	/// <summary>
	/// Represents the conversion factor from degrees to radians (π/180).
	/// </summary>
	public const float DegToRad = MathF.PI / 180f;

	/// <summary>
	/// Represents the conversion factor from radians to degrees (180/π).
	/// </summary>
	public const float RadToDeg = 180f / MathF.PI;

	/// <summary>
	/// Linearly interpolates between <paramref name="a"/> and <paramref name="b"/> by <paramref name="t"/>,
	/// clamping <paramref name="t"/> to the [0,1] range.
	/// </summary>
	/// <param name="a">The start value.</param>
	/// <param name="b">The end value.</param>
	/// <param name="t">Interpolation factor (0 = <paramref name="a"/>, 1 = <paramref name="b"/>).</param>
	/// <returns>The interpolated value.</returns>
	public static float Lerp(float a, float b, float t) =>
		a + ((b - a) * Clamp01(t));

	/// <summary>
	/// Performs a smooth (ease‑in‑out) interpolation between <paramref name="a"/> and <paramref name="b"/>.
	/// Uses the smoothstep curve 3t² − 2t³ for a smoother transition.
	/// </summary>
	/// <param name="a">The start value.</param>
	/// <param name="b">The end value.</param>
	/// <param name="t">Interpolation factor (0 = <paramref name="a"/>, 1 = <paramref name="b"/>).</param>
	/// <returns>The smoothly interpolated value.</returns>
	public static float SmoothLerp(float a, float b, float t)
	{
		t = Clamp01(t);
		// smoothstep curve: 3t²−2t³
		float u = t * t * (3f - 2f * t);
		return Lerp(a, b, u);
	}

	/// <summary>
	/// Linearly interpolates between <paramref name="a"/> and <paramref name="b"/> by <paramref name="t"/>
	/// without clamping.
	/// </summary>
	/// <param name="a">The start value.</param>
	/// <param name="b">The end value.</param>
	/// <param name="t">Interpolation factor.</param>
	/// <returns>The interpolated value.</returns>
	public static float LerpPrecise(float a, float b, float t) =>
		a + (b - a) * t;

	/// <summary>
	/// Clamps the given value to the [0,1] range.
	/// </summary>
	/// <param name="t">The value to clamp.</param>
	/// <returns><c>0</c> if <paramref name="t"/> is less than 0; <c>1</c> if greater than 1; otherwise <paramref name="t"/>.</returns>
	public static float Clamp01(float t) =>
		t < 0f ? 0f : t > 1f ? 1f : t;

	/// <summary>
	/// Calculates the midpoint between <paramref name="current"/> and <paramref name="target"/>.
	/// Optionally rounds the result.
	/// </summary>
	/// <param name="current">The current value.</param>
	/// <param name="target">The target value.</param>
	/// <param name="rounded">Whether to round the midpoint to the nearest integer.</param>
	/// <returns>The midpoint between <paramref name="current"/> and <paramref name="target"/>.</returns>
	public static float Center(float current, float target, bool rounded)
	{
		return rounded
			? MathF.Round((current - target) / 2f)
			: (current - target) / 2f;
	}

	/// <summary>
	/// Wraps an integer value into the range [<paramref name="min"/>, <paramref name="max"/>).
	/// </summary>
	/// <param name="value">The value to wrap.</param>
	/// <param name="min">The inclusive lower bound.</param>
	/// <param name="max">The exclusive upper bound. Must be greater than <paramref name="min"/>.</param>
	/// <returns>The wrapped value within [<paramref name="min"/>, <paramref name="max"/>).</returns>
	/// <exception cref="ArgumentException">Thrown if <paramref name="max"/> is less than or equal to <paramref name="min"/>.</exception>
	public static int Wrap(int value, int min, int max)
	{
		int range = max - min;
		if (range <= 0)
			throw new ArgumentException("max must be greater than min");

		int mod = (value - min) % range;
		if (mod < 0)
			mod += range;

		return mod + min;
	}

	/// <summary>
	/// Wraps a floating‑point value into the range [<paramref name="min"/>, <paramref name="max"/>).
	/// </summary>
	/// <param name="value">The value to wrap.</param>
	/// <param name="min">The inclusive lower bound.</param>
	/// <param name="max">The exclusive upper bound. Must be greater than <paramref name="min"/>.</param>
	/// <returns>The wrapped value within [<paramref name="min"/>, <paramref name="max"/>).</returns>
	/// <exception cref="ArgumentException">Thrown if <paramref name="max"/> is less than or equal to <paramref name="min"/>.</exception>
	public static float Wrap(float value, float min, float max)
	{
		float range = max - min;

		if (range <= 0f)
			throw new ArgumentException("max must be greater than min");

		float mod = (value - min) % range;

		if (mod < 0f)
			mod += range;

		return mod + min;
	}

	/// <summary>Determines whether two floats are approximately equal within the specified tolerance.</summary>
	/// <param name="a">The first value.</param>
	/// <param name="b">The second value.</param>
	/// <param name="epsilon">The tolerance. Default is 1e-6f.</param>
	/// <returns><see langword="true"/> if the values are equal within tolerance; otherwise, <see langword="false"/>.</returns>
	public static bool AlmostEquals(float a, float b, float epsilon = 1e-6f)
		=> MathF.Abs(a - b) <= epsilon;

	/// <summary>Determines whether a float is approximately zero within the specified tolerance.</summary>
	/// <param name="a">The value to check.</param>
	/// <param name="epsilon">The tolerance. Default is 1e-6f.</param>
	/// <returns><see langword="true"/> if the value is within tolerance of zero; otherwise, <see langword="false"/>.</returns>
	public static bool AlmostZero(float a, float epsilon = 1e-6f)
		=> MathF.Abs(a) <= epsilon;
}
