namespace Snap.Engine.Graphics;

public class ObjectPool<T> where T : class
{
	private readonly Stack<T> _pool = new();
	private readonly Func<T> _createFunc;
	private readonly Action<T> _resetAction;

	public ObjectPool(Func<T> createFunc, Action<T> resetAction = null)
	{
		_createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
		_resetAction = resetAction;
	}

	public T Rent()
	{
		lock (_pool)
		{
			return _pool.Count > 0 ? _pool.Pop() : _createFunc();
		}
	}

	public void Return(T item)
	{
		if (item == null) return;

		_resetAction?.Invoke(item);

		lock (_pool)
		{
			_pool.Push(item);
		}
	}
}
