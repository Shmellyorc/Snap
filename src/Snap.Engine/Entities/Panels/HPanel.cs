namespace Snap.Engine.Entities.Panels;

/// <summary>
/// A horizontal layout panel that arranges child entities side-by-side with optional spacing and alignment.
/// </summary>
public class HPanel : StackPanel
{
	/// <summary>
	/// Initializes a new <see cref="HPanel"/> with a specified spacing and child entities.
	/// </summary>
	/// <param name="spacing">The spacing between each child element, in pixels.</param>
	/// <param name="entities">Optional child entities to add to the panel.</param>
	public HPanel(int spacing, params Entity[] entities)
		: base(spacing, StackDirection.Horizontal, entities) { }

	/// <summary>
	/// Initializes a new <see cref="HPanel"/> with a default spacing of 4 pixels.
	/// </summary>
	/// <param name="entities">Optional child entities to add to the panel.</param>
	public HPanel(params Entity[] entities) : this(4, entities) { }
}