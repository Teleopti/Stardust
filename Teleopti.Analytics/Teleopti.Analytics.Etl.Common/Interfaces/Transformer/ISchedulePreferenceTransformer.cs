using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
    public interface ISchedulePreferenceTransformer : IEtlTransformer<IScheduleDay>
    {
        bool CheckIfPreferenceIsValid(IPreferenceRestriction preferenceRestriction);
    }
}
