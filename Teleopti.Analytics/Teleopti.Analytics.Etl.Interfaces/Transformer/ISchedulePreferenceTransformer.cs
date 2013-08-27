using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Interfaces.Transformer
{
    public interface ISchedulePreferenceTransformer : IEtlTransformer<IScheduleDay>
    {
        bool CheckIfPreferenceIsValid(IPreferenceRestriction preferenceRestriction);
    }
}
