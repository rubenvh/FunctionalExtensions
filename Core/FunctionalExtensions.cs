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
    /// This function allows you to perform an action on an object without changing the object itself.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="action"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static async Task<T> Tap<T>(this Task<T> source, Action<T> action)
    {
        var result = await source;
        action(result);
        return result;
    }
}