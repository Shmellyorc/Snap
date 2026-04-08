// namespace System;

// /// <summary>
// /// Provides extension methods for <see cref="Enum"/> values to measure text dimensions
// /// using the font associated with the enum via <see cref="AssetManager"/>.
// /// </summary>
// public static class EnumExtensions
// {
// 	/// <summary>
// 	/// Measures the width of the specified text using the font associated with this enum value.
// 	/// </summary>
// 	/// <param name="value">The enum value used to retrieve the font.</param>
// 	/// <param name="text">The text to measure.</param>
// 	/// <returns>The width of the rendered text.</returns>
// 	public static float MeasureWidth(this Enum value, string text)
// 	{
// 		var f = AssetManager.GetFont(value);
// 		return f.MeasureWidth(text);
// 	}

// 	/// <summary>
// 	/// Measures the height of the specified text using the font associated with this enum value.
// 	/// </summary>
// 	/// <param name="value">The enum value used to retrieve the font.</param>
// 	/// <param name="text">The text to measure.</param>
// 	/// <returns>The height of the rendered text.</returns>
// 	public static float MeasureHeight(this Enum value, string text)
// 	{
// 		var f = AssetManager.GetFont(value);
// 		return f.MeasureHeight(text);
// 	}

// 	/// <summary>
// 	/// Measures the size (width and height) of the specified text using the font associated with this enum value.
// 	/// </summary>
// 	/// <param name="value">The enum value used to retrieve the font.</param>
// 	/// <param name="text">The text to measure.</param>
// 	/// <returns>
// 	/// A <see cref="Vect2"/> where X is the width and Y is the height of the rendered text.
// 	/// </returns>
// 	public static Vect2 Measure(this Enum value, string text)
// 	{
// 		var f = AssetManager.GetFont(value);
// 		return f.Measure(text);
// 	}
// }
