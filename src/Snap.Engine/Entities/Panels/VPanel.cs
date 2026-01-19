namespace Snap.Engine.Entities.Panels;

/// <summary>
/// A vertical layout panel that stacks child entities top to bottom, with configurable spacing and alignment.
/// </summary>
public class VPanel : StackPanel
{
	/// <summary>
	/// Initializes a new <see cref="VPanel"/> with a specified spacing and child entities.
	/// </summary>
	/// <param name="spacing">The spacing between each child element.</param>
	/// <param name="entities">The entities to add to the panel.</param>
	public VPanel(int spacing, params Entity[] entities)
		: base(spacing, StackDirection.Vertical, entities) { }

	/// <summary>
	/// Initializes a new <see cref="VPanel"/> with a default spacing of 4 pixels.
	/// </summary>
	/// <param name="entities">The entities to add to the panel.</param>
	public VPanel(params Entity[] entities) : this(4, entities) { }
}