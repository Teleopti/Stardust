namespace Teleopti.Interfaces.Infrastructure
{
    /// <summary>
    /// Cache to hold non db data.
    /// </summary>
    public interface ICustomDataCache<T>
    {
        /// <summary>
        /// Gets the data by specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 1/19/2010
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get")]
        T Get(string key);

        /// <summary>
        /// Puts the specified value to the cache by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 1/19/2010
        /// </remarks>
        void Put(string key, T value);

        /// <summary>
        /// Deletes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 1/19/2010
        /// </remarks>
        void Delete(string key);
    }
}