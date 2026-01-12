namespace Snap.Engine.Entities.Graphics;

/// <summary>
/// Represents a nine-patch image entity that divides a texture into 9 regions (corners, edges, center)
/// to allow scalable UI elements without distortion.
/// </summary>
public sealed class Ninepatch : Entity
{
	[Flags]
	private enum NinePatchDirtyFlags
	{
		None,
		Dest = 1,
		Source = 2,
	}

	private readonly Texture _texture;
	private readonly Rect2 _source;
	private readonly int _left, _right, _top, _bottom;
	private readonly Rect2[] _src = new Rect2[9], _dst = new Rect2[9];
	private NinePatchDirtyFlags _dirtyFlags;
	private RenderTarget _rt;
	private bool _rtChecked;
	private Rect2 _oldBounds;

	// /// <summary>
	// /// Gets or sets the tint color applied to the ninepatch when rendering.
	// /// </summary>
	// public Color Color { get; set; } = Color.White;

	/// <summary>
	/// Gets or sets the position of the ninepatch.
	/// Setting this flag marks the destination as dirty, triggering recalculation.
	/// </summary>
	public new Vect2 Position
	{
		get => base.Position;
		set
		{
			if (base.Position == value)
				return;
			base.Position = value;

			_dirtyFlags |= NinePatchDirtyFlags.Dest;
		}
	}

	/// <summary>
	/// Gets or sets the size of the ninepatch.
	/// Setting this flag marks the destination as dirty, triggering recalculation.
	/// </summary>
	public new Vect2 Size
	{
		get => base.Size;
		set
		{
			if (base.Size == value)
				return;
			base.Size = value;

			_dirtyFlags |= NinePatchDirtyFlags.Dest;
		}
	}



	public new bool Visible
	{
		get => base.Visible;
		set
		{
			if (base.Visible == value)
				return;
			base.Visible = value;

			_dirtyFlags |= NinePatchDirtyFlags.Dest | NinePatchDirtyFlags.Source;
		}
	}


	/// <summary>
	/// Initializes a new instance of the <see cref="Ninepatch"/> class with the given texture,
	/// source rectangle defining the full texture region, and corner sizes for patch slicing.
	/// </summary>
	/// <param name="texture">The source texture containing the ninepatch image.</param>
	/// <param name="source">The rectangle region of the texture to use.</param>
	/// <param name="corners">The thickness of the corners to preserve in the ninepatch (left, top, right, bottom).</param>
	public Ninepatch(Texture texture, Rect2 source, Rect2 corners)
	{
		_texture = texture;
		_source = source;

		_left = (int)corners.Left;
		_top = (int)corners.Top;
		_right = (int)corners.Right;
		_bottom = (int)corners.Bottom;

		_dirtyFlags = NinePatchDirtyFlags.Dest | NinePatchDirtyFlags.Source;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Ninepatch"/> class using a spritesheet and patch name.
	/// </summary>
	/// <param name="texture">The source texture.</param>
	/// <param name="sheet">The spritesheet containing patch data.</param>
	/// <param name="patchName">The name of the patch in the spritesheet.</param>
	public Ninepatch(Texture texture, Spritesheet sheet, string patchName)
		: this(texture, sheet.GetBounds(patchName), sheet.GetPatch(patchName)) { }

	/// <summary>
	/// Called when the entity enters the scene.
	/// Initializes the source and destination patches based on current bounds.
	/// </summary>
	protected override void OnEnter()
	{
		_oldBounds = Bounds;

		CreatePatches(_source, _src);
		CreatePatches(Bounds, _dst);

		base.OnEnter();
	}

	/// <summary>
	/// Called every frame to update and render the ninepatch.
	/// Recalculates patches if position, size, or source changes.
	/// </summary>
	protected override void OnUpdate()
	{
		if (!_rtChecked)
		{
			this.TryGetAncestorOfType(out _rt);
			_rtChecked = true;
		}

		if (_oldBounds != Bounds)
		{
			_dirtyFlags |= NinePatchDirtyFlags.Dest;
			_oldBounds = Bounds;
		}

		if (_dirtyFlags != NinePatchDirtyFlags.None)
		{
			if (_dirtyFlags.HasFlag(NinePatchDirtyFlags.Source))
				CreatePatches(_source, _src);
			if (_dirtyFlags.HasFlag(NinePatchDirtyFlags.Dest))
			{
				if (_rt != null)
				{
					var world = this.GetGlobalPosition();
					var rtWorld = _rt.GetGlobalPosition();
					var local = world - rtWorld;
					var final = new Vect2(local.X, local.Y);

					CreatePatches(new Rect2(final, Size), _dst);
				}
				else
					CreatePatches(Bounds, _dst);
			}

			_dirtyFlags = NinePatchDirtyFlags.None;
		}

		if (_rt != null)
		{
			for (int i = 0; i < 9; i++)
				_rt.DrawBypassAtlas(_texture, _dst[i], _src[i], Color, Layer);
		}
		else
		{
			for (int i = 0; i < 9; i++)
				Renderer.DrawBypassAtlas(_texture, _dst[i], _src[i], Color, Layer);
		}

		base.OnUpdate();
	}

	private void CreatePatches(Rect2 sourceRectangle, Rect2[] patchCache)
	{
		float x = sourceRectangle.X;
		float y = sourceRectangle.Y;
		float w = sourceRectangle.Width;
		float h = sourceRectangle.Height;

		float middleWidth = w - _left - _right;
		float middleHeight = h - _top - _bottom;

		float[] xs = { x, x + _left, x + w - _right };
		float[] ys = { y, y + _top, y + h - _bottom };
		float[] widths = { _left, middleWidth, _right };
		float[] heights = { _top, middleHeight, _bottom };

		for (int row = 0, i = 0; row < 3; row++)
		{
			for (int col = 0; col < 3; col++, i++)
				patchCache[i] = new Rect2(xs[col], ys[row], widths[col], heights[row]);
		}
	}
}
