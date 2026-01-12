namespace Snap.Engine.Entities.Panels;

public enum StackDirection { Vertical, Horizontal }

public class StackPanel : Panel
{
	private float _spacing;
	private bool _isAutoSize = true;
	private HAlign _hAlign = HAlign.Left;
	private VAlign _vAlign = VAlign.Top;
	private StackDirection _direction = StackDirection.Vertical;

	/// <summary>
	/// Gets or sets the size of the panel.
	/// Setting this manually disables automatic sizing based on children.
	/// </summary>
	public new Vect2 Size
	{
		get => base.Size;
		set
		{
			if (base.Size == value)
				return;
			base.Size = value;
			_isAutoSize = false;

			SetDirtyState(DirtyState.Sort | DirtyState.Update);
		}
	}

	/// <summary>
	/// Gets or sets the horizontal alignment of the children within each row.
	/// </summary>
	public HAlign HAlign
	{
		get => _hAlign;
		set
		{
			if (_hAlign == value)
				return;
			_hAlign = value;
			// _isDirty = true;
			SetDirtyState(DirtyState.Sort | DirtyState.Update);
		}
	}

	/// <summary>
	/// Gets or sets the vertical alignment of the entire group of children within the panel.
	/// </summary>
	public VAlign VAlign
	{
		get => _vAlign;
		set
		{
			if (_vAlign == value)
				return;
			_vAlign = value;
			// _isDirty = true;
			SetDirtyState(DirtyState.Sort | DirtyState.Update);
		}
	}

	/// <summary>
	/// Gets or sets the spacing between each child element (in pixels).
	/// </summary>
	public float Spacing
	{
		get => _spacing;
		set
		{
			if (_spacing == value) return;
			_spacing = value;
			SetDirtyState(DirtyState.Sort | DirtyState.Update);
		}
	}

	public StackDirection Direction => _direction;

	public StackPanel(int spacing, StackDirection direction, params Entity[] children) : base(children)
	{
		_spacing = spacing;
		_direction = direction;

		Size = OnResize(children);
	}
	public StackPanel(StackDirection direction, params Entity[] children)
		: this(4, direction, children) { }

	protected override void OnDirty(DirtyState state)
	{
		var children = Children
			.Where(x => x.Visible && !x.IsExiting)
			.ToList();

		if (children.Count == 0)
		{
			if (_isAutoSize)
				base.Size = Vect2.Zero;
			base.OnDirty(state);
			return;
		}

		if (_isAutoSize)
			Size = OnResize(children);

		if (_direction == StackDirection.Vertical)
			VPanelStack(children);
		else
			HPanelStack(children);

		if (IsTopmostScreen || Parent == null)
			Screen?.SetDirtyState(DirtyState.Sort | DirtyState.Update);

		base.OnDirty(state);
	}

	private void HPanelStack(List<Entity> children)
	{
		var width = children.Sum(x => x.Size.X + _spacing) - _spacing;
		var height = children.Max(x => x.Size.Y);
		var offset = 0f;

		for (int i = 0; i < children.Count; i++)
		{
			var child = children[i];
			var eWidth = AlignHelpers.AlignWidth(Size.X, width, _hAlign);
			var eHeight = AlignHelpers.AlignHeight(Size.Y, height, _vAlign);

			child.Position = new Vect2(offset + eWidth, eHeight).Round();

			offset += child.Size.X;
			if (i < children.Count - 1)
				offset += _spacing;
		}
	}

	private void VPanelStack(List<Entity> children)
	{
		var width = children.Max(x => x.Size.X);
		var height = children.Sum(x => x.Size.Y + _spacing) - _spacing;
		var offset = 0f;

		for (int i = 0; i < children.Count; i++)
		{
			var child = children[i];
			var eWidth = AlignHelpers.AlignWidth(Size.X, width, _hAlign);
			var eHeight = AlignHelpers.AlignHeight(Size.Y, height, _vAlign);

			child.Position = new Vect2(eWidth, offset + eHeight).Round();

			offset += child.Size.Y;
			if (i < children.Count - 1)
				offset += _spacing;
		}
	}

	protected override Vect2 OnResize(IEnumerable<Entity> children)
	{
		if (!_isAutoSize)
			return Size;

		var visible = children
			.Where(x => x.Visible && !x.IsExiting)
			.ToList();

		if (visible.Count == 0)
			return Vect2.Zero;

		if (_direction == StackDirection.Vertical)
		{
			float width = visible.Max(x => x.Size.X);
			float totalHeight = 0f;
			for (int i = 0; i < visible.Count; i++)
			{
				totalHeight += visible[i].Size.Y;
				if (i < visible.Count - 1)
					totalHeight += _spacing;
			}
			return new Vect2(width, totalHeight);
		}
		else
		{
			float height = visible.Max(x => x.Size.Y);
			float totalWidth = 0f;
			for (int i = 0; i < visible.Count; i++)
			{
				totalWidth += visible[i].Size.X;
				if (i < visible.Count - 1)
					totalWidth += _spacing;
			}
			return new Vect2(totalWidth, height);
		}
	}
}