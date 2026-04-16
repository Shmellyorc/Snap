namespace Snap.Engine.Coroutines.Routines.Animations;

/// <summary>
/// Represents a pulse animation that oscillates between two values back and forth over time.
/// </summary>
/// <typeparam name="T">The type of value to interpolate.</typeparam>
/// <remarks>
/// Unlike a tween that goes from start to end once, a pulse continuously alternates direction
/// until manually stopped or after a specified number of cycles.
/// </remarks>
public sealed class Pulse<T> : IEnumerator
{
    private readonly T _a, _b;
    private readonly float _durationPerCycle;
    private readonly int _cycles;
    private readonly EaseType _type;
    private readonly Func<T, T, float, T> _lerp;
    private readonly Action<T> _onUpdate;
    private float _elapsed;
    private int _completedCycles;

    /// <summary>
    /// Gets the current element in the collection. Always returns null for this enumerator.
    /// </summary>
    public object Current => null;

    /// <summary>
    /// Initializes a new instance of the <see cref="Pulse{T}"/> class.
    /// </summary>
    /// <param name="a">The first value to oscillate between.</param>
    /// <param name="b">The second value to oscillate between.</param>
    /// <param name="durationPerCycle">The duration of one complete back-and-forth cycle in seconds.</param>
    /// <param name="type">The easing type to apply to the interpolation.</param>
    /// <param name="lerpFunc">A delegate that interpolates between two values based on a normalized progress (0 to 1).</param>
    /// <param name="onUpdate">A callback invoked each frame with the current interpolated value.</param>
    /// <param name="cycles">The number of complete oscillations to perform. Use -1 for infinite. Default is -1.</param>
    public Pulse(T a, T b, float durationPerCycle, EaseType type, Func<T, T, float, T> lerpFunc, Action<T> onUpdate, int cycles = -1)
    {
        _a = a;
        _b = b;
        _durationPerCycle = durationPerCycle;
        _cycles = cycles;
        _type = type;
        _lerp = lerpFunc;
        _onUpdate = onUpdate;
        _elapsed = 0f;
        _completedCycles = 0;
    }

    /// <summary>
    /// Advances the pulse by one frame.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the pulse is still in progress; <c>false</c> if it has completed.
    /// </returns>
    /// <remarks>
    /// This method should be called once per frame. It applies the easing function and interpolates
    /// between the two values based on the current direction, then invokes <c>onUpdate</c>.
    /// </remarks>
    public bool MoveNext()
    {
        // Check if we've completed all cycles
        if (_cycles != -1 && _completedCycles >= _cycles)
        {
            // Final value - end at _b for consistency
            _onUpdate?.Invoke(_b);
            return false;
        }

        float halfDuration = _durationPerCycle / 2f;

        if (_elapsed < _durationPerCycle)
        {
            float normalized;
            T currentValue;

            if (_elapsed < halfDuration)
            {
                // Going from A to B
                normalized = _elapsed / halfDuration;  // 0→1
                float eased = Easing.Ease(_type, normalized);
                currentValue = _lerp(_a, _b, eased);
            }
            else
            {
                // Going from B to A (backward)
                normalized = (_elapsed - halfDuration) / halfDuration;  // 0→1
                float eased = Easing.Ease(_type, normalized);
                currentValue = _lerp(_b, _a, eased);
            }

            _onUpdate?.Invoke(currentValue);
            _elapsed += Clock.Instance.DeltaTime;
            return true;
        }

        // Complete one full cycle
        _completedCycles++;
        _elapsed = 0f;

        // Continue to next frame
        return true;
    }

    /// <summary>
    /// Reset is not supported for this pulse and will throw an exception if called.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown when called.</exception>
    public void Reset() => throw new NotSupportedException();
}