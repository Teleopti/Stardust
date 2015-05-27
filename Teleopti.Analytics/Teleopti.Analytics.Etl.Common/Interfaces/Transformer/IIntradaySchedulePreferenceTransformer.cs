using System.Collections.Generic;
using System.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
    public interface IIntradaySchedulePreferenceTransformer
    {
        bool CheckIfPreferenceIsValid(IPreferenceRestriction preferenceRestriction);

		void Transform(IEnumerable<IPreferenceDay> rootList, DataTable table, ICommonStateHolder stateHolder, IScenario scenario);
    }
}
