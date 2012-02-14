using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Interfaces.Transformer
{
    /// <summary>
    /// Schedule Preference Transformer
    /// </summary>
    /// <remarks>
    /// Created by: Henryg
    /// Created date: 2009-11-18
    /// </remarks>
    public interface ISchedulePreferenceTransformer : IEtlTransformer<IScheduleDay>
    {
        /// <summary>
        /// Checks if preferences is set.
        /// </summary>
        /// <param name="preferenceRestriction">The preference restriction.</param>
        /// <returns></returns>
        bool CheckIfPreferenceIsValid(IPreferenceRestriction preferenceRestriction);
    }
}
