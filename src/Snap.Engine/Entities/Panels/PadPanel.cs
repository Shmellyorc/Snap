namespace Snap.Engine.Entities.Panels;

public sealed class PadPanel : Panel
{
	private readonly int _left, _right, _top, _bottom;

	public PadPanel(int value, Entity child)
		: this(value, value, value, value, child) { }
	public PadPanel(int horizontal, int vertical, Entity child)
		: this(horizontal, horizontal, vertical, vertical, child) { }
	public PadPanel(int left, int right, int top, int bottom, Entity child) : base(child)
	{
		_left = left;
		_right = right;
		_top = top;
		_bottom = bottom;

		Size = OnResize([child]);
	}

	protected override void OnDirty(DirtyState state)
	{
		var visible = Children
			.Where(x => x.Visible && !x.IsExiting);


		Size = OnResize(visible);

		foreach (var child in visible)
			child.Position = new Vect2(_left, _top);

		base.OnDirty(state);
	}

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
