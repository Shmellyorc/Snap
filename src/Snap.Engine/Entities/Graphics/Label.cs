using Snap.Engine.Graphics.Markup;

namespace Snap.Engine.Entities.Graphics;

/// <summary>
/// Represents a drawable text label entity with support for multi-line text, alignment, shadow, and partial text display.
/// </summary>
public sealed class Label : Entity
{
	private string _text = string.Empty;
	private bool _isDirty = true;
	private readonly List<string> _displayText = [];
	private readonly Font _font;
	private int _visibleLength = -1; // Default: Show full text
	private readonly Dictionary<string, Vect2> _cachedMeasurements = [];
	private float _cachedMaxHeight = 0f;
	private RenderTarget _rt;
	private bool _rtChecked;

	/// <summary>
	/// Gets or sets the offset for the text shadow.
	/// </summary>
	public Vect2 ShadowOffset { get; set; } = Vect2.One;

	/// <summary>
	/// Gets or sets the offset applied to the text position.
	/// </summary>
	public Vect2 Offset { get; set; } = Vect2.Zero;

	// /// <summary>
	// /// Gets or sets the color of the text.
	// /// </summary>
	// public Color Color { get; set; } = Color.White;

	/// <summary>
	/// Gets or sets the color of the text shadow.
	/// </summary>
	public Color ShadowColor { get; set; } = Color.Black * 0.2f;

	/// <summary>
	/// Gets or sets whether the text shadow is enabled.
	/// </summary>
	public bool Shadow { get; set; }

	/// <summary>
	/// Gets or sets the horizontal alignment of the text within the label's bounds.
	/// </summary>
	public HAlign HAlign { get; set; }

	/// <summary>
	/// Gets or sets the vertical alignment of the text within the label's bounds.
	/// </summary>
	public VAlign VAlign { get; set; }

	/// <summary>
	/// Gets the total length of the label's text.
	/// </summary>
	public int Length => _text.Length;

	/// <summary>
	/// Gets or sets the text displayed by the label.
	/// Setting the text marks the label as dirty, triggering a redraw.
	/// </summary>
	public string Text
	{
		get => _text;
		set
		{
			if (_text == value)
				return;
			_text = value;
			_isDirty = true;
		}
	}

	/// <summary>
	/// Gets or sets the size of the label.
	/// Changing the size marks the label as dirty, triggering a redraw.
	/// </summary>
	public new Vect2 Size
	{
		get => base.Size;
		set
		{
			if (base.Size == value)
				return;
			base.Size = value;
			_isDirty = true;
		}
	}

	/// <summary>
	/// Gets or sets the maximum visible length of the text.
	/// Use -1 to show the full text.
	/// </summary>
	public int VisibleLength
	{
		get => _visibleLength;
		set
		{
			if (_visibleLength == value)
				return;

			_visibleLength = value < 0 ? -1 : Math.Max(0, value); // Keep full text if -1
			_isDirty = true;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Label"/> class with the specified font.
	/// </summary>
	/// <param name="font">The font used to render the text.</param>
	public Label(Font font) => _font = font;

	/// <summary>
	/// Called every frame to update the label's visual representation.
	/// Recalculates text layout if the label is marked dirty.
	/// </summary>
	protected override void OnUpdate()
	{
		if (!_rtChecked)
		{
			this.TryGetAncestorOfType(out _rt);
			_rtChecked = true;
		}

		if (_isDirty)
		{
			_displayText.Clear();
			_cachedMeasurements.Clear(); // Clear previous measurements

			var processedText = _visibleLength == -1 ? _text : _text.TrimToLength(_visibleLength);

			var lines = _font.FormatText(processedText, (int)Size.X).Split(Environment.NewLine);

			foreach (var line in lines)
			{
				if (line.IsEmpty())
				{
					_displayText.Add("");
					_cachedMeasurements[""] = _font.Measure(" ");
					continue;
				}

				var trimmedLine = line.Trim();

				_displayText.Add(trimmedLine);
				_cachedMeasurements[trimmedLine] = _font.Measure(trimmedLine);// Cache measurement
			}

			if (_cachedMeasurements.Count == 0)
				_cachedMeasurements[""] = _font.Measure(" ");

			_cachedMaxHeight = _displayText.Sum(x => _cachedMeasurements[x].Y); // Cached total height
			_isDirty = false;
		}

		float offset = 0f;

		for (int i = 0; i < _displayText.Count; i++)
		{
			var txt = _displayText[i];
			var measure = _cachedMeasurements[txt]; // Retrieve cached measurement
			var offsetX = AlignHelpers.AlignWidth(Size.X, measure.X, HAlign);
			var offsetY = AlignHelpers.AlignHeight(Size.Y, _cachedMaxHeight, VAlign);

			if (_rt != null)
			{
				var world = this.GetGlobalPosition();
				var rtWorld = _rt.GetGlobalPosition();
				var local = world - rtWorld;
				var final = new Vect2(local.X + offsetX, local.Y + offsetY + offset);

				if (Shadow)
					_rt.DrawText(_font, txt, final + ShadowOffset + Offset, ShadowColor, Layer);

				_rt.DrawText(_font, txt, final + Offset, Color, Layer + 1);
			}
			else
			{
				var final = new Vect2(Position.X + offsetX, Position.Y + offsetY + offset);

				if (Shadow)
					Renderer.DrawText(_font, txt, final + ShadowOffset + Offset, ShadowColor, Layer);

				Renderer.DrawText(_font, txt, final + Offset, Color, Layer + 1);
			}

			offset += measure.Y;
		}

		base.OnUpdate();
	}
}
