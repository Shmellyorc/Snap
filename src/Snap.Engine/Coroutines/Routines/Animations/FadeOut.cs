namespace Snap.Engine.Coroutines.Routines.Animations;

/// <summary>
/// Gradually decreases the entity's alpha from its current value to the target alpha over time.
/// </summary>
/// <remarks>
/// The fade uses the entity's current alpha as the starting point and interpolates to the target alpha.
/// The entity's original color is preserved; only the alpha channel is modified.
/// It is the developer's responsibility to handle entity cleanup or disposal after the fade completes.
/// </remarks>
public sealed class FadeOut : IEnumerator
{
    private readonly Tween<float> _tween;

    /// <summary>
    /// Gets the current element in the collection. Always returns null for this enumerator.
    /// </summary>
    public object Current => null;

    /// <summary>
    /// Initializes a new instance of the <see cref="FadeOut"/> class.
    /// </summary>
    /// <param name="entity">The entity to fade out.</param>
    /// <param name="duration">The duration of the fade in seconds.</param>
    /// <param name="easeType">The easing type to apply to the fade.</param>
    /// <param name="color">The base color to apply alpha to. The entity's color will be set to this color with the interpolated alpha value.</param>
    /// <param name="targetAlpha">The target alpha value to fade to. Default is 0f (fully transparent).</param>
    public FadeOut(Entity entity, float duration, EaseType easeType, Color color, float targetAlpha = 0f)
    {
        float fromAlpha = entity.Color.A;
        
        _tween = new Tween<float>(fromAlpha, targetAlpha, duration, easeType, MathHelpers.Lerp,
            alpha => entity.Color = Color.WithAlpha(color, alpha));
    }

    /// <summary>
    /// Advances the fade out animation by one frame.
    /// </summary>
    /// <returns><c>true</c> if the fade is still in progress; <c>false</c> if completed.</returns>
    public bool MoveNext() => _tween.MoveNext();

    /// <summary>
    /// Reset is not supported for this fade and will throw an exception if called.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown when called.</exception>
    public void Reset() => throw new NotSupportedException();
}