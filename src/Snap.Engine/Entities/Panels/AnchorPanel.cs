namespace Snap.Engine.Entities.Panels;

public class AnchorPanel : Panel
{
	public HAlign HAlign { get; set; }
	public VAlign VAlign { get; set; }
	public Vect2 Offset { get; set; }

	public AnchorPanel(Entity child) : base(child) { }

	protected override void OnDirty(DirtyState state)
	{
		if (Parent == null) return;

		var parentSize = Parent.Size;
		var mySize = Size;

		float x = AlignHelpers.AlignWidth(parentSize.X, mySize.X, HAlign, Offset.X);
		float y = AlignHelpers.AlignHeight(parentSize.Y, mySize.Y, VAlign, Offset.Y);

		Position = new Vect2(x, y);

		base.OnDirty(state);
	}
}