namespace Snap.Engine.Helpers;

/// <summary>
/// Provides text‐formatting utilities such as word wrapping based on font metrics.
/// </summary>
public static class TextHelpers
{
	/// <summary>
	/// Wraps and formats the specified <paramref name="text"/> so that no line exceeds the given <paramref name="width"/>,
	/// using the measurements of the provided <paramref name="font"/>.
	/// </summary>
	/// <param name="font">The <see cref="Font"/> instance used to measure character widths and line heights.</param>
	/// <param name="text">The input text to wrap and format. May contain existing line breaks.</param>
	/// <param name="width">The maximum line width in pixels (or font units) allowed before wrapping.</param>
	/// <returns>
	/// A formatted string with line breaks inserted such that each line’s rendered width 
	/// does not exceed <paramref name="width"/>.
	/// </returns>
	public static string FormatText(Font font, string text, int width)
	{
		if (text.IsEmpty() || width == 0)
			return string.Empty;

		var sb = new StringBuilder();
		var lines = text.Split(["\r\n", "\n"], StringSplitOptions.None);

		foreach (var line in lines)
		{
			if (line.IsEmpty())
			{
				sb.AppendLine(); // Preserve empty lines
				continue;
			}

			var words = line.Split(' ');
			var current = new StringBuilder();

			foreach (var word in words)
			{
				string testLine = current.Length > 0 ? $"{current} {word}" : word;

				// Check if adding the word exceeds width
				if (font.Measure(testLine).X > width)
				{
					sb.AppendLine(current.ToString().Trim());
					current.Clear();
					current.Append(word);
				}
				else
				{
					if (current.Length > 0) current.Append(' ');
					current.Append(word);
				}
			}

			// Append last formatted line within this section
			if (current.Length > 0)
				sb.AppendLine(current.ToString().Trim());
		}

		return sb.ToString().TrimEnd(); // Trim only trailing newline
	}
}
