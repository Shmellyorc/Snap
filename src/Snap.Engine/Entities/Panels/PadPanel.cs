namespace Snap.Engine.Entities.Panels;

/// <summary>
/// A panel that adds padding around its child entity.
/// </summary>
/// <remarks>
/// This panel increases its size by specified padding values on all sides,
/// positioning its child content within the padded area.
/// </remarks>
public sealed class PadPanel : Panel
{
	private readonly int _left, _right, _top, _bottom;

	/// <summary>
	/// Initializes a new instance of the <see cref="PadPanel"/> class with uniform padding on all sides.
	/// </summary>
	/// <param name="value">The padding value to apply to all four sides (left, right, top, bottom).</param>
	/// <param name="child">The child entity to be padded.</param>
	public PadPanel(int value, Entity child)
		: this(value, value, value, value, child) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="PadPanel"/> class with horizontal and vertical padding.
	/// </summary>
	/// <param name="horizontal">The padding value to apply to the left and right sides.</param>
	/// <param name="vertical">The padding value to apply to the top and bottom sides.</param>
	/// <param name="child">The child entity to be padded.</param>
	public PadPanel(int horizontal, int vertical, Entity child)
		: this(horizontal, horizontal, vertical, vertical, child) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="PadPanel"/> class with individual padding for each side.
	/// </summary>
	/// <param name="left">The padding value for the left side.</param>
	/// <param name="right">The padding value for the right side.</param>
	/// <param name="top">The padding value for the top side.</param>
	/// <param name="bottom">The padding value for the bottom side.</param>
	/// <param name="child">The child entity to be padded.</param>
	public PadPanel(int left, int right, int top, int bottom, Entity child) : base(child)
	{
		_left = left;
		_right = right;
		_top = top;
		_bottom = bottom;

		Size = OnResize([child]);
	}

	/// <summary>
	/// Called when the panel requires a layout update.
	/// Recalculates the panel size based on child content and padding, then positions the child within the padded area.
	/// </summary>
	/// <param name="state">The type of change that triggered the update.</param>
	protected override void OnDirty(DirtyState state)
	{
		var visible = Children
			.Where(x => x.Visible && !x.IsExiting);


		Size = OnResize(visible);

		foreach (var child in visible)
			child.Position = new Vect2(_left, _top);

		base.OnDirty(state);
	}

	/// <summary>
	/// Calculates the total size required to accommodate the child entity with applied padding.
	/// </summary>
	/// <param name="children">The collection of child entities (typically one) to be padded.</param>
	/// <returns>
	/// A <see cref="Vect2"/> representing the total size (width and height) needed 
	/// to contain the largest visible child with padding applied to all sides.
	/// Returns <see cref="Vect2.Zero"/> if no children are visible.
	/// </returns>
	/// <remarks>
	/// The calculation considers only visible, non-exiting children and adds the 
	/// left/right padding to the maximum child width, and top/bottom padding to the maximum child height.
	/// </remarks>
	protected override Vect2 OnResize(IEnumerable<Entity> children)
	{
		var visible = children
			.Where(x => x.Visible && !x.IsExiting)
			.ToList();

		if (visible.Count == 0)
			return Vect2.Zero;

		float maxWidth = visible.Max(x => x.Size.X);
		float maxHeight = visible.Max(x => x.Size.Y);

		return new Vect2(maxWidth + _left + _right, maxHeight + _top + _bottom);
	}
}
