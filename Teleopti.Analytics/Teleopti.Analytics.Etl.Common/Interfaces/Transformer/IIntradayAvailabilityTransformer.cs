using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
	public interface IIntradayAvailabilityTransformer
	{
		void Transform(IEnumerable<IStudentAvailabilityDay> rootList, DataTable table, ICommonStateHolder stateHolder,
						IScenario scenario);
	}
}
