namespace Snap.Engine.Entities.Graphics;

/// <summary>
/// Defines the fill direction for a <see cref="Progress"/> entity.
/// </summary>
public enum ProgressDirection
{
	/// <summary>Fills from left to right.</summary>
	LeftToRight,

	/// <summary>Fills from right to left.</summary>
	RightToLeft,

	/// <summary>Fills from top to bottom.</summary>
	TopToBottom,

	/// <summary>Fills from bottom to top.</summary>
	BottomToTop
}

/// <summary>
/// A UI entity that renders a progress bar with customizable fill direction, colors, and value range.
/// </summary>
/// <remarks>
/// <para>
/// The progress bar consists of a background texture and a foreground texture that fills proportionally
/// to the current value within the specified <see cref="Min"/> and <see cref="Max"/> range.
/// </para>
/// <para>
/// Supports optional rounding of fill size, configurable colors, and multiple fill directions.
/// Rendering automatically adapts whether or not the entity is inside a <see cref="RenderTarget"/>.
/// </para>
/// </remarks>
public class Progress : Entity
{
	private RenderTarget _rt;
	private bool _rtChecked;
	private Texture _bg, _fg;

	/// <summary>
	/// Gets or sets the fill direction of the progress bar.
	/// </summary>
	public ProgressDirection Direction { get; set; } = ProgressDirection.LeftToRight;

	/// <summary>
	/// Gets or sets the background color of the progress bar.
	/// </summary>
	public Color BgColor { get; set; } = Color.White;

	/// <summary>
	/// Gets or sets the foreground (fill) color of the progress bar.
	/// </summary>
	public Color FgColor { get; set; } = Color.Blue;

	/// <summary>
	/// Gets or sets the minimum value of the progress bar range.
	/// </summary>
	public float Min { get; set; } = 0f;

	/// <summary>
	/// Gets or sets the maximum value of the progress bar range.
	/// </summary>
	public float Max { get; set; } = 1f;

	/// <summary>
	/// Gets or sets the current value of the progress bar.
	/// </summary>
	public float Value { get; set; } = 0f;

	/// <summary>
	/// Gets or sets a value indicating whether the fill size should be rounded to the nearest pixel.
	/// </summary>
	public bool Rounded { get; set; }

	public Vect2 PixelPercent { get; private set; }

	/// <summary>
	/// Called when the entity is first added to the scene.
	/// Initializes the background and foreground textures.
	/// </summary>
	protected override void OnEnter()
	{
		_bg = new Texture(Vect2.One, Color.White);
		_fg = new Texture(Vect2.One, Color.White);

		base.OnEnter();
	}

	/// <summary>
	/// Updates the progress bar each frame, recalculating the filled area and rendering it.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The filled area is calculated based on <see cref="Value"/>, clamped between <see cref="Min"/> and <see cref="Max"/>.
	/// </para>
	/// <para>
	/// If the progress bar is inside a <see cref="RenderTarget"/>, drawing is performed relative to its local coordinates;
	/// otherwise, rendering is done directly to the screen via the global <see cref="Renderer"/>.
	/// </para>
	/// </remarks>
	protected override void OnUpdate()
	{
		if (!_rtChecked)
		{
			this.TryGetAncestorOfType(out _rt);
			_rtChecked = true;
		}

		// Ensure Min is not greater than Max
		if (Min > Max)
			(Min, Max) = (Max, Min);

		float clampedValue = Math.Clamp(Value, Min, Max);
		float range = Max - Min;
		float percent = range > 0f ? (clampedValue - Min) / range : 0f;

		Vect2 fillSize;
		Vect2 fgPos = Position;

		switch (Direction)
		{
			case ProgressDirection.LeftToRight:
				fillSize = Rounded
					? new Vect2(MathF.Round(Size.X * percent), Size.Y)
					: new Vect2(Size.X * percent, Size.Y);
				break;

			case ProgressDirection.RightToLeft:
				fillSize = Rounded
					? new Vect2(MathF.Round(Size.X * percent), Size.Y)
					: new Vect2(Size.X * percent, Size.Y);
				fgPos.X += Size.X - fillSize.X;
				break;

			case ProgressDirection.TopToBottom:
				fillSize = Rounded
					? new Vect2(Size.X, MathF.Round(Size.Y * percent))
					: new Vect2(Size.X, Size.Y * percent);
				break;

			case ProgressDirection.BottomToTop:
				fillSize = Rounded
					? new Vect2(Size.X, MathF.Round(Size.Y * percent))
					: new Vect2(Size.X, Size.Y * percent);
				fgPos.Y += Size.Y - fillSize.Y;
				break;

			default:
				fillSize = Size;
				break;
		}

		Rect2 bgRect = new(Position, Size);
		Rect2 fgRect = new(fgPos, fillSize);
		PixelPercent = fillSize;

		if (_rt != null)
		{
			Vect2 world = this.GetGlobalPosition();
			Vect2 rtWorld = _rt.GetGlobalPosition();
			Vect2 local = world - rtWorld;

			bgRect.Position = local;
			fgRect.Position = local + (fgPos - Position);

			_rt.DrawBypassAtlas(_bg, bgRect, BgColor, Layer);
			_rt.DrawBypassAtlas(_fg, fgRect, FgColor, Layer);
		}
		else
		{
			Renderer.DrawBypassAtlas(_bg, bgRect, BgColor, Layer);
			Renderer.DrawBypassAtlas(_fg, fgRect, FgColor, Layer);
		}

		base.OnUpdate();
	}
}
