namespace Snap.Engine.Enums;

/// <summary>
/// Represents the buttons and controls available on a standard gamepad.
/// </summary>
public enum GamepadButton
{
	/// <summary>
	/// Represents the absence of a gamepad button or an unassigned button state.
	/// This value is used as a default/sentinel value and does not correspond to any physical input.
	/// </summary>
	None,
	
	/// <summary>
	/// The Y face button.
	/// </summary>
	YButton,

	/// <summary>
	/// The X face button.
	/// </summary>
	XButton,

	/// <summary>
	/// The B face button.
	/// </summary>
	BButton,

	/// <summary>
	/// The A face button.
	/// </summary>
	AButton,

	/// <summary>
	/// The Back or Select button.
	/// </summary>
	Back,

	/// <summary>
	/// The Start or Menu button.
	/// </summary>
	Start,

	/// <summary>
	/// The left directional pad button.
	/// </summary>
	DPadLeft,

	/// <summary>
	/// The right directional pad button.
	/// </summary>
	DPadRight,

	/// <summary>
	/// The up directional pad button.
	/// </summary>
	DPadUp,

	/// <summary>
	/// The down directional pad button.
	/// </summary>
	DPadDown,

	/// <summary>
	/// The left analog stick pressed as a button.
	/// </summary>
	LeftStick,

	/// <summary>
	/// The left analog stick tilted to the left.
	/// </summary>
	LeftStickLeft,

	/// <summary>
	/// The left analog stick tilted to the right.
	/// </summary>
	LeftStickRight,

	/// <summary>
	/// The left analog stick tilted upward.
	/// </summary>
	LeftStickUp,

	/// <summary>
	/// The left analog stick tilted downward.
	/// </summary>
	LeftStickDown,

	/// <summary>
	/// The right analog stick pressed as a button.
	/// </summary>
	RightStick,

	/// <summary>
	/// The right analog stick tilted to the left.
	/// </summary>
	RightStickLeft,

	/// <summary>
	/// The right analog stick tilted to the right.
	/// </summary>
	RightStickRight,

	/// <summary>
	/// The right analog stick tilted upward.
	/// </summary>
	RightStickUp,

	/// <summary>
	/// The right analog stick tilted downward.
	/// </summary>
	RightStickDown,

	/// <summary>
	/// The left bumper (shoulder) button.
	/// </summary>
	LeftBumper,

	/// <summary>
	/// The right bumper (shoulder) button.
	/// </summary>
	RightBumper,

	/// <summary>
	/// The left trigger button.
	/// </summary>
	LeftTrigger,

	/// <summary>
	/// The right trigger button.
	/// </summary>
	RightTrigger,
}
