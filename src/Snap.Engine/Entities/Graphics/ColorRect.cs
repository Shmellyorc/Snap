namespace Snap.Engine.Entities.Graphics;

/// <summary>
/// An entity that renders a solid colored rectangle.
/// </summary>
public sealed class ColorRect : Entity
{
	private Texture _texture;
	private RenderTarget _rt;
	private bool _rtChecked;

	// /// <summary>
	// /// Gets or sets the color used to tint the rectangle.
	// /// Defaults to the engine's clear color on initialization.
	// /// </summary>
	// public Color Color { get; set; } = Color.White;

	/// <summary>
	/// Initializes a new instance of the <see cref="ColorRect"/> class.
	/// Sets default size and color based on engine settings.
	/// </summary>
	public ColorRect()
	{
		// Always default to engine clear color
		Color = EngineSettings.Instance.ClearColor;
		Size = EngineSettings.Instance.Viewport;
	}

	/// <summary>
	/// Called when the entity enters the scene.
	/// Initializes the white 1x1 pixel texture used for rendering.
	/// </summary>
	protected override void OnEnter()
	{
		_texture = new Texture(Vect2.One); // Tint of White/Nothing

		base.OnEnter();
	}

	/// <summary>
	/// Called every frame to update and render the colored rectangle.
	/// </summary>
	protected override void OnUpdate()
	{
		if (!_rtChecked)
		{
			this.TryGetAncestorOfType(out _rt);
			_rtChecked = true;
		}

		if (Color.A <= 0 || !Visible)
			return;

		if (_rt != null)
		{
			// world-space origin of the RT and into RT-local coords
			var world = this.GetGlobalPosition();
			var rtWorld = _rt.GetGlobalPosition();
			var local = world - rtWorld;

			_rt.DrawBypassAtlas(_texture, new Rect2(local, Size), Color, depth: Layer);
		}
		else
			Renderer.DrawBypassAtlas(_texture, Bounds, Color, depth: Layer);

		base.OnUpdate();
	}
}
