namespace Teleopti.Ccc.WinCode.Converters
{
    /// <summary>
    /// Defines the parsing logic in a ToString xxx-converter
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IParseBehavior<T>
    {
        /// <summary>
        /// Converts from string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="returnValue">The return value.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        bool ConvertFromString(string value, out T returnValue);

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        string ConvertToString(T value);
    }
}
