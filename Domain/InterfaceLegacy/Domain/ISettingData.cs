namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Holds raw data for system settings
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2009-08-21
    /// </remarks>
    public interface ISettingData : IAggregateRoot
    {

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Key { get;}

        /// <summary>
        /// Get the value for this key. 
        /// If deserialization fails default value will be returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        T GetValue<T>(T defaultValue) where T : class, ISettingValue;

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        void SetValue<T>(T value) where T : class, ISettingValue;
    }
}
