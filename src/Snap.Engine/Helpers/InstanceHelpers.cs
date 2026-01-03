namespace Snap.Engine.Helpers;

/// <summary>
/// Provides helper methods for dynamically creating instances of types by name, <see cref="Type"/>, or object prototype.
/// </summary>
public static class InstanceHelpers
{
	/// <summary>
	/// Attempts to create an instance of type <typeparamref name="T"/> by searching loaded assemblies for the given type name.
	/// </summary>
	/// <typeparam name="T">The expected base or interface type of the created instance.</typeparam>
	/// <param name="instance">
	/// When this method returns, contains the created instance if successful; otherwise, the default value of <typeparamref name="T"/>.
	/// </param>
	/// <param name="name">The simple or full name of the type to instantiate.</param>
	/// <param name="ignoreCase">
	/// <c>true</c> to ignore case when matching the type name; <c>false</c> for case-sensitive matching.
	/// </param>
	/// <param name="args">Constructor arguments to pass when creating the instance.</param>
	/// <returns><c>true</c> if an instance was created successfully; otherwise, <c>false</c>.</returns>
	public static bool TryCreateInstance<T>(out T instance, string name, bool ignoreCase = true, params object[] args)
	{
		instance = CreateInstance<T>(name, ignoreCase, args);

		return instance != null;
	}

	/// <summary>
	/// Creates an instance of the specified <see cref="Type"/> if it can be assigned to <typeparamref name="T"/>,
	/// or returns <c>null</c> if assignment or instantiation fails.
	/// </summary>
	/// <typeparam name="T">The base or interface type required for the created instance.</typeparam>
	/// <param name="type">The <see cref="Type"/> to instantiate.</param>
	/// <param name="args">Constructor arguments to pass when creating the instance.</param>
	/// <returns>
	/// A new instance of <paramref name="type"/> cast to <typeparamref name="T"/>, or <c>null</c> if the type
	/// is not assignable or construction fails.
	/// </returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is <c>null</c>.</exception>
	public static T CreateInstanceFromType<T>(Type type, params object[] args) where T : class
	{
		if(type == null)
			return null;

		if (!typeof(T).IsAssignableFrom(type))
			return null;         // rather than ArgumentException

		var instance = Activator.CreateInstance(type, args) as T;
		return instance;         // null if ctor not found or invocation failed
	}

	/// <summary>
	/// Attempts to create an instance of the specified <see cref="Type"/> and assign it to <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The base or interface type required for the created instance.</typeparam>
	/// <param name="type">The <see cref="Type"/> to instantiate.</param>
	/// <param name="result">
	/// When this method returns, contains the created instance cast to <typeparamref name="T"/>, or <c>null</c> on failure.
	/// </param>
	/// <param name="args">Constructor arguments to pass when creating the instance.</param>
	/// <returns><c>true</c> if the instance was created successfully; otherwise, <c>false</c>.</returns>
	public static bool TryCreateInstanceFromType<T>(Type type, out T result, params object[] args) where T : class
	{
		try
		{
			result = CreateInstanceFromType<T>(type, args);
			return true;
		}
		catch
		{
			result = null;
			return false;
		}
	}

	/// <summary>
	/// Creates an instance of type <typeparamref name="T"/> by searching loaded assemblies for a type with the given name.
	/// </summary>
	/// <typeparam name="T">The expected base or interface type of the created instance.</typeparam>
	/// <param name="name">The simple or full name of the type to instantiate.</param>
	/// <param name="ignoreCase">
	/// <c>true</c> to ignore case when matching the type name; <c>false</c> for case-sensitive matching.
	/// </param>
	/// <param name="args">Constructor arguments to pass when creating the instance.</param>
	/// <returns>
	/// A new instance of the matching type cast to <typeparamref name="T"/>, or the default value of <typeparamref name="T"/>
	/// if no matching type is found.
	/// </returns>
	public static T CreateInstance<T>(string name, bool ignoreCase, params object[] args)
	{
		if (name.IsEmpty())
			return default!;

		var ap = AppDomain.CurrentDomain.GetAssemblies();
		var ignoreType = ignoreCase
			? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

		for (int i = ap.Length - 1; i >= 0; i--)
		{
			var asm = ap[i];
			var foundType = asm.GetType(name, false, ignoreCase);

			if (foundType == null)
			{
				foundType = asm.GetTypes()
					.FirstOrDefault(x => string.Equals(x.Name, name, ignoreType));
			}

			if (foundType == null)
				continue;

			if (!typeof(T).IsAssignableFrom(foundType))
				continue;

			return (T)Activator.CreateInstance(foundType, args)!;
		}

		return default;
	}

	/// <summary>
	/// Creates an instance of type <typeparamref name="T"/> using the runtime type of the provided object as the template.
	/// </summary>
	/// <typeparam name="T">The expected base or interface type of the created instance.</typeparam>
	/// <param name="obj">An object whose runtime type is used to locate the constructor.</param>
	/// <param name="args">Constructor arguments to pass when creating the instance.</param>
	/// <returns>
	/// A new instance of the same type as <paramref name="obj"/>, cast to <typeparamref name="T"/>,
	/// or the default value of <typeparamref name="T"/> if instantiation fails.
	/// </returns>
	public static T CreateInstanceFromObject<T>(object obj, params object[] args) =>
		CreateInstance<T>(obj.GetType().FullName, true, args);

	/// <summary>
	/// Attempts to create an instance of type <typeparamref name="T"/> using the runtime type of the provided object as the template.
	/// </summary>
	/// <typeparam name="T">The expected base or interface type of the created instance.</typeparam>
	/// <param name="instance">
	/// When this method returns, contains the created instance cast to <typeparamref name="T"/>, or the default value of <typeparamref name="T"/> on failure.
	/// </param>
	/// <param name="obj">An object whose runtime type is used to locate the constructor.</param>
	/// <param name="args">Constructor arguments to pass when creating the instance.</param>
	/// <returns><c>true</c> if the instance was created successfully; otherwise, <c>false</c>.</returns>
	public static bool TryCreateInstanceFromObject<T>(out T instance, object obj, params object[] args)
	{
		instance = CreateInstanceFromObject<T>(obj.GetType().FullName, true, args);

		return instance != null;
	}
}
