namespace Snap.Engine.Entities.Inputs;

/// <summary>
/// Represents a basic interactive button entity that responds to mouse input and can trigger actions when pressed.
/// </summary>
public class BaseButton : Entity
{
	private bool _disabled, _wasHovering, _wasPressed, _isDirty, _pressStartedInside;

	/// <summary>
	/// Gets or sets the current pressed state logic for this button.
	/// Determines when the <see cref="Pressed"/> callback is fired (on press or release).
	/// </summary>
	public BaseButtonPressedState ButtonState { get; set; } = BaseButtonPressedState.Released;

	/// <summary>
	/// Gets or sets whether the button is disabled.
	/// When disabled, the button ignores input and triggers <see cref="OnButtonDisabled"/>.
	/// </summary>
	public bool Disabled
	{
		get => _disabled;
		set
		{
			if (_disabled == value)
				return;
			_disabled = value;
			_isDirty = true;
		}
	}

	/// <summary>
	/// An optional callback invoked when the button is successfully pressed.
	/// Trigger timing depends on the <see cref="ButtonState"/>.
	/// </summary>
	public Action<BaseButton> Pressed { get; set; }

	/// <summary>
	/// Called when the mouse button is released and the cursor is over the button.
	/// Override to handle visual or logical changes.
	/// </summary>
	protected virtual void OnButtonUp() { }

	/// <summary>
	/// Called when the mouse button is pressed while over the button.
	/// Override to handle visual feedback or sound.
	/// </summary>
	protected virtual void OnButtonDown() { }

	/// <summary>
	/// Called when the cursor begins hovering over the button.
	/// Override to handle hover state changes.
	/// </summary>
	protected virtual void OnButtonHover() { }

	/// <summary>
	/// Called when the button becomes disabled.
	/// Override to provide custom disabled behavior.
	/// </summary>
	protected virtual void OnButtonDisabled() { }

	/// <summary>
	/// Updates the button state, checks for mouse interaction, and triggers the appropriate events.
	/// Handles hover detection, click input, and disabled state transitions.
	/// </summary>
	protected override void OnUpdate()
	{
		if (_isDirty)
		{
			if (_disabled)
				OnButtonDisabled();
			else
				OnButtonUp();

			_isDirty = false;
		}

		if (_disabled)
			return;

		var rect = Bounds;
		var pos = Input.Transform(Input.MousePosition, Camera);
		bool isMouseDown = Input.IsMousePressed(MouseButton.Left);
		bool isInside = rect.Contains(pos);

		if (isMouseDown)
		{
			if (isInside)
			{
				if (!_wasPressed && !_wasHovering)
				{
					// First frame press started inside the button
					_pressStartedInside = true;
					_wasPressed = true;
					_wasHovering = false;
					OnButtonDown();

					if (ButtonState == BaseButtonPressedState.Pressed)
						Pressed?.Invoke(this);
				}
			}
		}
		else // Mouse released
		{
			if (isInside)
			{
				if (_wasPressed && _pressStartedInside)
				{
					_wasPressed = false;
					_pressStartedInside = false;
					OnButtonUp();

					if (ButtonState == BaseButtonPressedState.Released)
						Pressed?.Invoke(this);
				}
				else if (!_wasHovering)
				{
					_wasHovering = true;
					OnButtonHover();
				}
			}
			else if (_wasHovering || _wasPressed)
			{
				_wasHovering = false;
				_wasPressed = false;
				_pressStartedInside = false;
				OnButtonUp();
			}
		}

		base.OnUpdate();
	}
}
