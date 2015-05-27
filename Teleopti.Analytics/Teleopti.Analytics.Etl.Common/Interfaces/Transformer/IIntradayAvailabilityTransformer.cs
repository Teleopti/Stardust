using System.Collections.Generic;
using System.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
	public interface IIntradayAvailabilityTransformer
	{
		void Transform(IEnumerable<IStudentAvailabilityDay> rootList, DataTable table, ICommonStateHolder stateHolder,
						IScenario scenario);
	}
}
