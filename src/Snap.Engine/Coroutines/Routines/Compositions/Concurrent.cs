namespace Snap.Engine.Coroutines.Routines.Compositions;

/// <summary>
/// Runs multiple <see cref="IEnumerator"/> routines concurrently and completes when all of them have finished.
/// </summary>
public class Concurrent : IEnumerator
{
	private readonly List<IEnumerator> _active;

	/// <summary>
	/// Gets the current value of the enumerator.
	/// Always returns <c>null</c> for <see cref="Concurrent"/>.
	/// </summary>
	public object Current => null;

	/// <summary>
	/// Initializes a new instance of the <see cref="Concurrent"/> class.
	/// </summary>
	/// <param name="routines">An array of IEnumerators to run concurrently.</param>
	/// <remarks>
	/// Null routines are ignored. Each routine is independently advanced each frame until all have completed.
	/// </remarks>
	public Concurrent(params IEnumerator[] routines)
		=> _active = new List<IEnumerator>(routines);

	/// <summary>
	/// Advances all active routines by one step.
	/// </summary>
	/// <returns>
	/// <c>true</c> if at least one routine is still running; <c>false</c> if all have completed.
	/// </returns>
	public bool MoveNext()
	{
		for (int i = _active.Count - 1; i >= 0; i--)
		{
			var r = _active[i];
			if (r == null || !r.MoveNext())
				_active.RemoveAt(i);
		}

		return _active.Count > 0;
	}

	/// <summary>
	/// Reset is not supported for this enumerator and will throw an exception if called.
	/// </summary>
	/// <exception cref="NotSupportedException">Always thrown when called.</exception>
	public void Reset() => throw new NotSupportedException();
}
