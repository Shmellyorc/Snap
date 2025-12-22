namespace Snap.Engine.Assets.Fonts;

/// <summary>
/// Represents a base class for font assets that provide glyph, kerning, and texture data.
/// </summary>
public class Font : IAsset
{
	internal SFTexture Texture { get; set; }

	/// <summary>
	/// Gets the collection of character glyphs defined in the font.
	/// </summary>
	public Dictionary<char, Glyph> Glyphs { get; protected set; } = [];

	/// <summary>
	/// Gets the kerning pairs defined between glyphs in the font.
	/// </summary>
	public Dictionary<(uint first, uint second), int> KerningLookup { get; protected set; } = [];

	/// <summary>
	/// Gets the unique identifier for this font asset.
	/// </summary>
	public uint Id { get; protected set; }

	/// <summary>
	/// Gets the tag or file path associated with the font source.
	/// </summary>
	public string Tag { get; protected set; }

	/// <summary>
	/// Gets a value indicating whether the font has been successfully loaded.
	/// </summary>
	public bool IsValid { get; protected set; }

	/// <summary>
	/// Gets the total number of bytes read from the font file.
	/// </summary>
	public ulong Length { get; protected set; }

	/// <summary>
	/// Gets the default spacing between glyphs.
	/// </summary>
	public virtual float Spacing { get; protected set; }

	/// <summary>
	/// Gets the native texture handle if the font is loaded; otherwise, returns 0.
	/// </summary>
	public uint Handle => IsValid ? Texture.NativeHandle : 0;

	/// <summary>
	/// Gets the line height used when rendering multi-line text.
	/// </summary>
	public virtual float LineSpacing { get; }

	/// <summary>
	/// Calculates the spacing offset for a specific character, factoring in advance and default spacing.
	/// </summary>
	/// <param name="c">The character to measure.</param>
	/// <returns>The spacing value in pixels.</returns>
	public virtual float GetSpacing(char c)
	{
		var g = Glyphs[c];

		return g.Advance + Spacing;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Font"/> class using a unique ID and font file tag.
	/// </summary>
	/// <param name="id">The unique identifier for the font.</param>
	/// <param name="filename">The file path or tag for the font source.</param>
	public Font(uint id, string filename)
	{
		Id = id;
		Tag = filename;
	}

	/// <summary>
	/// Loads the font data and returns its length in bytes. May be overridden by derived font types.
	/// </summary>
	/// <returns>The total number of bytes loaded.</returns>
	public virtual ulong Load() { return Length; }

	/// <summary>
	/// Unloads the font from memory and logs an informational message.
	/// </summary>
	public virtual void Unload()
	{
		if (!IsValid)
			return;

		Dispose();

		Logger.Instance.Log(LogLevel.Info, $"Unloaded asset {Id}, of {GetType().Name}");

		Length = 0ul;
	}

	/// <summary>
	/// Releases the resources used by the font, including its texture.
	/// </summary>
	public virtual void Dispose()
	{
		Texture?.Dispose();
		IsValid = false;
	}

	/// <summary>
	/// Measures the size of the given text string when rendered using this font.
	/// </summary>
	/// <param name="text">The text to measure.</param>
	/// <returns>A <see cref="Vect2"/> containing the width and height of the rendered text.</returns>
	public Vect2 Measure(string text)
	{
		var result = Vect2.Zero;
		float offsetX = 0f, offsetY = LineSpacing;

		for (int i = 0; i < text.Length; i++)
		{
			var c = text[i];

			if (c == '\r')
				continue;
			if (c == '\n')
			{
				offsetX = 0f;
				offsetY += LineSpacing;
				continue;
			}
			if (!Glyphs.TryGetValue(c, out var glyph))
				continue;

			offsetX += glyph.Advance;

			if (result.X < offsetX)
				result.X = offsetX;
			if (result.Y < offsetY)
				result.Y = offsetY;
		}

		result.X += MathF.Abs(Spacing);

		return result;
	}

	/// <summary>
	/// Gets the texture associated with this font.
	/// </summary>
	/// <returns>The font texture, or the default value if none is set.</returns>
	public virtual SFTexture GetTexture() { return default; }
}
