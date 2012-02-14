using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    //RK - right now seperate interfaces for global and personal is not needed.
    //Feel free to add them it you need this
    public interface ISettingDataRepository
    {
        T FindValueByKey<T>(string key, T defaultValue) where T : class, ISettingValue;

        /// <summary>
        /// Persists the setting value. The returned ISettingData must be used to enable further saving!
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        ISettingData PersistSettingValue(ISettingValue value);
    }
}
