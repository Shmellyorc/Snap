namespace Snap.Engine.Entities.Panels;

/// <summary>
/// A panel that automatically centers all of its child entities within its own bounds.
/// </summary>
/// <remarks>
/// <para>
/// The centering is recalculated whenever the panel's layout becomes dirty 
/// (e.g., size changes or child modifications).
/// </para>
/// <para>
/// Child positions are set so that each child's center aligns with the center of the panel.
/// </para>
/// </remarks>
public sealed class CenterPanel : AnchorPanel
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CenterPanel"/> class
	/// with the specified child entities.
	/// </summary>
	/// <param name="child">The child to add to this panel.</param>
	public CenterPanel(Entity child) : base(child)
	{
		VAlign = VAlign.Center;
		HAlign = HAlign.Center;
	}
}
