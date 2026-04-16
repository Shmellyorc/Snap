namespace Snap.Engine.Coroutines.Routines.Animations;

/// <summary>
/// Gradually increases the entity's alpha from its current value to the target alpha over time.
/// </summary>
/// <remarks>
/// The fade uses the entity's current alpha as the starting point and interpolates to the target alpha.
/// The entity's original color is preserved; only the alpha channel is modified.
/// </remarks>
public sealed class FadeIn : IEnumerator
{
    private readonly Tween<float> _tween;

    /// <summary>
    /// Gets the current element in the collection. Always returns null for this enumerator.
    /// </summary>
    public object Current => null;

    /// <summary>
    /// Initializes a new instance of the <see cref="FadeIn"/> class.
    /// </summary>
    /// <param name="entity">The entity to fade in.</param>
    /// <param name="duration">The duration of the fade in seconds.</param>
    /// <param name="easeType">The easing type to apply to the fade.</param>
    /// <param name="color">The base color to apply alpha to. The entity's color will be set to this color with the interpolated alpha value.</param>
    /// <param name="targetAlpha">The target alpha value to fade to. Default is 1f (fully opaque).</param>
    public FadeIn(Entity entity, float duration, EaseType easeType, Color color, float targetAlpha = 1f)
    {
        float fromAlpha = entity.Color.A;

        _tween = new Tween<float>(fromAlpha, targetAlpha, duration, easeType, MathHelpers.Lerp,
            alpha => entity.Color = Color.WithAlpha(color, alpha));
    }

    /// <summary>
    /// Advances the fade in animation by one frame.
    /// </summary>
    /// <returns><c>true</c> if the fade is still in progress; <c>false</c> if completed.</returns>
    public bool MoveNext() => _tween.MoveNext();

    /// <summary>
    /// Reset is not supported for this fade and will throw an exception if called.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown when called.</exception>
    public void Reset() => throw new NotSupportedException();
}