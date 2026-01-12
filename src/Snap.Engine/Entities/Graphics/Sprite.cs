namespace Snap.Engine.Entities.Graphics;

/// <summary>
/// Represents a drawable 2D sprite that can be positioned, scaled, rotated, and aligned within a scene.
/// </summary>
public class Sprite : Entity
{
	private readonly Texture _texture;
	private RenderTarget _rt;
	private bool _rtChecked;


	public Vect2 Offset { get; set; }


	/// <summary>
	/// The rectangular portion of the texture to render.
	/// </summary>
	public Rect2 Source { get; set; }

	/// <summary>
	/// The origin point used for rotation and scaling, relative to the sprite.
	/// Typically (0,0) is top-left and (0.5,0.5) is center.
	/// </summary>
	public Vect2 Origin { get; set; }

	/// <summary>
	/// Effects to apply when rendering the texture (e.g., flipping).
	/// </summary>
	public TextureEffects Effects { get; set; }

	/// <summary>
	/// The scaling factor applied to the sprite. Default is (1, 1).
	/// </summary>
	public Vect2 Scale { get; set; } = Vect2.One;

	/// <summary>
	/// The rotation angle in radians applied to the sprite.
	/// </summary>
	public float Rotation { get; set; } = 0f;

	/// <summary>
	/// The horizontal alignment used when positioning the sprite within its parent or render target.
	/// </summary>
	public HAlign HAlign { get; set; }

	/// <summary>
	/// The vertical alignment used when positioning the sprite within its parent or render target.
	/// </summary>
	public VAlign VAlign { get; set; }

	/// <summary>
	/// Initializes a new sprite with the specified texture and source rectangle.
	/// </summary>
	/// <param name="texture">The texture to render.</param>
	/// <param name="source">The rectangular region within the texture to draw.</param>
	public Sprite(Texture texture, Rect2 source)
	{
		_texture = texture;
		Source = source;

		Size = source.Size;
	}

	/// <summary>
	/// Initializes a new sprite using a source rectangle from a spritesheet.
	/// </summary>
	/// <param name="texture">The texture associated with the spritesheet.</param>
	/// <param name="sheet">The spritesheet data.</param>
	/// <param name="sheetName">The name of the sprite within the sheet.</param>
	public Sprite(Texture texture, Spritesheet sheet, string sheetName)
		: this(texture, sheet.GetBounds(sheetName)) { }

	/// <summary>
	/// Initializes a new sprite that uses the entire bounds of the given texture.
	/// </summary>
	/// <param name="texture">The texture to render fully.</param>
	public Sprite(Texture texture) : this(texture, texture.Bounds) { }

	/// <summary>
	/// Called every frame to update and render the sprite.
	/// Handles alignment, transforms, and drawing either to a render target or the main renderer.
	/// </summary>
	protected override void OnUpdate()
	{
		if (!_rtChecked)
		{
			this.TryGetAncestorOfType(out _rt);
			_rtChecked = true;
		}

		var offsetX = AlignHelpers.AlignWidth(Size.X, Source.Size.X, HAlign);
		var offsetY = AlignHelpers.AlignHeight(Size.Y, Source.Size.Y, VAlign);

		offsetX -= Origin.X * Size.X;
		offsetY -= Origin.Y * Size.Y;

		if (_rt != null)
		{
			// world-space origin of the RT and into RT-local coords
			var world = this.GetGlobalPosition();
			var rtWorld = _rt.GetGlobalPosition();
			var local = world - rtWorld;
			var final = new Vect2(local.X + offsetX, local.Y + offsetY) + Offset;

			if (_texture.RepeatedTexture) // don't add repeated textures into the atlas... creates massive amount of source rects
				_rt.DrawBypassAtlas(_texture, final, Source, Color, Origin, Scale, Rotation, Effects, Layer);
			else
				_rt.Draw(_texture, final, Source, Color, Origin, Scale, Rotation, Effects, Layer);
		}
		else
		{
			var final = new Vect2(Position.X + offsetX, Position.Y + offsetY) + Offset;

			if (_texture.RepeatedTexture)
				Renderer.DrawBypassAtlas(_texture, final, Source, Color, Origin, Scale, Rotation, Effects, Layer);
			else
				Renderer.Draw(_texture, final, Source, Color, Origin, Scale, Rotation, Effects, Layer);
		}

		base.OnUpdate();
	}
}
