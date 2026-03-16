namespace FluentSolidity.FunctionalExtensions;

public static class FunctionalExtensions
{
    /// <summary>
    /// This function allows you to perform an action on an object without changing the object itself.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="action"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Tap<T>(this T source, Action<T> action)
    {
        action(source);
        return source;
    }

    /// <summary>
    /// Async Tap: sync source, async action.
    /// </summary>
    public static async Task<T> Tap<T>(this T source, Func<T, Task> action)
    {
        await action(source);
        return source;
    }

    /// <summary>
    /// Async Tap: async source, sync action.
    /// </summary>
    public static async Task<T> Tap<T>(this Task<T> source, Action<T> action)
    {
        var result = await source;
        action(result);
        return result;
    }

    /// <summary>
    /// Async Tap: async source, async action.
    /// </summary>
    public static async Task<T> Tap<T>(this Task<T> source, Func<T, Task> action)
    {
        var result = await source;
        await action(result);
        return result;
    }
}